using AbyssalBot.Domain.Interfaces.Infrastructure;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AbyssalBot.Infrastructure.Database;

/// <summary>
/// In-memory implementation of IConfigurationRepository (for testing/development)
/// </summary>
public class InMemoryConfigurationRepository : IConfigurationRepository
{
    private readonly ConcurrentDictionary<string, string> _configurations = new();
    private readonly ConcurrentDictionary<string, BotFittingConfiguration> _fittings = new();

    public async Task<string?> GetConfigurationAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        _configurations.TryGetValue(key, out var value);
        return await Task.FromResult(value);
    }

    public async Task<T?> GetConfigurationAsync<T>(
        string key,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_configurations.TryGetValue(key, out var json))
        {
            var value = JsonSerializer.Deserialize<T>(json);
            return await Task.FromResult(value);
        }

        return null;
    }

    public async Task SetConfigurationAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        _configurations[key] = value;
        await Task.CompletedTask;
    }

    public async Task SetConfigurationAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        _configurations[key] = json;
        await Task.CompletedTask;
    }

    public async Task DeleteConfigurationAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        _configurations.TryRemove(key, out _);
        await Task.CompletedTask;
    }

    public async Task<Dictionary<string, string>> GetConfigurationsByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var results = _configurations
            .Where(kvp => kvp.Key.StartsWith(prefix))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return await Task.FromResult(results);
    }

    public async Task<T> GetOrCreateConfigurationAsync<T>(
        string key,
        T defaultValue,
        CancellationToken cancellationToken = default) where T : class
    {
        var existing = await GetConfigurationAsync<T>(key, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        await SetConfigurationAsync(key, defaultValue, cancellationToken);
        return defaultValue;
    }

    public async Task SaveShipFittingAsync(
        string fittingName,
        BotFittingConfiguration fitting,
        CancellationToken cancellationToken = default)
    {
        _fittings[fittingName] = fitting;
        await Task.CompletedTask;
    }

    public async Task<BotFittingConfiguration?> LoadShipFittingAsync(
        string fittingName,
        CancellationToken cancellationToken = default)
    {
        _fittings.TryGetValue(fittingName, out var fitting);
        return await Task.FromResult(fitting);
    }

    public async Task<IReadOnlyList<string>> GetSavedFittingsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_fittings.Keys.ToList());
    }
}
