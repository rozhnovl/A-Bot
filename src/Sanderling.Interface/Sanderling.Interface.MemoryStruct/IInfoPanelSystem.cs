using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanelSystem : IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		IUIElement ListSurroundingsButton
		{
			get;
		}
	}
}
