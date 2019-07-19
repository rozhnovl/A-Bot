using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class InfoPanelMissions : InfoPanel, IInfoPanelMissions, IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		public IUIElementText[] ListMissionButton
		{
			get;
			set;
		}

		public InfoPanelMissions()
		{
		}

		public InfoPanelMissions(IInfoPanel @base)
			: base(@base)
		{
		}
	}
}
