namespace AbyssalBot.Domain.Interfaces.Infrastructure;

/// <summary>
/// Repository for bot configuration and settings persistence
/// </summary>
public interface IConfigurationRepository
{
    /// <summary>
    /// Gets a configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<string?> GetConfigurationAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a strongly-typed configuration value
    /// </summary>
    /// <typeparam name="T">Configuration value type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T?> GetConfigurationAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Configuration value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetConfigurationAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a strongly-typed configuration value
    /// </summary>
    /// <typeparam name="T">Configuration value type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Configuration value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetConfigurationAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Deletes a configuration value
    /// </summary>
    /// <param name="key">Configuration key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteConfigurationAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all configuration values matching a key prefix
    /// </summary>
    /// <param name="prefix">Key prefix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Dictionary<string, string>> GetConfigurationsByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a configuration with a default value
    /// </summary>
    /// <typeparam name="T">Configuration value type</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<T> GetOrCreateConfigurationAsync<T>(
        string key,
        T defaultValue,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Saves a ship fitting configuration
    /// </summary>
    /// <param name="fittingName">Name of the fitting</param>
    /// <param name="fitting">Fitting configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveShipFittingAsync(
        string fittingName,
        BotFittingConfiguration fitting,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a ship fitting configuration
    /// </summary>
    /// <param name="fittingName">Name of the fitting</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<BotFittingConfiguration?> LoadShipFittingAsync(
        string fittingName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all saved fitting names
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IReadOnlyList<string>> GetSavedFittingsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a bot fitting configuration
/// </summary>
public record BotFittingConfiguration(
    string Name,
    string ShipType,
    int Tier,
    int OptimalAttackRange,
    int MaxTargetingRange,
    int MaxTargets,
    bool UseDrones,
    int MaxDrones,
    Dictionary<string, object> CustomSettings
);
