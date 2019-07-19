using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface INeocom : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		IUIElement EveMenuButton
		{
			get;
		}

		IUIElement CharButton
		{
			get;
		}

		ISprite[] Button
		{
			get;
		}

		IUIElementText Clock
		{
			get;
		}
	}
}
