using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for managing weapon systems, cycling, reloading, and ammunition selection
/// </summary>
/// <remarks>
/// Implements EVE Online weapon mechanics:
/// - Don't switch targets during weapon cycle
/// - Optimal reload timing (when safe)
/// - Ammunition selection based on target size and range
/// - Overheating weapons in critical situations
/// - Volley vs sustained DPS optimization
/// </remarks>
public class WeaponManagementService
{
    /// <summary>
    /// Determines if weapon target should be changed
    /// </summary>
    /// <param name="currentTargetId">Current target ID</param>
    /// <param name="newTargetId">Proposed new target ID</param>
    /// <param name="weaponCycleProgress">Current weapon cycle progress (0-1)</param>
    /// <param name="currentTargetHealth">Current target health percentage</param>
    /// <returns>True if target change is recommended</returns>
    /// <remarks>
    /// Don't switch targets mid-cycle - wastes DPS
    /// Exception: Current target is about to die
    /// </remarks>
    public bool ShouldChangeWeaponTarget(
        long currentTargetId,
        long newTargetId,
        double weaponCycleProgress,
        double currentTargetHealth)
    {
        // Same target - no change
        if (currentTargetId == newTargetId)
            return false;

        // Current target nearly dead - switch immediately to avoid overkill
        if (currentTargetHealth < 5)
            return true;

        // Mid-cycle - don't switch (wait for cycle to complete)
        if (weaponCycleProgress > 0.1 && weaponCycleProgress < 0.95)
            return false;

        // Cycle complete or just started - safe to switch
        return true;
    }

    /// <summary>
    /// Determines if weapons should be reloaded
    /// </summary>
    /// <param name="currentCharges">Current ammunition in weapon</param>
    /// <param name="maxCharges">Maximum ammunition capacity</param>
    /// <param name="inCombat">Whether currently in combat</param>
    /// <param name="enemiesRemaining">Number of enemies still alive</param>
    /// <param name="reloadTime">Reload time in seconds</param>
    /// <returns>Reload recommendation</returns>
    /// <remarks>
    /// Reload timing is critical:
    /// - Never reload during active combat (lose DPS)
    /// - Reload between spawns/waves
    /// - Reload if current ammo is wrong type for situation
    /// </remarks>
    public ReloadRecommendation ShouldReload(
        int currentCharges,
        int maxCharges,
        bool inCombat,
        int enemiesRemaining,
        double reloadTime)
    {
        // Empty - must reload
        if (currentCharges == 0)
        {
            return new ReloadRecommendation(
                true,
                null,
                "Weapon empty - reload required");
        }

        // Full - no reload needed
        if (currentCharges == maxCharges)
        {
            return new ReloadRecommendation(
                false,
                null,
                "Weapon fully loaded");
        }

        // In combat with enemies - don't reload
        if (inCombat && enemiesRemaining > 0)
        {
            // Exception: Very low ammo and long fight ahead
            if (currentCharges < maxCharges * 0.2 && enemiesRemaining > 3)
            {
                return new ReloadRecommendation(
                    true,
                    null,
                    "Low ammo with many enemies - reload now before running out");
            }

            return new ReloadRecommendation(
                false,
                null,
                "In combat - maintain DPS");
        }

        // No enemies - safe to reload
        if (enemiesRemaining == 0 && currentCharges < maxCharges * 0.8)
        {
            return new ReloadRecommendation(
                true,
                null,
                "Combat clear - reload preparation");
        }

        return new ReloadRecommendation(
            false,
            null,
            "No reload needed");
    }

