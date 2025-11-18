using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for optimizing ship tank (defensive systems) management
/// </summary>
/// <remarks>
/// Implements EVE Online tank optimization:
/// - Active tank cycling (don't activate at 100% HP)
/// - Buffer tank monitoring
/// - Damage type resistance optimization
/// - Overheating thresholds for critical situations
/// - Passive/active tank coordination
/// </remarks>
public class TankOptimizationService
{
    /// <summary>
    /// Determines if shield booster should be activated
    /// </summary>
    /// <param name="currentShield">Current shield HP</param>
    /// <param name="maxShield">Maximum shield HP</param>
    /// <param name="boosterAmount">HP repaired per cycle</param>
    /// <param name="cycleTime">Booster cycle time (seconds)</param>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <returns>True if booster should be activated</returns>
    /// <remarks>
    /// Don't activate if:
    /// - Shield is too high (would waste the cycle)
    /// - Incoming DPS is zero
    /// Do activate if:
    /// - Shield is below threshold
    /// - Under significant incoming damage
    /// </remarks>
    public bool ShouldActivateShieldBooster(
        double currentShield,
        double maxShield,
        double boosterAmount,
        double cycleTime,
        double incomingDps)
    {
        if (maxShield <= 0)
            return false;

        var shieldPercentage = (currentShield / maxShield) * 100;

        // Never activate at full shield
        if (shieldPercentage >= 98)
            return false;

        // Calculate damage we'll take during one cycle
        var damagePerCycle = incomingDps * cycleTime;

        // Calculate shield after one cycle (current - damage + repair)
        var shieldAfterCycle = currentShield - damagePerCycle + boosterAmount;

        // Don't overheal - if we'd be above 95% after cycle, wait
        if (shieldAfterCycle >= maxShield * 0.95)
            return false;

        // Activate based on shield percentage and incoming damage
        if (incomingDps > 200) // High DPS
            return shieldPercentage < 85;
        else if (incomingDps > 100) // Moderate DPS
            return shieldPercentage < 75;
        else if (incomingDps > 50) // Low DPS
            return shieldPercentage < 60;
        else // Minimal DPS
            return shieldPercentage < 40;
    }

    /// <summary>
    /// Determines which boosters to activate when multiple are available
    /// </summary>
    /// <param name="currentShield">Current shield HP</param>
    /// <param name="maxShield">Maximum shield HP</param>
    /// <param name="boosters">Available boosters</param>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <param name="capacitorPercentage">Current cap percentage</param>
    /// <returns>List of boosters that should be active</returns>
    /// <remarks>
    /// Use all boosters when:
    /// - Shield critical (< 30%)
    /// - Taking very high DPS
    /// Use one booster when:
    /// - Shield moderate and DPS manageable
    /// - Cap is low
    /// </remarks>
    public IReadOnlyList<BoosterModule> GetBoostersToActivate(
        double currentShield,
        double maxShield,
        IEnumerable<BoosterModule> boosters,
        double incomingDps,
        double capacitorPercentage)
    {
        var boosterList = boosters.ToList();
        if (!boosterList.Any())
            return Array.Empty<BoosterModule>();

        var shieldPercentage = (currentShield / maxShield) * 100;

        // Critical shield - activate all boosters
        if (shieldPercentage < 30)
            return boosterList;

        // Very high DPS - activate all boosters
        if (incomingDps > 300)
            return boosterList;

        // Low cap - use only most efficient booster
        if (capacitorPercentage < 30)
        {
            return boosterList
                .OrderBy(b => b.CapacitorUsage / b.RepairAmount) // Best HP per cap
                .Take(1)
                .ToList();
        }

        // Moderate situation - use boosters as needed
        if (shieldPercentage < 60 && incomingDps > 150)
        {
            // Use enough boosters to stay ahead of damage
            var totalRepairNeeded = incomingDps * 5; // 5 second buffer
            var runningTotal = 0.0;
            var toActivate = new List<BoosterModule>();

            foreach (var booster in boosterList.OrderByDescending(b => b.RepairAmount))
            {
                toActivate.Add(booster);
                runningTotal += booster.RepairAmount / booster.CycleTime;

                if (runningTotal >= incomingDps)
                    break;
            }

            return toActivate;
        }

        // Low threat - use single most efficient booster
        if (shieldPercentage < 70 && incomingDps > 50)
        {
            return boosterList
                .OrderByDescending(b => b.RepairAmount)
                .Take(1)
                .ToList();
        }

        return Array.Empty<BoosterModule>();
    }

