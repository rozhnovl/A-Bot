using System.Diagnostics;
using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class UIElementText : UIElement, IUIElementText, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public string Text
		{
			get;
			set;
		}

		public UIElementText(IUIElement @base, string text = null)
			: base(@base)
		{
			Text = text;
		}

		public UIElementText(IUIElementText @base)
			: this(@base, @base?.Text)
		{
		}

		public UIElementText()
		{
		}

		public override string ToString() => Text;
	}
}
