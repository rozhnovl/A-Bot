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
	public class EnterTextTask : IBotTask
	{
		private readonly string text;

		public EnterTextTask(string text)
		{
			this.text = text;
		}

		public IEnumerable<IBotTask> Component { get; }

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				yield return new MotionParam()
				{
					TextEntry = text
				}.AsRecommendation();
			}
		}
	}

	public class MenuPathTask : ISerializableBotTask
	{
		public Bot Bot;

		public IUIElement RootUIElement;

		public string[][] ListMenuListPriorityEntryRegexPattern;
		public VirtualKeyCode[] ModifierKeys { get; set; }

		public IEnumerable<IBotTask> Component => null;

		private bool MenuOpenOnRootPossible()
		{
			var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

			var menu = memoryMeasurement?.Menu?.FirstOrDefault();

			if (null == menu)
				return false;

			var overviewEntry = RootUIElement as Sanderling.Parse.IOverviewEntry;

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

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				var listMenu = Bot?.MemoryMeasurementAtTime?.Value?.Menu?.ToArray();

				var rootUIElement = RootUIElement;

				if (null == rootUIElement)
					yield break;

				IMenuEntry menuEntryToContinue = null;

				var mouseClickOnRootAge = Bot?.MouseClickLastAgeStepCountFromUIElement(RootUIElement);

				if (ListMenuListPriorityEntryRegexPattern == null)
				{
					foreach (var mod in ModifierKeys.EmptyIfNull())
						yield return mod.KeyDown()?.AsRecommendation();
					yield return RootUIElement?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left)?.AsRecommendation();
					foreach (var mod in ModifierKeys.EmptyIfNull())
						yield return mod.KeyUp()?.AsRecommendation();
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
				if (ModifierKeys != null)
				{
					foreach (var mod in ModifierKeys.EmptyIfNull())
						yield return mod.KeyDown()?.AsRecommendation();
					yield return (menuEntryToContinue ?? RootUIElement)?.MouseClick(buttonToUse)?.AsRecommendation();
					foreach (var mod in ModifierKeys.EmptyIfNull())
						yield return mod.KeyUp()?.AsRecommendation();
				}
				else
				{
					yield return (menuEntryToContinue ?? RootUIElement)?.MouseClick(buttonToUse)?.AsRecommendation();
				}

			}
		}

		public string ToJson() => ToString();

		public override string ToString()
		{
			return
				$"{nameof(MenuPathTask)}[{RootUIElement}@{string.Join("=>", ListMenuListPriorityEntryRegexPattern?.Select(a => string.Join("|", a)) ?? new string[0])}";
		}
	}
}