using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32MitBaiPlus8RefZuusctand : SictAuswertPyObj32Zuusctand
{
	public uint BaiPlus8Ref { get; private set; }

	public SictAuswertPyObj32MitBaiPlus8RefZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		Geändert = false;
		base.Aktualisiire(AusProzesLeeser, out var Geändert2, Zait, ZuLeeseListeOktetAnzaal);
		Geändert = Geändert2;
		BaiPlus8Ref = base.ObjektBegin.BaiPlus8UInt32;
	}
}
