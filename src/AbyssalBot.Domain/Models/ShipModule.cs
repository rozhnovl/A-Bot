using AbyssalBot.Domain.Enums;

namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents a ship module with its characteristics and state
/// </summary>
public record ShipModule(
    ModuleType Type,
    int OptimalRange = 4000,
    bool IsActive = false,
    bool IsBusy = false,
    bool IsOverloaded = false
)
{
    /// <summary>
    /// Determines if this module should be active
    /// </summary>
    public bool ShouldBeActive(ModuleActivationContext context) => Type switch
    {
        ModuleType.Hardener => true, // Always active
        ModuleType.Weapon => context.HasActiveTarget && context.DistanceToTarget <= OptimalRange,
        ModuleType.ShieldBooster => context.ShieldHitpoints < context.ShieldBoosterActivationThreshold,
        ModuleType.MWD => context.DistanceToTarget > context.MwdActivationDistance,
        _ => false
    };
}

/// <summary>
/// Context for determining module activation
/// </summary>
public record ModuleActivationContext(
    bool HasActiveTarget,
    int DistanceToTarget,
    double ShieldHitpoints,
    double CapacitorEnergy,
    double IncomingDps,
    int ShieldBoosterActivationThreshold = 600,
    int MwdActivationDistance = 2000
);
