using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WebUI.Esi;

public sealed class EsiIngestionService(
    IServiceScopeFactory scopes,
    EsiHttpClient client,
    IOptions<EsiOptions> options,
    EsiSyncHealth health,
    ILogger<EsiIngestionService> logger) : BackgroundService
{
    private readonly EsiOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = _options.PollInterval > TimeSpan.Zero
                ? _options.PollInterval : TimeSpan.FromMinutes(5);
            long? characterId = null;
            if (_options.HasCredentials)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var tokens = scope.ServiceProvider.GetRequiredService<IEsiTokenStore>();
                    var grant = await tokens.GetCharacterAsync(stoppingToken);
                    if (grant is { CharacterId: > 0 } && !string.IsNullOrWhiteSpace(grant.RefreshToken))
                    {
                        characterId = grant.CharacterId;
                        health.Attempt(grant.CharacterId);
                        var sink = scope.ServiceProvider.GetRequiredService<IEsiSnapshotSink>();
                        var access = await client.RefreshAccessTokenAsync(grant, stoppingToken);
                        if (!string.IsNullOrWhiteSpace(access.RotatedRefreshToken) &&
                            access.RotatedRefreshToken != grant.RefreshToken)
                            await tokens.SaveRefreshTokenAsync(grant.CharacterId, grant.RefreshToken,
                                access.RotatedRefreshToken, stoppingToken);
                        await client.VerifyCharacterAsync(grant.CharacterId, access.Value, stoppingToken);
                        var complete = await SyncCharacterAsync(grant.CharacterId, access.Value, sink, stoppingToken);
                        await scope.ServiceProvider.GetRequiredService<EsiSalesProjection>()
                            .ProjectAsync(grant.CharacterId, stoppingToken);
                        if (complete)
                            health.Success(grant.CharacterId);
                        else
                            health.Failure(grant.CharacterId,
                                "One or more ESI resources were not fully published; see sync warnings.");
                    }
                    else
                        health.Skipped("No authorized character refresh token is stored.");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (EsiBackoffException ex)
                {
                    delay = ex.RetryAfter > delay ? ex.RetryAfter : delay;
                    health.Failure(characterId, ex.Message);
                    logger.LogWarning("ESI throttled or unavailable (HTTP {Status}); next sync in {Delay}.",
                        (int?)ex.StatusCode, delay);
                }
                catch (Exception ex)
                {
                    health.Failure(characterId, ex.Message);
                    logger.LogError(ex, "ESI character synchronization failed; will retry next poll.");
                }
            }
            else
                health.Skipped("ESI client credentials are not configured.");

            try { await Task.Delay(delay, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task<bool> SyncCharacterAsync(long characterId, string accessToken,
        IEsiSnapshotSink sink, CancellationToken ct)
    {
        await SyncWalletAsync(characterId, accessToken, sink, ct);
        // The journal endpoint exposes a rolling 30-day window via page/X-Pages.
        var complete = await SyncPagedAsync(characterId, EsiResource.WalletJournal, accessToken, sink, ct);
        complete &= await SyncPagedAsync(characterId, EsiResource.IndustryJobs, accessToken, sink, ct);
        complete &= await SyncPagedAsync(characterId, EsiResource.ActiveOrders, accessToken, sink, ct);
        complete &= await SyncPagedAsync(characterId, EsiResource.OrderHistory, accessToken, sink, ct);
        complete &= await SyncPagedAsync(characterId, EsiResource.Assets, accessToken, sink, ct);
        return complete;
    }

    private async Task SyncWalletAsync(long characterId, string accessToken,
        IEsiSnapshotSink sink, CancellationToken ct)
    {
        var cursor = "first";
        for (var page = 0; page < Math.Max(1, _options.MaxWalletPages); page++)
        {
            var state = await FetchAsync(characterId, EsiResource.WalletTransactions,
                cursor, accessToken, sink, ct);
            if (state?.NextCursor is not { } next || next == cursor)
                break;
            if (page + 1 == Math.Max(1, _options.MaxWalletPages))
                logger.LogWarning("Wallet backfill for character {CharacterId} reached MaxWalletPages; increase it to fetch older transactions.", characterId);
            cursor = next;
        }
    }

    private async Task<bool> SyncPagedAsync(long characterId, EsiResource resource,
        string accessToken, IEsiSnapshotSink sink, CancellationToken ct)
    {
        var maximum = Math.Max(1, _options.MaxPagesPerResource);
        var first = await FetchAsync(characterId, resource, "1", accessToken, sink, ct);
        if (resource == EsiResource.WalletJournal && first?.TotalPages is not > 0)
            throw new InvalidDataException("ESI wallet journal did not supply a valid X-Pages value.");
        var total = first?.TotalPages ?? 1;
        if (total > maximum)
        {
            logger.LogWarning("{Resource} for character {CharacterId} has {Total} pages, above MaxPagesPerResource ({Maximum}); publication deferred.",
                resource, characterId, total, maximum);
            return false;
        }
        for (var page = 2; page <= total; page++)
        {
            var state = await FetchAsync(characterId, resource,
                page.ToString(CultureInfo.InvariantCulture), accessToken, sink, ct);
            if (state?.TotalPages != total)
            {
                logger.LogWarning("{Resource} X-Pages changed during sync for character {CharacterId}: page 1 reported {Expected}, page {Page} reported {Actual}; publication deferred.",
                    resource, characterId, total, page, state?.TotalPages);
                return false;
            }
        }
        await sink.CompleteResourceAsync(characterId, resource, total, ct);
        return true;
    }

    private async Task<EsiCacheState?> FetchAsync(long characterId, EsiResource resource,
        string cursor, string accessToken, IEsiSnapshotSink sink, CancellationToken ct)
    {
        var previous = await sink.GetCacheAsync(characterId, resource, cursor, ct);
        if (previous?.ExpiresAt > DateTimeOffset.UtcNow)
            return previous;

        var response = await client.GetPageAsync(characterId, resource, cursor,
            accessToken, previous?.ETag, ct);
        if (response.NotModified)
        {
            if (previous is null)
                throw new InvalidDataException("ESI returned 304 without a saved snapshot.");
            var renewed = previous with
            {
                ETag = response.Cache.ETag,
                ExpiresAt = response.Cache.ExpiresAt
            };
            await sink.UpdateCacheAsync(characterId, resource, cursor, renewed, ct);
            return renewed;
        }

        var raw = response.RawJson ?? throw new InvalidDataException("ESI returned no body.");
        var ids = ReadIds(raw, resource);
        string? nextCursor = null;
        if (resource == EsiResource.WalletTransactions && ids.Count > 0)
        {
            // from_id requests older transactions; preserve the last (lowest) ID.
            nextCursor = ids.Min().ToString(CultureInfo.InvariantCulture);
        }
        var cache = response.Cache with { TotalPages = response.TotalPages, NextCursor = nextCursor };
        var kind = resource switch
        {
            EsiResource.WalletTransactions => EsiRecordKind.WalletTransaction,
            EsiResource.WalletJournal => EsiRecordKind.WalletJournalEntry,
            EsiResource.IndustryJobs => EsiRecordKind.IndustryJob,
            EsiResource.ActiveOrders or EsiResource.OrderHistory => EsiRecordKind.Order,
            EsiResource.Assets => EsiRecordKind.Asset,
            _ => throw new ArgumentOutOfRangeException(nameof(resource))
        };
        await sink.SavePageAsync(new EsiSnapshotPage(characterId, resource, kind, cursor,
            raw, ids, cache, DateTimeOffset.UtcNow, response.TotalPages), ct);
        return cache;
    }

    private static IReadOnlyList<long> ReadIds(string rawJson, EsiResource resource)
    {
        var property = resource switch
        {
            EsiResource.WalletTransactions => "transaction_id",
            EsiResource.WalletJournal => "id",
            EsiResource.IndustryJobs => "job_id",
            EsiResource.ActiveOrders or EsiResource.OrderHistory => "order_id",
            EsiResource.Assets => "item_id",
            _ => throw new ArgumentOutOfRangeException(nameof(resource))
        };
        using var document = JsonDocument.Parse(rawJson);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"ESI {resource} did not return an array.");
        var ids = new List<long>();
        foreach (var row in document.RootElement.EnumerateArray())
            ids.Add(row.GetProperty(property).GetInt64());
        return ids;
    }
}
