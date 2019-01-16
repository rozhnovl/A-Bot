﻿using System.Collections.Generic;
using Sanderling.Motor;
using Sanderling.Interface.MemoryStruct;
using System.Linq;
using BotEngine.Common;
using Bib3.Geometrik;
using System;
using WindowsInput.Native;
using Bib3;

namespace Sanderling.ABot.Bot.Task
{
	public class HotkeyTask : IBotTask
	{
		public HotkeyTask(VirtualKeyCode key, params VirtualKeyCode[] mods)
		{
			Key = key;
			Modifiers = mods;
		}
		public VirtualKeyCode[] Modifiers;
		public VirtualKeyCode Key;

		public IEnumerable<IBotTask> Component => null;
		
		public IEnumerable<MotionParam> Effects
		{
			get
			{
				foreach (var m in Modifiers)
					yield return m.KeyDown();
				yield return Key.KeyboardPress();
				foreach (var m in Modifiers.Reverse())
					yield return m.KeyUp();
			}
		}
	}

	public class MenuPathTask : IBotTask
	{
		public Bot Bot;

		public IUIElement RootUIElement;

		public string[][] ListMenuListPriorityEntryRegexPattern;

		public VirtualKeyCode ModifierKey;

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

			if (regionExpected.Region.Intersection(menu.Region.WithSizeExpandedPivotAtCenter(10)).IsEmpty())
				return false;

			return true;
		}

		public IEnumerable<MotionParam> Effects
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
					yield return ModifierKey.KeyDown();
					yield return RootUIElement?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left);
					yield return ModifierKey.KeyUp();
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

				yield return
					menuEntryToContinue?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Left) ??
					RootUIElement?.MouseClick(BotEngine.Motor.MouseButtonIdEnum.Right);
			}
		}
	}

	static public class MenuTaskExtension
	{
		static public MenuPathTask ClickMenuEntryByRegexPattern(
			this IUIElement rootUIElement,
			Bot bot,
			string menuEntryRegexPattern)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement,
				ListMenuListPriorityEntryRegexPattern = new[] { new[] { menuEntryRegexPattern } },
			};
		}

		public static MenuPathTask ClickWithModifier(this IUIElement element, Bot bot, VirtualKeyCode modifier)
		{
			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = element,
				ModifierKey = modifier,
			};
		}
	}
}
