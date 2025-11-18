using Microsoft.Extensions.Logging;

namespace Sanderling.ABot.Bot.Strategies;

/// <summary>
/// Factory for creating strategy states with proper dependency injection
/// </summary>
public class StateFactory(
    ILogger<AbyssalFightState> abyssalFightLogger,
    INpcInfoProvider npcInfoProvider,
    IProviderFactory providerFactory) : IStateFactory
{
    public AbyssalFightState CreateAbyssalFightState()
    {
        return new AbyssalFightState(abyssalFightLogger, npcInfoProvider, providerFactory);
    }

    public ReloadAtStationState CreateReloadAtStationState(params (string, int)[] keptInventoryItems)
    {
        return new ReloadAtStationState(keptInventoryItems);
    }

    public WarpToBookmarkInSystemState CreateWarpToBookmarkInSystemState(string bookmarkName)
    {
        return new WarpToBookmarkInSystemState(bookmarkName);
    }

    public ShipCheckingState CreateShipCheckingState()
    {
        return new ShipCheckingState();
    }

    public WaitForCommandState CreateWaitForCommandState()
    {
        return new WaitForCommandState();
    }
}
