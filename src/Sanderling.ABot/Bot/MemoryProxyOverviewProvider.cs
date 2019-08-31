using System.Collections.Generic;
using System.Linq;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyOverviewProvider : IOverviewProvider
	{
		public MemoryProxyOverviewProvider(Bot bot)
		{
			Entries = bot.MemoryMeasurementAtTime.Value.WindowOverview.Single().ListView.Entry
				.Select(e => new MemoryProxyOverviewEntry(e, bot)).ToList<IOverviewEntry>();
		}

		public IList<IOverviewEntry> Entries { get; }
	}
}