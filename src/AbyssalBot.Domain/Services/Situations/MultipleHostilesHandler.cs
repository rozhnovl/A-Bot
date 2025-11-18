using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles situations with multiple hostile targets
/// </summary>
public class MultipleHostilesHandler : IMultipleHostilesHandler
{
    private const int DefensiveThreshold = 5;
    private const int CriticalThreshold = 10;
    private const double HighDpsThreshold = 500.0;

    public SituationResponse HandleMultipleHostiles(
        IReadOnlyList<Target> hostiles,
        double currentIncomingDps,
        Target? orbitBeacon)
    {
        var enemyCount = hostiles.Count;

        // No issue with few enemies
        if (enemyCount < DefensiveThreshold)
        {
            return SituationResponse.NoAction("Enemy count manageable");
        }

        var decisions = new List<CombatDecision>();
        var reasoning = new List<string>();
        var priority = SituationPriority.Normal;

        // Critical: Too many enemies
        if (enemyCount >= CriticalThreshold)
        {
            priority = SituationPriority.MultipleHostiles;
            reasoning.Add($"CRITICAL: {enemyCount} enemies engaging");

            // Defensive orbit on beacon if available
            if (orbitBeacon != null)
            {
                decisions.Add(new ManeuverDecision(
                    ShipManeuverType.Orbit,
                    orbitBeacon,
                    7500,
                    $"Defensive orbit with {enemyCount} enemies"
                ));
                reasoning.Add("Entering defensive orbit pattern");
            }

            // If DPS is very high, consider this extremely dangerous
            if (currentIncomingDps > HighDpsThreshold)
            {
                reasoning.Add($"Extreme incoming DPS: {currentIncomingDps:F0}");

                // Overheat hardeners
                decisions.Add(new ModuleDecision(
                    ModuleType.Hardener,
                    true,
                    true,
                    "Multiple enemies with high DPS - overheating hardeners"
                ));

                // Overheat shield boosters
                decisions.Add(new ModuleDecision(
                    ModuleType.ShieldBooster,
                    true,
                    true,
                    "Multiple enemies with high DPS - overheating boosters"
                ));

                reasoning.Add("Overheating all defensive modules");
            }
        }
        // Moderate: Several enemies
        else if (enemyCount >= DefensiveThreshold)
        {
            priority = SituationPriority.LowCapacitor; // Medium priority
            reasoning.Add($"Multiple enemies: {enemyCount} hostiles");

            // Ensure defensive positioning
            if (orbitBeacon != null && orbitBeacon.Distance > 15000)
            {
                decisions.Add(new ManeuverDecision(
                    ShipManeuverType.Orbit,
                    orbitBeacon,
                    7500,
                    "Defensive positioning for multiple enemies"
                ));
                reasoning.Add("Maintaining defensive position");
            }
        }

        // Focus fire strategy: prioritize DPS reduction
        var priorityTarget = DeterminePriorityTarget(hostiles);
        if (priorityTarget != null && !priorityTarget.IsTargeted)
        {
            decisions.Add(new LockTargetDecision(
                priorityTarget,
                $"Priority target for DPS reduction: {priorityTarget.Name}"
            ));
            reasoning.Add($"Targeting {priorityTarget.Name} for DPS reduction");
        }

        return new SituationResponse(
            priority,
            $"Multiple Hostiles ({enemyCount})",
            string.Join(". ", reasoning),
            decisions
        );
    }

    /// <summary>
    /// Determines which target should be prioritized to reduce incoming DPS
    /// </summary>
    private Target? DeterminePriorityTarget(IReadOnlyList<Target> hostiles)
    {
        // Priority order for DPS reduction:
        // 1. Closest high-DPS enemies (frigates that get in close)
        // 2. Known high-DPS ships
        // 3. Closest enemy

        // For now, simple heuristic: closest enemy (frigates are usually high DPS and close)
        return hostiles
            .Where(h => h.IsEnemy && !h.IsTargeted)
            .OrderBy(h => h.Distance)
            .FirstOrDefault();
    }
}
