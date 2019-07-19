using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowAgentBrowser : WindowAgent, IWindowAgent, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public WindowAgentBrowser(WindowAgent @base)
			: base(@base)
		{
		}

		public WindowAgentBrowser()
		{
		}
	}
}
