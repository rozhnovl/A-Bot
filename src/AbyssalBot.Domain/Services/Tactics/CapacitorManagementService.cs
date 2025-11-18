using System;
using System.Collections.Generic;
using System.Linq;

namespace AbyssalBot.Domain.Services.Tactics;

/// <summary>
/// Service for managing ship capacitor and module activation based on cap stability
/// </summary>
/// <remarks>
/// Implements EVE Online capacitor mechanics including:
/// - Stable/unstable capacitor detection
/// - Module priority when cap is low
/// - Cap booster usage optimization
/// - Overheating recommendations based on cap status
/// </remarks>
public class CapacitorManagementService
{
    /// <summary>
    /// Calculates capacitor stability percentage
    /// </summary>
    /// <param name="currentCap">Current capacitor amount (GJ)</param>
    /// <param name="maxCap">Maximum capacitor amount (GJ)</param>
    /// <param name="capRechargeRate">Capacitor recharge rate (GJ/s)</param>
    /// <param name="capUsageRate">Total cap usage rate from modules (GJ/s)</param>
    /// <returns>Stable cap percentage (0-100), or -1 if cap is unstable</returns>
    /// <remarks>
    /// EVE capacitor recharge formula uses Peak Recharge = 2.5 * MaxCap / RechargeTime
    /// Peak occurs at 25% capacitor. Formula: dCap/dt = 10/T * (sqrt(cap) - sqrt(MaxCap))
    /// where cap is in fraction (0-1) and T is recharge time in seconds
    /// </remarks>
    public double CalculateStableCapPercentage(
        double currentCap,
        double maxCap,
        double capRechargeRate,
        double capUsageRate)
    {
        if (maxCap <= 0)
            return -1;

        // If we're using less cap than peak recharge, we're stable
        if (capUsageRate <= capRechargeRate * 1.1) // 1.1 for safety margin
        {
            // Calculate stable percentage using simplified model
            // At equilibrium: recharge rate = usage rate
            // Recharge peaks at 25%, so if usage < peak, we'll stabilize somewhere
            var usageRatio = capUsageRate / capRechargeRate;

            if (usageRatio < 0.5)
                return 75; // High stability
            else if (usageRatio < 0.8)
                return 50; // Medium stability
            else
                return 30; // Low but stable
        }

        return -1; // Unstable - will cap out
    }

    /// <summary>
    /// Determines if capacitor is stable with current module configuration
    /// </summary>
    /// <param name="capRechargeRate">Cap recharge rate (GJ/s)</param>
    /// <param name="capUsageRate">Total cap usage from all active modules (GJ/s)</param>
    /// <returns>True if capacitor is stable</returns>
    public bool IsCapacitorStable(double capRechargeRate, double capUsageRate)
    {
        return capUsageRate <= capRechargeRate * 1.05; // 5% safety margin
    }

    /// <summary>
    /// Calculates time until capacitor runs out (if unstable)
    /// </summary>
    /// <param name="currentCap">Current capacitor (GJ)</param>
    /// <param name="capUsageRate">Net cap usage rate (usage - recharge, GJ/s)</param>
    /// <returns>Seconds until cap runs out, or -1 if cap is stable</returns>
    public double CalculateTimeUntilCapEmpty(double currentCap, double capUsageRate)
    {
        if (capUsageRate <= 0)
            return -1; // Stable or gaining cap

        return currentCap / capUsageRate;
    }

    /// <summary>
    /// Gets module activation priority when capacitor is low
    /// </summary>
    /// <returns>List of module categories in priority order (highest to lowest)</returns>
    /// <remarks>
    /// Priority order for low cap situations:
    /// 1. Hardeners - Keep you alive by reducing incoming damage
    /// 2. Shield Boosters - Active tank for immediate survival
    /// 3. Weapons - Need to kill enemies to end the threat
    /// 4. Prop Mod - Speed/defense, but can be sacrificed if cap critical
    /// 5. EWAR/Utility - Nice to have but not essential for survival
    /// </remarks>
    public IReadOnlyList<ModulePriority> GetLowCapModulePriorities()
    {
        return new[]
        {
            new ModulePriority("Hardener", 1, true, "Critical - reduces incoming damage"),
            new ModulePriority("Shield Booster", 2, true, "Critical - active tank for survival"),
            new ModulePriority("Armor Repairer", 2, true, "Critical - active tank for survival"),
            new ModulePriority("Weapon", 3, false, "Important - need to kill enemies"),
            new ModulePriority("Propulsion", 4, false, "Useful - defense through speed/transversal"),
            new ModulePriority("EWAR", 5, false, "Optional - utility modules"),
            new ModulePriority("Cap Booster", 1, true, "Critical - restores capacitor"),
        };
    }

