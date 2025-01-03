using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowDroneView
	{
		IList<IDronesWindowEntryGroupStructure> DroneGroups { get; set; }
		IDronesWindowEntryGroupStructure? DroneGroupInBay { get; set; }
		IDronesWindowEntryGroupStructure? DroneGroupInSpace { get; set; }
	}

	public interface IDronesWindowEntryGroupStructure
	{
		public IDronesWindowDroneGroupHeader? Header { get; set; }
		public IList<IDronesWindowEntryDrone> Children { get; set; }
	}
	public interface IDronesWindowEntryDrone
	{
		public IDronesWindowEntryDroneStructure Entry { get; set; }
	}

	public interface IDronesWindowEntryDroneStructure
	{
		public string? MainText { get; set; }
	}
	public interface IDronesWindowDroneGroupHeader
	{
		//public UITreeNodeWithDisplayRegion UINode { get; set; }
		public string? MainText { get; set; }
		//public DronesWindowDroneGroupHeaderQuantity? QuantityFromTitle { get; set; }
	}

}
