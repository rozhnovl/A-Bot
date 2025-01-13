using WindowsInput.Native;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Task
{
	static public class MenuTaskExtension
	{
		static public MenuPathTask ClickMenuEntryByRegexPattern(
			this IUiElementProvider rootUIElement,
			Bot bot,
			params string[] menuEntryRegexPattern)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement.Element,
				ListMenuListPriorityEntryRegexPattern = menuEntryRegexPattern.Select(p => new[]{p}).ToArray(),
			};
		}

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

		static public MenuPathTask ClickMenuEntryByRegexPattern(
			this IUIElement rootUIElement,
			Bot bot,
			params string[] menuEntryRegexPattern)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement,
				ListMenuListPriorityEntryRegexPattern = menuEntryRegexPattern.Select(p => new[] { p }).ToArray(),
			};
		}

		public static MenuPathTask ClickWithModifier(this IUIElement element, Bot bot, params VirtualKeyCode[] modifiers)
		{
			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = element,
				ModifierKeys = modifiers,
			};
		}

		public static MultiClickTask ClickWithModifier(this IEnumerable<IUIElement> elements, params VirtualKeyCode[] modifiers)
		{
			return new MultiClickTask()
			{
				uiElements = elements.ToArray(),
				ModifierKeys = modifiers,
			};
		}

		static public MenuPathTask ClickMenuEntryWithModifierKey(
			this IUIElement rootUIElement,
			Bot bot,
			VirtualKeyCode? modifierKey)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement,
				ModifierKeys = modifierKey != null ? new[] { modifierKey.Value } : null,
			};
		}
	}
}