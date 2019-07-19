using Bib3;
using Bib3.Geometrik;
using BotEngine.Interface;
using BotEngine.WinApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BotEngine.Windows
{
	public static class Extension
	{
		public static Vektor2DInt AsVektor2DInt(this POINT point)
		{
			return new Vektor2DInt(point.x, point.y);
		}

		public static POINT AsWindowsPoint(this Vektor2DInt vector)
		{
			return new POINT((int)vector.A, (int)vector.B);
		}

		public static RangeOfPages[] ListRangeOfPagesFromProcessWithId(uint processId, bool pageContentRead)
		{
			ListModuleAndListRangeOfPagesFromProcessWithId(processId, out bool _, out PROCESS_BASIC_INFORMATION _, out long? _, out SictProcessModuleInfo[] _, out RangeOfPages[] listRangeOfPages, listeModuleErsctele: false, listRangeOfPagesRead: true, pageContentRead);
			return listRangeOfPages;
		}

		public static void ListModuleAndListRangeOfPagesFromProcessWithId(uint processId, out bool isWow64Process, out PROCESS_BASIC_INFORMATION processBasicInfo, out long? wow64PEBBaseAddress32, out SictProcessModuleInfo[] listeModuleInfo, out RangeOfPages[] listRangeOfPages, bool listeModuleErsctele, bool listRangeOfPagesRead, bool listRangeOfPagesContentRead)
		{
			isWow64Process = false;
			processBasicInfo = default(PROCESS_BASIC_INFORMATION);
			wow64PEBBaseAddress32 = null;
			listeModuleInfo = null;
			listRangeOfPages = null;
			Process currentProcess = Process.GetCurrentProcess();
			bool wow64Process = false;
			Kernel32.IsWow64Process(currentProcess.Handle, out wow64Process);
			Process processById = Process.GetProcessById((int)processId);
			if (processById == null)
			{
				return;
			}
			PROCESS_ACCESS_RIGHT dwDesiredAccess = (PROCESS_ACCESS_RIGHT)3088;
			IntPtr intPtr = Kernel32.OpenProcess(dwDesiredAccess, 0, processId);
			Kernel32.IsWow64Process(intPtr, out isWow64Process);
			int pSize;
			int num = ntdll.NtQueryInformationProcess(intPtr, PROCESSINFOCLASS.ProcessBasicInformation, ref processBasicInfo, processBasicInfo.Size, out pSize);
			List<SictProcessModuleInfo> list = new List<SictProcessModuleInfo>();
			if (listeModuleErsctele)
			{
				IntPtr[] array = new IntPtr[1024];
				if (psapi.EnumProcessModulesEx(intPtr, array, (uint)(array.Length * IntPtr.Size), out uint lpcbNeeded, 3u))
				{
					uint num2 = lpcbNeeded / 8u;
					for (int i = 0; i < num2; i++)
					{
						IntPtr hModule = array[i];
						psapi.MODULEINFO lpmodinfo = default(psapi.MODULEINFO);
						if (psapi.GetModuleInformation(intPtr, hModule, out lpmodinfo, (uint)Marshal.SizeOf((object)lpmodinfo)) && lpmodinfo.lpBaseOfDll.ToInt64() > 0)
						{
							StringBuilder stringBuilder = new StringBuilder(1024);
							StringBuilder stringBuilder2 = new StringBuilder(1024);
							psapi.GetModuleFileNameEx(intPtr, hModule, stringBuilder2, stringBuilder2.Capacity);
							psapi.GetModuleBaseName(intPtr, hModule, stringBuilder, stringBuilder.Capacity);
							SictProcessModuleInfo item = new SictProcessModuleInfo(lpmodinfo.lpBaseOfDll.ToInt64(), lpmodinfo.SizeOfImage, stringBuilder.ToString(), stringBuilder2.ToString());
							list.Add(item);
						}
					}
				}
				listeModuleInfo = list.ToArray();
			}
			SictMesungZaitraumAusStopwatch sictMesungZaitraumAusStopwatch = new SictMesungZaitraumAusStopwatch(beginJezt: true);
			long num3 = 0L;
			if (listRangeOfPagesRead | listRangeOfPagesContentRead)
			{
				List<RangeOfPages> list2 = new List<RangeOfPages>();
				ulong address = 0uL;
				IntPtr handle = processById.Handle;
				while (true)
				{
					MEMORY_BASIC_INFORMATION lpBuffer = default(MEMORY_BASIC_INFORMATION);
					IntPtr zero = IntPtr.Zero;
					try
					{
						zero = (ProcessMemoryReader.CastToIntPtrAvoidOverflow(address) ?? IntPtr.Zero);
					}
					catch (OverflowException)
					{
						break;
					}
					if (!Kernel32.VirtualQueryEx(handle, zero, out lpBuffer, (uint)Marshal.SizeOf((object)lpBuffer)) || (0 < list2.Count && lpBuffer.BaseAddress.ToInt64() <= list2.Last().BasicInfo.BaseAddress.ToInt64()))
					{
						break;
					}
					byte[] array2 = null;
					if (listRangeOfPagesContentRead && (lpBuffer.State & ALLOCATION_TYPE.MEM_COMMIT) == ALLOCATION_TYPE.MEM_COMMIT)
					{
						array2 = new byte[lpBuffer.RegionSize.ToInt64()];
						SictMesungZaitraumAusStopwatch sictMesungZaitraumAusStopwatch2 = new SictMesungZaitraumAusStopwatch(beginJezt: true);
						Kernel32.ReadProcessMemory(intPtr, lpBuffer.BaseAddress, array2, (IntPtr)array2.LongLength, out IntPtr lpNumberOfBytesRead);
						sictMesungZaitraumAusStopwatch2.EndeSezeJezt();
						num3 += (sictMesungZaitraumAusStopwatch2.DauerMikro ?? 0);
						if (lpNumberOfBytesRead.ToInt64() < 1)
						{
							array2 = null;
						}
						else if (lpNumberOfBytesRead.ToInt64() < array2.LongLength)
						{
							byte[] array3 = new byte[lpNumberOfBytesRead.ToInt64()];
							Array.Copy(array2, array3, array3.Length);
						}
					}
					list2.Add(new RangeOfPages(lpBuffer, array2));
					address = (ulong)(lpBuffer.BaseAddress.ToInt64() + lpBuffer.RegionSize.ToInt64());
				}
				listRangeOfPages = list2.ToArray();
			}
			sictMesungZaitraumAusStopwatch.EndeSezeJezt();
		}

		public static byte[] Kernel32ReadProcessMemory(IntPtr processHandle, long address, long listOctetCountMax)
		{
			IntPtr? intPtr = address.CastToIntPtrAvoidingOverflow();
			if (!intPtr.HasValue)
			{
				return null;
			}
			byte[] array = new byte[listOctetCountMax];
			IntPtr lpNumberOfBytesRead = IntPtr.Zero;
			uint num = Kernel32.ReadProcessMemory(processHandle, intPtr.Value, array, (IntPtr)array.Length, out lpNumberOfBytesRead);
			long num2 = lpNumberOfBytesRead.ToInt64();
			if (num2 < int.MinValue || int.MaxValue < num2)
			{
				throw new ArgumentOutOfRangeException("readByteCountInt64=" + num2.ToString());
			}
			int num3 = lpNumberOfBytesRead.ToInt32();
			if (listOctetCountMax <= num3)
			{
				return array;
			}
			byte[] array2 = new byte[num3];
			Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
			return array2;
		}

		public static long Kernel32ReadProcessMemory(IntPtr processHandle, long address, long listOctetCountMax, byte[] destinationArray)
		{
			if (destinationArray == null)
			{
				return 0L;
			}
			IntPtr? intPtr = address.CastToIntPtrAvoidingOverflow();
			if (!intPtr.HasValue)
			{
				return 0L;
			}
			listOctetCountMax = Math.Min(listOctetCountMax, destinationArray.LongLength);
			IntPtr lpNumberOfBytesRead = IntPtr.Zero;
			uint num = Kernel32.ReadProcessMemory(processHandle, intPtr.Value, destinationArray, (IntPtr)listOctetCountMax, out lpNumberOfBytesRead);
			long num2 = lpNumberOfBytesRead.ToInt64();
			if (num2 < int.MinValue || int.MaxValue < num2)
			{
				throw new ArgumentOutOfRangeException("readByteCountInt64=" + num2.ToString());
			}
			return lpNumberOfBytesRead.ToInt32();
		}

		public static IntPtr? CastToIntPtrAvoidingOverflow(this long @int)
		{
			if (4 == IntPtr.Size)
			{
				if (@int < 0)
				{
					return null;
				}
				if (uint.MaxValue < @int)
				{
					return null;
				}
			}
			return (IntPtr)(int)@int;
		}

		public static long Kernel32ReadProcessMemory(IntPtr processHandle, long address, long listOctetCountMax, uint[] destinationArray)
		{
			if (destinationArray == null)
			{
				return 0L;
			}
			IntPtr? intPtr = address.CastToIntPtrAvoidingOverflow();
			if (!intPtr.HasValue)
			{
				return 0L;
			}
			listOctetCountMax = Math.Min(listOctetCountMax, destinationArray.LongLength);
			IntPtr lpNumberOfBytesRead = IntPtr.Zero;
			uint num = Kernel32.ReadProcessMemory(processHandle, intPtr.Value, destinationArray, (IntPtr)listOctetCountMax, out lpNumberOfBytesRead);
			long num2 = lpNumberOfBytesRead.ToInt64();
			if (num2 < int.MinValue || int.MaxValue < num2)
			{
				throw new ArgumentOutOfRangeException("readByteCountInt64=" + num2.ToString());
			}
			return lpNumberOfBytesRead.ToInt32();
		}

		public static bool GültigeZaigerZiil(MEMORY_BASIC_INFORMATION memoryBasicInfo)
		{
			return memoryBasicInfo.State == ALLOCATION_TYPE.MEM_COMMIT && ((int)memoryBasicInfo.Protect & -256) == 0;
		}

		public static KeyValuePair<uint[], int> Raster32BitFromHBitmap(IntPtr hBitmap)
		{
			GDI32.BITMAP_STRUCT res = default(GDI32.BITMAP_STRUCT);
			GDI32.GetBitmapStruct(hBitmap, Marshal.SizeOf(typeof(GDI32.BITMAP_STRUCT)), out res);
			if (32 != res.bmBitsPixel)
			{
				return default(KeyValuePair<uint[], int>);
			}
			int bmWidth = res.bmWidth;
			int bmHeight = res.bmHeight;
			uint[] array = new uint[bmWidth * bmHeight];
			GDI32.GetBitmapBits(hBitmap, array.Length * 4, array);
			return new KeyValuePair<uint[], int>(array, bmWidth);
		}

		public static KeyValuePair<uint[], int> Raster32BitVonClientRectVonWindowMitHandle(IntPtr windowHandle, out RectInt windowClientRect, bool flagCaptureblt = false)
		{
			windowClientRect = RectInt.Empty;
			IntPtr dC = User32.GetDC(windowHandle);
			try
			{
				if (!(IntPtr.Zero == dC))
				{
					User32.GetClientRect(windowHandle, out RECT lpRect);
					POINT lpPoint = lpRect.LeftTop;
					POINT lpPoint2 = lpRect.RightBottom;
					User32.ClientToScreen(windowHandle, ref lpPoint);
					User32.ClientToScreen(windowHandle, ref lpPoint2);
					windowClientRect = RectInt.FromMinPointAndMaxPoint(lpPoint.AsVektor2DInt(), lpPoint2.AsVektor2DInt());
					int nWidth = lpRect.Right - lpRect.Left;
					int nHeight = lpRect.Bottom - lpRect.Top;
					IntPtr intPtr = GDI32.CreateCompatibleDC(dC);
					IntPtr intPtr2 = IntPtr.Zero;
					try
					{
						intPtr2 = GDI32.CreateCompatibleBitmap(dC, nWidth, nHeight);
						if (intPtr2 == IntPtr.Zero)
						{
							return default(KeyValuePair<uint[], int>);
						}
						IntPtr hObject = GDI32.SelectObject(intPtr, intPtr2);
						int num = 13369376;
						if (flagCaptureblt)
						{
							num |= 0x40000000;
						}
						GDI32.BitBlt(intPtr, 0, 0, nWidth, nHeight, dC, 0, 0, num);
						GDI32.SelectObject(intPtr, hObject);
					}
					finally
					{
						GDI32.DeleteDC(intPtr);
					}
					try
					{
						return Raster32BitFromHBitmap(intPtr2);
					}
					finally
					{
						GDI32.DeleteObject(intPtr2);
					}
				}
				return default(KeyValuePair<uint[], int>);
			}
			finally
			{
				User32.ReleaseDC(windowHandle, dC);
			}
		}

		public static KeyValuePair<uint[], int> Raster32BitVonClientRectVonWindowMitHandle(IntPtr windowHandle, bool flagCaptureblt = false)
		{
			RectInt windowClientRect;
			return Raster32BitVonClientRectVonWindowMitHandle(windowHandle, out windowClientRect, flagCaptureblt);
		}

		public static Vektor2DInt? User32GetCursorPos()
		{
			if (!User32.GetCursorPos(out POINT pos))
			{
				return null;
			}
			return pos.AsVektor2DInt();
		}
	}
}
