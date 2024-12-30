using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPyObj32TextureZuusctand : SictAuswertPyObj32MitBaiPlus8RefZuusctand
{
	public uint RefTexture { get; private set; }

	public SictAuswertPyObj32TextureZuusctand(long HerkunftAdrese, long BeginZait)
		: base(HerkunftAdrese, BeginZait)
	{
	}

	public override void Aktualisiire(IMemoryReader AusProzesLeeser, out bool Geändert, long Zait, int? ZuLeeseListeOktetAnzaal = null)
	{
		Geändert = false;
		base.Aktualisiire(AusProzesLeeser, out bool Geändert2, Zait, ZuLeeseListeOktetAnzaal);
		Geändert = Geändert2;
		RefTexture = base.BaiPlus8Ref;
	}
}
