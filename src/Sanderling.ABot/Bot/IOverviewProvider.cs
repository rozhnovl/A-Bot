using System.Collections.Generic;
using Sanderling.ABot.Bot.Strategies;

namespace Sanderling.ABot.Bot
{
	public interface IOverviewProvider
	{
		IList<IOverviewEntry> Entries { get; }
		int CalculateApproximateDps(NpcInfoProvider npcInfoProvider);
	}
}