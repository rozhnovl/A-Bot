using System;
using System.Collections.Generic;

namespace BotEngine.Interface.Process.Snapshot
{
	public class SnapshotReader : IMemoryReader
	{
		public readonly KeyValuePair<long, byte[]>[] MemoryBaseAddressAndListOctet;

		public MemoryReaderModuleInfo[] Modules()
		{
			throw new NotImplementedException();
		}

		public int ReadBytes(long address, int bytesCount, byte[] destinationArray)
		{
			KeyValuePair<long, byte[]>[] memoryBaseAddressAndListOctet = MemoryBaseAddressAndListOctet;
			for (int i = 0; i < memoryBaseAddressAndListOctet.Length; i++)
			{
				KeyValuePair<long, byte[]> keyValuePair = memoryBaseAddressAndListOctet[i];
				long num = address - keyValuePair.Key;
				long num2 = keyValuePair.Value.Length - num;
				if (0 <= num && 0 < num2)
				{
					int num3 = (int)Math.Min(num2, bytesCount);
					Buffer.BlockCopy(keyValuePair.Value, (int)num, destinationArray, 0, num3);
					return num3;
				}
			}
			return 0;
		}

		public SnapshotReader(KeyValuePair<long, byte[]>[] baseAddressAndListOctet)
		{
			MemoryBaseAddressAndListOctet = baseAddressAndListOctet;
		}
	}
}
