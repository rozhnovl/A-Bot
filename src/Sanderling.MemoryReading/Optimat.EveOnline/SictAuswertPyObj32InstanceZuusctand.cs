using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32InstanceZuusctand : SictAuswertPyObj32Zuusctand
{
	public SictAuswertPyObj32InstanceZuusctand(long herkunftAdrese, long beginZait)
		: base(herkunftAdrese, beginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader ausProzesLeeser, out bool geändert, long zait, int? zuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(ausProzesLeeser, out geändert, zait, zuLeeseListeOktetAnzaal);
		if (base.AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektListeOktetAnzaal)
		{
			base.Aktualisiire(ausProzesLeeser, out geändert, zait, Math.Max(ObjektListeOktetAnzaal, zuLeeseListeOktetAnzaal ?? 0));
		}
	}
}
