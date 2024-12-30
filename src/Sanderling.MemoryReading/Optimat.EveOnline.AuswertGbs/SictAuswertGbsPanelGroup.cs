using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsPanelGroup
{
	public readonly UINodeInfoInTree PanelGroupAst;

	public readonly string ContainerMengeEntryName;

	public UINodeInfoInTree ContainerMengeEntryAst { get; private set; }

	public PanelGroup Ergeebnis { get; private set; }

	public SictAuswertGbsPanelGroup(UINodeInfoInTree PanelGroupAst, string ContainerMengeEntryName = "Container")
	{
		this.PanelGroupAst = PanelGroupAst;
		this.ContainerMengeEntryName = ContainerMengeEntryName;
	}

	public virtual void Berecne()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		if (PanelGroupAst != null && true == PanelGroupAst.VisibleIncludingInheritance)
		{
			string ContainerMengeEntryName = this.ContainerMengeEntryName;
			ContainerMengeEntryAst = PanelGroupAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals(ContainerMengeEntryName, Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);
			PanelGroup ergeebnis = new PanelGroup((IUIElement)(object)PanelGroupAst.AlsContainer());
			Ergeebnis = ergeebnis;
		}
	}
}
