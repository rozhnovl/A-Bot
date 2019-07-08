using System;
using Bib3;
using BotEngine.Common;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.Parse;
using System.Collections.Generic;
using System.Linq;
using BotEngine.Motor;
using JetBrains.Annotations;

namespace Sanderling.ABot.Bot
{
	static public class BotExtension
	{
		static readonly EWarTypeEnum[][] listEWarPriorityGroup = new[]
		{
			new[] { EWarTypeEnum.ECM },
			new[] { EWarTypeEnum.Web},
			new[] { EWarTypeEnum.WarpDisrupt, EWarTypeEnum.WarpScramble },
		};

		static public int AttackPriorityIndexForOverviewEntryEWar(IEnumerable<EWarTypeEnum> setEWar)
		{
			var setEWarRendered = setEWar?.ToArray();

			return
				listEWarPriorityGroup.FirstIndexOrNull(priorityGroup => priorityGroup.ContainsAny(setEWarRendered)) ??
				(listEWarPriorityGroup.Length + (0 < setEWarRendered?.Length ? 0 : 1));
		}

		static public int AttackPriorityIndex(
			this Bot bot,
			Sanderling.Parse.IOverviewEntry entry) =>
			AttackPriorityIndexForOverviewEntryEWar(bot?.OverviewMemory?.SetEWarTypeFromOverviewEntry(entry));

		static public bool ShouldBeIncludedInStepOutput(this IBotTask task) =>
			(task?.ContainsEffect() ?? false) || task is DiagnosticTask;

		static public bool LastContainsEffect(this IEnumerable<IBotTask> listTask) =>
			listTask?.LastOrDefault()?.ContainsEffect() ?? false;

		static public IEnumerable<MotionParam> ApplicableEffects(this IBotTask task) =>
			task?.ClientActions?.WhereNotDefault();

		static public bool ContainsEffect(this IBotTask task) =>
			0 < task?.ApplicableEffects()?.Count();

		static public IEnumerable<IBotTask[]> TakeSubsequenceWhileUnwantedInferenceRuledOut(this IEnumerable<IBotTask[]> listTaskPath) =>
			listTaskPath
			?.EnumerateSubsequencesStartingWithFirstElement()
			?.OrderBy(subsequenceTaskPath => 1 == subsequenceTaskPath?.Count(BotExtension.LastContainsEffect))
			?.LastOrDefault();

		static public IUIElementText TitleElementText(this IModuleButtonTooltip tooltip)
		{
			var tooltipHorizontalCenter = tooltip?.RegionCenter()?.A;

			var setLabelIntersectingHorizontalCenter =
				tooltip?.LabelText
					?.Where(label => label?.Region.Min0 < tooltipHorizontalCenter && tooltipHorizontalCenter < label?.Region.Max0);

			return
				setLabelIntersectingHorizontalCenter
					?.OrderByCenterVerticalDown()?.FirstOrDefault();
		}

		[Pure]

		static public IBotTask ClickTask(this IUIElement element)
		{
			return new BotTask()
			{
				ClientActions = new[] { element.MouseClick(MouseButtonIdEnum.Left) }
			};
		}
		[Pure]

		static public IBotTask DoubleClickTask(this IUIElement element)
		{
			return new BotTask()
			{
				ClientActions = new[] { element.MouseDoubleClick(MouseButtonIdEnum.Left) }
			};
		}

		static public bool ShouldBeActivePermanent(this Accumulation.IShipUiModule module, Bot bot)
		{
			System.Diagnostics.Debug.WriteLine($"Checking module {module.TooltipLast}" +
				$" IsHardener: {module?.TooltipLast?.Value?.IsHardener}" +
			                                   $" IsActive: {module.IsActive(bot)}" +
			                                   $" RampActive: {module.RampActive}" +
				$" ;RegionX: {module.Region.Min0};RegionY: {module.Region.Min1}");
			
			if(module?.TooltipLast?.Value?.IsHardener?? false)
				return true;

			return
				bot?.ConfigSerialAndStruct.Value?.ModuleActivePermanentSetTitlePattern
					?.Any(activePermanentTitlePattern =>
						module?.TooltipLast?.Value?.TitleElementText()?.Text
							?.RegexMatchSuccessIgnoreCase(activePermanentTitlePattern) ?? false) ?? false;
		}
	}
}
