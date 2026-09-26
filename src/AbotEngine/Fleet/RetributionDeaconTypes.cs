namespace AbotEngine.Fleet;

/// <summary>The three fixed jobs in the 2x Retribution + Deacon doctrine.</summary>
public enum RetributionDeaconRole
{
    TankRetribution,
    WingRetribution,
    Deacon,
}

/// <summary>
/// Rollout is deliberately staged. Anomaly training exercises anchoring, aggro and repairs without
/// enabling any Abyss-only room manoeuvre; T3 Electrical enables the Gustav room doctrine.
/// </summary>
public enum RetributionDeaconEnvironment
{
    AnomalyTraining,
    T3Electrical,
}

public enum RetributionDeaconRoomKind
{
    Generic,
    Kikimora,
    Vhetaguth,
    Leshak,
    Deepwatcher,
    Karybdis,
    Overmind,
    AngelsOrSleepers,
    RogueDroneFrigates,
    RogueDroneBattlecruiser,
    Vedmak,
}

public enum LaserCrystal
{
    KeepCurrent,
    Aurora,
    Xray,
    Standard,
    Gamma,
    Multifrequency,
    Gleam,
}

public enum PropulsionOrder
{
    KeepCurrent,
    Off,
    On,
}

/// <summary>How the two Retributions distribute weapon targets on this tick.</summary>
public enum RetributionDeaconFireMode
{
    Hold,
    FocusFire,
    /// <summary>Targets that each Retribution can kill within the solo-volley budget are divided.</summary>
    Shuffle,
}

/// <summary>
/// Damage profile for the calibrated Gustav T3 beginner beam fit. The multiplier is the damage
/// modifier shown by one fitted Small Focused Beam Laser II; four grouped guns fire each volley.
/// It may be overridden per character in fleet.config.json when skills or fittings differ.
/// </summary>
public sealed record RetributionLaserWeaponProfile
{
    public string Name { get; init; } = "T3 Beginner Multibox / beam";
    public int TurretCount { get; init; } = 4;
    public double PerTurretDamageMultiplier { get; init; } = 11.7142;
    /// <summary>Optional conservative correction for application; 1 means nominal turret damage.</summary>
    public double ApplicationFactor { get; init; } = 1;
    public LaserCrystal CurrentCrystal { get; init; } = LaserCrystal.Multifrequency;
    public bool CurrentCrystalKnown { get; init; } = true;
    /// <summary>Measured in-space ranges and charge damage for this exact fitted beam group.</summary>
    public IReadOnlyList<RetributionCrystalBallistics> CrystalBallistics { get; init; } =
        RetributionCrystalCatalog.Carried;
}

public enum RetributionDeaconPositioningKind
{
    Hold,
    /// <summary>Initial room commit. Only the tank Retribution receives this order.</summary>
    CommitTank,
    OrbitFleetmate,
    OrbitEnemy,
    KeepRangeFromEnemy,
    ApproachEnemy,
    ApproachCache,
    /// <summary>Leave a blue cloud and/or a tracking pylon before stopping.</summary>
    ClearHazards,
    /// <summary>With no Sparkneedles available, keep pressure on the Overmind toward the arena edge.</summary>
    PushEnemyToBoundary,
}

public sealed record RetributionDeaconPositioning(
    RetributionDeaconPositioningKind Kind,
    long TargetId = 0,
    int RangeMeters = 0)
{
    public static readonly RetributionDeaconPositioning Hold =
        new(RetributionDeaconPositioningKind.Hold);
}

/// <summary>One fleet member as observed by the armor-logistics doctrine.</summary>
public sealed record RetributionDeaconMemberState
{
    public required int Pid { get; init; }
    public required RetributionDeaconRole Role { get; init; }
    public required bool InSpace { get; init; }
    public int ShieldPct { get; init; }
    public int ArmorPct { get; init; }
    public int StructPct { get; init; }
    public int CapPct { get; init; } = -1;
    public int IncomingDps { get; init; }
    public int Attackers { get; init; }
    public RetributionLaserWeaponProfile Weapon { get; init; } = new();
}

/// <summary>
/// An overview entity. Flags are intentionally explicit: live perception can fill them from SDE data,
/// while offline scenarios can state only the facts relevant to the room under test.
/// </summary>
public sealed record RetributionDeaconEnemyState
{
    public required long Id { get; init; }
    public string Name { get; init; } = "";
    public int DistanceMeters { get; init; }
    public int EstimatedEhp { get; init; }
    public global::AbotEngine.NpcDefenseProfile Defense { get; init; } = new();
    public int ApproxDps { get; init; }
    public bool IsFrigate { get; init; }
    public bool IsCruiser { get; init; }
    public bool IsBattlecruiser { get; init; }
    public bool IsBattleship { get; init; }
    public bool IsElite { get; init; }
    public bool IsNeuting { get; init; }
    public bool IsWebbing { get; init; }
    public bool IsPainting { get; init; }
    public bool IsScrambling { get; init; }
    public bool IsDamping { get; init; }
    public bool IsTrackingDisrupting { get; init; }
    public bool IsRepairing { get; init; }
}

