namespace WebUI.Esi;

public sealed record EsiSyncSnapshot(
    string State,
    long? CharacterId,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? Message);

// Operational state is intentionally separate from persisted observations: a failed
// poll must never erase the last known wallet/order/asset data.
public sealed class EsiSyncHealth
{
    private readonly object _gate = new();
    private EsiSyncSnapshot _snapshot = new("waiting", null, null, null, null, null);

    public EsiSyncSnapshot Snapshot()
    {
        lock (_gate) return _snapshot;
    }

    public void Attempt(long characterId)
    {
        lock (_gate) _snapshot = _snapshot with
        {
            State = "syncing", CharacterId = characterId,
            LastAttemptAt = DateTimeOffset.UtcNow, Message = null
        };
    }

    public void Success(long characterId)
    {
        lock (_gate) _snapshot = _snapshot with
        {
            State = "healthy", CharacterId = characterId,
            LastSuccessAt = DateTimeOffset.UtcNow, Message = null
        };
    }

    public void Failure(long? characterId, string error)
    {
        lock (_gate) _snapshot = _snapshot with
        {
            State = "error", CharacterId = characterId ?? _snapshot.CharacterId,
            LastFailureAt = DateTimeOffset.UtcNow,
            Message = error.Length <= 300 ? error : error[..300]
        };
    }

    public void Skipped(string reason)
    {
        lock (_gate) _snapshot = _snapshot with { State = "not-connected", Message = reason };
    }
}
