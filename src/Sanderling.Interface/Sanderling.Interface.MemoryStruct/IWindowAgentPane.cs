using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowAgentPane : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		string Html
		{
			get;
		}
	}
}
