using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class InSpaceBracket : Container, IInSpaceBracket, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public string Name
		{
			get;
			set;
		}

		public InSpaceBracket()
			: this(null)
		{
		}

		public InSpaceBracket(IUIElement @base)
			: base(@base)
		{
			Name = (@base as IInSpaceBracket)?.Name;
		}
	}
}
