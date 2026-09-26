namespace FleetOrchestrator.Fleet.Activities;

/// <summary>Safe default: nobody acts. Still samples and reports every member's status.</summary>
public sealed class IdleActivity : IFleetActivity
{
    public string Name => "Idle";
    public string Description => "Do nothing; just sample and report fleet status (safe default).";

    public void Coordinate(IReadOnlyList<FleetMemberContext> members, IFleetCoordination fleet) { }

    public MemberDecision Decide(FleetMemberContext ctx)
    {
        var s = ctx.Fleet.StatusOf(ctx.Member.Pid);
        var where = s?.SolarSystem ?? "?";
        var hp = s is null ? "?" : $"A{s.ArmorPct}/S{s.ShieldPct}/H{s.StructPct}";
        return MemberDecision.Say($"idle [{ctx.Member.Role}] in {where}, hp {hp}");
    }
}

/// <summary>Ratting combat anomalies: scout tags, DPS focus-fires the called primary, healer reps.</summary>
public sealed class AnomalyRattingActivity : CombatActivityBase
{
    public override string Name => "AnomalyRatting";
    public override string Description => "Clear combat anomalies: focus-fire called primary, healer reps low armor.";
    protected override int HealArmorThresholdPct => 65;
}

/// <summary>Fallback ratting pass over asteroid belts after combat anomalies are exhausted.</summary>
public sealed class BeltRattingActivity : CombatActivityBase
{
    public override string Name => "BeltRatting";
    public override string Description => "Visit each asteroid belt once and clear hostile NPCs.";
    protected override int HealArmorThresholdPct => 65;
}

/// <summary>Abyssal deadspace: tighter heal threshold, same focus-fire coordination.</summary>
public sealed class AbyssalActivity : CombatActivityBase
{
    public override string Name => "Abyssal";
    public override string Description => "Run abyssal deadspace as a fleet; aggressive heal threshold for spiky incoming DPS.";
    protected override int HealArmorThresholdPct => 80;
}
