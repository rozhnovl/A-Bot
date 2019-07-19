using Bib3;

namespace BotEngine.Windows
{
	public class SictProcessEnvironmentBlock
	{
		[SictVersazInDaatesctrukturAtribut(192, 3)]
		[SictVersazInDaatesctrukturAtribut(384, 4)]
		public int ProcessHeap;

		[SictVersazInDaatesctrukturAtribut(1088, 3)]
		[SictVersazInDaatesctrukturAtribut(1856, 4)]
		public int NumberOfHeaps;

		[SictVersazInDaatesctrukturAtribut(1152, 3, 32)]
		[SictVersazInDaatesctrukturAtribut(1920, 4)]
		public long ProcessHeaps;

		public SictProcessEnvironmentBlock()
		{
		}

		public SictProcessEnvironmentBlock(byte[] sictListeOktetAbbild, SictStructAusrictungTyp processEnvironmentBlockAusrictungTyp)
		{
			SictVersazInDaatesctrukturAtribut.ScraibeNaacDaatesctruktuur(this, (int)processEnvironmentBlockAusrictungTyp, sictListeOktetAbbild, littleEndian: true);
		}
	}
}
