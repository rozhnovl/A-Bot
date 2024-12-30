using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32VarZuusctand : SictAuswertPyObj32Zuusctand
{
	public int ob_size { get; private set; }

	public SictAuswertPyObj32VarZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(AusProzesLeeser, out Geändert, Zait, ZuLeeseListeOktetAnzaal);
		ob_size = base.ObjektBegin.BaiPlus8Int32;
	}
}
