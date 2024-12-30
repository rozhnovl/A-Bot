using Bib3.Geometrik;
using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IUIElement : IObjectIdInMemory, IObjectIdInt64
	{
		RectInt? Region
		{
			get;
		}

		int? InTreeIndex
		{
			get;
		}

		int? ChildLastInTreeIndex
		{
			get;
		}

		IUIElement RegionInteraction
		{
			get;
		}
	}
}
