namespace Sanderling.Interface.MemoryStruct
{
	public class MarketItemTypeDetailsMarketData : UIElement
	{
		public IListViewAndControl SellerView;

		public IListViewAndControl BuyerView;

		public MarketItemTypeDetailsMarketData()
		{
		}

		public MarketItemTypeDetailsMarketData(IUIElement @base)
			: base(@base)
		{
		}
	}
}
