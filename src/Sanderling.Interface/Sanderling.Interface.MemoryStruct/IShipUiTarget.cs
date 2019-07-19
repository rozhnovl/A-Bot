using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUiTarget : IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		IUIElementText[] LabelText
		{
			get;
		}

		IShipHitpointsAndEnergy Hitpoints
		{
			get;
		}

		ShipUiTargetAssignedGroup[] Assigned
		{
			get;
		}
	}
}
