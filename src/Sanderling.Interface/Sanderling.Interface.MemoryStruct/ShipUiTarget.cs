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
