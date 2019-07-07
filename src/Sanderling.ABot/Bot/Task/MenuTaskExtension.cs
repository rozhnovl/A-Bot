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
		
		public static MenuPathTask ClickWithModifier(this IUIElement element, Bot bot, VirtualKeyCode? modifier)
		{
			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = element,
				ModifierKey = modifier,
			};
		}

		static public MenuPathTask ClickMenuEntryWithModifierKey(
			this IUIElement rootUIElement,
			Bot bot,
			VirtualKeyCode modifierKey)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement,
				ModifierKey = modifierKey,
			};
		}
	}
}