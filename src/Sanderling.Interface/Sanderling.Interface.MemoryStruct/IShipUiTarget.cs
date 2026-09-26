using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUiTarget : IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		string[] LabelText
		{
			get;
		}

		/// <summary>The TARGET's shield/armor/hull, in per-mille (0–1000) — populated by the Eve64 parser.</summary>
		IShipHitpointsAndEnergy Hitpoints
		{
			get;
		}

		[System.Obsolete("Not populated by the Eve64 parser — always null (assigned drone/weapon icons are not parsed yet).")]
		ShipUiTargetAssignedGroup[] Assigned
		{
			get;
		}
		public int? Distance { get; }
		/// <summary>The FC's tag drawn on the target icon ("1".."9", "A".."Z"), or null (Eve64: EveLabelMediumBold under iconPar).</summary>
		string? Tag { get; }
	}
}
