namespace Sanderling.Interface.MemoryStruct
{
	public class Tab : UIElement
	{
		public IUIElementText Label;

		public int? LabelColorOpacityMilli;

		public int? BackgroundOpacityMilli;

		public Tab()
		{
		}

		public Tab(IUIElement @base)
			: base(@base)
		{
		}
	}
}
