using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IScroll : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IColumnHeader[] ColumnHeader
		{
			get;
		}

		IUIElement Clipper
		{
			get;
		}

		IUIElement ScrollHandleBound
		{
			get;
		}

		IUIElement ScrollHandle
		{
			get;
		}
	}
}
