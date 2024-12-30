using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32StrZuusctand : SictAuswertPyObj32VarZuusctand
{
	public const int StringBeginOktetIndex = 20;

	public int ob_shash { get; private set; }

	public int ob_sstate { get; private set; }

	public string WertString { get; private set; }

	public SictAuswertPyObj32StrZuusctand(long herkunftAdrese, long beginZait)
		: base(herkunftAdrese, beginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader ausProzesLeeser, out bool geändert, long zait, int? zuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(ausProzesLeeser, out geändert, zait, zuLeeseListeOktetAnzaal);
		ob_shash = base.ObjektBegin.BaiPlus12Int32;
		ob_sstate = base.ObjektBegin.BaiPlus16Int32;
		int num = base.ob_size;
		ObjektListeOktetAnzaal = 20 + num;
		if (base.AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektListeOktetAnzaal)
		{
			base.Aktualisiire(ausProzesLeeser, out geändert, zait, (int?)Math.Max(ObjektListeOktetAnzaal, zuLeeseListeOktetAnzaal ?? 0));
		}
		KeyValuePair<byte[], int> ausScpaicerLeeseLezteListeOktetUndAnzaal = base.AusScpaicerLeeseLezteListeOktetUndAnzaal;
		byte[] array = ((ausScpaicerLeeseLezteListeOktetUndAnzaal.Key == null) ? null : ausScpaicerLeeseLezteListeOktetUndAnzaal.Key.Skip(20).Take(ausScpaicerLeeseLezteListeOktetUndAnzaal.Value).TakeWhile((byte zaicenOktet) => zaicenOktet != 0)
			.ToArray());
		WertString = ((array == null) ? null : Encoding.ASCII.GetString(array));
	}
}
