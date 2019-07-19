namespace Sanderling.Interface.MemoryStruct
{
	public interface IExpandable
	{
		IUIElement ExpandToggleButton
		{
			get;
		}

		bool? IsExpanded
		{
			get;
		}
	}
}
