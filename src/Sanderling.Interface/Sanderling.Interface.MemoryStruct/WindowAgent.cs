using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowAgent : Window, IWindowAgent, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public WindowAgent()
		{
		}

		public WindowAgent(IWindow @base)
			: base(@base)
		{
		}
	}
}
