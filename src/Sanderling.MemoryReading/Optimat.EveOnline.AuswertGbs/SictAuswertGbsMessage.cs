using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsMessage
{
	public readonly UINodeInfoInTree AstMessage;

	public UINodeInfoInTree AstLabel { get; private set; }

	public string AstLabelSetText { get; private set; }

	public string AstLabelText { get; private set; }

	public IUIElementText Ergeebnis { get; private set; }

	public SictAuswertGbsMessage(UINodeInfoInTree AstMessage)
	{
		this.AstMessage = AstMessage;
	}

	public void Berecne()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		if (AstMessage != null && true == AstMessage.VisibleIncludingInheritance)
		{
			AstLabel = AstMessage.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Label", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			if (AstLabel != null)
			{
				AstLabelSetText = AstLabel.SetText;
			}
			Ergeebnis = (IUIElementText)new UIElementText(AstMessage.AsUIElementIfVisible(), (string)null)
			{
				Text = AstLabelText
			};
		}
	}
}
