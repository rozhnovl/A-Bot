using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BotEngine.WinApi
{
	public static class psapi
	{
		public enum SictEnumProcessModulesExFilterFlag
		{
			LIST_MODULES_DEFAULT,
			LIST_MODULES_32BIT,
			LIST_MODULES_64BIT,
			LIST_MODULES_ALL
		}

		public struct MODULEINFO
		{
			public IntPtr lpBaseOfDll;

			public uint SizeOfImage;

			public IntPtr EntryPoint;
		}

		[DllImport("psapi.dll", SetLastError = true)]
		public static extern bool EnumProcessModules(IntPtr hProcess, [In] [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

		[DllImport("psapi.dll")]
		public static extern bool EnumProcessModulesEx(IntPtr hProcess, [In] [Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] IntPtr[] lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded, uint dwFilterFlag);

		[DllImport("psapi.dll", SetLastError = true)]
		public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

		[DllImport("psapi.dll")]
		public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

		[DllImport("psapi.dll")]
		public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
	}
}
