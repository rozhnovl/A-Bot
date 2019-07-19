using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class Inventory : UIElement, IInventory, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IListViewAndControl ListView
		{
			get;
			set;
		}

		public Inventory()
			: this(null)
		{
		}

		public Inventory(IUIElement @base)
			: base(@base)
		{
		}
	}
}
