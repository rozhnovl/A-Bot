using AbyssalBot.Domain.Enums;

namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents a combat decision type
/// </summary>
public enum CombatDecisionType
{
    None,
    ActivateModule,
    DeactivateModule,
    LockTarget,
    UnlockTarget,
    OrbitTarget,
    ApproachTarget,
    KeepAtRange,
    LaunchDrones,
    ReturnDrones,
    EngageDrones
}

/// <summary>
/// Represents a combat decision made by the combat strategy
/// </summary>
public abstract record CombatDecision(
    CombatDecisionType Type,
    string Reason
);

/// <summary>
/// Decision to activate or deactivate a module
/// </summary>
public record ModuleDecision(
    ModuleType ModuleType,
    bool Activate,
    bool Overload,
    string Reason
) : CombatDecision(
    Activate ? CombatDecisionType.ActivateModule : CombatDecisionType.DeactivateModule,
    Reason
);

/// <summary>
/// Decision to lock a target
/// </summary>
public record LockTargetDecision(
    Target Target,
    string Reason
) : CombatDecision(CombatDecisionType.LockTarget, Reason);

/// <summary>
/// Decision to unlock a target
/// </summary>
public record UnlockTargetDecision(
    Target Target,
    string Reason
) : CombatDecision(CombatDecisionType.UnlockTarget, Reason);

/// <summary>
/// Decision to change maneuver
/// </summary>
public record ManeuverDecision(
    ShipManeuverType ManeuverType,
    Target? Target,
    int Distance,
    string Reason
) : CombatDecision(
    ManeuverType switch
    {
        ShipManeuverType.Orbit => CombatDecisionType.OrbitTarget,
        ShipManeuverType.Approach => CombatDecisionType.ApproachTarget,
        ShipManeuverType.KeepAtRange => CombatDecisionType.KeepAtRange,
        _ => CombatDecisionType.None
    },
    Reason
);

/// <summary>
/// Decision to launch drones
/// </summary>
public record LaunchDronesDecision(
    string Reason
) : CombatDecision(CombatDecisionType.LaunchDrones, Reason);

/// <summary>
/// Decision to return drones
/// </summary>
public record ReturnDronesDecision(
    string Reason
) : CombatDecision(CombatDecisionType.ReturnDrones, Reason);

/// <summary>
/// Decision to engage drones on target
/// </summary>
public record EngageDronesDecision(
    Target Target,
    string Reason
) : CombatDecision(CombatDecisionType.EngageDrones, Reason);
