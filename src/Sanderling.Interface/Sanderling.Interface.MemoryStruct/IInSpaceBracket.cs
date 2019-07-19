using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInSpaceBracket : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		string Name
		{
			get;
		}
	}
}
