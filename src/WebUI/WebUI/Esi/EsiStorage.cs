using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebUI.Data;

namespace WebUI.Esi;

public sealed class EsiStoredGrant
{
    public long CharacterId { get; set; }
    public string ProtectedRefreshToken { get; set; } = "";
}

public sealed class EsiStoredPage
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public EsiResource Resource { get; set; }
    public string Cursor { get; set; } = "";
    public string RawJson { get; set; } = "[]";
    public string? ETag { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public int? TotalPages { get; set; }
    public string? NextCursor { get; set; }
    public DateTimeOffset FetchedAt { get; set; }
}

public sealed class EsiStoredRecord
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public EsiRecordKind Kind { get; set; }
    public long RecordId { get; set; }
    public EsiResource Resource { get; set; }
    public string RawJson { get; set; } = "{}";
    public DateTimeOffset ObservedAt { get; set; }
    public bool IsCurrent { get; set; } = true;
}

public sealed class EsiSqliteStore(
    IDbContextFactory<ProductionDbContext> factory,
    IOptions<EsiOptions> options,
    IDataProtectionProvider protection) : IEsiTokenStore, IEsiSnapshotSink
{
    private readonly EsiOptions _options = options.Value;
    private readonly IDataProtector _protector = protection.CreateProtector("A-Bot.ESI.RefreshToken.v1");

    public async Task<EsiCharacterGrant?> GetCharacterAsync(CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var saved = _options.CharacterId > 0
            ? await db.EsiGrants.FindAsync([_options.CharacterId], ct)
            : await db.EsiGrants.OrderBy(g => g.CharacterId).FirstOrDefaultAsync(ct);
        if (saved is not null)
            return new EsiCharacterGrant(saved.CharacterId, _protector.Unprotect(saved.ProtectedRefreshToken));
        return _options.CharacterId > 0 && !string.IsNullOrWhiteSpace(_options.RefreshToken)
            ? new EsiCharacterGrant(_options.CharacterId, _options.RefreshToken) : null;
    }

    public async Task SaveGrantAsync(long characterId, string refreshToken, CancellationToken ct)
    {
        if (characterId <= 0 || string.IsNullOrWhiteSpace(refreshToken) ||
            _options.CharacterId > 0 && _options.CharacterId != characterId)
            throw new InvalidOperationException("The ESI authorization is for a different character.");
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.EsiGrants.AnyAsync(g => g.CharacterId != characterId, ct))
            throw new InvalidOperationException("This installation currently supports one ESI character. Reconcile and migrate existing data before changing the owner.");
        var saved = await db.EsiGrants.FindAsync([characterId], ct);
        if (saved is null)
            db.EsiGrants.Add(new EsiStoredGrant { CharacterId = characterId,
                ProtectedRefreshToken = _protector.Protect(refreshToken) });
        else saved.ProtectedRefreshToken = _protector.Protect(refreshToken);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveRefreshTokenAsync(long characterId, string expectedRefreshToken,
        string newRefreshToken, CancellationToken ct)
    {
        if (_options.CharacterId > 0 && characterId != _options.CharacterId || string.IsNullOrWhiteSpace(newRefreshToken))
            throw new InvalidOperationException("Invalid ESI refresh-token rotation.");
        await using var db = await factory.CreateDbContextAsync(ct);
        var saved = await db.EsiGrants.FindAsync([characterId], ct);
        var current = saved is null ? _options.RefreshToken : _protector.Unprotect(saved.ProtectedRefreshToken);
        if (current != expectedRefreshToken)
            throw new InvalidOperationException("ESI refresh token changed concurrently.");
        if (saved is null)
            db.EsiGrants.Add(new EsiStoredGrant { CharacterId = characterId,
                ProtectedRefreshToken = _protector.Protect(newRefreshToken) });
        else saved.ProtectedRefreshToken = _protector.Protect(newRefreshToken);
        await db.SaveChangesAsync(ct);
    }

    public async Task<EsiCacheState?> GetCacheAsync(long characterId, EsiResource resource,
        string cursor, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var page = await db.EsiPages.AsNoTracking().FirstOrDefaultAsync(p =>
            p.CharacterId == characterId && p.Resource == resource && p.Cursor == cursor, ct);
        return page is null ? null : new EsiCacheState(page.ETag, page.ExpiresAt,
            page.TotalPages, page.NextCursor);
    }

    public async Task SavePageAsync(EsiSnapshotPage page, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var stored = await db.EsiPages.FirstOrDefaultAsync(p => p.CharacterId == page.CharacterId &&
            p.Resource == page.Resource && p.Cursor == page.Cursor, ct);
        if (stored is null)
        {
            stored = new EsiStoredPage { CharacterId = page.CharacterId,
                Resource = page.Resource, Cursor = page.Cursor };
            db.EsiPages.Add(stored);
        }
        stored.RawJson = page.RawJson;
        stored.ETag = page.Cache.ETag;
        stored.ExpiresAt = page.Cache.ExpiresAt;
        stored.TotalPages = page.TotalPages;
        stored.NextCursor = page.Cache.NextCursor;
        stored.FetchedAt = page.FetchedAt;

        using var json = JsonDocument.Parse(page.RawJson);
        var rows = json.RootElement.EnumerateArray().ToArray();
        if (rows.Length != page.RecordIds.Count)
            throw new InvalidDataException("ESI page IDs do not match rows.");
        // Cursor-based wallet transactions are immutable events. Numbered resources
        // are published by CompleteResourceAsync only after every page is available.
        if (page.Resource == EsiResource.WalletTransactions)
        {
            var ids = page.RecordIds.ToArray();
            var old = await db.EsiRecords.Where(r => r.CharacterId == page.CharacterId &&
                r.Kind == page.RecordKind && ids.Contains(r.RecordId)).ToDictionaryAsync(r => r.RecordId, ct);
            for (var i = 0; i < ids.Length; i++)
            {
                if (!old.TryGetValue(ids[i], out var record))
                {
                    record = new EsiStoredRecord { CharacterId = page.CharacterId,
                        Kind = page.RecordKind, RecordId = ids[i] };
                    db.EsiRecords.Add(record);
                    old[ids[i]] = record;
                }
                record.Resource = page.Resource;
                record.RawJson = rows[i].GetRawText();
                record.ObservedAt = page.FetchedAt;
                record.IsCurrent = true;
            }
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateCacheAsync(long characterId, EsiResource resource,
        string cursor, EsiCacheState cache, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var page = await db.EsiPages.FirstAsync(p => p.CharacterId == characterId &&
            p.Resource == resource && p.Cursor == cursor, ct);
        page.ETag = cache.ETag;
        page.ExpiresAt = cache.ExpiresAt;
        page.TotalPages = cache.TotalPages;
        page.NextCursor = cache.NextCursor;
        page.FetchedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task CompleteResourceAsync(long characterId, EsiResource resource,
        int totalPages, CancellationToken ct)
    {
        if (resource == EsiResource.WalletTransactions) return;
        await using var db = await factory.CreateDbContextAsync(ct);
        var pages = await db.EsiPages.AsNoTracking().Where(p => p.CharacterId == characterId &&
            p.Resource == resource).ToListAsync(ct);
        var completePages = pages.Where(p => int.TryParse(p.Cursor, out var n) &&
            n >= 1 && n <= totalPages).ToList();
        if (completePages.Select(p => p.Cursor).Distinct().Count() != totalPages)
            throw new InvalidDataException($"Incomplete ESI {resource} snapshot: expected {totalPages} pages.");
        var (kind, idName) = resource switch
        {
            EsiResource.Assets => (EsiRecordKind.Asset, "item_id"),
            EsiResource.ActiveOrders or EsiResource.OrderHistory => (EsiRecordKind.Order, "order_id"),
            EsiResource.IndustryJobs => (EsiRecordKind.IndustryJob, "job_id"),
            EsiResource.WalletJournal => (EsiRecordKind.WalletJournalEntry, "id"),
            _ => throw new ArgumentOutOfRangeException(nameof(resource))
        };
        var current = new Dictionary<long, (string Json, DateTimeOffset ObservedAt)>();
        foreach (var page in completePages)
        {
            using var json = JsonDocument.Parse(page.RawJson);
            foreach (var row in json.RootElement.EnumerateArray())
                current[row.GetProperty(idName).GetInt64()] = (row.GetRawText(), page.FetchedAt);
        }
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var currentIds = current.Keys.ToArray();
        var existing = await db.EsiRecords.Where(r => r.CharacterId == characterId &&
            r.Kind == kind && currentIds.Contains(r.RecordId)).ToDictionaryAsync(r => r.RecordId, ct);
        foreach (var (id, value) in current)
        {
            if (!existing.TryGetValue(id, out var record))
            {
                record = new EsiStoredRecord { CharacterId = characterId, Kind = kind, RecordId = id };
                db.EsiRecords.Add(record);
            }
            record.Resource = resource;
            record.RawJson = value.Json;
            record.ObservedAt = value.ObservedAt;
            record.IsCurrent = true;
        }
        if (resource is EsiResource.ActiveOrders or EsiResource.Assets)
        {
            var previous = await db.EsiRecords.Where(r => r.CharacterId == characterId &&
                r.Resource == resource).ToListAsync(ct);
            foreach (var record in previous)
                if (!current.ContainsKey(record.RecordId)) record.IsCurrent = false;
        }
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
