using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32MitBaiPlus20RefZuusctand : SictAuswertPyObj32Zuusctand
{
	public uint BaiPlus20Ref { get; private set; }

	public SictAuswertPyObj32MitBaiPlus20RefZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
		ObjektListeOktetAnzaal = 24;
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		Geändert = false;
		base.Aktualisiire(AusProzesLeeser, out var Geändert2, Zait, ZuLeeseListeOktetAnzaal);
		Geändert = Geändert2;
		BaiPlus20Ref = base.ObjektBegin.BaiPlus20UInt32;
	}
}
