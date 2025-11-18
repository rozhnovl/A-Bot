using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services;

/// <summary>
/// Service for making ship tanking decisions (shield/armor management)
/// </summary>
public class TankingDecisionService
{
    private const double ShieldCriticalThreshold = 600;
    private const double ShieldSafeThreshold = 800;
    private const double CapacitorLowThreshold = 400;
    private const double HighDpsThreshold = 150;
    private const double CriticalShieldThreshold = 150;

    /// <summary>
    /// Decides which shield boosters should be active
    /// </summary>
    public IEnumerable<ModuleDecision> DecideShieldBoosting(
        ShipHitpointsAndEnergy hitpoints,
        double incomingDps,
        IEnumerable<ShipModule> shieldBoosters)
    {
        var boosters = shieldBoosters.ToList();
        if (boosters.Count == 0)
            yield break;

        // Critical shield - activate all boosters with overload
        if (hitpoints.Shield < CriticalShieldThreshold)
        {
            foreach (var booster in boosters.Where(b => !b.IsActive))
            {
                yield return new ModuleDecision(
                    ModuleType.ShieldBooster,
                    true,
                    true,
                    $"Critical shield ({hitpoints.Shield:F0}), activating all boosters with overload"
                );
            }
            yield break;
        }

        // Shield needs repair
        if (hitpoints.Shield < ShieldCriticalThreshold)
        {
            // High incoming DPS - use all boosters
            if (incomingDps > HighDpsThreshold)
            {
                foreach (var booster in boosters.Where(b => !b.IsActive))
                {
                    yield return new ModuleDecision(
                        ModuleType.ShieldBooster,
                        true,
                        false,
                        $"High DPS ({incomingDps:F1}), activating booster"
                    );
                }
            }
            else
            {
                // Low DPS - use single booster
                var inactiveBooster = boosters.FirstOrDefault(b => !b.IsActive);
                if (inactiveBooster != null)
                {
                    yield return new ModuleDecision(
                        ModuleType.ShieldBooster,
                        true,
                        false,
                        $"Shield low ({hitpoints.Shield:F0}), activating single booster"
                    );
                }

                // Deactivate other boosters to save cap
                foreach (var activeBooster in boosters.Where(b => b.IsActive).Skip(1))
                {
                    yield return new ModuleDecision(
                        ModuleType.ShieldBooster,
                        false,
                        false,
                        "Deactivating extra booster to save capacitor"
                    );
                }
            }
        }
        else if (hitpoints.Shield > ShieldSafeThreshold || hitpoints.Capacitor < CapacitorLowThreshold)
        {
            // Shield safe or cap low - turn off boosters
            foreach (var booster in boosters.Where(b => b.IsActive))
            {
                yield return new ModuleDecision(
                    ModuleType.ShieldBooster,
                    false,
                    false,
                    $"Shield safe ({hitpoints.Shield:F0}) or capacitor low ({hitpoints.Capacitor:F0}), " +
                    "deactivating booster"
                );
            }
        }
    }

    /// <summary>
    /// Decides if hardeners should be active (they should always be active)
    /// </summary>
    public IEnumerable<ModuleDecision> DecideHardeners(IEnumerable<ShipModule> hardeners)
    {
        foreach (var hardener in hardeners.Where(h => !h.IsActive))
        {
            yield return new ModuleDecision(
                ModuleType.Hardener,
                true,
                false,
                "Hardeners should always be active"
            );
        }
    }
}
