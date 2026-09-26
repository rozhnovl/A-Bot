namespace AbotEngine.Fleet;

// Pure decision model for a spider-tanked frigate trio (3x Hawk). Everything here is
// plain data + a pure decision function (FleetBrain) so the "brain" can be reasoned
// about and tested without a live client. The live layer fills FleetPerception from
// parsed UIs and turns HawkOrders back into motions.

/// <summary>One fleet member as the brain sees it this tick.</summary>
public sealed record HawkState
{
    public required int Pid { get; init; }
    public required bool InSpace { get; init; }
    public int ShieldPct { get; init; }
    public int ArmorPct { get; init; }
    public int StructPct { get; init; }
    /// <summary>Capacitor %, if known (active reps need cap); -1 when unknown.</summary>
    public int CapPct { get; init; } = -1;
    /// <summary>Estimated incoming DPS on this member (from NPCs targeting it).</summary>
    public int IncomingDps { get; init; }
    /// <summary>How many NPCs are currently shooting this member.</summary>
    public int Attackers { get; init; }
    /// <summary>Object ids this member currently has locked.</summary>
    public IReadOnlyList<long> LockedTargetIds { get; init; } = Array.Empty<long>();
}

/// <summary>One NPC on grid as the brain sees it.</summary>
public sealed record NpcState
{
    public required long Id { get; init; }
    public string Name { get; init; } = "";
    public int Distance { get; init; }
    /// <summary>Rough remaining EHP estimate (lower = kill faster); 0 if unknown.</summary>
    public int EstimatedEhp { get; init; }
    public int ApproxDps { get; init; }
    public bool IsWebbing { get; init; }
    public bool IsScrambling { get; init; }
    public bool IsNeuting { get; init; }
    /// <summary>Deviant Automata Suppressor — AoE that strips drones/missiles.</summary>
    public bool IsSuppressor { get; init; }
    /// <summary>Karybdis Tyrannos — huge short-range alpha; keep range.</summary>
    public bool IsKarybdis { get; init; }
}

public sealed record RoomState
{
    public int Index { get; init; }
    public int TimerRemainingSec { get; init; } = 20 * 60;
    public bool GatePresent { get; init; }
}

/// <summary>The full picture handed to the brain each tick.</summary>
public sealed record FleetPerception
{
    public required IReadOnlyList<HawkState> Members { get; init; }
    public IReadOnlyList<NpcState> Enemies { get; init; } = Array.Empty<NpcState>();
    public RoomState Room { get; init; } = new();
}

public enum PositioningKind
{
    Hold,
    /// <summary>Orbit a fleetmate (AnchorId = pid) to keep tight formation for remote reps.</summary>
    OrbitAnchor,
    /// <summary>Orbit an NPC (AnchorId = object id) at RangeMeters — used to tank by angular velocity.</summary>
    OrbitTarget,
}

public sealed record Positioning(PositioningKind Kind, long AnchorId = 0, int RangeMeters = 0)
{
    public static readonly Positioning Hold = new(PositioningKind.Hold);
}

/// <summary>What one Hawk should do this tick.</summary>
public sealed record HawkOrders
{
    public required int Pid { get; init; }
    /// <summary>Fleetmate pid to remote-shield-rep, or null to rep nobody.</summary>
    public int? RepTargetPid { get; init; }
    public bool OverheatReps { get; init; }
    /// <summary>NPC id to focus-fire, or null when nothing to shoot.</summary>
    public long? PrimaryTargetId { get; init; }
    public Positioning Positioning { get; init; } = Positioning.Hold;
    /// <summary>Whether the MWD should be running (needed for angular-velocity tanking, e.g. Karybdis).</summary>
    public bool MwdOn { get; init; }
    /// <summary>True when the fleet should disengage / push the gate (can't sustain, or timer).</summary>
    public bool Bail { get; init; }
    /// <summary>Human-readable reasoning for logs/dashboard.</summary>
    public string Intent { get; init; } = "";
}

public sealed record FleetDecision
{
    public required IReadOnlyDictionary<int, HawkOrders> Orders { get; init; }
    public long? PrimaryTargetId { get; init; }
    public int? RepPriorityPid { get; init; }
    public bool RepBroken { get; init; }
    public bool Bail { get; init; }
    public string Summary { get; init; } = "";
}

/// <summary>Tunable thresholds for the spider-tank brain.</summary>
public sealed record FleetThresholds
{
    /// <summary>Below this shield %, a member becomes the rep-priority (gets focused reps).</summary>
    public int RepFocusShieldPct { get; init; } = 65;
    /// <summary>Once the focused member climbs above this, allow switching away.</summary>
    public int RepReleaseShieldPct { get; init; } = 90;
    /// <summary>Anti-flicker: a new candidate must be at least this many % lower to steal focus.</summary>
    public int RepSwitchHysteresisPct { get; init; } = 15;
    /// <summary>Overheat remote reps when the focused member is below this shield %.</summary>
    public int OverheatShieldPct { get; init; } = 35;
    /// <summary>Consider bailing when the focused member is this low AND reps can't cover incoming.</summary>
    public int BailShieldPct { get; init; } = 20;
    /// <summary>Estimated shield/s one remote shield booster restores (for active-tank math).</summary>
    public int RepOutputPerModule { get; init; } = 60;
    /// <summary>Remote reps fitted per Hawk.</summary>
    public int RepModulesPerShip { get; init; } = 2;
    /// <summary>Bail on the room timer when this little time remains and the room isn't clear.</summary>
    public int BailTimerSec { get; init; } = 90;
    /// <summary>Tight orbit range so remote reps stay in optimal.</summary>
    public int AnchorOrbitRangeM { get; init; } = 1000;

    // Karybdis Tyrannos is tanked by ANGULAR VELOCITY, not distance: angular = transversal / range,
    // so a tighter orbit (with MWD on) raises angular velocity and out-tracks its beam. When a
    // Karybdis is on grid the fleet orbits it and gradually contracts the orbit each tick until it
    // can no longer apply, bottoming out at the minimum.
    public int KarybdisOrbitStartM { get; init; } = 7500;
    public int KarybdisOrbitMinM { get; init; } = 1000;
    public int KarybdisOrbitStepM { get; init; } = 500;
}
