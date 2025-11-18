namespace Sanderling.ABot.Bot.Strategies;

/// <summary>
/// Factory interface for creating strategy states with configuration parameters
/// </summary>
public interface IStateFactory
{
    AbyssalFightState CreateAbyssalFightState();
    ReloadAtStationState CreateReloadAtStationState(params (string, int)[] keptInventoryItems);
    WarpToBookmarkInSystemState CreateWarpToBookmarkInSystemState(string bookmarkName);
    ShipCheckingState CreateShipCheckingState();
    WaitForCommandState CreateWaitForCommandState();
}
