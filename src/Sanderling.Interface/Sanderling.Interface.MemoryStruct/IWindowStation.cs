using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowStation : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IUIElementText[] AboveServicesLabel
		{
			get;
		}

		IUIElement UndockButton
		{
			get;
		}

		ISprite[] ServiceButton
		{
			get;
		}

		LobbyAgentEntry[] AgentEntry
		{
			get;
		}

		IUIElementText[] AgentEntryHeader
		{
			get;
		}
	}
}
