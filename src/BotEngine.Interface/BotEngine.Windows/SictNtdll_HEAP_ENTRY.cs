using Bib3;

namespace BotEngine.Windows
{
	public class SictNtdll_HEAP_ENTRY
	{
		[SictVersazInDaatesctrukturAtribut(0, 3)]
		public ushort Size;

		[SictVersazInDaatesctrukturAtribut(16, 3)]
		public byte Flags;

		[SictVersazInDaatesctrukturAtribut(24, 3)]
		public byte SmallTagIndex;

		[SictVersazInDaatesctrukturAtribut(32, 3)]
		public ushort PreviousSize;

		[SictVersazInDaatesctrukturAtribut(48, 3)]
		public byte SegmentOffset;

		[SictVersazInDaatesctrukturAtribut(56, 3)]
		public byte ExtendedBlockSignature;

		public SictNtdll_HEAP_ENTRY()
		{
		}

		public SictNtdll_HEAP_ENTRY(byte[] sictListeOktetAbbild, SictStructAusrictungTyp ausrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)ausrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
