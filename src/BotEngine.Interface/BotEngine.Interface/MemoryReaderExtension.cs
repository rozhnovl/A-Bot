using BotEngine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BotEngine.Interface
{
	public static class MemoryReaderExtension
	{
		public const int Specialisation_RuntimeCost_BlockSize = 4096;

		public static byte[] ReadBytes(this IMemoryReader memoryReader, long address, int bytesCountMax)
		{
			byte[] array = new byte[bytesCountMax];
			int num = memoryReader.ReadBytes(address, bytesCountMax, array);
			if (num < 1)
			{
				return null;
			}
			if (num == array.Length)
			{
				return array;
			}
			byte[] array2 = new byte[num];
			Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
			return array2;
		}

		public static uint? ReadPointerPath32(this IMemoryReader memoryReader, KeyValuePair<string, uint[]> rootModuleNameAndListOffset)
		{
			return memoryReader.ReadPointerPath32(rootModuleNameAndListOffset.Key, rootModuleNameAndListOffset.Value);
		}

		public static uint? ReadPointerPath32(this IMemoryReader memoryReader, string rootModuleName, uint[] listOffset)
		{
			if (memoryReader == null)
			{
				return null;
			}
			if (listOffset == null)
			{
				return null;
			}
			if (listOffset.Length < 1)
			{
				return null;
			}
			uint num = 0u;
			if (rootModuleName != null)
			{
				MemoryReaderModuleInfo[] array = memoryReader.Modules();
				if (array == null)
				{
					return null;
				}
				MemoryReaderModuleInfo memoryReaderModuleInfo = array.FirstOrDefault((MemoryReaderModuleInfo module) => module.ModuleName.EqualsIgnoreCase(rootModuleName));
				if (memoryReaderModuleInfo == null)
				{
					return null;
				}
				num = (uint)memoryReaderModuleInfo.BaseAddress;
			}
			uint num2 = num;
			for (int i = 0; i < listOffset.Length - 1; i++)
			{
				uint num3 = listOffset[i];
				num2 += num3;
				uint? num4 = memoryReader.ReadUInt32(num2);
				if (!num4.HasValue)
				{
					return null;
				}
				num2 = num4.Value;
			}
			return num2 + listOffset.LastOrDefault();
		}

		public static uint? ReadUInt32(this IMemoryReader memoryReader, long address)
		{
			if (memoryReader == null)
			{
				return null;
			}
			uint[] array = memoryReader.ReadArray<uint>(address, 4);
			if (array == null)
			{
				return null;
			}
			if (array.Length < 1)
			{
				return null;
			}
			return array[0];
		}

		public static string ReadStringAsciiNullTerminated(this byte[] listeOktet, int beginIndex, int lengthMax = 4096)
		{
			if (listeOktet == null)
			{
				return null;
			}
			byte[] bytes = listeOktet.Skip(beginIndex).TakeWhile((byte @byte) => @byte != 0).ToArray();
			return Encoding.ASCII.GetString(bytes);
		}

		public static string ReadStringAsciiNullTerminated(this IMemoryReader memoryReader, long address, int lengthMax = 4096)
		{
			return memoryReader?.ReadBytes(address, lengthMax)?.ReadStringAsciiNullTerminated(0, lengthMax);
		}

		public static T[] ReadArray<T>(this IMemoryReader memoryReader, long address, int numberOfBytes) where T : struct
		{
			if (memoryReader == null)
			{
				return null;
			}
			byte[] array = memoryReader.ReadBytes(address, numberOfBytes);
			if (array == null)
			{
				return null;
			}
			int num = Marshal.SizeOf(typeof(T));
			int num2 = (array.Length - 1) / num + 1;
			T[] array2 = new T[num2];
			Buffer.BlockCopy(array, 0, array2, 0, array.Length);
			return array2;
		}

		public static IEnumerable<long> AddressesHoldingValue32Aligned32(this IMemoryReader memoryReader, uint searchedValue, long searchedRegionBegin, long searchedRegionEnd)
		{
			if (memoryReader == null)
			{
				yield break;
			}
			long firstBlockAddress = searchedRegionBegin / 4096 * 4096;
			long lastBlockAddress = searchedRegionEnd / 4096 * 4096;
			for (long blockAddress = firstBlockAddress; blockAddress <= lastBlockAddress; blockAddress += 4096)
			{
				uint[] blockValues = memoryReader.ReadArray<uint>(blockAddress, 4096);
				if (blockValues == null)
				{
					continue;
				}
				for (int inBlockIndex = 0; inBlockIndex < blockValues.Length; inBlockIndex++)
				{
					long address = blockAddress + inBlockIndex * 4;
					if (address >= searchedRegionBegin && searchedRegionEnd >= address && searchedValue == blockValues[inBlockIndex])
					{
						yield return address;
					}
				}
			}
		}
	}
}
