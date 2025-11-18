using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Enums;

namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides ship-specific state information from memory
/// </summary>
public interface IShipStateReader
{
    /// <summary>
    /// Reads the current ship status including HP, energy, and capacitor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ShipHitpointsAndEnergy> ReadShipStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the ship fitting configuration including all modules
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ShipFitting> ReadShipFittingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current drone state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<DroneState> ReadDroneStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current ship velocity and heading
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ShipMovement> ReadShipMovementAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the ship is currently warping
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> IsWarpingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current maneuver state
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ShipManeuverType> GetCurrentManeuverAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the ship can start a new maneuver
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> CanStartManeuverAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents ship movement information
/// </summary>
public record ShipMovement(
    double Velocity,
    double Heading,
    bool IsMoving,
    bool IsOrbiting,
    bool IsApproaching
);
