using BotEngine.Windows;
using System;
using System.Runtime.InteropServices;

namespace BotEngine.WinApi
{
	public static class ntdll
	{
		[DllImport("ntdll.dll", SetLastError = true)]
		public static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, IntPtr processInformation, uint processInformationLength, IntPtr returnLength);

		[DllImport("ntdll.dll", SetLastError = true)]
		public static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

		[DllImport("ntdll.dll", SetLastError = true)]
		public static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref long addr, int cb, out int pSize);
	}
}
