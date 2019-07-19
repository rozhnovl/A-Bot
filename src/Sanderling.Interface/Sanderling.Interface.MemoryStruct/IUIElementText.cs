using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IUIElementText : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		string Text
		{
			get;
		}
	}
}
