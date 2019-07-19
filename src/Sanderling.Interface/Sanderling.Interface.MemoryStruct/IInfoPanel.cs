using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanel : IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
		IContainer HeaderContent
		{
			get;
		}

		IContainer ExpandedContent
		{
			get;
		}

		string HeaderText
		{
			get;
		}
	}
}
