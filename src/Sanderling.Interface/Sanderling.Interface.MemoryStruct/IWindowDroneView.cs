using BotEngine;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowDroneView : IWindow
	{
		IListViewAndControl ListView
		{
			get;
		}
	}
}
