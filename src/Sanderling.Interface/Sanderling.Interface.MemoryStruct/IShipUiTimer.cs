using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUiTimer : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		string Name
		{
			get;
		}
	}
}
