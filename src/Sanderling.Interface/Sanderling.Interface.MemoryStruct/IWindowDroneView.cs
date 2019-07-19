using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowDroneView : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IListViewAndControl ListView
		{
			get;
		}
	}
}