/// <summary>Room facts that cannot be inferred from the hostile list alone.</summary>
public sealed record RetributionDeaconRoomState
{
    public int Index { get; init; }
    public int TimerRemainingSec { get; init; } = 20 * 60;
    public long? CacheId { get; init; }
    public bool NearBlueCloud { get; init; }
    public bool NearTrackingPylon { get; init; }
    public bool SparkneedlesPresent { get; init; }
    public bool LootPending { get; init; }
    public bool TagsPresent { get; init; }
    public bool FormationReady { get; init; }
    public bool ReactiveHardenersReset { get; init; }
}

public sealed record RetributionDeaconPerception
{
    public RetributionDeaconEnvironment Environment { get; init; } =
        RetributionDeaconEnvironment.AnomalyTraining;
    public required IReadOnlyList<RetributionDeaconMemberState> Members { get; init; }
    public IReadOnlyList<RetributionDeaconEnemyState> Enemies { get; init; } =
        Array.Empty<RetributionDeaconEnemyState>();
    public RetributionDeaconRoomState Room { get; init; } = new();
}

/// <summary>One repair module assignment. Repeating a pid means several reps go to that ship.</summary>
public sealed record RepairAssignment(int ModuleIndex, int TargetPid, bool Overheat = false);

public sealed record RetributionDeaconOrders
{
    public required int Pid { get; init; }
    public IReadOnlyList<RepairAssignment> Repairs { get; init; } = Array.Empty<RepairAssignment>();
    public long? PrimaryTargetId { get; init; }
    public RetributionDeaconFireMode FireMode { get; init; } = RetributionDeaconFireMode.Hold;
    public RetributionDeaconPositioning Positioning { get; init; } = RetributionDeaconPositioning.Hold;
    public PropulsionOrder Propulsion { get; init; } = PropulsionOrder.KeepCurrent;
    public LaserCrystal Crystal { get; init; } = LaserCrystal.KeepCurrent;
    /// <summary>Range-only chance for the selected crystal at the target's current distance.</summary>
    public double? RangeHitChance { get; init; }
    /// <summary>Do not waste capacitor/cycles beyond optimal + one falloff.</summary>
    public bool HoldFireForRange { get; init; }
    /// <summary>Fractional nominal volleys and whole shots for this ship, target and crystal.</summary>
    public double? SoloVolleysToKill { get; init; }
    public int? SoloShotsToKill { get; init; }
    public bool OverheatThermalHardener { get; init; }
    public bool ResetReactiveHardener { get; init; }
    public bool ClearSequenceTags { get; init; }
    public bool HoldGate { get; init; }
    public string Intent { get; init; } = "";
}

public sealed record RetributionDeaconDecision
{
    public required IReadOnlyDictionary<int, RetributionDeaconOrders> Orders { get; init; }
    public RetributionDeaconRoomKind RoomKind { get; init; }
    public long? PrimaryTargetId { get; init; }
    public RetributionDeaconFireMode FireMode { get; init; } = RetributionDeaconFireMode.Hold;
    public int? AggroTargetPid { get; init; }
    public bool OpeningCommit { get; init; }
    public bool GateReady { get; init; }
    public string Summary { get; init; } = "";
}

public sealed record RetributionDeaconThresholds
{
    public int DeaconRepOutputPerModule { get; init; } = 80;
    public int DeaconRepModules { get; init; } = 3;
    public int WingRepModules { get; init; } = 1;
    public int FocusArmorPct { get; init; } = 75;
    public int OverheatArmorPct { get; init; } = 35;
    public int OverheatIncomingDps { get; init; } = 300;
    public int LowCapPct { get; init; } = 18;
    public int GateCapPct { get; init; } = 45;
    /// <summary>T3 training rule: three or more Kikimoras count as a pack and get all available reps.</summary>
    public int KikimoraAllInCount { get; init; } = 3;
    public int AnchorOrbitMeters { get; init; } = 1000;
    public int TightOrbitMeters { get; init; } = 500;
    public int RogueBattlecruiserRangeMeters { get; init; } = 25000;
    /// <summary>Split fire only when each assigned target takes at most this many solo volleys.</summary>
    public double ShuffleMaxSoloVolleys { get; init; } = 5;
    public int ShuffleMaxTargetDistanceMeters { get; init; } = 30000;
}
