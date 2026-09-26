namespace WebUI.Esi;

public enum EsiResource
{
    WalletTransactions,
    IndustryJobs,
    ActiveOrders,
    OrderHistory,
    Assets,
    WalletJournal
}

public enum EsiRecordKind
{
    WalletTransaction,
    IndustryJob,
    Order,
    Asset,
    WalletJournalEntry
}

public sealed record EsiCharacterGrant(long CharacterId, string RefreshToken);

// The application owns encrypted/secure persistence and OAuth consent for this one character.
public interface IEsiTokenStore
{
    Task<EsiCharacterGrant?> GetCharacterAsync(CancellationToken cancellationToken);

    // Persist a rotated token before any further ESI work. An implementation should use
    // expectedRefreshToken as a compare-and-swap guard when multiple workers can run.
    Task SaveRefreshTokenAsync(
        long characterId, string expectedRefreshToken, string newRefreshToken,
        CancellationToken cancellationToken);
}

public sealed record EsiCacheState(
    string? ETag, DateTimeOffset? ExpiresAt,
    int? TotalPages = null, string? NextCursor = null);

public sealed record EsiSnapshotPage(
    long CharacterId,
    EsiResource Resource,
    EsiRecordKind RecordKind,
    string Cursor,
    string RawJson,
    IReadOnlyList<long> RecordIds,
    EsiCacheState Cache,
    DateTimeOffset FetchedAt,
    int? TotalPages);

// Key each page by (characterId, resource, cursor), and each row by
// (characterId, recordKind, RecordIds[i]). Active and historical orders share
// the Order kind, so an order moving to history retains its identity.
// Save the raw page and cache atomically.
public interface IEsiSnapshotSink
{
    Task<EsiCacheState?> GetCacheAsync(
        long characterId, EsiResource resource, string cursor,
        CancellationToken cancellationToken);

    Task SavePageAsync(EsiSnapshotPage page, CancellationToken cancellationToken);

    // A 304 has no body: retain the previously saved page and ETag, update expiry.
    Task UpdateCacheAsync(
        long characterId, EsiResource resource, string cursor,
        EsiCacheState cache, CancellationToken cancellationToken);

    // Called only after every page of a paginated resource is available. The sink
    // can now replace its active view and discard pages no longer in X-Pages.
    Task CompleteResourceAsync(
        long characterId, EsiResource resource, int totalPages,
        CancellationToken cancellationToken);
}
