using System;
using Bib3;
using BotEngine.Common;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.Parse;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using WindowsInput.Native;
using BotEngine.Motor;
using JetBrains.Annotations;

namespace Sanderling.ABot.Bot
{
	static public class BotExtension
	{
		private static readonly EWarTypeEnum[][] listEWarPriorityGroup = new[]
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
			Interface.MemoryStruct.IOverviewEntry entry) =>
			AttackPriorityIndexForOverviewEntryEWar(bot?.OverviewMemory?.SetEWarTypeFromOverviewEntry(entry));

		static public bool ShouldBeIncludedInStepOutput(this IBotTask task) =>
			(task?.ContainsEffect() ?? false) || task is DiagnosticTask;

		static public bool LastContainsEffect(this IEnumerable<IBotTask> listTask) =>
			listTask?.LastOrDefault()?.ContainsEffect() ?? false;

		static public IEnumerable<MotionRecommendation> ApplicableEffects(this IBotTask task) =>
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
					?.Where(label => label?.Region?.Min0 < tooltipHorizontalCenter && tooltipHorizontalCenter < label?.Region?.Max0);

			return
				setLabelIntersectingHorizontalCenter
					?.OrderByCenterVerticalDown()?.FirstOrDefault();
		}

		[Pure]

		static public ISerializableBotTask ClickTask(this IUIElement element)
		{
			return new BotTask("Click on " + element)
			{
				ClientActions = new[] { element.MouseClick(MouseButtonIdEnum.Left).AsRecommendation() }
			};
		}
		[Pure]

		static public ISerializableBotTask DoubleClickTask(this IUIElement element)
		{
			return new BotTask("Double click on " + element)
			{
				ClientActions = new[] { element.MouseDoubleClick(MouseButtonIdEnum.Left).AsRecommendation() }
			};
		}

		public static ISerializableBotTask DragElementTo(this IUIElement from, IUIElement to, VirtualKeyCode? modifier = null)
		{
			return new BotTask(nameof(DragElementTo))
			{
				ClientActions = new[]
				{
					from.MouseClick(MouseButtonIdEnum.Left).AsRecommendation(),
					modifier?.KeyDown()?.AsRecommendation(),
					from.MouseDragAndDropOn(to.RegionInteraction, MouseButtonIdEnum.Left)
						.AsRecommendation(),
					modifier?.KeyUp()?.AsRecommendation(),
				}
			};
		}

		static public bool ShouldBeActivePermanent(this ShipUIModuleButton module, Bot bot)
		{
			throw new NotImplementedException();
			//System.Diagnostics.Debug.WriteLine($"Checking module {module.TooltipLast}" +
			//	$" IsHardener: {module?.TooltipLast?.Value?.IsHardener}" +
			//                                   $" IsActive: {module.IsActive}" +
			//                                   $" RampActive: {module.RampActive}" +
			//	$" ;RegionX: {module.Region?.Min0};RegionY: {module.Region?.Min1}");

			//if(module?.TooltipLast?.Value?.IsHardener?? false)
			//	return true;

			//return
			//	bot?.ConfigSerialAndStruct.Value?.ModuleActivePermanentSetTitlePattern
			//		?.Any(activePermanentTitlePattern =>
			//			module?.TooltipLast?.Value?.TitleElementText()?.Text
			//				?.RegexMatchSuccessIgnoreCase(activePermanentTitlePattern) ?? false) ?? false;
		}
	}
}
