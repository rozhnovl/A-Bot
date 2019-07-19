using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class InfoPanelSystem : InfoPanel, IInfoPanelSystem, IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		public IUIElement ListSurroundingsButton
		{
			get;
			set;
		}

		public InfoPanelSystem()
		{
		}

		public InfoPanelSystem(IInfoPanel @base)
			: base(@base)
		{
		}
	}
}
