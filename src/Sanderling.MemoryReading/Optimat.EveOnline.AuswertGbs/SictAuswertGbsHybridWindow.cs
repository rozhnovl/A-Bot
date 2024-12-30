using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsHybridWindow : SictAuswertGbsMessageBox
{
	public HybridWindow ErgeebnisScpezHybridWindow { get; private set; }

	public new static HybridWindow BerecneFürWindowAst(UINodeInfoInTree WindowAst)
	{
		if (WindowAst == null)
		{
			return null;
		}
		SictAuswertGbsHybridWindow sictAuswertGbsHybridWindow = new SictAuswertGbsHybridWindow(WindowAst);
		sictAuswertGbsHybridWindow.Berecne();
		return sictAuswertGbsHybridWindow.ErgeebnisScpezHybridWindow;
	}

	public SictAuswertGbsHybridWindow(UINodeInfoInTree WindowAst)
		: base(WindowAst)
	{
	}

	public override void Berecne()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Berecne();
		MessageBox ergeebnisScpez = base.ErgeebnisScpez;
		if (ergeebnisScpez != null)
		{
			HybridWindow ergeebnisScpezHybridWindow = new HybridWindow(ergeebnisScpez);
			ErgeebnisScpezHybridWindow = ergeebnisScpezHybridWindow;
		}
	}
}
