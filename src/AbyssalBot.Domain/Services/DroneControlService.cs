using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services;

/// <summary>
/// Service for making drone control decisions
/// </summary>
public class DroneControlService
{
    private const int MaxDroneRange = 55000;

    /// <summary>
    /// Decides if drones should be launched
    /// </summary>
    public LaunchDronesDecision? DecideLaunchDrones(DroneState droneState)
    {
        if (droneState.ShouldLaunchDrones)
        {
            return new LaunchDronesDecision(
                $"Launching drones ({droneState.DronesInBay} available, " +
                $"{droneState.DronesInSpace}/{droneState.MaxDronesInSpace} in space)"
            );
        }

        return null;
    }

    /// <summary>
    /// Decides if drones should engage a target
    /// </summary>
    public EngageDronesDecision? DecideEngageDrones(
        DroneState droneState,
        Target target)
    {
        if (!target.ShouldAssignDrones(MaxDroneRange))
        {
            return null;
        }

        if (droneState.HasIdleDrones)
        {
            return new EngageDronesDecision(
                target,
                $"Engaging {droneState.CountInStatus(DroneStatus.Idle)} idle drones on {target.Name}"
            );
        }

        return null;
    }

    /// <summary>
    /// Decides if drones should be returned
    /// </summary>
    public ReturnDronesDecision? DecideReturnDrones(
        DroneState droneState,
        bool noEnemiesRemaining)
    {
        if (noEnemiesRemaining && droneState.HasActiveNonReturningDrones)
        {
            return new ReturnDronesDecision(
                $"No enemies remaining, returning {droneState.DronesInSpace} drones"
            );
        }

        return null;
    }

    /// <summary>
    /// Gets all drone decisions for a combat situation
    /// </summary>
    public IEnumerable<CombatDecision> GetDroneDecisions(
        DroneState droneState,
        Target? activeTarget,
        bool noEnemiesRemaining)
    {
        // Return drones if no enemies
        if (noEnemiesRemaining)
        {
            var returnDecision = DecideReturnDrones(droneState, noEnemiesRemaining);
            if (returnDecision != null)
            {
                yield return returnDecision;
            }
            yield break;
        }

        // Launch drones if needed
        var launchDecision = DecideLaunchDrones(droneState);
        if (launchDecision != null)
        {
            yield return launchDecision;
        }

        // Engage drones on active target
        if (activeTarget != null)
        {
            var engageDecision = DecideEngageDrones(droneState, activeTarget);
            if (engageDecision != null)
            {
                yield return engageDecision;
            }
        }
    }
}
