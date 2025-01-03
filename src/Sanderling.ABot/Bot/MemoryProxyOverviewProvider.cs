using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Strategies;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyOverviewProvider : IOverviewProvider
	{
		public MemoryProxyOverviewProvider(Bot bot)
		{
			Entries = bot.MemoryMeasurementAtTime.Value.WindowOverview.Single().Entries
				.Select(e => new MemoryProxyOverviewEntry(e, bot)).ToList<IOverviewEntry>();
		}

		public IList<IOverviewEntry> Entries { get; }
		public int CalculateApproximateDps(NpcInfoProvider npcInfoProvider)
		{
			throw new System.NotImplementedException();
		}
	}
}