namespace AbotEngine.Fleet;

/// <summary>
/// The decision core for a spider-tanked 3x Hawk trio. Pure logic over <see cref="FleetPerception"/>;
/// the only mutable state is small hysteresis memory (last rep priority / last primary) so decisions
/// don't flicker frame-to-frame. Homogeneous fleet: every Hawk both shoots and remote-reps.
///
/// Because the tank is ACTIVE (remote shield boosters), the brain reasons about incoming DPS vs. the
/// fleet's total rep output: it focuses reps on whoever the NPCs are hitting, overheats when that
/// member is low, and flags rep-broken / bail when incoming exceeds what the fleet can rep.
/// </summary>
public sealed class FleetBrain
{
    private readonly FleetThresholds t;
    private int? lastRepPriorityPid;
    private long? lastPrimaryId;
    /// <summary>Current Karybdis orbit radius; contracts each tick while it is on grid. -1 = no Karybdis.</summary>
    private int karybdisOrbitRadius = -1;

    public FleetBrain(FleetThresholds? thresholds = null) => t = thresholds ?? new FleetThresholds();

    public FleetDecision Decide(FleetPerception p)
    {
        var alive = p.Members.Where(m => m.InSpace).ToList();
        if (alive.Count == 0)
            return new FleetDecision { Orders = new Dictionary<int, HawkOrders>(), Summary = "no ships in space" };

        // --- rep priority: lowest shield, with hysteresis to avoid flicker -----------------
        var byShield = alive.OrderBy(m => m.ShieldPct).ThenByDescending(m => m.IncomingDps).ToList();
        var worst = byShield[0];

        HawkState repPriority = worst;
        var lastStill = lastRepPriorityPid is int lp ? alive.FirstOrDefault(m => m.Pid == lp) : null;
        if (lastStill is not null && lastStill.ShieldPct < t.RepReleaseShieldPct)
        {
            // Keep the current focus unless someone is meaningfully worse (hysteresis).
            repPriority = (worst.ShieldPct <= lastStill.ShieldPct - t.RepSwitchHysteresisPct) ? worst : lastStill;
        }
        lastRepPriorityPid = repPriority.Pid;

        // The focused ship still contributes its reps — to the next-lowest member.
        var secondTarget = byShield.FirstOrDefault(m => m.Pid != repPriority.Pid);

        // --- active-tank math -------------------------------------------------------------
        var reppers = alive.Where(m => !CapLocked(m)).ToList();
        var maxRep = reppers.Count * t.RepModulesPerShip * t.RepOutputPerModule;
        var incomingOnPriority = repPriority.IncomingDps;
        var repBroken = incomingOnPriority > maxRep && maxRep > 0;
        var overheat = repPriority.ShieldPct < t.OverheatShieldPct || repBroken;

        var enemiesRemain = p.Enemies.Count > 0;
        var timerBail = p.Room.TimerRemainingSec <= t.BailTimerSec && enemiesRemain;
        var tankBail = repPriority.ShieldPct < t.BailShieldPct && repBroken;
        var bail = timerBail || tankBail;

        // --- focus fire -------------------------------------------------------------------
        long? primary = SelectPrimary(p.Enemies);
        lastPrimaryId = primary;

        // --- Karybdis: tank by angular velocity, not distance -----------------------------
        // While a Karybdis is on grid, the whole fleet orbits IT with MWD on and a steadily
        // contracting radius (tighter orbit => higher angular velocity => its beam mis-tracks).
        var karybdis = p.Enemies.FirstOrDefault(e => e.IsKarybdis);
        if (karybdis is not null)
            karybdisOrbitRadius = karybdisOrbitRadius < 0
                ? t.KarybdisOrbitStartM
                : Math.Max(t.KarybdisOrbitMinM, karybdisOrbitRadius - t.KarybdisOrbitStepM);
        else
            karybdisOrbitRadius = -1;

        // Anchor: stay near whoever the NPCs are shooting (the ship being tanked).
        var anchor = alive.OrderByDescending(m => m.Attackers).ThenBy(m => m.ShieldPct).First();

        // --- per-member orders ------------------------------------------------------------
        var orders = new Dictionary<int, HawkOrders>();
        foreach (var m in alive)
        {
            int? repTarget = CapLocked(m)
                ? null
                : (m.Pid == repPriority.Pid ? secondTarget?.Pid : repPriority.Pid);

            Positioning pos =
                karybdis is not null ? new Positioning(PositioningKind.OrbitTarget, karybdis.Id, karybdisOrbitRadius)
                : m.Pid == anchor.Pid ? Positioning.Hold
                : new Positioning(PositioningKind.OrbitAnchor, anchor.Pid, t.AnchorOrbitRangeM);

            orders[m.Pid] = new HawkOrders
            {
                Pid = m.Pid,
                RepTargetPid = repTarget,
                OverheatReps = overheat && repTarget == repPriority.Pid,
                PrimaryTargetId = primary,
                Positioning = pos,
                MwdOn = karybdis is not null,   // MWD up for angular velocity vs the Karybdis
                Bail = bail,
                Intent = BuildIntent(m, repTarget, repPriority, primary, overheat, bail, karybdis, karybdisOrbitRadius),
            };
        }

        var summary =
            $"repPriority=pid {repPriority.Pid} ({repPriority.ShieldPct}% sh, {incomingOnPriority} in-dps) " +
            $"maxRep={maxRep}{(repBroken ? " REP-BROKEN" : "")}{(bail ? " BAIL" : "")} " +
            $"primary={(primary?.ToString() ?? "none")} enemies={p.Enemies.Count} timer={p.Room.TimerRemainingSec}s";

        return new FleetDecision
        {
            Orders = orders,
            PrimaryTargetId = primary,
            RepPriorityPid = repPriority.Pid,
            RepBroken = repBroken,
            Bail = bail,
            Summary = summary,
        };
    }

