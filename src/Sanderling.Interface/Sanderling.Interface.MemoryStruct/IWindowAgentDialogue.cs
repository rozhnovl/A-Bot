using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowAgentDialogue : IWindowAgent, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IWindowAgentPane LeftPane
		{
			get;
		}

		IWindowAgentPane RightPane
		{
			get;
		}
	}
}
