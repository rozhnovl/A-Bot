using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISquadronsUI
	{
		IEnumerable<ISquadronUI> SetSquadron
		{
			get;
		}

		IUIElement LaunchAllButton
		{
			get;
		}

		IUIElement OpenBayButton
		{
			get;
		}

		IUIElement RecallAllButton
		{
			get;
		}
	}
}
