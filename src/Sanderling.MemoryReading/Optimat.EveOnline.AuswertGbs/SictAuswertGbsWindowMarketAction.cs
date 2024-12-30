using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowMarketAction : SictAuswertGbsWindow
{
	public WindowMarketAction ErgeebnisScpez;

	public new static WindowMarketAction BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowMarketAction sictAuswertGbsWindowMarketAction = new SictAuswertGbsWindowMarketAction(windowAst);
		sictAuswertGbsWindowMarketAction.Berecne();
		return sictAuswertGbsWindowMarketAction.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowMarketAction(UINodeInfoInTree windowNode)
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
			ErgeebnisScpez = new WindowMarketAction((IWindow)(object)ergeebnis);
		}
	}
}
