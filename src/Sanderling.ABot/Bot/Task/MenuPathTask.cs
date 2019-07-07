using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using Bib3;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class MenuPathTask : IBotTask
	{
		public Bot Bot;

		public IUIElement RootUIElement;

		public string[][] ListMenuListPriorityEntryRegexPattern;
		public VirtualKeyCode? ModifierKey { get; set; }

		public IEnumerable<IBotTask> Component => null;

		bool MenuOpenOnRootPossible()
		{
			var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

			var menu = memoryMeasurement?.Menu?.FirstOrDefault();

			if (null == menu)
				return false;

			var overviewEntry = RootUIElement as IOverviewEntry;

			IUIElement regionExpected = RootUIElement;

			if (null != overviewEntry)
			{
				regionExpected = memoryMeasurement?.WindowOverview?.FirstOrDefault();

				if (!(overviewEntry.IsSelected ?? false))
					return false;

				if (!(menu?.Entry?.Any(menuEntry => menuEntry?.Text?.RegexMatchSuccessIgnoreCase(@"remove.*overview") ?? false) ?? false))
					return false;
			}

			//if (regionExpected.Region.Intersection(menu.Region.WithSizeExpandedPivotAtCenter(10)).IsEmpty())
			//	return false;

			return true;
		}

		public IEnumerable<MotionParam> ClientActions
		{
			get
			{
				var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

				var listMenu = memoryMeasurement?.Menu?.ToArray();

				var rootUIElement = RootUIElement;

				if (null == rootUIElement)
					yield break;

				IMenuEntry menuEntryToContinue = null;

				var mouseClickOnRootAge = Bot?.MouseClickLastAgeStepCountFromUIElement(RootUIElement);

				if (ListMenuListPriorityEntryRegexPattern == null)
				{
					yield return ModifierKey?.KeyDown();
					yield return RootUIElement?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left);
					yield return ModifierKey?.KeyUp();
					yield break;
				}

				if (MenuOpenOnRootPossible() && mouseClickOnRootAge <= listMenu?.Length)
				{
					var levelCount = Math.Min(ListMenuListPriorityEntryRegexPattern?.Length ?? 0, listMenu?.Length ?? 0);

					for (int levelIndex = 0; levelIndex < levelCount; levelIndex++)
					{
						var listPriorityEntryRegexPattern = ListMenuListPriorityEntryRegexPattern[levelIndex];

						var menuEntry =
							listPriorityEntryRegexPattern
								?.WhereNotDefault()
								?.Select(priorityEntryRegexPattern =>
									listMenu[levelIndex]?.Entry
										?.FirstOrDefault(c => c?.Text?.RegexMatchSuccessIgnoreCase(priorityEntryRegexPattern) ?? false))
								?.WhereNotDefault()?.FirstOrDefault();

						if (null == menuEntry)
							break;

						menuEntryToContinue = menuEntry;

						if (!(menuEntry?.HighlightVisible ?? false))
							break;
					}
				}

				var buttonToUse = ListMenuListPriorityEntryRegexPattern.IsNullOrEmpty() || menuEntryToContinue != null
					? BotEngine.Motor.MouseButtonIdEnum.Left
					: BotEngine.Motor.MouseButtonIdEnum.Right;
				if (ModifierKey != null)
				{
					yield return ModifierKey.Value.KeyDown();
					yield return (menuEntryToContinue ?? RootUIElement)?.MouseClick(buttonToUse);
					yield return ModifierKey.Value.KeyUp();
				}
				else
				{
					yield return (menuEntryToContinue ?? RootUIElement)?.MouseClick(buttonToUse);
				}

			}
		}
	}
}