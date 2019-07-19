using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IDroneViewEntryItem : IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		IShipHitpointsAndEnergy Hitpoints
		{
			get;
		}
	}
}
