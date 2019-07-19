using System;

namespace BotEngine.Windows
{
	public struct CURSORINFO
	{
		public uint cbSize;

		public uint flags;

		public IntPtr hCursor;

		public POINT ptScreenPos;
	}
}
