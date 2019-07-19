using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class Scroll : UIElement, IScroll, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public IColumnHeader[] ColumnHeader
		{
			get;
			set;
		}

		public IUIElement Clipper
		{
			get;
			set;
		}

		public IUIElement ScrollHandleBound
		{
			get;
			set;
		}

		public IUIElement ScrollHandle
		{
			get;
			set;
		}

		public Scroll()
			: this(null)
		{
		}

		public Scroll(IUIElement @base)
			: base(@base)
		{
		}

		public Scroll(IScroll @base)
			: this((IUIElement)@base)
		{
			ColumnHeader = @base?.ColumnHeader;
			Clipper = @base?.Clipper;
			ScrollHandleBound = @base?.ScrollHandleBound;
			ScrollHandle = @base?.ScrollHandle;
		}
	}
}
