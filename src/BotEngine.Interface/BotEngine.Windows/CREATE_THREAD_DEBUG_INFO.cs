using System;

namespace BotEngine.Windows
{
	public struct CREATE_THREAD_DEBUG_INFO
	{
		public IntPtr hThread;

		public IntPtr lpThreadLocalBase;

		public IntPtr lpStartAddress;
	}
}
