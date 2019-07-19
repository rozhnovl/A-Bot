using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISquadronContainer : IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		int? SquadronNumber
		{
			get;
		}

		ISquadronHealth Health
		{
			get;
		}

		bool? IsSelected
		{
			get;
		}

		string Hint
		{
			get;
		}
	}
}
