using AbyssalBot.Domain.Enums;

namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents the complete combat context for decision making
/// </summary>
public record CombatContext(
    ShipFitting Fitting,
    ShipHitpointsAndEnergy Hitpoints,
    ShipManeuverType CurrentManeuver,
    TargetCollection Targets,
    DroneState DroneState,
    double IncomingDps,
    IReadOnlyList<Target> AvailableTargets
)
{
    /// <summary>
    /// Checks if ship is in dangerous condition
    /// </summary>
    public bool IsInDanger => Hitpoints.IsCritical || IncomingDps > 300;

    /// <summary>
    /// Checks if there are enemies to fight
    /// </summary>
    public bool HasEnemies => AvailableTargets.Any(t => t.IsEnemy);

    /// <summary>
    /// Gets the current active combat target
    /// </summary>
    public Target? ActiveCombatTarget => Targets.GetActiveCombatTarget();

    /// <summary>
    /// Checks if ship can engage in combat
    /// </summary>
    public bool CanEngageCombat => Fitting.GetWeapon() != null && HasEnemies;
}
