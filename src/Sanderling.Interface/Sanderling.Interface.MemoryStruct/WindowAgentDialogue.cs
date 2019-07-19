using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowAgentDialogue : WindowAgent, IWindowAgentDialogue, IWindowAgent, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IWindowAgentPane LeftPane
		{
			get;
			set;
		}

		public IWindowAgentPane RightPane
		{
			get;
			set;
		}

		public WindowAgentDialogue()
		{
		}

		public WindowAgentDialogue(WindowAgent @base)
			: base(@base)
		{
		}
	}
}
