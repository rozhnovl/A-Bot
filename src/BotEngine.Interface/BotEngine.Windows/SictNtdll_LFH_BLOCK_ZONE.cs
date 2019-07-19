using Bib3;
using System;

namespace BotEngine.Windows
{
	public class SictNtdll_LFH_BLOCK_ZONE
	{
		[SictVersazInDaatesctrukturAtribut(0, 3, 4)]
		public long ListEntryFlink;

		[SictVersazInDaatesctrukturAtribut(32, 3, 4)]
		public long ListEntryBlink;

		[SictVersazInDaatesctrukturAtribut(64, 3, 4)]
		public long FreePointer;

		[SictVersazInDaatesctrukturAtribut(96, 3, 4)]
		public long Limit;

		public static int ListeOktetAnzaal(SictStructAusrictungTyp ausrictungTyp)
		{
			switch (ausrictungTyp)
			{
			case SictStructAusrictungTyp.SctrukturBezaicner_Windows7_32Bit:
				return 16;
			default:
				throw new NotImplementedException();
			}
		}

		public SictNtdll_LFH_BLOCK_ZONE()
		{
		}

		public SictNtdll_LFH_BLOCK_ZONE(byte[] sictListeOktetAbbild, SictStructAusrictungTyp ausrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)ausrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
