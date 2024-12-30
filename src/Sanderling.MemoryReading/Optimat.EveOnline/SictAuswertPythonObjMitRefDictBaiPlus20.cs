using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjMitRefDictBaiPlus20 : SictAuswertPythonObjMitRefBaiPlus20, ISictAuswertPythonObjMitRefDict
{
	public long RefDict => base.RefBaiOktet20;

	public SictAuswertPythonObjDict Dict { get; set; }

	public SictAuswertPythonObjMitRefDictBaiPlus20(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 24);
	}
}
