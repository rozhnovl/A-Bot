using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Interfaces;

/// <summary>
/// Provides targets from the overview or other sources
/// </summary>
public interface ITargetProvider
{
    /// <summary>
    /// Gets all available targets from overview
    /// </summary>
    IReadOnlyList<Target> GetAvailableTargets();

    /// <summary>
    /// Gets all enemy targets
    /// </summary>
    IReadOnlyList<Target> GetEnemyTargets();

    /// <summary>
    /// Finds a specific target by name pattern
    /// </summary>
    Target? FindTargetByName(string namePattern);

    /// <summary>
    /// Finds targets by type
    /// </summary>
    IReadOnlyList<Target> FindTargetsByType(string type);
}
