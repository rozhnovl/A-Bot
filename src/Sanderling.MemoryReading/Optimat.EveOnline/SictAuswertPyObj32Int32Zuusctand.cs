using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32Int32Zuusctand : SictAuswertPyObj32Zuusctand
{
	public int WertInt32 { get; private set; }

	public SictAuswertPyObj32Int32Zuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(AusProzesLeeser, out Geändert, Zait, ZuLeeseListeOktetAnzaal);
		WertInt32 = base.ObjektBegin.BaiPlus8Int32;
	}
}
