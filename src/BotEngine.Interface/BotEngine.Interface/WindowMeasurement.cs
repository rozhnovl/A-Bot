using Bib3.Geometrik;

namespace BotEngine.Interface
{
	public class WindowMeasurement
	{
		public Raster2D<uint> ClientRectRaster;

		public RectInt ClientRect;

		public string WindowTitle;

		public WindowMeasurement()
		{
		}

		public WindowMeasurement(Raster2D<uint> clientRectRaster, RectInt clientRect, string windowTitle = null)
		{
			ClientRectRaster = clientRectRaster;
			ClientRect = clientRect;
			WindowTitle = windowTitle;
		}
	}
}
