using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public class InfoPanel : UIElement, IInfoPanel, IUIElement, IObjectIdInMemory, IObjectIdInt64, IExpandable
	{
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

		public IContainer HeaderContent
		{
			get;
			set;
		}

		public IContainer ExpandedContent
		{
			get;
			set;
		}

		public string HeaderText => HeaderContent?.LabelText?.Largest()?.Text;

		public InfoPanel()
		{
		}

		public InfoPanel(IUIElement @base)
			: base(@base)
		{
		}

		public InfoPanel(IInfoPanel @base)
			: this((IUIElement)@base)
		{
			IsExpanded = @base?.IsExpanded;
			ExpandToggleButton = @base?.ExpandToggleButton;
			HeaderContent = @base?.HeaderContent;
			ExpandedContent = @base?.ExpandedContent;
		}
	}
}
