namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Repository for Abyssal deadspace spawn data and statistics
/// </summary>
public interface IAbyssDataRepository
{
    /// <summary>
    /// Records an abyssal room encounter
    /// </summary>
    /// <param name="encounter">Encounter data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordEncounterAsync(
        AbyssalEncounter encounter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical encounter data for a specific room type
    /// </summary>
    /// <param name="roomType">Room type</param>
    /// <param name="tier">Abyssal tier (1-6)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<AbyssalEncounter>> GetEncountersAsync(
        string roomType,
        int tier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets spawn statistics for a specific room type
    /// </summary>
    /// <param name="roomType">Room type</param>
    /// <param name="tier">Abyssal tier (1-6)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<SpawnStatistics> GetSpawnStatisticsAsync(
        string roomType,
        int tier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets loot statistics for a specific tier
    /// </summary>
    /// <param name="tier">Abyssal tier (1-6)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<LootStatistics> GetLootStatisticsAsync(
        int tier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a complete abyssal run
    /// </summary>
    /// <param name="run">Run data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordRunAsync(
        AbyssalRun run,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets run history with filtering
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<AbyssalRun>> GetRunHistoryAsync(
        RunHistoryFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overall performance statistics
    /// </summary>
    /// <param name="tier">Optional tier filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<PerformanceStatistics> GetPerformanceStatisticsAsync(
        int? tier = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an abyssal room encounter
/// </summary>
public record AbyssalEncounter(
    Guid Id,
    string RoomType,
    int Tier,
    IReadOnlyList<string> Spawns,
    DateTimeOffset Timestamp,
    TimeSpan Duration,
    bool Completed,
    int DamageTaken,
    int DamageDealt
);

/// <summary>
/// Represents a complete abyssal run
/// </summary>
public record AbyssalRun(
    Guid Id,
    int Tier,
    string Filament,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    bool Completed,
    IReadOnlyList<AbyssalEncounter> Encounters,
    IReadOnlyList<LootItem> Loot,
    decimal EstimatedValue
);

/// <summary>
/// Represents spawn statistics for a room
/// </summary>
public record SpawnStatistics(
    string RoomType,
    int Tier,
    int TotalEncounters,
    Dictionary<string, int> SpawnCounts,
    Dictionary<string, double> SpawnProbabilities
);

/// <summary>
/// Represents loot statistics
/// </summary>
public record LootStatistics(
    int Tier,
    int TotalRuns,
    decimal AverageLootValue,
    decimal MedianLootValue,
    decimal TotalValue,
    Dictionary<string, int> ItemCounts
);

/// <summary>
/// Represents performance statistics
/// </summary>
public record PerformanceStatistics(
    int TotalRuns,
    int CompletedRuns,
    int FailedRuns,
    double CompletionRate,
    TimeSpan AverageRunTime,
    decimal AverageLootValue,
    decimal TotalProfit
);

/// <summary>
/// Represents a loot item
/// </summary>
public record LootItem(
    string Name,
    int Quantity,
    decimal EstimatedValue
);

/// <summary>
/// Represents run history filter criteria
/// </summary>
public record RunHistoryFilter(
    int? Tier = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    bool? CompletedOnly = null,
    int Skip = 0,
    int Take = 100
);