    private bool CapLocked(HawkState m) => m.CapPct is >= 0 and <= 15;

    private long? SelectPrimary(IReadOnlyList<NpcState> enemies)
    {
        if (enemies.Count == 0) return null;

        // Tier: EWAR that pins/breaks us > drone/missile Suppressor > everything else.
        static int Tier(NpcState e) =>
            e.IsWebbing || e.IsScrambling || e.IsNeuting ? 3 : e.IsSuppressor ? 2 : 1;

        var ranked = enemies
            .OrderByDescending(Tier)
            .ThenBy(e => e.EstimatedEhp > 0 ? e.EstimatedEhp : int.MaxValue) // kill fastest first
            .ThenBy(e => e.Distance)
            .ToList();

        var top = ranked[0];

        // Hysteresis: keep the current primary if it's still up and in the same threat tier,
        // so we don't thrash targets among equals — but switch up to a higher-tier threat.
        if (lastPrimaryId is long lastId)
        {
            var last = enemies.FirstOrDefault(e => e.Id == lastId);
            if (last is not null && Tier(last) >= Tier(top))
                return last.Id;
        }
        return top.Id;
    }

    private static string BuildIntent(
        HawkState m, int? repTarget, HawkState repPriority, long? primary, bool overheat, bool bail,
        NpcState? karybdis, int karybdisRadius)
    {
        if (bail) return "BAIL: disengage / push gate";
        var rep = repTarget is int rt
            ? (rt == repPriority.Pid ? $"rep pid {rt}{(overheat ? " (overheat)" : "")}" : $"rep pid {rt}")
            : (m.CapPct is >= 0 and <= 15 ? "no rep (cap low)" : "no rep");
        var fire = primary is long pr ? $"shoot {pr}" : "hold fire";
        var move = karybdis is not null
            ? $"orbit Karybdis @ {karybdisRadius}m, MWD on (angular tank)"
            : "hold formation";
        return $"{rep}; {fire}; {move}";
    }
}
