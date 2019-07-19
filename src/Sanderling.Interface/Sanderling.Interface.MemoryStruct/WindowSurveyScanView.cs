using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowSurveyScanView : Window, IWindowSurveyScanView, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IListViewAndControl ListView
		{
			get;
			set;
		}

		public WindowSurveyScanView(IWindow @base)
			: base(@base)
		{
		}

		public WindowSurveyScanView()
		{
		}
	}
}
