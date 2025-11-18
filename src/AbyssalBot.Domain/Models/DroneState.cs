namespace AbyssalBot.Domain.Models;

/// <summary>
/// Represents the state of a drone
/// </summary>
public enum DroneStatus
{
    InBay,
    InSpace,
    Idle,
    Engaging,
    Returning,
    Unknown
}

/// <summary>
/// Represents the overall drone bay state
/// </summary>
public record DroneState(
    int DronesInBay,
    int DronesInSpace,
    int MaxDronesInSpace,
    IReadOnlyList<DroneStatus> DroneStatuses
)
{
    /// <summary>
    /// Checks if drones should be launched
    /// </summary>
    public bool ShouldLaunchDrones =>
        DronesInBay > 0 && DronesInSpace < MaxDronesInSpace;

    /// <summary>
    /// Checks if there are idle drones in space
    /// </summary>
    public bool HasIdleDrones =>
        DronesInSpace > 0 && DroneStatuses.Contains(DroneStatus.Idle);

    /// <summary>
    /// Checks if there are drones in space not returning
    /// </summary>
    public bool HasActiveNonReturningDrones =>
        DronesInSpace > 0 && !DroneStatuses.Contains(DroneStatus.Returning);

    /// <summary>
    /// Checks if all drones are safely in bay
    /// </summary>
    public bool AllDronesInBay => DronesInSpace == 0;

    /// <summary>
    /// Gets the count of drones in a specific state
    /// </summary>
    public int CountInStatus(DroneStatus status) =>
        DroneStatuses.Count(s => s == status);
}
