using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class ColumnHeader : Container, IColumnHeader, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, IUIElementText
	{
		public int? ColumnIndex
		{
			get;
			set;
		}

		public string Text => base.LabelText?.Largest()?.Text;

		public ColumnHeader()
			: this(null)
		{
		}

		public ColumnHeader(IContainer @base)
			: base(@base)
		{
			ColumnIndex = (@base as IColumnHeader)?.ColumnIndex;
		}
	}
}
