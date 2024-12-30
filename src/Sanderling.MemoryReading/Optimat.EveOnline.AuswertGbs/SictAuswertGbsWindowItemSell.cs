using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowItemSell : SictAuswertGbsWindow
{
	public WindowItemSell ErgeebnisScpez;

	public new static WindowItemSell BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowItemSell sictAuswertGbsWindowItemSell = new SictAuswertGbsWindowItemSell(windowAst);
		sictAuswertGbsWindowItemSell.Berecne();
		return sictAuswertGbsWindowItemSell.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowItemSell(UINodeInfoInTree windowNode)
		: base(windowNode)
	{
	}

	public override void Berecne()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		base.Berecne();
		Window ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			ErgeebnisScpez = new WindowItemSell((IWindow)(object)ergeebnis);
		}
	}
}
