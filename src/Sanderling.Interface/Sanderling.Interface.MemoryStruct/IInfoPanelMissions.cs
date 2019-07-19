using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanelMissions : IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		IUIElementText[] ListMissionButton
		{
			get;
		}
	}
}
