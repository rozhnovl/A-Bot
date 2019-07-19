using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowStation : Window, IWindowStation, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IUIElementText[] AboveServicesLabel
		{
			get;
			set;
		}

		public IUIElement UndockButton
		{
			get;
			set;
		}

		public ISprite[] ServiceButton
		{
			get;
			set;
		}

		public LobbyAgentEntry[] AgentEntry
		{
			get;
			set;
		}

		public IUIElementText[] AgentEntryHeader
		{
			get;
			set;
		}

		public WindowStation(IWindow @base)
			: base(@base)
		{
		}

		public WindowStation()
		{
		}
	}
}
