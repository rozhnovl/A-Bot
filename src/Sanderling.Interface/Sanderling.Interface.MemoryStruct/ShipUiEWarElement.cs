namespace Sanderling.Interface.MemoryStruct
{
	public class ShipUiEWarElement : UIElement
	{
		public string EWarType;

		public IObjectIdInMemory IconTexture;

		public ShipUiEWarElement()
		{
		}

		public ShipUiEWarElement(IUIElement @base)
			: base(@base)
		{
		}
	}
}
