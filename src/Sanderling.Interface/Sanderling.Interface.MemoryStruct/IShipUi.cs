using BotEngine;
using System;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	// NOTE ON LEGACY MEMBERS: this interface predates the Eve64 memory reader. Members marked
	// [Obsolete] below are NEVER populated by the Eve64 parser (Eve64/Parser.cs) — reading them
	// silently yields null, which is exactly how the first live Hawk takeover died (tank logic
	// read HitpointsAndEnergy and NRE'd every combat tick). If the parser learns one of these,
	// remove its attribute. The live, populated surface is: Indication, ModuleButtons(+Rows),
	// HitpointsPercent, Capacitor, plus StopButton/MaxSpeedButton/OffensiveBuffButtons/HeatGauges
	// on the concrete ShipUi class.
	public interface IShipUi : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		[Obsolete("Not populated by the Eve64 parser — always null.")]
		IUIElement Center
		{
			get;
		}

		IShipUIIndication Indication
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null. Ship gauges live in HitpointsPercent (0–100 percent) and Capacitor.LevelFromPmarksPercent.")]
		IShipHitpointsAndEnergy HitpointsAndEnergy
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship speed is not parsed yet).")]
		IUIElementText SpeedLabel
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship EWar indicators are not parsed yet).")]
		ShipUiEWarElement[] EWarElement
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null. Use ShipUi.StopButton.")]
		IUIElement ButtonSpeed0
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null. Use ShipUi.MaxSpeedButton.")]
		IUIElement ButtonSpeedMax
		{
			get;
		}

		public List<ShipUIModuleButton> ModuleButtons { get; }
		public ModuleButtonsRows ModuleButtonsRows { get; }

		/// <summary>Shield/armor/structure as 0–100 percent, as the Eve64 parser fills them
		/// (the legacy <see cref="HitpointsAndEnergy"/> is NOT populated by that parser).</summary>
		public Hitpoints HitpointsPercent { get; }
		/// <summary>Capacitor gauge; <see cref="ShipUICapacitor.LevelFromPmarksPercent"/> is 0–100.</summary>
		public ShipUICapacitor Capacitor { get; }

		[Obsolete("Not populated by the Eve64 parser — always null.")]
		IUIElementText[] Readout
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship speed is not parsed yet).")]
		long? SpeedMilli
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (ship timers are not parsed yet).")]
		IShipUiTimer[] Timer
		{
			get;
		}

		[Obsolete("Not populated by the Eve64 parser — always null (TODO in Eve64/Parser.cs).")]
		ISquadronsUI SquadronsUI
		{
			get;
		}
	}
}
