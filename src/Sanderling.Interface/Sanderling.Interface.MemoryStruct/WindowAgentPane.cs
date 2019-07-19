using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowAgentPane : UIElement, IWindowAgentPane, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public string Html
		{
			get;
			set;
		}

		public WindowAgentPane()
		{
		}

		public WindowAgentPane(IUIElement @base)
			: base(@base)
		{
		}
	}
}
