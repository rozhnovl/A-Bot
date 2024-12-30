using System;
using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjTrinityTr2Sprite2dTexture : SictAuswertPythonObjMitRefBaiPlus8
{
	public SictAuswertPythonObjTrinityTr2Sprite2dTexture(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}

	public override int ListeOktetAnzaalBerecne()
	{
		return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
	}
}
