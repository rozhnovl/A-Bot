namespace Sanderling.Interface.MemoryStruct
{
	public class WindowFittingMgmt : Window
	{
		public IListViewAndControl FittingView;

		public WindowFittingMgmt(IWindow @base)
			: base(@base)
		{
		}

		public WindowFittingMgmt()
		{
		}
	}
}
