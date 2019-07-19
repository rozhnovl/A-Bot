using BotEngine;
using BotEngine.Interface;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IListEntry : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		int? ContentBoundLeft
		{
			get;
		}

		KeyValuePair<IColumnHeader, string>[] ListColumnCellLabel
		{
			get;
		}

		IUIElement GroupExpander
		{
			get;
		}

		bool? IsGroup
		{
			get;
		}

		bool? IsExpanded
		{
			get;
		}

		ColorORGB[] ListBackgroundColor
		{
			get;
		}

		ISprite[] SetSprite
		{
			get;
		}
	}
}