    /// <summary>
    /// Determines if tank modules should be overheated
    /// </summary>
    /// <param name="currentHpPercentage">Current HP percentage (shield/armor)</param>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <param name="repairPerSecond">Current repair per second</param>
    /// <param name="capacitorPercentage">Current cap percentage</param>
    /// <returns>Overheating recommendation</returns>
    /// <remarks>
    /// Overheat when:
    /// - HP critical (< 30%)
    /// - Taking more damage than we can repair normally
    /// - Cap is sufficient to support overheated modules
    /// </remarks>
    public OverheatTankRecommendation ShouldOverheatTank(
        double currentHpPercentage,
        double incomingDps,
        double repairPerSecond,
        double capacitorPercentage)
    {
        // Critical HP - overheat everything
        if (currentHpPercentage < 25)
        {
            return new OverheatTankRecommendation(
                true,
                true, // Overheat boosters
                true, // Overheat hardeners
                "Critical HP - overheat all tank modules");
        }

        // Low HP and incoming DPS exceeds repair rate
        if (currentHpPercentage < 40 && incomingDps > repairPerSecond * 1.2)
        {
            // Only overheat if we have cap for it
            if (capacitorPercentage > 40)
            {
                return new OverheatTankRecommendation(
                    true,
                    true,
                    false, // Don't overheat hardeners yet
                    "Low HP and high DPS - overheat boosters");
            }
        }

        // Moderate HP but very high DPS
        if (currentHpPercentage < 60 && incomingDps > repairPerSecond * 1.5)
        {
            if (capacitorPercentage > 50)
            {
                return new OverheatTankRecommendation(
                    true,
                    true,
                    true,
                    "Very high incoming DPS - overheat tank");
            }
        }

        return new OverheatTankRecommendation(
            false,
            false,
            false,
            "Tank situation manageable");
    }

    /// <summary>
    /// Calculates effective resistance against incoming damage
    /// </summary>
    /// <param name="resistances">Resistance profile (EM, Thermal, Kinetic, Explosive)</param>
    /// <param name="damageProfile">Incoming damage profile by type</param>
    /// <returns>Effective resistance percentage (0-100)</returns>
    /// <remarks>
    /// Weighted resistance based on actual incoming damage distribution
    /// </remarks>
    public double CalculateEffectiveResistance(
        ResistanceProfile resistances,
        DamageProfile damageProfile)
    {
        var totalIncomingDamage = damageProfile.TotalDps;
        if (totalIncomingDamage <= 0)
            return 0;

        // Calculate weighted resistance
        var effectiveResistance =
            (resistances.EM * damageProfile.EmDps +
             resistances.Thermal * damageProfile.ThermalDps +
             resistances.Kinetic * damageProfile.KineticDps +
             resistances.Explosive * damageProfile.ExplosiveDps) / totalIncomingDamage;

        return effectiveResistance * 100;
    }

