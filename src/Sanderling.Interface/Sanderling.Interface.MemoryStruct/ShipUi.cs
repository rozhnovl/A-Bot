using BotEngine;
using System;

namespace Sanderling.Interface.MemoryStruct
{
	public class ShipUi : Container, IShipUi, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ICloneable
	{
		// Members marked [Obsolete] are never assigned by the Eve64 parser — see the note on IShipUi.
		[Obsolete("Not populated by the Eve64 parser — always null.")]
		public IUIElement Center
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null. Ship gauges live in HitpointsPercent (0–100 percent) and Capacitor.LevelFromPmarksPercent.")]
		public IShipHitpointsAndEnergy HitpointsAndEnergy
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship speed is not parsed yet).")]
		public IUIElementText SpeedLabel
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship EWar indicators are not parsed yet).")]
		public ShipUiEWarElement[] EWarElement
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null. Use StopButton.")]
		public IUIElement ButtonSpeed0
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null. Use MaxSpeedButton.")]
		public IUIElement ButtonSpeedMax
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null.")]
		public IUIElementText[] Readout
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship speed is not parsed yet).")]
		public long? SpeedMilli
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship timers are not parsed yet).")]
		public IShipUiTimer[] Timer
		{
			get;
			set;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (TODO in Eve64/Parser.cs).")]
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
		public string? ManeuverTarget { get; set; }
		public IUIElement UINode { get; set; }
	}
	public interface IShipUIIndication
	{
		ShipManeuverType? ManeuverType { get; }
		public string? ManeuverTarget { get; }
	}
	public class ShipUIModuleButton
	{
		public IUIElement UINode { get; set; }
		public IUIElement SlotUINode { get; set; }
		/// <summary>Physical EVE rack from ShipSlot name: High, Medium or Low.</summary>
		public string Rack { get; set; }
		/// <summary>Zero-based physical slot number, used for the F1..F8 hotkey mapping.</summary>
		public int? SlotIndex { get; set; }
		public bool? IsActive { get; set; }
		/// <summary>
		/// The client's own <c>isDeactivating</c> flag: the module was told to stop and is finishing
		/// its last cycle. Clicking it again in that state CANCELS the stop — which is how a shield
		/// booster got stuck on (Tiara Parvi, 2026-09-26). Null when the client did not expose it.
		/// </summary>
		public bool? IsDeactivating { get; set; }
		public bool IsHiliteVisible { get; set; }
		public bool IsBusy { get; set; }
		public int? RampRotationMilli { get; set; }
		public ModuleInfo? ModuleInfo { get; set; }

		/// <summary>
		/// Repeating launchers drop <c>ramp_active</c> between volleys; a click then toggles them off.
		/// Treat glow or the busy overlay as still running.
		/// </summary>
		public bool AppearsActive => IsActive == true || IsBusy || IsHiliteVisible;
	}

	public class ModuleInfo
	{
		public bool IsWeapon { get; set; }
		public int ModuleId { get; set; }
		/// <summary>Loaded charge/crystal type id, read from the module's main icon when present.</summary>
		public int? ChargeTypeId { get; set; }
		/// <summary>
		/// Charges loaded in ONE module of the group behind this button — the HUD's own <c>quantity</c>
		/// entry (a full LML II shows 53; seen live counting 53 → 51 → … → 2 → 53 on reload). Null when
		/// the client did not expose it; 0 is a real "launchers are empty".
		/// </summary>
		public int? ChargeQuantity { get; set; }
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
		KeepAtRange,
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
