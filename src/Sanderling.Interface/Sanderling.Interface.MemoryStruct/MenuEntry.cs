using BotEngine;
using System.Linq;
using TLSchema.Messages;

namespace Sanderling.Interface.MemoryStruct
{
	public class MenuEntry : Container, IMenuEntry, IUIElementText, IUIElement, IObjectIdInMemory, IObjectIdInt64, IContainer
	{
		public bool? HighlightVisible
		{
			get;
			set;
		}

		public string Text => base.LabelText?.Select((IUIElementText label) => label?.Text)?.OrderByDescending((string text) => text?.Length ?? (-1))?.FirstOrDefault();

		public MenuEntry()
			: this(null)
		{
		}

		public MenuEntry(IUIElement @base)
			: base(@base)
		{
		}

		public override string ToString() => Text;
	}
}
