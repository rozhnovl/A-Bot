using BotEngine;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class TreeViewEntry : Container, ITreeViewEntry, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ISelectable, IExpandable, IUIElementText
	{
		public ITreeViewEntry[] Child
		{
			get;
			set;
		}

		public bool? IsSelected
		{
			get;
			set;
		}

		public bool? IsExpanded
		{
			get;
			set;
		}

		public IUIElement ExpandToggleButton
		{
			get;
			set;
		}

		public override IUIElement RegionInteraction => base.LabelText?.Largest();

		public string Text => base.LabelText?.Where((IUIElementText label) => 0 < label?.Text?.Trim()?.Length)?.OrderByCenterVerticalDown()?.FirstOrDefault()?.Text;

		public TreeViewEntry()
		{
		}

		public TreeViewEntry(IUIElement @base)
			: base(@base)
		{
		}
	}
}
