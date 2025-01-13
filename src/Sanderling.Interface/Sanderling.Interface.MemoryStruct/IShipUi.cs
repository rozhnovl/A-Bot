using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUi : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IUIElement Center
		{
			get;
		}

		IShipUIIndication Indication
		{
			get;
		}

		IShipHitpointsAndEnergy HitpointsAndEnergy
		{
			get;
		}

		IUIElementText SpeedLabel
		{
			get;
		}

		ShipUiEWarElement[] EWarElement
		{
			get;
		}

		IUIElement ButtonSpeed0
		{
			get;
		}

		IUIElement ButtonSpeedMax
		{
			get;
		}
		
		public List<ShipUIModuleButton> ModuleButtons { get; }
		public ModuleButtonsRows ModuleButtonsRows { get; }

		IUIElementText[] Readout
		{
			get;
		}

		long? SpeedMilli
		{
			get;
		}

		IShipUiTimer[] Timer
		{
			get;
		}

		ISquadronsUI SquadronsUI
		{
			get;
		}
	}
}
