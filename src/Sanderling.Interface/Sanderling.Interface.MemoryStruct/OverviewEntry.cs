using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class OverviewEntry : ListEntry, IOverviewEntry, IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		public ISprite[] RightIcon
		{
			get;
			set;
		}

		public IEnumerable<string> MainIconSetIndicatorName
		{
			get;
			set;
		}

		public OverviewEntry(IListEntry @base)
			: base(@base)
		{
		}

		public OverviewEntry()
		{
		}
	}
}
