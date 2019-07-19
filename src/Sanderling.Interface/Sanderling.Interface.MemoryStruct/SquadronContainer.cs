using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class SquadronContainer : Container, ISquadronContainer, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		public int? SquadronNumber
		{
			get;
			set;
		}

		public ISquadronHealth Health
		{
			get;
			set;
		}

		public bool? IsSelected
		{
			get;
			set;
		}

		public string Hint
		{
			get;
			set;
		}

		public SquadronContainer()
		{
		}

		public SquadronContainer(IUIElement @base)
			: base(@base)
		{
		}
	}
}
