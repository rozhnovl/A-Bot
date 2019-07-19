using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowDroneView : Window, IWindowDroneView, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IListViewAndControl ListView
		{
			get;
			set;
		}

		public WindowDroneView(IWindow @base)
			: base(@base)
		{
		}

		public WindowDroneView()
		{
		}
	}
}
