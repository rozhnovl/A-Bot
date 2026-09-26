using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WebUI.Esi;

public sealed class EsiHttpClient(HttpClient http, IOptions<EsiOptions> options)
{
    private readonly EsiOptions _options = options.Value;

    public async Task<EsiAccessToken> ExchangeCodeAsync(string code, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://login.eveonline.com/v2/oauth/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = _options.RedirectUri
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        using var response = await http.SendAsync(request, ct);
        ThrowIfBackoff(response, DateTimeOffset.UtcNow);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var access = json.RootElement.GetProperty("access_token").GetString();
        var refresh = json.RootElement.GetProperty("refresh_token").GetString();
        if (string.IsNullOrWhiteSpace(access) || string.IsNullOrWhiteSpace(refresh))
            throw new InvalidDataException("SSO returned incomplete authorization tokens.");
        return new EsiAccessToken(access, refresh);
    }

    public async Task<long> GetAuthorizedCharacterIdAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://login.eveonline.com/v2/oauth/verify");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        using var response = await http.SendAsync(request, ct);
        ThrowIfBackoff(response, DateTimeOffset.UtcNow);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return json.RootElement.GetProperty("CharacterID").GetInt64();
    }

    public async Task<EsiAccessToken> RefreshAccessTokenAsync(EsiCharacterGrant grant, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://login.eveonline.com/v2/oauth/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = grant.RefreshToken
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")));
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        using var response = await http.SendAsync(request, ct);
        ThrowIfBackoff(response, DateTimeOffset.UtcNow);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = document.RootElement;
        var accessToken = root.GetProperty("access_token").GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidDataException("SSO returned no access token.");
        // SSO may rotate refresh tokens. The caller must persist this before using access.
        var rotatedRefreshToken = root.TryGetProperty("refresh_token", out var rotated)
            ? rotated.GetString() : null;
        return new EsiAccessToken(accessToken, rotatedRefreshToken);
    }

    public async Task VerifyCharacterAsync(long characterId, string accessToken, CancellationToken ct)
    {
        if (await GetAuthorizedCharacterIdAsync(accessToken, ct) != characterId)
            throw new InvalidDataException("SSO token belongs to a different character.");
    }

    public async Task<EsiPageResponse> GetPageAsync(
        long characterId, EsiResource resource, string cursor, string accessToken,
        string? etag, CancellationToken ct)
    {
        var path = resource switch
        {
            EsiResource.WalletTransactions => $"/characters/{characterId}/wallet/transactions/" +
                (cursor == "first" ? "" : $"?from_id={cursor}"),
            EsiResource.WalletJournal => $"/characters/{characterId}/wallet/journal/" + PageQuery(cursor),
            EsiResource.IndustryJobs => $"/characters/{characterId}/industry/jobs/?include_completed=true",
            EsiResource.ActiveOrders => $"/characters/{characterId}/orders/",
            EsiResource.OrderHistory => $"/characters/{characterId}/orders/history/" + PageQuery(cursor),
            EsiResource.Assets => $"/characters/{characterId}/assets/" + PageQuery(cursor),
            _ => throw new ArgumentOutOfRangeException(nameof(resource))
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://esi.evetech.net" + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);
        request.Headers.TryAddWithoutValidation("X-Compatibility-Date", _options.CompatibilityDate);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(etag))
            request.Headers.TryAddWithoutValidation("If-None-Match", etag);

        using var response = await http.SendAsync(request, ct);
        var now = DateTimeOffset.UtcNow;
        var expires = Header(response, "Expires") is { } value &&
            DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var parsed) && parsed > now
                ? parsed : now.AddMinutes(5);
        var newEtag = response.Headers.ETag?.ToString() ?? Header(response, "ETag") ?? etag;
        ThrowIfBackoff(response, now);
        if (response.StatusCode == HttpStatusCode.NotModified)
            return new EsiPageResponse(true, null, new EsiCacheState(newEtag, expires), null);
        response.EnsureSuccessStatusCode();
        var rawJson = await response.Content.ReadAsStringAsync(ct);
        using var document = JsonDocument.Parse(rawJson);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"ESI {resource} did not return a JSON array.");
        int? pages = int.TryParse(Header(response, "X-Pages"), out var n) && n > 0 ? n : null;
        return new EsiPageResponse(false, rawJson, new EsiCacheState(newEtag, expires), pages);
    }

    private static string PageQuery(string cursor) => cursor == "1" ? "" : $"?page={cursor}";

    private static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() :
        response.Content.Headers.TryGetValues(name, out values) ? values.FirstOrDefault() : null;

    private static TimeSpan RetryAfter(HttpResponseMessage response, DateTimeOffset now)
    {
        var header = response.Headers.RetryAfter;
        var delay = header?.Delta ?? (header?.Date is { } date ? date - now : TimeSpan.FromMinutes(1));
        return delay > TimeSpan.Zero ? delay : TimeSpan.FromSeconds(1);
    }

    private static void ThrowIfBackoff(HttpResponseMessage response, DateTimeOffset now)
    {
        if (response.StatusCode is HttpStatusCode.TooManyRequests or (HttpStatusCode)420 or HttpStatusCode.ServiceUnavailable)
            throw new EsiBackoffException(response.StatusCode, RetryAfter(response, now));
    }
}

public sealed record EsiPageResponse(
    bool NotModified, string? RawJson, EsiCacheState Cache, int? TotalPages);

public sealed record EsiAccessToken(string Value, string? RotatedRefreshToken);

public sealed class EsiBackoffException(HttpStatusCode statusCode, TimeSpan retryAfter)
    : HttpRequestException($"ESI returned {(int)statusCode}; retry after {retryAfter}.", null, statusCode)
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
