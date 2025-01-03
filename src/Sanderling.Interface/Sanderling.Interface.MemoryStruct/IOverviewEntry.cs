using BotEngine;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	//public interface IOverviewEntry : IListEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable
	//{
	//	ISprite[] RightIcon
	//	{
	//		get;
	//	}

	//	IEnumerable<string> MainIconSetIndicatorName
	//	{
	//		get;
	//	}
	//}
	public interface IOverviewEntry
	{
		//public UITreeNodeWithDisplayRegion UINode { get; set; }
		public IUIElement UiElement { get; }
		public long Id { get; }
		public List<string> TextsLeftToRight { get; set; }
		public Dictionary<string, string> CellsTexts { get; set; }
		public string? ObjectDistance { get; set; }
		public int? ObjectDistanceInMeters { get; set; }
		public string? ObjectName { get; set; }
		public string? ObjectType { get; set; }
		public string? ObjectAlliance { get; set; }
		public ColorComponents? IconSpriteColorPercent { get; set; }
		public HashSet<string> NamesUnderSpaceObjectIcon { get; set; }
		public List<ColorComponents> BgColorFillsPercent { get; set; } 
		public List<string> RightAlignedIconsHints { get; set; }
		public OverviewWindowEntryCommonIndications CommonIndications { get; set; }
		public int? OpacityPercent { get; set; }
	}

	public class OverviewWindowEntryCommonIndications
	{
		public bool Targeting { get; set; }
		public bool TargetedByMe { get; set; }
		public bool IsJammingMe { get; set; }
		public bool IsWarpDisruptingMe { get; set; }
	}
}
