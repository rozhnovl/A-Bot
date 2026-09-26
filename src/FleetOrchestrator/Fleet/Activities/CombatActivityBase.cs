namespace FleetOrchestrator.Fleet.Activities;

/// <summary>
/// Shared coordination for any activity that involves fighting: choose an anchor,
/// call a primary target from the anchor/scout's overview, and raise heal requests
/// when a member drops below the armor threshold. Concrete combat activities reuse
/// this and only differ in per-role behavior and target policy.
///
/// All of this is pure blackboard logic (no input actions), so it is always safe to
/// run — it just decides *what* the fleet intends; execution is separate.
/// </summary>
public abstract class CombatActivityBase : IFleetActivity
{
    /// <summary>Raise a heal request when a member's armor drops to or below this percent.</summary>
    protected virtual int HealArmorThresholdPct => 70;

    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual void Coordinate(IReadOnlyList<FleetMemberContext> members, IFleetCoordination fleet)
    {
        // 1. Anchor: prefer the scout, else the first member.
        var anchor = members.FirstOrDefault(m => m.Member.Role == FleetRole.Scout)
                     ?? members.FirstOrDefault();
        fleet.AnchorPid = anchor?.Member.Pid;

        // 2. Primary target: call it from the anchor's overview if we don't have a fresh one.
        if (anchor != null)
        {
            var candidate = UiSummary.FirstOverviewTargetName(anchor.Ui);
            if (candidate != null)
                fleet.PrimaryTarget = new PrimaryTargetCall(candidate, anchor.Member.Pid, anchor.TickTimeMs);
        }

        // 3. Heal requests: anyone below threshold needs reps; clear when recovered.
        foreach (var ctx in members)
        {
            var status = fleet.StatusOf(ctx.Member.Pid);
            var armor = status?.ArmorPct;
            if (armor is int a && a <= HealArmorThresholdPct)
                fleet.RequestHeal(new HealRequest(ctx.Member.Pid, a, status?.ShieldPct ?? -1, ctx.TickTimeMs));
            else
                fleet.ClearHeal(ctx.Member.Pid);
        }
    }

    public MemberDecision Decide(FleetMemberContext ctx) => ctx.Member.Role switch
    {
        FleetRole.Scout => DecideScout(ctx),
        FleetRole.Healer => DecideHealer(ctx),
        FleetRole.Dps or FleetRole.Anchor => DecideDps(ctx),
        _ => MemberDecision.Say("idle (unknown role)"),
    };

    protected virtual MemberDecision DecideScout(FleetMemberContext ctx)
    {
        var overview = ctx.Fleet.StatusOf(ctx.Member.Pid)?.OverviewEntries ?? 0;
        var system = ctx.Ui.InfoPanelContainer?.LocationInfo?.CurrentSolarSystemName ?? "?";
        return MemberDecision.Say($"scout: in {system}, {overview} overview entries; primary call = {Describe(ctx.Fleet.PrimaryTarget)}");
    }

    protected virtual MemberDecision DecideDps(FleetMemberContext ctx)
    {
        var primary = ctx.Fleet.PrimaryTarget;
        if (primary is null)
            return MemberDecision.Say("dps: holding — no primary called");

        // TODO(real behavior): lock `primary.TargetName` in overview, activate weapons/drones.
        var locked = ctx.Ui.Target?.Length ?? 0;
        return MemberDecision.Say($"dps: engage '{primary.TargetName}' (called by pid {primary.CallerPid}); currently {locked} locked");
    }

    protected virtual MemberDecision DecideHealer(FleetMemberContext ctx)
    {
        var needing = ctx.Fleet.ActiveHealRequests
            .OrderBy(h => h.ArmorPct)
            .ToList();

        if (needing.Count == 0)
            return MemberDecision.Say("healer: all green — standing by");

        var worst = needing[0];
        // TODO(real behavior): lock the member's ship, activate remote armor reps on it.
        return MemberDecision.Say($"healer: repair pid {worst.Pid} @ {worst.ArmorPct}% armor" +
                                  (needing.Count > 1 ? $" (+{needing.Count - 1} more waiting)" : ""));
    }

    protected static string Describe(PrimaryTargetCall? call) =>
        call is null ? "none" : $"'{call.TargetName}'";
}
