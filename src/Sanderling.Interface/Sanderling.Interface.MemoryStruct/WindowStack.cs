namespace Sanderling.Interface.MemoryStruct
{
	public class WindowStack : Window
	{
		public Tab[] Tab;

		public IWindow TabSelectedWindow;

		public WindowStack()
		{
		}

		public WindowStack(IWindow @base)
			: base(@base)
		{
		}
	}
}
