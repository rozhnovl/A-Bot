namespace Sanderling.Interface.MemoryStruct
{
	public class LobbyAgentEntry : UIElement
	{
		public IUIElementText[] LabelText;

		public IUIElement StartConversationButton;

		public LobbyAgentEntry(IUIElement @base)
			: base(@base)
		{
		}

		public LobbyAgentEntry()
		{
		}
	}
}
