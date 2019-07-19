using Bib3;

namespace BotEngine.Windows
{
	public class SictNtdll_HEAP_SUBSEGMENT
	{
		[SictVersazInDaatesctrukturAtribut(0, 3, 4)]
		public long LocalInfo;

		[SictVersazInDaatesctrukturAtribut(32, 3, 4)]
		public long UserBlocks;

		[SictVersazInDaatesctrukturAtribut(128, 3)]
		public ushort BlockSize;

		[SictVersazInDaatesctrukturAtribut(160, 3)]
		public ushort BlockCount;

		public SictNtdll_HEAP_SUBSEGMENT()
		{
		}

		public SictNtdll_HEAP_SUBSEGMENT(byte[] sictListeOktetAbbild, SictStructAusrictungTyp ausrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)ausrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
