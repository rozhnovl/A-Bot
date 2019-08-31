using WindowsInput.Native;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyOverviewEntry : SimpleOverviewEntry
	{
		private readonly Bot bot;
		private readonly Sanderling.Parse.IOverviewEntry memoryOverviewEntry;

		public MemoryProxyOverviewEntry(Sanderling.Parse.IOverviewEntry overviewEntry, Bot bot)
			: base(overviewEntry.Type, overviewEntry.Name, overviewEntry.MainIconIsRed.Value,
				(int) overviewEntry.DistanceMax, overviewEntry.MeTargeted.Value, overviewEntry.MeActiveTarget.Value, overviewEntry.Id)
		{
			this.memoryOverviewEntry = overviewEntry;
			this.bot = bot;
		}

		public override ISerializableBotTask ClickMenuEntryByRegexPattern(string path1, string path2)
		{
			return memoryOverviewEntry.ClickMenuEntryByRegexPattern(bot, path1, path2);
		}

		public override ISerializableBotTask GetSelectTask()
		{
			return memoryOverviewEntry.ClickWithModifier(bot, VirtualKeyCode.CONTROL);
		}
	}
}