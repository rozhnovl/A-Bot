using System.Security.Claims;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebUI.Data;

namespace WebUI.Esi;

public static class EsiEndpoints
{
    private const string Scopes = "esi-wallet.read_character_wallet.v1 esi-industry.read_character_jobs.v1 esi-markets.read_character_orders.v1 esi-assets.read_assets.v1";

    public static IEndpointRouteBuilder MapEsiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/esi/connect", (ClaimsPrincipal user, IOptions<EsiOptions> options,
            EsiOAuthState oauthState) =>
        {
            var configured = options.Value;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!IsOperator(user, configured.OperatorEmail)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(userId) || !configured.HasCredentials ||
                !Uri.TryCreate(configured.RedirectUri, UriKind.Absolute, out var redirect) ||
                (redirect.Scheme != Uri.UriSchemeHttps &&
                 !(redirect.Scheme == Uri.UriSchemeHttp && redirect.Host == "localhost")))
                return Results.BadRequest("Configure Esi:ClientId, ClientSecret and HTTPS RedirectUri first.");
            var state = oauthState.Issue(userId);
            var url = "https://login.eveonline.com/v2/oauth/authorize?response_type=code" +
                $"&client_id={Uri.EscapeDataString(configured.ClientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(configured.RedirectUri)}" +
                $"&scope={Uri.EscapeDataString(Scopes)}" +
                $"&state={Uri.EscapeDataString(state)}";
            return Results.Redirect(url);
        }).RequireAuthorization();

        endpoints.MapGet("/esi/callback", async (HttpRequest request, ClaimsPrincipal user,
            IOptions<EsiOptions> options, EsiOAuthState oauthState,
            EsiHttpClient client, EsiSqliteStore store,
            CancellationToken ct) =>
        {
            var code = request.Query["code"].ToString();
            var state = request.Query["state"].ToString();
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!IsOperator(user, options.Value.OperatorEmail)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state) ||
                string.IsNullOrWhiteSpace(userId)) return Results.BadRequest("Missing OAuth code or state.");
            if (!oauthState.TryConsume(state, userId))
                return Results.BadRequest("Invalid, expired or already used ESI authorization state.");
            var token = await client.ExchangeCodeAsync(code, ct);
            var characterId = await client.GetAuthorizedCharacterIdAsync(token.Value, ct);
            await store.SaveGrantAsync(characterId, token.RotatedRefreshToken!, ct);
            return Results.Redirect("/?esi=connected");
        }).RequireAuthorization();

        var api = endpoints.MapGroup("/api/production/esi");
        api.MapGet("/status", async (HttpRequest request, IConfiguration configuration,
            IDbContextFactory<ProductionDbContext> factory, EsiSyncHealth health,
            CancellationToken ct) =>
        {
            if (!ProductionApiExtensions.IsAuthorized(request, configuration)) return Results.Unauthorized();
            await using var db = await factory.CreateDbContextAsync(ct);
            var pages = await db.EsiPages.AsNoTracking().ToListAsync(ct);
            var grants = await db.EsiGrants.AsNoTracking().Select(g => g.CharacterId).ToListAsync(ct);
            return Results.Ok(new
            {
                connectedCharacters = grants,
                sync = health.Snapshot(),
                resources = pages.GroupBy(p => new { p.CharacterId, p.Resource }).Select(g => new
                {
                    g.Key.CharacterId,
                    resource = g.Key.Resource.ToString(),
                    pages = g.Count(),
                    lastFetchedAt = g.Max(p => p.FetchedAt),
                    oldestExpiry = g.Min(p => p.ExpiresAt)
                }).ToList(),
                generatedAt = DateTimeOffset.UtcNow
            });
        });
        api.MapGet("/records", async (HttpRequest request, IConfiguration configuration,
            IDbContextFactory<ProductionDbContext> factory, string? kind, int? limit,
            long? beforeId,
            CancellationToken ct) =>
        {
            if (!ProductionApiExtensions.IsAuthorized(request, configuration)) return Results.Unauthorized();
            if (!Enum.TryParse<EsiRecordKind>(kind, true, out var selected) ||
                !Enum.IsDefined(selected))
                return Results.BadRequest("kind must be WalletTransaction, WalletJournalEntry, IndustryJob, Order or Asset.");
            await using var db = await factory.CreateDbContextAsync(ct);
            var query = db.EsiRecords.AsNoTracking().Where(r => r.Kind == selected);
            if (beforeId is > 0) query = query.Where(r => r.RecordId < beforeId);
            var rows = await query.OrderByDescending(r => r.RecordId)
                .Take(Math.Clamp(limit ?? 100, 1, 500))
                .ToListAsync(ct);
            return Results.Ok(new { items = rows.Select(r => new
            {
                r.CharacterId, r.RecordId, resource = r.Resource.ToString(),
                r.IsCurrent, r.ObservedAt,
                data = System.Text.Json.JsonDocument.Parse(r.RawJson).RootElement.Clone()
            }).ToList(), nextBeforeId = rows.Count > 0 ? (long?)rows[^1].RecordId : null });
        });
        return endpoints;
    }

    private static bool IsOperator(ClaimsPrincipal user, string configuredEmail) =>
        !string.IsNullOrWhiteSpace(configuredEmail) &&
        (string.Equals(user.FindFirstValue(ClaimTypes.Email), configuredEmail,
             StringComparison.OrdinalIgnoreCase) ||
         string.Equals(user.Identity?.Name, configuredEmail, StringComparison.OrdinalIgnoreCase));
}

public sealed class EsiOAuthState(IDataProtectionProvider protection)
{
    private readonly ITimeLimitedDataProtector _protector = protection
        .CreateProtector("A-Bot.ESI.OAuthState.v1").ToTimeLimitedDataProtector();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _pending = new();

    public string Issue(string userId)
    {
        foreach (var item in _pending.Where(x => x.Value < DateTimeOffset.UtcNow))
            _pending.TryRemove(item.Key, out _);
        var state = _protector.Protect($"{userId}:{Guid.NewGuid():N}", TimeSpan.FromMinutes(10));
        _pending[state] = DateTimeOffset.UtcNow.AddMinutes(10);
        return state;
    }

    public bool TryConsume(string state, string userId)
    {
        if (!_pending.TryRemove(state, out var expires) || expires < DateTimeOffset.UtcNow)
            return false;
        try
        {
            var value = _protector.Unprotect(state);
            return value.StartsWith(userId + ":", StringComparison.Ordinal);
        }
        catch (Exception) { return false; }
    }
}
