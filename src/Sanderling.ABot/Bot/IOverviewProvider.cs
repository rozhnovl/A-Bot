using System.Collections.Generic;

namespace Sanderling.ABot.Bot
{
	public interface IOverviewProvider
	{
		IList<IOverviewEntry> Entries { get; }
	}
}