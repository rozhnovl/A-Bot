using System;
using WindowsInput.Native;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyOverviewEntry : SimpleOverviewEntry
	{
		private readonly Bot bot;
		private readonly Interface.MemoryStruct.IOverviewEntry memoryOverviewEntry;

		public MemoryProxyOverviewEntry(Interface.MemoryStruct.IOverviewEntry overviewEntry, Bot bot)
			: base(overviewEntry.ObjectType, overviewEntry.ObjectName, overviewEntry.IconSpriteColorPercent?.IsRed()??false,
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

		[Newtonsoft.Json.JsonIgnore]
		public override IUIElement? SelectElement => memoryOverviewEntry.UiElement;

		public override string? Tag
		{
			get
			{
				var cells = memoryOverviewEntry.CellsTexts;
				if (cells == null) return null;
				foreach (var (header, text) in cells)
				{
					if (!string.Equals(header?.Trim(), "Tag", StringComparison.OrdinalIgnoreCase)) continue;
					var tag = text?.Trim();
					return string.IsNullOrEmpty(tag) ? null : tag;
				}
				return null;
			}
		}

		public override OverviewWindowEntryCommonIndications CommonIndications => memoryOverviewEntry.CommonIndications;

		public override ISerializableBotTask GetApproachTask()
		{
			return memoryOverviewEntry.UiElement.ClickWithModifier(bot, VirtualKeyCode.VK_Q);
		}
	}
}