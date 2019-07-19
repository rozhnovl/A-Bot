using Bib3;

namespace BotEngine.Windows
{
	public class SictNtdll_HEAP_LIST_LOOKUP
	{
		[SictVersazInDaatesctrukturAtribut(0, 3)]
		public int ExtendedLookup;

		[SictVersazInDaatesctrukturAtribut(32, 3)]
		public uint ArraySize;

		[SictVersazInDaatesctrukturAtribut(64, 3)]
		public uint ExtraItem;

		[SictVersazInDaatesctrukturAtribut(96, 3)]
		public uint ItemCount;

		[SictVersazInDaatesctrukturAtribut(128, 3)]
		public uint OutOfRangeItems;

		[SictVersazInDaatesctrukturAtribut(160, 3)]
		public uint BaseIndex;

		[SictVersazInDaatesctrukturAtribut(640, 3)]
		public ulong Encoding;

		[SictVersazInDaatesctrukturAtribut(1024, 3)]
		public ushort ProcessHeapsListIndex;

		public SictNtdll_HEAP_LIST_LOOKUP()
		{
		}

		public SictNtdll_HEAP_LIST_LOOKUP(byte[] sictListeOktetAbbild, SictStructAusrictungTyp ausrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)ausrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
