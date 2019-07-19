using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IColumnHeader : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, IUIElementText
	{
		int? ColumnIndex
		{
			get;
		}
	}
}
