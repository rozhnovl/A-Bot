using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUiTarget : IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		string[] LabelText
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
		public int? Distance { get; }
	}
}
