using AbyssalBot.Domain.Enums;

namespace AbyssalBot.Domain.Models;

/// <summary>
/// Base record for situation handler responses
/// </summary>
public record SituationResponse(
    SituationPriority Priority,
    string Situation,
    string Reasoning,
    IReadOnlyList<CombatDecision> Decisions
)
{
    /// <summary>
    /// Indicates if this situation requires immediate action
    /// </summary>
    public bool IsEmergency => Priority >= SituationPriority.CapacitorEmergency;

    /// <summary>
    /// Indicates if any decisions were made
    /// </summary>
    public bool HasDecisions => Decisions.Count > 0;

    /// <summary>
    /// Creates a no-action response
    /// </summary>
    public static SituationResponse NoAction(string situation) =>
        new(SituationPriority.Normal, situation, "No action needed", Array.Empty<CombatDecision>());
}

/// <summary>
/// Context for EWAR situations
/// </summary>
public record EwarContext(
    EwarType EwarType,
    Target? EwarSource,
    ShipHitpointsAndEnergy Hitpoints,
    IReadOnlyList<Target> AvailableTargets
);

/// <summary>
/// Represents a ship's current status
/// </summary>
public record ShipStatus(
    ShipHitpointsAndEnergy Hitpoints,
    ShipFitting Fitting,
    double IncomingDps,
    bool IsOverheating = false
);

/// <summary>
/// Represents available inventory space
/// </summary>
public record InventorySpace(
    double CurrentVolume,
    double MaxVolume,
    double CurrentValue
)
{
    /// <summary>
    /// Available volume in m³
    /// </summary>
    public double AvailableVolume => MaxVolume - CurrentVolume;

    /// <summary>
    /// Percentage full (0-100)
    /// </summary>
    public double PercentageFull => MaxVolume > 0 ? (CurrentVolume / MaxVolume) * 100 : 0;

    /// <summary>
    /// Is cargo nearly full (> 80%)
    /// </summary>
    public bool IsNearlyFull => PercentageFull > 80;
}

/// <summary>
/// Represents a loot target
/// </summary>
public record LootTarget(
    long Id,
    string Name,
    string Type,
    double Volume,
    double EstimatedValue,
    int Distance
)
{
    /// <summary>
    /// Value per m³
    /// </summary>
    public double ValueDensity => Volume > 0 ? EstimatedValue / Volume : 0;
}

/// <summary>
/// Represents an Abyss room
/// </summary>
public record AbyssRoom(
    string RoomType,
    IReadOnlyList<Target> Enemies,
    Target? Cache,
    bool HasConduit
)
{
    /// <summary>
    /// Number of enemies in room
    /// </summary>
    public int EnemyCount => Enemies.Count;

    /// <summary>
    /// Is this a cache room
    /// </summary>
    public bool IsCacheRoom => Cache != null;
}

/// <summary>
/// Room combat strategy
/// </summary>
public record RoomStrategy(
    string StrategyName,
    IReadOnlyList<Target> PriorityTargets,
    string Reasoning,
    bool ShouldLootCache = false
);

/// <summary>
/// Ammo type selection
/// </summary>
public record AmmoType(
    string Name,
    string DamageType,
    int OptimalRange,
    double DamageMultiplier = 1.0
);

/// <summary>
/// Weapon type
/// </summary>
public record WeaponType(
    string Name,
    int BaseOptimalRange,
    int BaseFalloffRange,
    string PreferredDamageType
);
