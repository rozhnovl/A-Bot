namespace Sanderling.Interface.MemoryStruct
{
	public class MarketMyOrders : Container
	{
		public IListViewAndControl SellOrderView;

		public IListViewAndControl BuyOrderView;

		public MarketMyOrders(IContainer @base)
			: base(@base)
		{
		}

		public MarketMyOrders()
		{
		}
	}
}
