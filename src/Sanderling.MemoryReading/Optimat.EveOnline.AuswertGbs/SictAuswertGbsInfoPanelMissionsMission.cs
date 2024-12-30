using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsInfoPanelMissionsMission
{
	public readonly UINodeInfoInTree AstMission;

	public UINodeInfoInTree AstLabel { get; private set; }

	public UINodeInfoInTree[] MengeAstKandidaatMission { get; private set; }

	public IUIElementText Ergeebnis { get; private set; }

	public SictAuswertGbsInfoPanelMissionsMission(UINodeInfoInTree astMission)
	{
		AstMission = astMission;
	}

	public void Berecne()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		if (AstMission != null)
		{
			AstLabel = AstMission.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			if (AstLabel != null)
			{
				string text = AstLabel.LabelText();
				Ergeebnis = (IUIElementText)new UIElementText(AstMission.AsUIElementIfVisible(), text);
				Ergeebnis = Ergeebnis;
			}
		}
	}
}
