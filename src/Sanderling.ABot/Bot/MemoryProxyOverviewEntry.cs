using WindowsInput.Native;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyOverviewEntry : SimpleOverviewEntry
	{
		private readonly Bot bot;
		private readonly Interface.MemoryStruct.IOverviewEntry memoryOverviewEntry;

		public MemoryProxyOverviewEntry(Interface.MemoryStruct.IOverviewEntry overviewEntry, Bot bot)
			: base(overviewEntry.ObjectType, overviewEntry.ObjectName, overviewEntry.IconSpriteColorPercent.RPercent > 50 && overviewEntry.IconSpriteColorPercent.GPercent < 100,
				(int) overviewEntry.ObjectDistanceInMeters, overviewEntry.CommonIndications.Targeting, overviewEntry.CommonIndications.TargetedByMe, overviewEntry.Id)
		{
			this.memoryOverviewEntry = overviewEntry;
			this.bot = bot;
		}

		public override ISerializableBotTask ClickMenuEntryByRegexPattern(string path1, string path2)
		{
			return memoryOverviewEntry.UiElement.ClickMenuEntryByRegexPattern(bot, path1, path2);
		}

		public override ISerializableBotTask GetSelectTask()
		{
			return memoryOverviewEntry.UiElement.ClickWithModifier(bot, VirtualKeyCode.CONTROL);
		}
	}
}