    /// <summary>
    /// Selects optimal ammunition type based on target and situation
    /// </summary>
    /// <param name="weaponType">Type of weapon system</param>
    /// <param name="targetSignature">Target signature radius</param>
    /// <param name="targetDistance">Distance to target</param>
    /// <param name="weaponOptimalRange">Weapon optimal range</param>
    /// <param name="targetVelocity">Target velocity</param>
    /// <returns>Recommended ammunition type</returns>
    /// <remarks>
    /// Ammo selection trade-offs:
    /// - Short range ammo: High damage, short range, good tracking
    /// - Long range ammo: Lower damage, long range, poor tracking
    /// - Precision ammo: Lower damage, excellent vs small targets
    /// - Fury ammo: High damage, poor vs small targets
    /// </remarks>
    public AmmoRecommendation SelectOptimalAmmo(
        WeaponSystem weaponType,
        double targetSignature,
        double targetDistance,
        double weaponOptimalRange,
        double targetVelocity)
    {
        return weaponType switch
        {
            WeaponSystem.Missiles => SelectMissileAmmo(targetSignature, targetDistance, targetVelocity),
            WeaponSystem.Projectile => SelectProjectileAmmo(targetDistance, weaponOptimalRange, targetSignature),
            WeaponSystem.Hybrid => SelectHybridAmmo(targetDistance, weaponOptimalRange, targetSignature),
            WeaponSystem.Energy => SelectEnergyAmmo(targetDistance, weaponOptimalRange, targetSignature),
            _ => new AmmoRecommendation("Standard", "Default ammunition")
        };
    }

    private AmmoRecommendation SelectMissileAmmo(double targetSignature, double targetDistance, double targetVelocity)
    {
        const double SmallSigThreshold = 50;  // Frigate-sized
        const double FastVelocityThreshold = 500;

        // Small, fast target - use precision
        if (targetSignature < SmallSigThreshold || targetVelocity > FastVelocityThreshold)
        {
            return new AmmoRecommendation(
                "Precision",
                "Small/fast target - precision missiles for better application");
        }

        // Large target - use fury for maximum damage
        if (targetSignature > 100)
        {
            return new AmmoRecommendation(
                "Fury",
                "Large target - fury for maximum damage");
        }

        // Medium target - standard ammo
        return new AmmoRecommendation(
            "Standard",
            "Medium target - balanced ammunition");
    }

    private AmmoRecommendation SelectProjectileAmmo(double targetDistance, double optimalRange, double targetSignature)
    {
        const double SmallSigThreshold = 50;

        // Beyond optimal - use long range
        if (targetDistance > optimalRange * 1.2)
        {
            return new AmmoRecommendation(
                "Long Range",
                "Target beyond optimal - long range ammo (EMP/Fusion)");
        }

        // Small target at close range - use short range high tracking
        if (targetSignature < SmallSigThreshold && targetDistance < optimalRange * 0.8)
        {
            return new AmmoRecommendation(
                "Short Range",
                "Small target close range - high tracking ammo (Hail/Fusion)");
        }

        // Medium range - use optimal ammo
        return new AmmoRecommendation(
            "Medium Range",
            "Within optimal - balanced ammo (Phased Plasma)");
    }

    private AmmoRecommendation SelectHybridAmmo(double targetDistance, double optimalRange, double targetSignature)
    {
        const double SmallSigThreshold = 50;

        // Long range - use javelin/spike
        if (targetDistance > optimalRange * 1.3)
        {
            return new AmmoRecommendation(
                "Long Range",
                "Extended range - spike/javelin ammunition");
        }

        // Close range, small target
        if (targetSignature < SmallSigThreshold && targetDistance < optimalRange)
        {
            return new AmmoRecommendation(
                "Short Range",
                "Close small target - antimatter/void for tracking");
        }

        return new AmmoRecommendation(
            "Medium Range",
            "Standard range - null/iridium ammunition");
    }

    private AmmoRecommendation SelectEnergyAmmo(double targetDistance, double optimalRange, double targetSignature)
    {
        // Energy weapons typically have better range/damage built-in
        // Focus on damage type selection based on target resists (not implemented here)

        if (targetDistance > optimalRange * 1.2)
        {
            return new AmmoRecommendation(
                "Long Range",
                "Extended range - multifrequency/scorch crystals");
        }

        return new AmmoRecommendation(
            "Short Range",
            "Optimal range - conflag/radio crystals for max DPS");
    }

