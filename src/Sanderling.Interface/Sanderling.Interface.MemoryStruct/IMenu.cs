using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IMenu : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IEnumerable<IMenuEntry> Entry
		{
			get;
		}
	}
}
