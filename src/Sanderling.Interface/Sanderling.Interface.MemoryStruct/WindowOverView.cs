using BotEngine;
using System;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowOverView : Window, IWindowOverview, IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64, ICloneable
	{
		public Tab[] PresetTab
		{
			get;
			set;
		}

		//public IListViewAndControl<IOverviewEntry> ListView
		//{
		//	get;
		//	set;
		//}

		public string ViewportOverallLabelString
		{
			get;
			set;
		}

		public WindowOverView(IWindow @base)
			: base(@base)
		{
		}

		public WindowOverView()
		{
		}

		public WindowOverView Copy()
		{
			return this.CopyByPolicyMemoryMeasurement();
		}

		public object Clone()
		{
			return Copy();
		}

		public IList<IOverviewEntry> Entries
		{ get;
			set;
		}
	}
}