    /// <summary>
    /// Determines if weapons should be overheated
    /// </summary>
    /// <param name="targetTimeToKill">Estimated time to kill current target (seconds)</param>
    /// <param name="ourShieldPercentage">Our shield percentage</param>
    /// <param name="enemiesRemaining">Number of enemies remaining</param>
    /// <param name="moduleHeatLevel">Current module heat level (0-1)</param>
    /// <returns>Overheating recommendation</returns>
    /// <remarks>
    /// Overheat weapons when:
    /// - Need to kill target quickly (we're taking heavy damage)
    /// - Enemy is high priority (logi, EWAR)
    /// - Module heat is not critical
    /// Don't overheat when:
    /// - Many enemies remaining (will burn out module)
    /// - Module already very hot
    /// </remarks>
    public OverheatWeaponRecommendation ShouldOverheatWeapons(
        double targetTimeToKill,
        double ourShieldPercentage,
        int enemiesRemaining,
        double moduleHeatLevel)
    {
        const double HeatDangerThreshold = 0.9; // 90% heat
        const double CriticalShieldThreshold = 30;

        // Module too hot - don't overheat
        if (moduleHeatLevel > HeatDangerThreshold)
        {
            return new OverheatWeaponRecommendation(
                false,
                "Module heat critical - risk of burnout");
        }

        // Critical shield - overheat to kill faster
        if (ourShieldPercentage < CriticalShieldThreshold)
        {
            return new OverheatWeaponRecommendation(
                true,
                "Critical shield - overheat to reduce fight duration");
        }

        // Quick kill possible with overheat
        if (targetTimeToKill < 10 && targetTimeToKill > 3)
        {
            return new OverheatWeaponRecommendation(
                true,
                "Can secure quick kill with overheat");
        }

        // Many enemies and low heat - overheat for efficiency
        if (enemiesRemaining <= 2 && moduleHeatLevel < 0.3)
        {
            return new OverheatWeaponRecommendation(
                true,
                "Few enemies remaining - safe to overheat");
        }

        return new OverheatWeaponRecommendation(
            false,
            "Normal operation sufficient");
    }

    /// <summary>
    /// Calculates time to kill target with current DPS
    /// </summary>
    /// <param name="targetCurrentHp">Target's current HP</param>
    /// <param name="ourDps">Our DPS against target</param>
    /// <param name="targetRepairRate">Target's repair rate (if any)</param>
    /// <returns>Estimated seconds to kill, or -1 if we can't break their tank</returns>
    public double CalculateTimeToKill(
        double targetCurrentHp,
        double ourDps,
        double targetRepairRate = 0)
    {
        var netDps = ourDps - targetRepairRate;

        if (netDps <= 0)
            return -1; // Can't break tank

        return targetCurrentHp / netDps;
    }

    /// <summary>
    /// Determines if we should stop shooting to save ammunition
    /// </summary>
    /// <param name="targetHealthPercentage">Target health percentage</param>
    /// <param name="dronesAssigned">Whether drones are assigned to target</param>
    /// <param name="dronesDps">DPS from drones</param>
    /// <param name="ammoRemaining">Ammunition charges remaining</param>
    /// <returns>True if should stop shooting (drones can finish)</returns>
    /// <remarks>
    /// Sometimes better to let drones finish low-HP targets to save ammo/cap
    /// </remarks>
    public bool ShouldLetDronesFinish(
        double targetHealthPercentage,
        bool dronesAssigned,
        double dronesDps,
        int ammoRemaining)
    {
        // Don't do this if we're low on ammo anyway
        if (ammoRemaining < 10)
            return false;

        // Target very low and drones assigned - let them finish
        if (targetHealthPercentage < 10 && dronesAssigned && dronesDps > 50)
            return true;

        return false;
    }
}

/// <summary>
/// Weapon system categories
/// </summary>
public enum WeaponSystem
{
    Missiles,
    Projectile,
    Hybrid,
    Energy,
    Drones
}

/// <summary>
/// Reload timing recommendation
/// </summary>
/// <param name="ShouldReload">Whether to reload now</param>
/// <param name="SuggestedAmmoType">Suggested ammo type to load (null = keep current)</param>
/// <param name="Reason">Explanation</param>
public record ReloadRecommendation(
    bool ShouldReload,
    string? SuggestedAmmoType,
    string Reason
);

/// <summary>
/// Ammunition selection recommendation
/// </summary>
/// <param name="AmmoType">Recommended ammo type</param>
/// <param name="Reason">Explanation</param>
public record AmmoRecommendation(
    string AmmoType,
    string Reason
);

/// <summary>
/// Weapon overheating recommendation
/// </summary>
/// <param name="ShouldOverheat">Whether to overheat weapons</param>
/// <param name="Reason">Explanation</param>
public record OverheatWeaponRecommendation(
    bool ShouldOverheat,
    string Reason
);
