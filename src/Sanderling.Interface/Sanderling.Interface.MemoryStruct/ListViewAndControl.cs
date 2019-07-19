using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class ListViewAndControl<EntryT> : UIElement, IListViewAndControl<EntryT>, IUIElement, IObjectIdInMemory, IObjectIdInt64, IListViewAndControl where EntryT : class, IListEntry
	{
		public IColumnHeader[] ColumnHeader
		{
			get;
			set;
		}

		public EntryT[] Entry
		{
			get;
			set;
		}

		public IScroll Scroll
		{
			get;
			set;
		}

		IListEntry[] IListViewAndControl.Entry
		{
			get
			{
				return Entry;
			}
		}

		public ListViewAndControl()
		{
		}

		public ListViewAndControl(IUIElement @base)
			: base(@base)
		{
		}
	}
}
