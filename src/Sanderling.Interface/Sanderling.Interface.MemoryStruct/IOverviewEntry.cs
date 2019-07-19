using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IOverviewEntry : IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		ISprite[] RightIcon
		{
			get;
		}

		IEnumerable<string> MainIconSetIndicatorName
		{
			get;
		}
	}
}
