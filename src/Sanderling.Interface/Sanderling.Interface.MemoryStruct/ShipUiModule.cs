using Bib3.Geometrik;
using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class ShipUiModule : UIElement, IShipUiModule, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public bool? ModuleButtonVisible
		{
			get;
			set;
		}

		public IObjectIdInMemory ModuleButtonIconTexture
		{
			get;
			set;
		}

		public string ModuleButtonQuantity
		{
			get;
			set;
		}

		public bool RampActive
		{
			get;
			set;
		}

		public int? RampRotationMilli
		{
			get;
			set;
		}

		public bool? HiliteVisible
		{
			get;
			set;
		}

		public bool? GlowVisible
		{
			get;
			set;
		}

		public bool? BusyVisible
		{
			get;
			set;
		}

		public bool? OverloadOn
		{
			get;
			set;
		}

		public override IUIElement RegionInteraction => this.WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(16L, 16L));

		public ShipUiModule()
		{
		}

		public ShipUiModule(IUIElement @base)
			: base(@base)
		{
		}
	}
}
