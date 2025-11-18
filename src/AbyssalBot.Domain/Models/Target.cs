namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents a combat target
/// </summary>
public record Target(
    long Id,
    string Name,
    string Type,
    int Distance,
    bool IsEnemy,
    bool IsTargeted = false,
    bool IsTargeting = false,
    bool IsSelected = false,
    bool WeaponAssigned = false,
    bool DroneAssigned = false
)
{
    /// <summary>
    /// Checks if this target is within range
    /// </summary>
    public bool IsInRange(int maxRange) => Distance <= maxRange;

    /// <summary>
    /// Checks if this target should be attacked
    /// </summary>
    public bool ShouldAttack(int attackRange) =>
        IsEnemy && IsTargeted && IsInRange(attackRange);

    /// <summary>
    /// Checks if drones should be assigned to this target
    /// </summary>
    public bool ShouldAssignDrones(int maxDroneRange = 55000) =>
        IsEnemy && IsTargeted && !DroneAssigned && IsInRange(maxDroneRange);
}

/// <summary>
/// Represents a collection of targets
/// </summary>
public record TargetCollection(
    IReadOnlyList<Target> AllTargets,
    Target? SelectedTarget
)
{
    /// <summary>
    /// Gets all targeted enemies
    /// </summary>
    public IEnumerable<Target> TargetedEnemies =>
        AllTargets.Where(t => t.IsEnemy && t.IsTargeted);

    /// <summary>
    /// Count of current targets
    /// </summary>
    public int Count => AllTargets.Count;

    /// <summary>
    /// Checks if we have a selected target
    /// </summary>
    public bool HasSelectedTarget => SelectedTarget != null;

    /// <summary>
    /// Gets the active combat target (selected and targeted)
    /// </summary>
    public Target? GetActiveCombatTarget() =>
        SelectedTarget is { IsTargeted: true, IsEnemy: true } ? SelectedTarget : null;
}
