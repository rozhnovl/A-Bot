using Sanderling.ABot.Bot;

namespace FleetOrchestrator.Fleet;

/// <summary>Role a single EVE client plays inside the fleet.</summary>
public enum FleetRole
{
    Scout,
    Dps,
    Healer,
    Anchor,
}

/// <summary>
/// A compact, serializable snapshot of one member's situation, published to the
/// shared coordination bus every tick. This is intentionally small — the kind of
/// thing that could travel over Redis without a second thought (unlike the full
/// 6 MB UI tree, which must never leave the local process).
/// </summary>
public record FleetMemberStatus(
    int Pid,
    FleetRole Role,
    string? SolarSystem,
    int? ArmorPct,
    int? ShieldPct,
    int? StructPct,
    int TargetsLocked,
    int OverviewEntries,
    bool InSpace,
    long UpdatedAtMs);

/// <summary>A "primary target" call broadcast to the fleet.</summary>
public record PrimaryTargetCall(string TargetName, int CallerPid, long CalledAtMs);

/// <summary>A request for the healer to repair a struggling member.</summary>
public record HealRequest(int Pid, int ArmorPct, int ShieldPct, long RequestedAtMs);

/// <summary>
/// The result of an activity deciding what one member should do this tick.
/// <see cref="Intent"/> is a human-readable line for the log; <see cref="Motions"/>
/// are the concrete input actions (empty until a role's real behavior is wired,
/// and only executed when the orchestrator runs with --live).
/// </summary>
public record MemberDecision(string Intent, IReadOnlyList<MotionRecommendation> Motions)
{
    public static MemberDecision Say(string intent) =>
        new(intent, Array.Empty<MotionRecommendation>());

    public static MemberDecision Act(string intent, IReadOnlyList<MotionRecommendation> motions) =>
        new(intent, motions);
}
