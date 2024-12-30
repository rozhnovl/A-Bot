using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32Float64Zuusctand : SictAuswertPyObj32Zuusctand
{
	public double WertFloat64 { get; private set; }

	public SictAuswertPyObj32Float64Zuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(AusProzesLeeser, out Geändert, Zait, ZuLeeseListeOktetAnzaal);
		WertFloat64 = base.ObjektBegin.BaiPlus8Double;
	}
}
