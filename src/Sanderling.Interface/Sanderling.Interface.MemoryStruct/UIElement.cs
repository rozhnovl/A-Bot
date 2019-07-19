using Bib3.Geometrik;
using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class UIElement : ObjectIdInMemory, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public RectInt Region
		{
			get;
			set;
		}

		public int? InTreeIndex
		{
			get;
			set;
		}

		public int? ChildLastInTreeIndex
		{
			get;
			set;
		}

		public virtual IUIElement RegionInteraction => this;

		public UIElement()
			: this(null)
		{
		}

		public UIElement(IObjectIdInMemory @base)
			: base(@base)
		{
		}

		public UIElement(IUIElement @base)
			: this((IObjectIdInMemory)@base)
		{
			Region = (@base?.Region ?? RectInt.Empty);
			InTreeIndex = @base?.InTreeIndex;
			ChildLastInTreeIndex = @base?.ChildLastInTreeIndex;
		}

		public override string ToString()
		{
			return this.SensorObjectToString();
		}
	}
}
