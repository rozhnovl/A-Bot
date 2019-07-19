namespace Sanderling.Interface.MemoryStruct
{
	public class WindowRegionalMarket : Window
	{
		public TabGroup LeftTabGroup;

		public IListViewAndControl QuickbarView;

		public TabGroup RightTabGroup;

		public MarketItemTypeDetails SelectedItemTypeDetails;

		public MarketMyOrders MyOrders;

		public WindowRegionalMarket(IWindow @base)
			: base(@base)
		{
		}

		public WindowRegionalMarket()
		{
		}
	}
}
