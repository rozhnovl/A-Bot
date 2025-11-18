using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles optimal timing for module reloads to maximize combat efficiency
/// </summary>
public class ReloadTimingHandler : IReloadTimingHandler
{
    private const double LowCapThreshold = 30.0;
    private const int SafeReloadDistance = 20000;
    private readonly Dictionary<ModuleType, DateTime> _lastReloadTimes = new();

    public bool ShouldReloadNow(CombatContext context, ModuleType moduleType)
    {
        return moduleType switch
        {
            ModuleType.Weapon => ShouldReloadWeapons(context),
            ModuleType.ShieldBooster => ShouldReloadCapBooster(context),
            _ => false
        };
    }

    public string GetReloadReasoning(CombatContext context, ModuleType moduleType)
    {
        if (!ShouldReloadNow(context, moduleType))
        {
            return GetNoReloadReason(context, moduleType);
        }

        return moduleType switch
        {
            ModuleType.Weapon => "No targets in weapon range - safe to reload",
            ModuleType.ShieldBooster => "Capacitor stable and safe - reloading cap booster",
            _ => "Unknown module type"
        };
    }

    private bool ShouldReloadWeapons(CombatContext context)
    {
        // Don't reload if we have an active target in range
        if (context.ActiveCombatTarget != null)
        {
            var weapon = context.Fitting.GetWeapon();
            if (weapon != null && context.ActiveCombatTarget.Distance <= weapon.OptimalRange)
            {
                return false;
            }
        }

        // Safe to reload if:
        // 1. No active combat target
        // 2. All enemies are far away
        // 3. We're not in immediate danger

        var closestEnemy = context.AvailableTargets
            .Where(t => t.IsEnemy)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        // No enemies = safe to reload
        if (closestEnemy == null)
        {
            return true;
        }

        // Enemies far away = safe to reload
        if (closestEnemy.Distance > SafeReloadDistance)
        {
            return true;
        }

        // Not safe to reload
        return false;
    }

    private bool ShouldReloadCapBooster(CombatContext context)
    {
        var capPercentage = context.Hitpoints.CapacitorPercentage;

        // NEVER reload cap booster when cap is low
        if (capPercentage < LowCapThreshold)
        {
            return false;
        }

        // Don't reload if we're taking heavy damage
        if (context.IncomingDps > 300)
        {
            return false;
        }

        // Don't reload if shield is critically low
        if (context.Hitpoints.ShieldPercentage < 30)
        {
            return false;
        }

        // Check if other modules are reloading (stagger reloads)
        var recentReload = _lastReloadTimes.Values
            .Any(time => (DateTime.UtcNow - time).TotalSeconds < 5);

        if (recentReload)
        {
            return false;
        }

        // Safe to reload if:
        // 1. Cap is above 30%
        // 2. Not taking heavy damage
        // 3. Shield is healthy
        // 4. No other recent reloads

        // Record this reload
        _lastReloadTimes[ModuleType.ShieldBooster] = DateTime.UtcNow;

        return true;
    }

    private string GetNoReloadReason(CombatContext context, ModuleType moduleType)
    {
        return moduleType switch
        {
            ModuleType.Weapon => GetWeaponNoReloadReason(context),
            ModuleType.ShieldBooster => GetCapBoosterNoReloadReason(context),
            _ => "Not safe to reload"
        };
    }

    private string GetWeaponNoReloadReason(CombatContext context)
    {
        if (context.ActiveCombatTarget != null)
        {
            return $"Active target in range at {context.ActiveCombatTarget.Distance}m";
        }

        var closestEnemy = context.AvailableTargets
            .Where(t => t.IsEnemy)
            .OrderBy(t => t.Distance)
            .FirstOrDefault();

        if (closestEnemy != null && closestEnemy.Distance < SafeReloadDistance)
        {
            return $"Enemy too close at {closestEnemy.Distance}m";
        }

        return "Not safe to reload weapons";
    }

    private string GetCapBoosterNoReloadReason(CombatContext context)
    {
        if (context.Hitpoints.CapacitorPercentage < LowCapThreshold)
        {
            return $"Capacitor too low at {context.Hitpoints.CapacitorPercentage:F1}%";
        }

        if (context.IncomingDps > 300)
        {
            return $"Taking heavy damage: {context.IncomingDps:F0} DPS";
        }

        if (context.Hitpoints.ShieldPercentage < 30)
        {
            return $"Shield critically low at {context.Hitpoints.ShieldPercentage:F1}%";
        }

        var recentReload = _lastReloadTimes.Values
            .Any(time => (DateTime.UtcNow - time).TotalSeconds < 5);

        if (recentReload)
        {
            return "Another module recently reloaded - staggering reloads";
        }

        return "Not safe to reload cap booster";
    }
}
