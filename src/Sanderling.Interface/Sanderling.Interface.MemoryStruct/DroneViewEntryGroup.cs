namespace Sanderling.Interface.MemoryStruct
{
	public class DroneViewEntryGroup : DroneViewEntry
	{
		public IUIElementText Caption;

		public DroneViewEntryGroup(IListEntry @base)
			: base(@base)
		{
		}

		public DroneViewEntryGroup()
		{
		}
	}
}
