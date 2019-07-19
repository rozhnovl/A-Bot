using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IListViewAndControl
	{
		IColumnHeader[] ColumnHeader
		{
			get;
		}

		IListEntry[] Entry
		{
			get;
		}

		IScroll Scroll
		{
			get;
		}
	}
	public interface IListViewAndControl<out EntryT> : IUIElement, IObjectIdInMemory, IObjectIdInt64, IListViewAndControl where EntryT : IListEntry
	{
		new EntryT[] Entry
		{
			get;
		}
	}
}
