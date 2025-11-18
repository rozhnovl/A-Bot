using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides overview and target information from memory
/// </summary>
public interface IOverviewReader
{
    /// <summary>
    /// Reads all entries currently visible in the overview
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<Target>> ReadOverviewEntriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all currently locked targets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<TargetCollection> ReadTargetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the currently selected target
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Target?> GetSelectedTargetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds targets matching a specific filter
    /// </summary>
    /// <param name="filter">Target filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<Target>> FindTargetsAsync(
        TargetFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the maximum number of targets the ship can lock
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> GetMaxTargetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current number of locked targets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<int> GetCurrentTargetCountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents target filtering criteria
/// </summary>
public record TargetFilter(
    bool? IsEnemy = null,
    bool? IsTargeted = null,
    int? MaxDistance = null,
    string? NamePattern = null,
    string? TypePattern = null
);
