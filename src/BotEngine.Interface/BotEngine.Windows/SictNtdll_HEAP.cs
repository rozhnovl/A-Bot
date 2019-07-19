using Bib3;

namespace BotEngine.Windows
{
	public class SictNtdll_HEAP
	{
		[SictVersazInDaatesctrukturAtribut(128, 3, 4)]
		public long SegmentListEntryFlink;

		[SictVersazInDaatesctrukturAtribut(160, 3, 4)]
		public long SegmentListEntryBlink;

		[SictVersazInDaatesctrukturAtribut(192, 3, 4)]
		public long Heap;

		[SictVersazInDaatesctrukturAtribut(224, 3, 4)]
		public long BaseAddress;

		[SictVersazInDaatesctrukturAtribut(256, 3)]
		public uint NumberOfPages;

		[SictVersazInDaatesctrukturAtribut(288, 3, 4)]
		public long FirstEntry;

		[SictVersazInDaatesctrukturAtribut(320, 3, 4)]
		public long LastValidEntry;

		[SictVersazInDaatesctrukturAtribut(352, 3)]
		public uint NumberOfUnCommittedPages;

		[SictVersazInDaatesctrukturAtribut(608, 3)]
		public uint EncodeFlagMask;

		[SictVersazInDaatesctrukturAtribut(640, 3)]
		public ulong Encoding;

		[SictVersazInDaatesctrukturAtribut(1024, 3)]
		public ushort ProcessHeapsListIndex;

		[SictVersazInDaatesctrukturAtribut(1280, 3, 4)]
		public long VirtualAllocdBlocksFlink;

		[SictVersazInDaatesctrukturAtribut(1312, 3, 4)]
		public long VirtualAllocdBlocksBlink;

		[SictVersazInDaatesctrukturAtribut(1344, 3, 4)]
		public long SegmentListFlink;

		[SictVersazInDaatesctrukturAtribut(1376, 3, 4)]
		public long SegmentListBlink;

		[SictVersazInDaatesctrukturAtribut(1472, 3, 4)]
		public long BlocksIndex;

		[SictVersazInDaatesctrukturAtribut(1568, 3)]
		public int FreeLists;

		[SictVersazInDaatesctrukturAtribut(1696, 3, 4)]
		public long FrontEndHeap;

		public SictNtdll_HEAP()
		{
		}

		public SictNtdll_HEAP(byte[] sictListeOktetAbbild, SictStructAusrictungTyp sctrukturAusrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)sctrukturAusrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
