namespace Sanderling.ABot.Bot;

/// <summary>
/// Factory for creating providers that depend on the current bot state
/// </summary>
public interface IProviderFactory
{
    IOverviewProvider CreateOverviewProvider(Bot bot);
    IInventoryProvider CreateInventoryProvider(Bot bot);
}
