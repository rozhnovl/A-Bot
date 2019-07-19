using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowProbeScanner : Window, IWindowProbeScanner, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IListViewAndControl ScanResultView
		{
			get;
			set;
		}

		public WindowProbeScanner(IWindow @base)
			: base(@base)
		{
		}

		public WindowProbeScanner()
		{
		}
	}
}
