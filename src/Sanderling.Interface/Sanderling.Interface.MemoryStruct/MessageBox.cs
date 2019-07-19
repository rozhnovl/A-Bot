namespace Sanderling.Interface.MemoryStruct
{
	public class MessageBox : Window
	{
		public string TopCaptionText;

		public string MainEditText;

		public MessageBox(IWindow @base)
			: base(@base)
		{
		}

		public MessageBox()
		{
		}
	}
}
