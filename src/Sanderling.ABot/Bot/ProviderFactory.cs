namespace Sanderling.ABot.Bot;

/// <summary>
/// Default implementation of provider factory
/// </summary>
public class ProviderFactory : IProviderFactory
{
    public IOverviewProvider CreateOverviewProvider(Bot bot)
    {
        return new MemoryProxyOverviewProvider(bot);
    }

    public IInventoryProvider CreateInventoryProvider(Bot bot)
    {
        return new MemoryProxyInventoryProvider(bot);
    }
}
