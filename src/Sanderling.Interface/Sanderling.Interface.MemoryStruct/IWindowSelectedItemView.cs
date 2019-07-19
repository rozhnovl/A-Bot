using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowSelectedItemView : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		ISprite[] ActionSprite
		{
			get;
		}
	}
}
