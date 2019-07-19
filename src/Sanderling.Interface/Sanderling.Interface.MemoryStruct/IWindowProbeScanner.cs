using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowProbeScanner : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IListViewAndControl ScanResultView
		{
			get;
		}
	}
}
