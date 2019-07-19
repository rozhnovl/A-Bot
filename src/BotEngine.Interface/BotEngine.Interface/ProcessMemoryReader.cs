using BotEngine.WinApi;
using BotEngine.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BotEngine.Interface
{
	public class ProcessMemoryReader : IMemoryReader, IDisposable
	{
		public readonly int ProcessId;

		private IntPtr ProcessHandle;

		private MemoryReaderModuleInfo[] ModulesCache;

		public MemoryReaderModuleInfo[] Modules()
		{
			return ModulesCache;
		}

		public static MemoryReaderModuleInfo[] ModulesOfProcess(int processId)
		{
			System.Diagnostics.Process processById = System.Diagnostics.Process.GetProcessById(processId);
			List<MemoryReaderModuleInfo> list = new List<MemoryReaderModuleInfo>();
			foreach (ProcessModule item in processById.Modules.OfType<ProcessModule>())
			{
				list.Add(new MemoryReaderModuleInfo(item.ModuleName, item.BaseAddress.ToInt64()));
			}
			return list.ToArray();
		}

		public ProcessMemoryReader(int processId)
		{
			ProcessId = processId;
			ProcessHandle = Kernel32.OpenProcess(PROCESS_ACCESS_RIGHT.PROCESS_VM_READ, 0, (uint)processId);
			ModulesCache = ModulesOfProcess(processId);
		}

		public ProcessMemoryReader(System.Diagnostics.Process process)
			: this(process.Id)
		{
		}

		public void Dispose()
		{
			Kernel32.CloseHandle(ProcessHandle);
		}

		public static IntPtr? CastToIntPtrAvoidOverflow(ulong address)
		{
			if (4 == IntPtr.Size)
			{
				if (address < 0)
				{
					return null;
				}
				if (uint.MaxValue < address)
				{
					return null;
				}
				return (IntPtr)(int)address;
			}
			return (IntPtr)(long)(uint)address;
		}

		public static IntPtr? CastToIntPtrAvoidOverflow(long address)
		{
			return CastToIntPtrAvoidOverflow((ulong)address);
		}

		public int ReadBytes(long address, int bytesCount, byte[] destinationArray)
		{
			if (destinationArray == null)
			{
				return 0;
			}
			IntPtr lpNumberOfBytesRead = IntPtr.Zero;
			IntPtr? intPtr = CastToIntPtrAvoidOverflow(address);
			if (!intPtr.HasValue)
			{
				return 0;
			}
			int value = Math.Min(bytesCount, destinationArray.Length);
			if (Kernel32.ReadProcessMemory(ProcessHandle, intPtr.Value, destinationArray, (IntPtr)value, out lpNumberOfBytesRead) != 0)
			{
				return (int)lpNumberOfBytesRead;
			}
			return 0;
		}
	}
}
