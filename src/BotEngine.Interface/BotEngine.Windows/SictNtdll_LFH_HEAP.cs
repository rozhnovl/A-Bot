using Bib3;

namespace BotEngine.Windows
{
	public class SictNtdll_LFH_HEAP
	{
		[SictVersazInDaatesctrukturAtribut(192, 3, 4)]
		public long SubSegmentZonesFlink;

		[SictVersazInDaatesctrukturAtribut(224, 3, 4)]
		public long SubSegmentZonesBlink;

		[SictVersazInDaatesctrukturAtribut(256, 3)]
		public uint ZoneBlockSize;

		[SictVersazInDaatesctrukturAtribut(288, 3, 4)]
		public long Heap;

		public SictNtdll_LFH_HEAP()
		{
		}

		public SictNtdll_LFH_HEAP(byte[] sictListeOktetAbbild, SictStructAusrictungTyp ausrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)ausrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
