using Bib3;
using BotEngine.Windows;
using System.Collections.Generic;
using System.Linq;

namespace BotEngine.Interface.Process.Snapshot
{
	public class ProcessSnapshot
	{
		public readonly RangeOfPages[] RangeOfPages;

		public readonly KeyValuePair<long, byte[]>[] MemoryBaseAddressAndListOctet;

		public ProcessSnapshot()
		{
		}

		public ProcessSnapshot(RangeOfPages[] rangeOfPages, KeyValuePair<long, byte[]>[] memoryBaseAddressAndListOctet)
		{
			RangeOfPages = rangeOfPages;
			MemoryBaseAddressAndListOctet = memoryBaseAddressAndListOctet;
		}

		public ProcessSnapshot(RangeOfPages[] setRangeOfPages)
			: this(setRangeOfPages?.Select((RangeOfPages rangeOfPages) => new RangeOfPages(rangeOfPages.BasicInfo))?.ToArray(), setRangeOfPages?.WhereNotDefault()?.Select((RangeOfPages rangeOfPages) => new KeyValuePair<long, byte[]>((long)rangeOfPages.BasicInfo.BaseAddress, rangeOfPages.MemoryFromBaseAddressListOctet))?.ToArray())
		{
		}
	}
}
