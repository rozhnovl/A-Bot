using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BotEngine.Common;
using BotEngine.Motor;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public abstract class SimpleBotTask : IBotTask
	{
		protected readonly Bot bot;

		protected SimpleBotTask(Bot bot)
		{
			this.bot = bot;
		}

		public virtual IEnumerable<IBotTask> Component { get; } = null;
		public virtual IEnumerable<MotionParam> ClientActions { get; } = null;
		
	}

	public class LootTask : SimpleBotTask
	{
		public bool HasWreckToLoot = true;

		public override IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurementAtTime = bot?.MemoryMeasurementAtTime;
				var memoryMeasurementAccu = bot?.MemoryMeasurementAccu;
				var memoryMeasurement = memoryMeasurementAtTime?.Value;

				var OverviewTabActive = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.PresetTab
					?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 1500)?.FirstOrDefault();
				var OverviewTabLoot = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.PresetTab
					?.Where(tab => tab?.Label.Text.RegexMatchSuccess(Config.LootTabName) ?? false)
					.FirstOrDefault();

				var propmod =
					memoryMeasurementAccu?.ShipUiModule?.Where(module =>
						module?.TooltipLast?.Value?.IsAfterburner ?? false);

				// switch tabs for wrecks
				if (OverviewTabLoot != OverviewTabActive)
					yield return new BotTask {ClientActions = new[] {OverviewTabLoot?.MouseClick(MouseButtonIdEnum.Left)}};

				var listOverviewCommanderWreck = memoryMeasurement?.WindowOverview?.FirstOrDefault()?.ListView?.Entry
					?.Where(entry => entry?.Type?.Contains("Commander") ?? true).ToList()
					.FirstOrDefault();

				// if there is a officer wreck -> loot it
				if (listOverviewCommanderWreck != null)
				{
					//bot?.SetCommanderWreck(true);

					var isApproaching = memoryMeasurement?.ShipUi?.Indication?.LabelText?.Any(text =>
						                    text?.Text?.RegexMatchSuccess("approaching", RegexOptions.IgnoreCase) ??
						                    false) ?? false;
					var LootButton = memoryMeasurement?.WindowInventory?[0]?.ButtonText
						?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Loot All"));

					if (isApproaching == false && listOverviewCommanderWreck.DistanceMin > 2000)
						yield return listOverviewCommanderWreck.ClickMenuEntryByRegexPattern(bot, "open cargo");

					//if ((isApproaching == true) && (ApproachDistance < 12000))
					//	yield return bot?.DeactivateModule(propmod);

					if (LootButton != null)
						yield return new BotTask { ClientActions = new[] {LootButton?.MouseClick(MouseButtonIdEnum.Left)}};
				}
				else
				{
					HasWreckToLoot = false;
					//bot?.SetCommanderWreck(false);
				}
			}
		}

		public LootTask(Bot bot) : base(bot)
		{
		}
	}

	public static class Config
	{
		public static string LootTabName { get; } = "Loot";
		public static string CombatTabName { get; } = "General";
	}
}