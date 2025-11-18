namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Provides inventory, cargo, and hangar access from memory
/// </summary>
public interface IInventoryReader
{
    /// <summary>
    /// Reads the current cargo hold contents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<InventoryItem>> ReadCargoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the drone bay contents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<InventoryItem>> ReadDroneBayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cargo capacity information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<CargoCapacity> GetCargoCapacityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds items in cargo matching a filter
    /// </summary>
    /// <param name="filter">Item filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<InventoryItem>> FindItemsAsync(
        InventoryItemFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if cargo contains a specific item
    /// </summary>
    /// <param name="itemName">Item name to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> HasItemAsync(string itemName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total value of items in cargo
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<decimal> GetCargoValueAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an inventory item
/// </summary>
public record InventoryItem(
    string Name,
    string Type,
    int Quantity,
    double Volume,
    decimal? EstimatedValue = null
);

/// <summary>
/// Represents cargo capacity information
/// </summary>
public record CargoCapacity(
    double UsedVolume,
    double MaxVolume,
    int ItemCount
)
{
    /// <summary>
    /// Gets the percentage of cargo space used
    /// </summary>
    public double PercentageFull => MaxVolume > 0 ? (UsedVolume / MaxVolume) * 100 : 0;

    /// <summary>
    /// Gets the remaining cargo space
    /// </summary>
    public double RemainingVolume => MaxVolume - UsedVolume;

    /// <summary>
    /// Checks if cargo is nearly full (>90%)
    /// </summary>
    public bool IsNearlyFull => PercentageFull >= 90;

    /// <summary>
    /// Checks if cargo is full
    /// </summary>
    public bool IsFull => UsedVolume >= MaxVolume;
}

/// <summary>
/// Represents inventory item filtering criteria
/// </summary>
public record InventoryItemFilter(
    string? NamePattern = null,
    string? TypePattern = null,
    int? MinQuantity = null,
    decimal? MinValue = null
);
