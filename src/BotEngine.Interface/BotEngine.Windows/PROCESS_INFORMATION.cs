using System;

namespace BotEngine.Windows
{
	public struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public uint dwProcessId;

		public uint dwThreadId;
	}
}
