using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowChatChannel : Window
	{
		public IListViewAndControl<IChatParticipantEntry> ParticipantView;

		public IListViewAndControl MessageView;

		public IEnumerable<IChatParticipantEntry> Participant
		{
			get
			{
				IListViewAndControl<IChatParticipantEntry> participantView = ParticipantView;
				return (participantView == null) ? null : participantView.Entry?.OfType<IChatParticipantEntry>();
			}
		}

		public IEnumerable<ChatMessage> Message => MessageView?.Entry?.OfType<ChatMessage>();

		public IUIElementInputText MessageInput
		{
			get;
			set;
		}

		public WindowChatChannel(IWindow window)
			: base(window)
		{
		}

		public WindowChatChannel()
		{
		}
	}
}
