using BotEngine;
using BotEngine.Interface;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class ListEntry : Container, IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		public int? ContentBoundLeft
		{
			get;
			set;
		}

		public KeyValuePair<IColumnHeader, string>[] ListColumnCellLabel
		{
			get;
			set;
		}

		public IUIElement GroupExpander
		{
			get;
			set;
		}

		public bool? IsGroup
		{
			get;
			set;
		}

		public bool? IsExpanded
		{
			get;
			set;
		}

		public bool? IsSelected
		{
			get;
			set;
		}

		public ColorORGB[] ListBackgroundColor
		{
			get;
			set;
		}

		public ISprite[] SetSprite
		{
			get;
			set;
		}

		public ListEntry()
			: this(null)
		{
		}

		public ListEntry(IUIElement @base)
			: base(@base)
		{
			IListEntry listEntry = @base as IListEntry;
			ContentBoundLeft = listEntry?.ContentBoundLeft;
			ListColumnCellLabel = listEntry?.ListColumnCellLabel;
			GroupExpander = listEntry?.GroupExpander;
			IsGroup = listEntry?.IsGroup;
			IsExpanded = listEntry?.IsExpanded;
			IsSelected = listEntry?.IsSelected;
			ListBackgroundColor = listEntry?.ListBackgroundColor;
			SetSprite = listEntry?.SetSprite;
		}
	}
}
