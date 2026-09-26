using Eve64;

namespace FleetOrchestrator.Fleet;

/// <summary>
/// Everything one activity needs to decide for a single member this tick: the member,
/// its freshly parsed UI, and the shared fleet blackboard.
/// </summary>
public sealed class FleetMemberContext
{
    public required FleetMember Member { get; init; }
    public required ParsedUserInterface Ui { get; init; }
    public required IFleetCoordination Fleet { get; init; }
    public required long TickTimeMs { get; init; }
}

/// <summary>
/// A switchable fleet-wide activity — the thing the whole fleet is "doing" right now
/// (ratting an anomaly, running an abyss, mining, traveling to staging, sitting idle).
/// Switching the active activity re-tasks every role at once, which is how the fleet
/// changes what it does for variety.
///
/// Each tick the orchestrator calls <see cref="Coordinate"/> once (to update shared
/// plan: primary target, anchor, heal needs) and then <see cref="Decide"/> per member
/// (to turn that shared plan + the member's role into concrete intent/actions).
/// </summary>
public interface IFleetActivity
{
    /// <summary>Stable identifier used to select this activity (case-insensitive).</summary>
    string Name { get; }

    /// <summary>One-line description shown when listing activities.</summary>
    string Description { get; }

    /// <summary>
    /// Update the shared blackboard from the whole fleet's situation before any
    /// per-member decision is made. Pure coordination logic — no input actions here.
    /// </summary>
    void Coordinate(IReadOnlyList<FleetMemberContext> members, IFleetCoordination fleet);

    /// <summary>Decide what a single member should do, given its role and the shared plan.</summary>
    MemberDecision Decide(FleetMemberContext ctx);
}
