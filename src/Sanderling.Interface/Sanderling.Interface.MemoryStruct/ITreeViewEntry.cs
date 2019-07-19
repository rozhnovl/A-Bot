using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ITreeViewEntry : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable, IExpandable, IUIElementText
	{
		ITreeViewEntry[] Child
		{
			get;
		}
	}
}
