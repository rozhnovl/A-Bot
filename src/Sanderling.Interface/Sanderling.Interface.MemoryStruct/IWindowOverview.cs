using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowOverview //: IWindow, IContainer, IUIElement, IObjectIdInMemory, IObjectIdInt64
	{
		//Tab[] PresetTab
		//{
		//	get;
		//}

		//IListViewAndControl<IOverviewEntry> ListView
		//{
		//	get;
		//}

		IList<IOverviewEntry> Entries { get; }
		//string ViewportOverallLabelString
		//{
		//	get;
		//}
	}
}
