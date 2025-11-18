using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Interfaces;

/// <summary>
/// Provides the current state of the ship from memory/sensor data
/// </summary>
public interface IShipStateProvider
{
    /// <summary>
    /// Gets the current ship fitting configuration
    /// </summary>
    ShipFitting GetShipFitting();

    /// <summary>
    /// Gets the current hitpoints and energy status
    /// </summary>
    ShipHitpointsAndEnergy GetHitpointsAndEnergy();

    /// <summary>
    /// Gets the current maneuver type
    /// </summary>
    ShipManeuverType GetCurrentManeuver();

    /// <summary>
    /// Gets the current targets
    /// </summary>
    TargetCollection GetCurrentTargets();

    /// <summary>
    /// Gets the current drone state
    /// </summary>
    DroneState GetDroneState();

    /// <summary>
    /// Checks if ship can start maneuvering (not in warp, etc.)
    /// </summary>
    bool CanStartManeuver();

    /// <summary>
    /// Checks if ship is currently in Abyssal space
    /// </summary>
    bool IsInAbyss();
}