    /// <summary>
    /// Identifies resistance holes (weakest resist vs incoming damage)
    /// </summary>
    /// <param name="resistances">Current resistances</param>
    /// <param name="damageProfile">Incoming damage distribution</param>
    /// <returns>Damage type we're weakest against (taking most damage from)</returns>
    public DamageType IdentifyResistanceHole(
        ResistanceProfile resistances,
        DamageProfile damageProfile)
    {
        var damages = new Dictionary<DamageType, double>
        {
            { DamageType.EM, damageProfile.EmDps * (1 - resistances.EM) },
            { DamageType.Thermal, damageProfile.ThermalDps * (1 - resistances.Thermal) },
            { DamageType.Kinetic, damageProfile.KineticDps * (1 - resistances.Kinetic) },
            { DamageType.Explosive, damageProfile.ExplosiveDps * (1 - resistances.Explosive) }
        };

        return damages.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// Calculates time to live (TTL) under current damage
    /// </summary>
    /// <param name="currentHp">Current HP</param>
    /// <param name="incomingDps">Incoming DPS after resistances</param>
    /// <param name="repairPerSecond">Repair per second</param>
    /// <returns>Seconds until death, or -1 if tank is holding</returns>
    public double CalculateTimeToLive(
        double currentHp,
        double incomingDps,
        double repairPerSecond)
    {
        var netDps = incomingDps - repairPerSecond;

        if (netDps <= 0)
            return -1; // Tank is holding

        return currentHp / netDps;
    }

    /// <summary>
    /// Determines if we should switch from active to buffer tank strategy
    /// </summary>
    /// <param name="capacitorPercentage">Current cap percentage</param>
    /// <param name="capIsStable">Whether cap is stable</param>
    /// <param name="currentHpPercentage">Current HP percentage</param>
    /// <returns>True if should deactivate active tank and rely on buffer</returns>
    /// <remarks>
    /// Sometimes better to turn off boosters and save cap for weapons/escape
    /// If cap is dying and we can't sustain active tank anyway
    /// </remarks>
    public bool ShouldSwitchToBufferTank(
        double capacitorPercentage,
        bool capIsStable,
        double currentHpPercentage)
    {
        // If cap unstable and low, and we have decent HP buffer, turn off active tank
        if (!capIsStable && capacitorPercentage < 20 && currentHpPercentage > 50)
            return true;

        // If cap critical and HP is not critical, save cap for escape/weapons
        if (capacitorPercentage < 10 && currentHpPercentage > 40)
            return true;

        return false;
    }

    /// <summary>
    /// Recommends which hardeners should be active based on incoming damage
    /// </summary>
    /// <param name="availableHardeners">All available hardeners</param>
    /// <param name="damageProfile">Incoming damage profile</param>
    /// <param name="capacitorPercentage">Current cap percentage</param>
    /// <param name="maxActiveHardeners">Maximum number of hardeners to run (cap limited)</param>
    /// <returns>Hardeners that should be active</returns>
    public IReadOnlyList<HardenerModule> GetHardenersToActivate(
        IEnumerable<HardenerModule> availableHardeners,
        DamageProfile damageProfile,
        double capacitorPercentage,
        int maxActiveHardeners = 999)
    {
        var hardeners = availableHardeners.ToList();

        // Always run all hardeners unless cap critical
        if (capacitorPercentage > 25)
            return hardeners;

        // Cap critical - prioritize hardeners by incoming damage type
        return hardeners
            .OrderByDescending(h => GetDamageTypeWeight(h.DamageType, damageProfile))
            .Take(maxActiveHardeners)
            .ToList();
    }

    private double GetDamageTypeWeight(DamageType type, DamageProfile profile)
    {
        return type switch
        {
            DamageType.EM => profile.EmDps,
            DamageType.Thermal => profile.ThermalDps,
            DamageType.Kinetic => profile.KineticDps,
            DamageType.Explosive => profile.ExplosiveDps,
            _ => 0
        };
    }
}

/// <summary>
/// Shield or armor booster module
/// </summary>
/// <param name="Name">Module name</param>
/// <param name="RepairAmount">HP repaired per cycle</param>
/// <param name="CycleTime">Cycle time in seconds</param>
/// <param name="CapacitorUsage">Cap used per cycle (GJ)</param>
public record BoosterModule(
    string Name,
    double RepairAmount,
    double CycleTime,
    double CapacitorUsage
);

/// <summary>
/// Resistance module (hardener, coating, etc.)
/// </summary>
/// <param name="Name">Module name</param>
/// <param name="DamageType">Damage type it resists</param>
/// <param name="ResistanceBonus">Resistance bonus (0-1)</param>
/// <param name="CapacitorUsage">Cap per second (0 for passive)</param>
public record HardenerModule(
    string Name,
    DamageType DamageType,
    double ResistanceBonus,
    double CapacitorUsage
);

/// <summary>
/// Resistance profile across all damage types
/// </summary>
/// <param name="EM">EM resistance (0-1, where 0.7 = 70% resistance)</param>
/// <param name="Thermal">Thermal resistance (0-1)</param>
/// <param name="Kinetic">Kinetic resistance (0-1)</param>
/// <param name="Explosive">Explosive resistance (0-1)</param>
public record ResistanceProfile(
    double EM,
    double Thermal,
    double Kinetic,
    double Explosive
);

/// <summary>
/// Incoming damage distribution
/// </summary>
/// <param name="EmDps">EM DPS</param>
/// <param name="ThermalDps">Thermal DPS</param>
/// <param name="KineticDps">Kinetic DPS</param>
/// <param name="ExplosiveDps">Explosive DPS</param>
public record DamageProfile(
    double EmDps,
    double ThermalDps,
    double KineticDps,
    double ExplosiveDps
)
{
    /// <summary>
    /// Total DPS across all damage types
    /// </summary>
    public double TotalDps => EmDps + ThermalDps + KineticDps + ExplosiveDps;
}

/// <summary>
/// Tank overheating recommendation
/// </summary>
/// <param name="ShouldOverheat">Whether any overheating is recommended</param>
/// <param name="OverheatBoosters">Whether to overheat repair modules</param>
/// <param name="OverheatHardeners">Whether to overheat hardeners</param>
/// <param name="Reason">Explanation</param>
public record OverheatTankRecommendation(
    bool ShouldOverheat,
    bool OverheatBoosters,
    bool OverheatHardeners,
    string Reason
);

/// <summary>
/// Damage types in EVE Online
/// </summary>
public enum DamageType
{
    EM,
    Thermal,
    Kinetic,
    Explosive,
    Omni // All types
}
