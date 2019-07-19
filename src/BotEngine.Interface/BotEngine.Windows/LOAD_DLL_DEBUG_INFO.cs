using System;

namespace BotEngine.Windows
{
	public struct LOAD_DLL_DEBUG_INFO
	{
		public IntPtr hFile;

		public IntPtr lpBaseOfDll;

		public uint dwDebugInfoFileOffset;

		public uint nDebugInfoSize;

		public IntPtr lpImageName;

		public ushort fUnicode;
	}
}
