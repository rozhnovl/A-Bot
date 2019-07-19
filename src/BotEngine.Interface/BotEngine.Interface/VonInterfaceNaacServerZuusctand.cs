using Bib3;
using System.Collections.Generic;

namespace BotEngine.Interface
{
	public class VonInterfaceNaacServerZuusctand
	{
		public long Zait;

		public KeyValuePair<long, object>[] MengeFunkAusgefüürtAinmaal;

		public readonly List<byte[]> MengeAssemblyGelaadeIdent = new List<byte[]>();

		public readonly List<object> MengeKomponente = new List<object>();

		public WertZuZaitpunktStruct<string> ExceptionLezte;

		public WertZuZaitpunktStruct<string> AingangExceptionLezte;

		public readonly Queue<WertZuZaitpunktStruct<string>> BerictAinfacSclange = new Queue<WertZuZaitpunktStruct<string>>();
	}
}
