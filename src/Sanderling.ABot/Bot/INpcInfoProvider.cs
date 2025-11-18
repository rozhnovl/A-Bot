using Sanderling.ABot.Bot.Strategies;

namespace Sanderling.ABot.Bot;

public interface INpcInfoProvider
{
    double CalculateApproximateDps(IList<IOverviewEntry> entries);
    double CalculateApproximateDps(IOverviewProvider overviewProvider);
    int CalcTargetPriority(IOverviewEntry entry);
    bool IsOrbitBeacon(IOverviewEntry entry);
}
