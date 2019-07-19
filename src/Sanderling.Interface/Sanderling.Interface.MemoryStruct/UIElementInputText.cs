using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class UIElementInputText : UIElementText, IUIElementInputText, IUIElementText, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public UIElementInputText(IUIElement @base, string label = null)
			: base(@base, label)
		{
		}

		public UIElementInputText(IUIElementText @base)
			: this(@base, @base?.Text)
		{
		}

		public UIElementInputText()
		{
		}
	}
}
