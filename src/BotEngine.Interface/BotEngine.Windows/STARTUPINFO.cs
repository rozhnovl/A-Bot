using System;

namespace BotEngine.Windows
{
	public struct STARTUPINFO
	{
		private uint cb;

		private string lpReserved;

		private string lpDesktop;

		private string lpTitle;

		private uint dwX;

		private uint dwY;

		private uint dwXSize;

		private uint dwYSize;

		private uint dwXCountChars;

		private uint dwYCountChars;

		private uint dwFillAttribute;

		private uint dwFlags;

		private ushort wShowWindow;

		private ushort cbReserved2;

		private IntPtr lpReserved2;

		private IntPtr hStdInput;

		private IntPtr hStdOutput;

		private IntPtr hStdError;
	}
}
