using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsTab
{
	public readonly UINodeInfoInTree TabAst;

	public UINodeInfoInTree LabelClipperAst { get; private set; }

	public UINodeInfoInTree LabelAst { get; private set; }

	public string LabelText { get; private set; }

	public ColorORGB LabelColor { get; private set; }

	public Tab Ergeebnis { get; private set; }

	public SictAuswertGbsTab(UINodeInfoInTree tabAst)
	{
		TabAst = tabAst;
	}

	public virtual void Berecne()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		if (TabAst == null || true != TabAst.VisibleIncludingInheritance)
		{
			return;
		}
		LabelAst = TabAst.LargestLabelInSubtree(3);
		if (LabelAst != null)
		{
			LabelColor = ColorORGB.VonVal(LabelAst.Color);
			LabelText = LabelAst.LabelText();
			if (LabelText != null && LabelColor != null)
			{
				int? oMilli = LabelColor.OMilli;
				UIElementText label = new UIElementText(LabelAst.AsUIElementIfVisible(), LabelText);
				Ergeebnis = new Tab(TabAst.AsUIElementIfVisible())
				{
					Label = (IUIElementText)(object)label,
					LabelColorOpacityMilli = oMilli
				};
			}
		}
	}
}
