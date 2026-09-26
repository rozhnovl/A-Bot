using Bib3.Geometrik;
using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class ShipUiTarget : UIElement, IShipUiTarget, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		public string[] LabelText
		{
			get;
			set;
		}

		public bool? IsSelected
		{
			get;
			set;
		}

		public IShipHitpointsAndEnergy Hitpoints
		{
			get;
			set;
		}

		public IUIElement RegionInteractionElement
		{
			get;
			set;
		}

		public int? Distance { get; set; }
		public override IUIElement RegionInteraction => RegionInteractionElement?.WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(40L, 40L));

		[System.Obsolete("Not populated by the Eve64 parser — always null (assigned drone/weapon icons are not parsed yet).")]
		public ShipUiTargetAssignedGroup[] Assigned
		{
			get;
			set;
		}

		public ShipUiTarget()
			: this(null)
		{
		}

		public ShipUiTarget(IUIElement @base)
			: base(@base)
		{
		}
	}
}
