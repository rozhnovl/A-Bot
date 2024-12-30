using System;
using System.Linq;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjLong : SictAuswertPythonObjVar
{
	public const int IntBeginOktetIndex = 12;

	public byte[] WertSictListeOktet { get; private set; }

	public long WertSictIntModulo64Abbild { get; private set; }

	public SictAuswertPythonObjLong(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12 + 2 * base.ob_size);
	}

	public override void Ersctele(IMemoryReader DaatenKwele)
	{
		base.Ersctele(DaatenKwele);
		LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(DaatenKwele);
		byte[] listeOktet = base.ListeOktet;
		if (listeOktet != null)
		{
			WertSictListeOktet = listeOktet.Skip(12).ToArray();
			WertSictIntModulo64Abbild = WertSictIntModulo64(WertSictListeOktet);
		}
	}

	public static long WertSictIntModulo64(byte[] WertSictListeOktet)
	{
		if (WertSictListeOktet == null)
		{
			return 0L;
		}
		if (8 <= WertSictListeOktet.Length)
		{
			return BitConverter.ToInt64(WertSictListeOktet, 0);
		}
		byte[] array = new byte[8];
		for (int i = 0; i < WertSictListeOktet.Length; i++)
		{
			array[i] = WertSictListeOktet[i];
		}
		return BitConverter.ToInt64(array, 0);
	}
}
