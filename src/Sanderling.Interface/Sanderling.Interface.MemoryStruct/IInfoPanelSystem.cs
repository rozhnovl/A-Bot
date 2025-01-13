using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanelContainer
	{
		public IInfoPanelLocationInfo LocationInfo { get; }
		//public IInfoPanelSystem InfoPanelCurrentSystem { get;  }
		public IInfoPanelRoute InfoPanelRoute { get;  }
		public IInfoPanelMissions InfoPanelMissions { get;  }

	}
	public interface IInfoPanelLocationInfo
	{
		public IUIElement? ListSurroundingsButtonElement { get; }
		public string CurrentSolarSystemName { get; set; }
		public int SecurityStatusPercent { get; set; }
		public string? ExpandedContent { get; set; }
	}
	public interface IInfoPanelSystem : IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		IUIElement ListSurroundingsButton
		{
			get;
		}
	}
}