using System;

namespace BotEngine.Windows
{
	public struct MEMORY_BASIC_INFORMATION
	{
		public IntPtr BaseAddress;

		public IntPtr AllocationBase;

		public MEMORY_PROTECTION_FLAG AllocationProtect;

		public IntPtr RegionSize;

		public ALLOCATION_TYPE State;

		public MEMORY_PROTECTION_FLAG Protect;

		public uint Type;
	}
}
