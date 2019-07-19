using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class ChatParticipantEntry : ListEntry, IChatParticipantEntry, IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	{
		public IUIElementText NameLabel
		{
			get;
			set;
		}

		public ISprite StatusIcon
		{
			get;
			set;
		}

		public IEnumerable<ISprite> FlagIcon
		{
			get;
			set;
		}

		public ChatParticipantEntry(IListEntry @base)
			: base(@base)
		{
		}

		public ChatParticipantEntry()
		{
		}
	}
}
