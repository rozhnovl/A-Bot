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
		public ShipUICapacitor Capacitor { get; set; }
		public Hitpoints HitpointsPercent { get; set; }
		public IShipUIIndication Indication { get; set; }
		public List<ShipUIModuleButton> ModuleButtons { get; set; }
		public ModuleButtonsRows ModuleButtonsRows { get; set; }
		public List<OffensiveBuffButton> OffensiveBuffButtons { get; set; }
		public IUIElement? StopButton { get; set; }
		public IUIElement? MaxSpeedButton { get; set; }
		public ShipUIHeatGauges? HeatGauges { get; set; }
	}

	public class ShipUIIndication: IShipUIIndication
	{
		public ShipManeuverType? ManeuverType { get; set; }
		public IUIElement UINode { get; set; }
	}
	public interface IShipUIIndication
	{
		ShipManeuverType? ManeuverType { get; }
	}
	public class ShipUIModuleButton
	{
		public IUIElement UINode { get; set; }
		public IUIElement SlotUINode { get; set; }
		public bool? IsActive { get; set; }
		public bool IsHiliteVisible { get; set; }
		public bool IsBusy { get; set; }
		public int? RampRotationMilli { get; set; }
		public ModuleInfo? ModuleInfo { get; set; }
	}

	public class ModuleInfo
	{
		public bool IsWeapon { get; set; }
		public int ModuleId { get; set; }
		public int ChargeCount { get; set; }
		public int MaxCharges { get; set; }
	}

	public class ShipUICapacitor
	{
		public IUIElement UINode { get; set; }
		public List<ShipUICapacitorPmark> Pmarks { get; set; }
		public int? LevelFromPmarksPercent { get; set; }
	}

	public class ShipUICapacitorPmark
	{
		public IUIElement UINode { get; set; }
		public ColorComponents? ColorPercent { get; set; }
	}

	public class ShipUIHeatGauges
	{
		public IUIElement UINode { get; set; }
		public List<ShipUIHeatGauge> Gauges { get; set; }
	}

	public class ShipUIHeatGauge
	{
		public IUIElement UINode { get; set; }
		public int? RotationPercent { get; set; }
		public int? HeatPercent { get; set; }
	}

	public class ColorComponents
	{
		public int APercent { get; set; }
		public int RPercent { get; set; }
		public int GPercent { get; set; }
		public int BPercent { get; set; }
	}
	public class Hitpoints
	{
		public int Structure { get; set; }
		public int Armor { get; set; }
		public int Shield { get; set; }
	}

	public enum ShipManeuverType
	{
		None,
		Warp,
		Jump,
		Orbit,
		Approach,
		Docked,
	}


	public class OffensiveBuffButton
	{
		public IUIElement UINode { get; set; }
		public string Name { get; set; }
	}

	public class ModuleButtonsRows
	{
		public List<ShipUIModuleButton> Top { get; set; }
		public List<ShipUIModuleButton> Middle { get; set; }
		public List<ShipUIModuleButton> Bottom { get; set; }
	}
}
