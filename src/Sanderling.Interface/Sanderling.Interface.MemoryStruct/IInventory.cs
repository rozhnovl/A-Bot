using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInventory : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IListViewAndControl ListView
		{
			get;
		}
	}
}
