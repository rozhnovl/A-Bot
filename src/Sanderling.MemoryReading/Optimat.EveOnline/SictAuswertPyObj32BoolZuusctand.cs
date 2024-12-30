using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32BoolZuusctand : SictAuswertPyObj32Int32Zuusctand
{
	public bool? WertBool { get; private set; }

	public SictAuswertPyObj32BoolZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		base.Aktualisiire(AusProzesLeeser, out Geändert, Zait, ZuLeeseListeOktetAnzaal);
		WertBool = base.WertInt32 != 0;
	}
}
