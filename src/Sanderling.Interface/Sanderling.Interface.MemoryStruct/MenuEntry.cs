using BotEngine;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class MenuEntry : Container, IMenuEntry, IUIElementText, IUIElement, IObjectIdInMemory, IObjectIdInt64, IContainer
	{
		public bool? HighlightVisible
		{
			get;
			set;
		}

		public string Text { get; init; }

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
