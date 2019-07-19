using BotEngine.Windows;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BotEngine.WinApi
{
	public static class Kernel32
	{
		[DllImport("kernel32.dll")]
		public static extern void RtlMoveMemory(IntPtr destination, IntPtr source, int length);

		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();

		[DllImport("kernel32.dll")]
		public static extern uint WaitForDebugEvent(out DEBUG_EVENT lpDebugEvent, [In] uint dwMilliseconds);

		[DllImport("kernel32.dll")]
		public static extern uint ContinueDebugEvent([In] uint dwProcessId, [In] uint dwThreadId, [In] DEBUG_CONTINUE_STATUS dwContinueStatus);

		[DllImport("kernel32.dll")]
		public static extern uint CreateProcess([In] string pszImageName, [In] [Out] string pszCmdLine, [In] IntPtr psaProcess, [In] IntPtr psaThread, [In] uint fInheritHandles, [In] PROCESS_CREATION_FLAG fdwCreate, [In] IntPtr pvEnvironment, [In] IntPtr pszCurDir, [In] ref STARTUPINFO psiStartInfo, out PROCESS_INFORMATION pProcInfo);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(PROCESS_ACCESS_RIGHT dwDesiredAccess, int bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr size, out IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, uint[] lpBuffer, IntPtr size, out IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		public static extern uint WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

		[DllImport("kernel32.dll")]
		public static extern uint GetThreadContext(IntPtr hThread, ref THREAD_CONTEXT32 lpContext);

		[DllImport("kernel32.dll")]
		public static extern uint SetThreadContext(IntPtr hThread, ref THREAD_CONTEXT32 lpContext);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenThread(THREAD_ACCESS_RIGHT dwDesiredAccess, uint bInheritHandle, uint dwThreadId);

		[DllImport("kernel32.dll")]
		public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

		[DllImport("kernel32.dll")]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, ALLOCATION_TYPE flAllocationType, MEMORY_PROTECTION_FLAG flProtect);

		[DllImport("kernel32.dll")]
		public static extern uint SuspendThread(IntPtr hThread);

		[DllImport("kernel32.dll")]
		public static extern uint ResumeThread(IntPtr hThread);

		[DllImport("kernel32.dll")]
		public static extern uint VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, MEMORY_PROTECTION_FLAG flNewProtect, ref MEMORY_PROTECTION_FLAG lpflOldProtect);

		[DllImport("kernel32.dll")]
		public static extern int DebugActiveProcess(uint dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern int DebugActiveProcessStop(uint dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern int DebugBreakProcess(IntPtr process);

		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process([In] IntPtr processHandle, [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

		[DllImport("Kernel32.Dll")]
		public static extern int GetUserDefaultLocaleName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder localeName, int nMaxCount);

		public static string GetUserDefaultLocaleName()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (GetUserDefaultLocaleName(stringBuilder, 99) == 0)
			{
				return null;
			}
			return stringBuilder.ToString();
		}
	}
}
