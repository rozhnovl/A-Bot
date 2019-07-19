using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IMenuEntry : IUIElementText, IUIElement, IObjectIdInMemory, IObjectIdInt64, IContainer
	{
		bool? HighlightVisible
		{
			get;
		}
	}
}
