using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefDictBaiPlus12 : SictAuswertPythonObjMitRefBaiPlus12, ISictAuswertPythonObjMitRefDict
{
	public long RefDict => base.RefBaiOktet12;

	public SictAuswertPythonObjDict Dict { get; set; }

	public SictAuswertPythonObjMitRefDictBaiPlus12(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 16);
	}
}
