using System;

namespace BotEngine.Windows
{
	public struct CREATE_PROCESS_DEBUG_INFO
	{
		public IntPtr hFile;

		public IntPtr hProcess;

		public IntPtr hThread;

		public IntPtr lpBaseOfImage;

		public uint dwDebugInfoFileOffset;

		public uint nDebugInfoSize;

		public IntPtr lpThreadLocalBase;

		public IntPtr lpStartAddress;

		public IntPtr lpImageName;

		public ushort fUnicode;
	}
}
