using BotEngine;
using System;

namespace Sanderling.Interface.MemoryStruct
{
	public class ShipUi : Container, IShipUi, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ICloneable
	{
		public IUIElement Center
		{
			get;
			set;
		}

		public IContainer Indication
		{
			get;
			set;
		}

		public IShipHitpointsAndEnergy HitpointsAndEnergy
		{
			get;
			set;
		}

		public IUIElementText SpeedLabel
		{
			get;
			set;
		}

		public ShipUiEWarElement[] EWarElement
		{
			get;
			set;
		}

		public IUIElement ButtonSpeed0
		{
			get;
			set;
		}

		public IUIElement ButtonSpeedMax
		{
			get;
			set;
		}

		public IShipUiModule[] Module
		{
			get;
			set;
		}

		public IUIElementText[] Readout
		{
			get;
			set;
		}

		public long? SpeedMilli
		{
			get;
			set;
		}

		public IShipUiTimer[] Timer
		{
			get;
			set;
		}

		public ISquadronsUI SquadronsUI
		{
			get;
			set;
		}

		public ShipUi()
		{
		}

		public ShipUi(IUIElement @base)
			: base(@base)
		{
		}

		public ShipUi Copy()
		{
			return this.CopyByPolicyMemoryMeasurement();
		}

		public object Clone()
		{
			return Copy();
		}
	}
}
