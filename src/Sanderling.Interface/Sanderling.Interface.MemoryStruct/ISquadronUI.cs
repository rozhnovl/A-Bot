using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISquadronUI : IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		ISquadronContainer Squadron
		{
			get;
		}

		IEnumerable<ISquadronAbilityIcon> SetAbilityIcon
		{
			get;
		}
	}
}
