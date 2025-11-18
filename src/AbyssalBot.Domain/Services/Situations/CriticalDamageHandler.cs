using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles critical damage situations with emergency tanking and retreat logic
/// </summary>
public class CriticalDamageHandler : ICriticalDamageHandler
{
    private const double CriticalStructureThreshold = 50.0;
    private const double CriticalShieldThreshold = 20.0;
    private const double CriticalArmorThreshold = 30.0;

    public SituationResponse HandleCriticalDamage(ShipStatus status)
    {
        var hitpoints = status.Hitpoints;
        var decisions = new List<CombatDecision>();
        var reasoning = new List<string>();

        // Check hull/structure damage - HIGHEST PRIORITY
        if (hitpoints.HullPercentage < CriticalStructureThreshold)
        {
            reasoning.Add($"CRITICAL: Structure at {hitpoints.HullPercentage:F1}%");

            // This is truly critical - would need warp out logic here
            // For Abyssal sites, we can't warp so we overheat everything
            decisions.AddRange(OverheatAllTankModules(status.Fitting, "Critical structure damage"));

            // Try to reduce incoming DPS by kiting
            reasoning.Add("Emergency evasive maneuvers needed");

            return new SituationResponse(
                SituationPriority.CriticalDamage,
                "Critical Structure Damage",
                string.Join(". ", reasoning),
                decisions
            );
        }

        // Check shield damage (for active shield tanks)
        if (hitpoints.ShieldPercentage < CriticalShieldThreshold)
        {
            reasoning.Add($"Critical shield at {hitpoints.ShieldPercentage:F1}%");

            // Overheat all shield boosters
            var boosters = status.Fitting.GetShieldBoosters().ToList();
            foreach (var booster in boosters)
            {
                if (!booster.IsOverloaded && booster.IsActive)
                {
                    decisions.Add(new ModuleDecision(
                        ModuleType.ShieldBooster,
                        true,
                        true,
                        $"Critical shield - overheating booster"
                    ));
                }
                else if (!booster.IsActive)
                {
                    decisions.Add(new ModuleDecision(
                        ModuleType.ShieldBooster,
                        true,
                        true,
                        "Activating and overheating shield booster"
                    ));
                }
            }

            // Overheat hardeners too
            var hardeners = status.Fitting.GetModulesByType(ModuleType.Hardener).ToList();
            foreach (var hardener in hardeners.Where(h => !h.IsOverloaded))
            {
                decisions.Add(new ModuleDecision(
                    ModuleType.Hardener,
                    true,
                    true,
                    "Overheating hardener for maximum resistance"
                ));
            }

            reasoning.Add("Overheating all tank modules");

            return new SituationResponse(
                SituationPriority.CriticalDamage,
                "Critical Shield Damage",
                string.Join(". ", reasoning),
                decisions
            );
        }

        // Check armor damage (for armor tanks)
        if (hitpoints.ArmorPercentage < CriticalArmorThreshold)
        {
            reasoning.Add($"Critical armor at {hitpoints.ArmorPercentage:F1}%");

            // Overheat all tank modules
            decisions.AddRange(OverheatAllTankModules(status.Fitting, "Critical armor damage"));

            reasoning.Add("Overheating all defensive modules");

            return new SituationResponse(
                SituationPriority.CriticalDamage,
                "Critical Armor Damage",
                string.Join(". ", reasoning),
                decisions
            );
        }

        // No critical damage detected
        return SituationResponse.NoAction("Ship status normal");
    }

    private IEnumerable<CombatDecision> OverheatAllTankModules(ShipFitting fitting, string reason)
    {
        var decisions = new List<CombatDecision>();

        // Overheat all shield boosters
        foreach (var booster in fitting.GetShieldBoosters())
        {
            if (!booster.IsOverloaded)
            {
                decisions.Add(new ModuleDecision(
                    ModuleType.ShieldBooster,
                    true,
                    true,
                    $"{reason} - overheating booster"
                ));
            }
        }

        // Overheat all hardeners
        foreach (var hardener in fitting.GetModulesByType(ModuleType.Hardener))
        {
            if (!hardener.IsOverloaded)
            {
                decisions.Add(new ModuleDecision(
                    ModuleType.Hardener,
                    true,
                    true,
                    $"{reason} - overheating hardener"
                ));
            }
        }

        return decisions;
    }
}
