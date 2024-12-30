using System;
using System.Linq;
using System.Text;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjStr : SictAuswertPythonObjVar
{
	public const int StringBeginOktetIndex = 20;

	public int ob_shash { get; private set; }

	public int ob_sstate { get; private set; }

	public string String { get; private set; }

	public SictAuswertPythonObjStr(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 20 + base.ob_size);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		ob_shash = Int32AusListeOktet(12) ?? 0;
		ob_sstate = Int32AusListeOktet(16) ?? 0;
		LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(DaatenKwele);
		byte[] listeOktet = base.ListeOktet;
		if (listeOktet != null)
		{
			byte[] source = listeOktet.Skip(20).ToArray();
			String = Encoding.ASCII.GetString(source.TakeWhile((byte t) => t != 0).ToArray());
		}
	}
}
