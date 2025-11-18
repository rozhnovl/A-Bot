using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services;

/// <summary>
/// Service for making ship maneuver decisions
/// </summary>
public class ManeuverDecisionService
{
    private const double HighDpsThreshold = 200;
    private const int OrbitDistance = 5000;
    private const int KeepAtRangeDistance = 500;

    /// <summary>
    /// Decides the appropriate maneuver based on combat situation
    /// </summary>
    public ManeuverDecision? DecideManeuver(
        ShipManeuverType currentManeuver,
        double incomingDps,
        Target? orbitBeacon,
        int distanceToBeacon)
    {
        // High DPS situation - need to orbit for defense
        if (incomingDps > HighDpsThreshold)
        {
            if (currentManeuver != ShipManeuverType.Orbit && orbitBeacon != null)
            {
                return new ManeuverDecision(
                    ShipManeuverType.Orbit,
                    orbitBeacon,
                    OrbitDistance,
                    $"High incoming DPS ({incomingDps:F1}), orbiting {orbitBeacon.Name} for defense"
                );
            }
        }
        else
        {
            // Low DPS - can maintain position for better damage application
            if (currentManeuver is not (ShipManeuverType.Approach
                or ShipManeuverType.KeepAtRange
                or ShipManeuverType.Orbit) && orbitBeacon != null)
            {
                return new ManeuverDecision(
                    ShipManeuverType.KeepAtRange,
                    orbitBeacon,
                    KeepAtRangeDistance,
                    $"Low incoming DPS ({incomingDps:F1}), keeping at range for damage application"
                );
            }
        }

        return null;
    }

    /// <summary>
    /// Decides if MWD should be active
    /// </summary>
    public bool ShouldActivateMWD(
        double incomingDps,
        int distanceToTarget,
        ShipManeuverType currentManeuver)
    {
        // High DPS and orbiting - MWD could be risky
        if (incomingDps > HighDpsThreshold && currentManeuver == ShipManeuverType.Orbit)
        {
            return false; // Let the player decide or implement more complex logic
        }

        // Low DPS - use MWD if far from target
        return distanceToTarget > 2000;
    }

    /// <summary>
    /// Selects the best beacon to orbit
    /// </summary>
    public Target? SelectOrbitBeacon(
        IEnumerable<Target> availableTargets,
        Target? coreCache,
        Target? conduit,
        NpcInformationService npcInfoService)
    {
        // Priority: Orbit beacon NPCs > Core cache > Conduit
        var orbitBeacon = availableTargets
            .Where(t => npcInfoService.IsOrbitBeacon(t.Name))
            .OrderBy(t => npcInfoService.CalculateTargetPriority(t.Name, t.Type))
            .FirstOrDefault();

        return orbitBeacon ?? coreCache ?? conduit;
    }
}