    /// <summary>
    /// Determines which modules should be deactivated when cap is critical
    /// </summary>
    /// <param name="currentCapPercentage">Current cap percentage (0-100)</param>
    /// <param name="isStable">Whether cap is stable</param>
    /// <param name="timeUntilEmpty">Seconds until cap runs out (-1 if stable)</param>
    /// <returns>List of module types that should be turned off</returns>
    public IReadOnlyList<string> GetModulesToDeactivate(
        double currentCapPercentage,
        bool isStable,
        double timeUntilEmpty)
    {
        var toDeactivate = new List<string>();

        // Critical cap (< 20%) - turn off everything except essential
        if (currentCapPercentage < 20)
        {
            toDeactivate.Add("Propulsion");
            toDeactivate.Add("EWAR");

            if (currentCapPercentage < 10)
            {
                toDeactivate.Add("Weapon"); // Survive first, kill later
            }
        }
        // Low cap (< 40%) and unstable
        else if (currentCapPercentage < 40 && !isStable)
        {
            toDeactivate.Add("Propulsion");

            // If we'll run out very soon, start being more aggressive
            if (timeUntilEmpty > 0 && timeUntilEmpty < 30)
            {
                toDeactivate.Add("EWAR");
            }
        }
        // Medium cap (< 60%) and very unstable
        else if (currentCapPercentage < 60 && timeUntilEmpty > 0 && timeUntilEmpty < 45)
        {
            toDeactivate.Add("Propulsion"); // Sacrifice speed to maintain combat capability
        }

        return toDeactivate;
    }

    /// <summary>
    /// Determines if we should use cap booster charges
    /// </summary>
    /// <param name="currentCapPercentage">Current cap percentage</param>
    /// <param name="isStable">Whether cap is stable</param>
    /// <param name="chargesRemaining">Number of cap booster charges left</param>
    /// <param name="inCombat">Whether currently in combat</param>
    /// <returns>True if cap booster should be activated</returns>
    /// <remarks>
    /// Conservative usage - save charges for when really needed
    /// </remarks>
    public bool ShouldUseCapBooster(
        double currentCapPercentage,
        bool isStable,
        int chargesRemaining,
        bool inCombat)
    {
        if (chargesRemaining <= 0)
            return false;

        // Emergency situations
        if (currentCapPercentage < 25 && inCombat)
            return true;

        // Unstable and dropping fast
        if (!isStable && currentCapPercentage < 40 && inCombat)
            return true;

        // Save charges if we have few left
        if (chargesRemaining <= 3)
            return currentCapPercentage < 15 && inCombat;

        return false;
    }

    /// <summary>
    /// Determines if modules should be overheated based on cap status
    /// </summary>
    /// <param name="currentCapPercentage">Current cap percentage</param>
    /// <param name="isStable">Whether cap is stable</param>
    /// <param name="shieldPercentage">Current shield percentage</param>
    /// <returns>Overheating recommendation with reasons</returns>
    /// <remarks>
    /// Overheating increases module effectiveness but also cap consumption.
    /// Only recommend when absolutely necessary and cap can handle it.
    /// </remarks>
    public OverheatRecommendation GetOverheatRecommendation(
        double currentCapPercentage,
        bool isStable,
        double shieldPercentage)
    {
        // Never overheat if cap is too low - will just drain cap faster
        if (currentCapPercentage < 30)
        {
            return new OverheatRecommendation(
                false,
                Array.Empty<string>(),
                "Capacitor too low for overheating");
        }

        // Critical shield + stable cap = overheat tank
        if (shieldPercentage < 30 && currentCapPercentage > 50)
        {
            return new OverheatRecommendation(
                true,
                new[] { "Shield Booster", "Hardener" },
                "Critical shield - overheat tank modules");
        }

        // Moderate danger + good cap = overheat weapons to kill faster
        if (shieldPercentage < 50 && currentCapPercentage > 60 && isStable)
        {
            return new OverheatRecommendation(
                true,
                new[] { "Weapon" },
                "Overheat weapons to reduce fight duration");
        }

        return new OverheatRecommendation(
            false,
            Array.Empty<string>(),
            "No overheating recommended - manage capacitor");
    }

    /// <summary>
    /// Calculates optimal cap percentage to activate shield booster
    /// </summary>
    /// <param name="maxShield">Maximum shield HP</param>
    /// <param name="boosterAmount">HP repaired per cycle</param>
    /// <param name="incomingDps">Current incoming DPS</param>
    /// <returns>Shield percentage threshold for activation</returns>
    /// <remarks>
    /// Don't activate at 100% (wasted cycle), but don't wait too long either
    /// </remarks>
    public double CalculateBoosterActivationThreshold(
        double maxShield,
        double boosterAmount,
        double incomingDps)
    {
        // Don't activate if we'd overheal by more than one cycle
        var wasteThreshold = ((maxShield - boosterAmount) / maxShield) * 100;

        // Under high DPS, activate earlier
        if (incomingDps > 200)
            wasteThreshold = Math.Max(wasteThreshold - 10, 70);

        return Math.Max(wasteThreshold, 75); // Never above 85%
    }
}

/// <summary>
/// Represents module activation priority
/// </summary>
/// <param name="ModuleType">Type of module</param>
/// <param name="Priority">Priority level (1 = highest)</param>
/// <param name="KeepActiveInLowCap">Whether to keep active even when cap is low</param>
/// <param name="Reason">Explanation of priority</param>
public record ModulePriority(
    string ModuleType,
    int Priority,
    bool KeepActiveInLowCap,
    string Reason
);

/// <summary>
/// Represents an overheating recommendation
/// </summary>
/// <param name="ShouldOverheat">Whether overheating is recommended</param>
/// <param name="ModulesToOverheat">Which module types to overheat</param>
/// <param name="Reason">Explanation of recommendation</param>
public record OverheatRecommendation(
    bool ShouldOverheat,
    IReadOnlyList<string> ModulesToOverheat,
    string Reason
);
