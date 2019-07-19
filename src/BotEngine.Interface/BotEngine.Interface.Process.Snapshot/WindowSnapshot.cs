using System.Collections.Generic;

namespace BotEngine.Interface.Process.Snapshot
{
	public class WindowSnapshot
	{
		public readonly KeyValuePair<uint[], int> ClientRectRaster;

		public WindowSnapshot()
		{
		}

		public WindowSnapshot(KeyValuePair<uint[], int> clientRectRaster)
		{
			ClientRectRaster = clientRectRaster;
		}
	}
}
