using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class SquadronsUI : Container, ISquadronsUI
	{
		public IUIElement LaunchAllButton
		{
			get;
			set;
		}

		public IUIElement OpenBayButton
		{
			get;
			set;
		}

		public IUIElement RecallAllButton
		{
			get;
			set;
		}

		public IEnumerable<ISquadronUI> SetSquadron
		{
			get;
			set;
		}

		public SquadronsUI()
		{
		}

		public SquadronsUI(IUIElement @base)
			: base(@base)
		{
		}
	}
}
