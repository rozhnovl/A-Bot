using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowSelectedItemView : IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		ISprite[] ActionSprite
		{
			get;
		}

		/// <summary>Name of the object the panel is currently describing (its nameLabel).</summary>
		string? SelectedItemName { get; }

		/// <summary>
		/// The panel's action buttons keyed by the client's own node name — "selectedItemApproach",
		/// "selectedItemOrbit", "selectedItemActivateGate", … They are icon-only, so the node name is
		/// the only reliable identifier.
		/// </summary>
		System.Collections.Generic.IReadOnlyDictionary<string, IUIElement>? ActionButtons { get; }
	}
}
