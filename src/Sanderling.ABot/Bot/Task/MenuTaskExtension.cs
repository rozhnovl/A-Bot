using WindowsInput.Native;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Task
{
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

		static public MenuPathTask ClickMenuEntryByRegexPattern(
			this IUIElement rootUIElement,
			Bot bot,
			string firstMenu, string secondMenu)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement,
				ListMenuListPriorityEntryRegexPattern = new[] { new[] { firstMenu }, new[] { secondMenu } },
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