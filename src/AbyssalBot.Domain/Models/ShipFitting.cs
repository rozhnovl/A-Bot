using AbyssalBot.Domain.Enums;

namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents a complete ship fitting configuration
/// </summary>
public record ShipFitting(
    IReadOnlyList<ShipModule> HighSlots,
    IReadOnlyList<ShipModule> MidSlots,
    IReadOnlyList<ShipModule> LowSlots,
    int MaxTargetingRange,
    int MaxTargets,
    int MaxDronesInSpace,
    int OptimalAttackRange = 11000
)
{
    /// <summary>
    /// Gets all modules of a specific type
    /// </summary>
    public IEnumerable<ShipModule> GetModulesByType(ModuleType type)
    {
        return AllModules.Where(m => m.Type == type);
    }

    /// <summary>
    /// Gets all active modules
    /// </summary>
    public IEnumerable<ShipModule> GetActiveModules()
    {
        return AllModules.Where(m => m.IsActive);
    }

    /// <summary>
    /// Gets modules that should always be active (e.g., hardeners)
    /// </summary>
    public IEnumerable<ShipModule> GetAlwaysActiveModules()
    {
        return AllModules.Where(m => m.Type == ModuleType.Hardener);
    }

    /// <summary>
    /// Gets the primary weapon module
    /// </summary>
    public ShipModule? GetWeapon()
    {
        return AllModules.FirstOrDefault(m => m.Type == ModuleType.Weapon);
    }

    /// <summary>
    /// Gets all shield booster modules
    /// </summary>
    public IEnumerable<ShipModule> GetShieldBoosters()
    {
        return AllModules.Where(m => m.Type == ModuleType.ShieldBooster);
    }

    /// <summary>
    /// Gets the MWD module
    /// </summary>
    public ShipModule? GetMWD()
    {
        return AllModules.FirstOrDefault(m => m.Type == ModuleType.MWD);
    }

    /// <summary>
    /// All modules in the fitting
    /// </summary>
    public IEnumerable<ShipModule> AllModules =>
        HighSlots.Concat(MidSlots).Concat(LowSlots);

    /// <summary>
    /// Checks if the ship has drones
    /// </summary>
    public bool HasDrones => MaxDronesInSpace > 0;
}
