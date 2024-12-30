using System;
using System.Collections.Generic;
using System.Linq;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32LongZuusctand : SictAuswertPyObj32VarZuusctand
{
	public const int IntBeginOktetIndex = 12;

	public byte[] WertSictListeOktet { get; private set; }

	public long WertSictIntModulo64Abbild { get; private set; }

	public SictAuswertPyObj32LongZuusctand(long herkunftAdrese, long beginZait)
		: base(herkunftAdrese, beginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader ausProzesLeeser, out bool geändert, long zait, int? zuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(ausProzesLeeser, out geändert, zait, zuLeeseListeOktetAnzaal);
		int num = base.ob_size;
		ObjektListeOktetAnzaal = Math.Min(256, 12 + 2 * num);
		if (base.AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektListeOktetAnzaal)
		{
			base.Aktualisiire(ausProzesLeeser, out geändert, zait, (int?)Math.Max(ObjektListeOktetAnzaal, zuLeeseListeOktetAnzaal ?? 0));
		}
		KeyValuePair<byte[], int> ausScpaicerLeeseLezteListeOktetUndAnzaal = base.AusScpaicerLeeseLezteListeOktetUndAnzaal;
		byte[] wertSictListeOktet = ((ausScpaicerLeeseLezteListeOktetUndAnzaal.Key == null) ? null : ausScpaicerLeeseLezteListeOktetUndAnzaal.Key.Skip(12).Take(ausScpaicerLeeseLezteListeOktetUndAnzaal.Value).ToArray());
		WertSictIntModulo64Abbild = SictAuswertPythonObjLong.WertSictIntModulo64(wertSictListeOktet);
	}
}
