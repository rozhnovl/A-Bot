using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IChatParticipantEntry : IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		IUIElementText NameLabel
		{
			get;
		}

		ISprite StatusIcon
		{
			get;
		}

		IEnumerable<ISprite> FlagIcon
		{
			get;
		}
	}
}
