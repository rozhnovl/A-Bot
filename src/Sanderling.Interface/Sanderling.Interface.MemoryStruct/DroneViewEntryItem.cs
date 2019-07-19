using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class DroneViewEntryItem : DroneViewEntry, IDroneViewEntryItem, IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		public IShipHitpointsAndEnergy Hitpoints
		{
			get;
			set;
		}

		public DroneViewEntryItem(IListEntry @base)
			: base(@base)
		{
		}

		public DroneViewEntryItem()
		{
		}
	}
}
