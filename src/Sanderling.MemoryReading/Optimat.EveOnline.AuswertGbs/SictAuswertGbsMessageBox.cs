using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsMessageBox : SictAuswertGbsWindow
{
	public UINodeInfoInTree AstMainContainerBottom { get; private set; }

	public UINodeInfoInTree AstMainContainerTopParent { get; private set; }

	public UINodeInfoInTree AstMainContainerTopParentCaption { get; private set; }

	public UINodeInfoInTree AstMainContainerBottomButtonGroup { get; private set; }

	public MessageBox ErgeebnisScpez { get; private set; }

	public new static MessageBox BerecneFürWindowAst(UINodeInfoInTree windowNode)
	{
		if (windowNode == null)
		{
			return null;
		}
		SictAuswertGbsMessageBox sictAuswertGbsMessageBox = new SictAuswertGbsMessageBox(windowNode);
		sictAuswertGbsMessageBox.Berecne();
		return sictAuswertGbsMessageBox.ErgeebnisScpez;
	}

	public SictAuswertGbsMessageBox(UINodeInfoInTree windowNode)
		: base(windowNode)
	{
	}

	public override void Berecne()
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		base.Berecne();
		Window ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			UINodeInfoInTree astMainContainer = base.AstMainContainer;
			AstMainContainerTopParent = astMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("topParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstMainContainerTopParentCaption = AstMainContainerTopParent?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveCaptionLarge", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstMainContainerBottom = astMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("bottom", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			AstMainContainerBottomButtonGroup = AstMainContainerBottom?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveButtonGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("ButtonGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			if (AstMainContainerBottomButtonGroup != null)
			{
				string topCaptionText = ((AstMainContainerTopParentCaption == null) ? null : AstMainContainerTopParentCaption.LabelText());
				string mainEditText = null;
				ErgeebnisScpez = new MessageBox((IWindow)(object)ergeebnis)
				{
					TopCaptionText = topCaptionText,
					MainEditText = mainEditText
				};
			}
		}
	}
}
