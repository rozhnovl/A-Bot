using System;
using System.Runtime.InteropServices;

namespace BotEngine.WinApi
{
	public class GDI32
	{
		public struct BITMAP_STRUCT
		{
			public int bmType;

			public int bmWidth;

			public int bmHeight;

			public int bmWidthBytes;

			public short bmPlanes;

			public short bmBitsPixel;

			public IntPtr bmBits;
		}

		public const int SRCCOPY = 13369376;

		public const int CAPTUREBLT = 1073741824;

		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteDC(IntPtr hDC);

		[DllImport("gdi32.dll")]
		public static extern int GetBitmapBits(IntPtr hBitmap, int oktetAnzaal, byte[] puferBitmapListeOktet);

		[DllImport("gdi32.dll")]
		public static extern int GetBitmapBits(IntPtr hBitmap, int oktetAnzaal, uint[] puferBitmapListeOktet);

		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll", EntryPoint = "GetObject")]
		public static extern int GetBitmapStruct(IntPtr hObject, int nCount, out BITMAP_STRUCT res);
	}
}
