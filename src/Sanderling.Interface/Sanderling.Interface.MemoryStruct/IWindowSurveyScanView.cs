using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowSurveyScanView : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IListViewAndControl ListView
		{
			get;
		}
	}
}
