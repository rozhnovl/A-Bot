using Bib3;

namespace BotEngine.Windows
{
	public class SictNtdll_LIST_ENTRY
	{
		[SictVersazInDaatesctrukturAtribut(0, 3, 4)]
		public long Flink;

		[SictVersazInDaatesctrukturAtribut(32, 3, 4)]
		public long Blink;

		public SictNtdll_LIST_ENTRY()
		{
		}

		public SictNtdll_LIST_ENTRY(byte[] sictListeOktetAbbild, SictStructAusrictungTyp sctrukturAusrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)sctrukturAusrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
