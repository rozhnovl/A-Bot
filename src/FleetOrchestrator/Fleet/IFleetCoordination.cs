using System.Collections.Concurrent;

namespace FleetOrchestrator.Fleet;

/// <summary>
/// The shared coordination "blackboard" for the fleet. Roles publish their status
/// here and read each other's, activities post the primary-target call, the anchor,
/// and heal requests here.
///
/// It is deliberately an interface so the transport can change without touching any
/// activity or role logic. Today the only implementation is <see cref="InMemoryFleetCoordination"/>
/// (shared objects inside the single orchestrator process — zero latency, no dependency).
/// A Redis-backed implementation with the exact same contract can be dropped in later
/// if the fleet ever needs a live dashboard or to span several machines; at EVE's
/// ~1s server tick, Redis pub/sub latency is irrelevant, so nothing here assumes in-process.
/// </summary>
public interface IFleetCoordination
{
    /// <summary>Name of the fleet-wide activity currently in effect (e.g. "AnomalyRatting").</summary>
    string ActivityName { get; set; }

    void PublishStatus(FleetMemberStatus status);
    IReadOnlyCollection<FleetMemberStatus> AllStatuses { get; }
    FleetMemberStatus? StatusOf(int pid);

    /// <summary>The called primary target the fleet should focus fire, if any.</summary>
    PrimaryTargetCall? PrimaryTarget { get; set; }

    /// <summary>Pid of the member the fleet anchors on (follows / warps with).</summary>
    int? AnchorPid { get; set; }

    void RequestHeal(HealRequest request);
    IReadOnlyCollection<HealRequest> ActiveHealRequests { get; }
    void ClearHeal(int pid);
}

/// <summary>In-process, thread-safe coordination blackboard.</summary>
public sealed class InMemoryFleetCoordination : IFleetCoordination
{
    private readonly ConcurrentDictionary<int, FleetMemberStatus> statuses = new();
    private readonly ConcurrentDictionary<int, HealRequest> healRequests = new();

    public string ActivityName { get; set; } = "Idle";

    public PrimaryTargetCall? PrimaryTarget { get; set; }

    public int? AnchorPid { get; set; }

    public void PublishStatus(FleetMemberStatus status) => statuses[status.Pid] = status;

    public IReadOnlyCollection<FleetMemberStatus> AllStatuses => statuses.Values.ToArray();

    public FleetMemberStatus? StatusOf(int pid) => statuses.TryGetValue(pid, out var s) ? s : null;

    public void RequestHeal(HealRequest request) => healRequests[request.Pid] = request;

    public IReadOnlyCollection<HealRequest> ActiveHealRequests => healRequests.Values.ToArray();

    public void ClearHeal(int pid) => healRequests.TryRemove(pid, out _);
}
