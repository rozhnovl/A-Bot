using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsInfoPanelGen
{
	public readonly UINodeInfoInTree InfoPanelAst;

	public UINodeInfoInTree TopContAst { get; private set; }

	public UINodeInfoInTree HeaderBtnContAst { get; private set; }

	public UINodeInfoInTree HeaderBtnContExpandButtonAst { get; private set; }

	public UINodeInfoInTree HeaderContAst { get; private set; }

	public UINodeInfoInTree MainContAst { get; private set; }

	public InfoPanel Ergeebnis { get; private set; }

	public SictAuswertGbsInfoPanelGen(UINodeInfoInTree InfoPanelAst)
	{
		this.InfoPanelAst = InfoPanelAst;
	}

	public virtual void Berecne()
	{
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Expected O, but got Unknown
		if (InfoPanelAst != null && true == InfoPanelAst.VisibleIncludingInheritance)
		{
			TopContAst = InfoPanelAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("topCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			HeaderBtnContAst = TopContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("headerBtnCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			HeaderBtnContExpandButtonAst = HeaderBtnContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("Sprite", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			HeaderContAst = TopContAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer() && string.Equals("headerCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			MainContAst = InfoPanelAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("ContainerAutoSize", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) && string.Equals("mainCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
			Container expandedContent = MainContAst.AlsContainer();
			IUIElementText[] array = MainContAst?.ExtraktMengeLabelString()?.OrdnungLabel<IUIElementText>()?.ToArray();
			bool? isExpanded = null;
			IUIElement expandToggleButton = null;
			if (MainContAst != null)
			{
				isExpanded = MainContAst.VisibleIncludingInheritance;
			}
			if (HeaderBtnContExpandButtonAst != null)
			{
				expandToggleButton = (IUIElement)(object)HeaderBtnContExpandButtonAst.AlsSprite();
			}
			IUIElementText val = TopContAst?.ExtraktMengeLabelString()?.Grööste<IUIElementText>();
			Container headerContent = TopContAst?.AlsContainer();
			InfoPanel ergeebnis = new InfoPanel((IUIElement)(object)InfoPanelAst.AlsContainer())
			{
				IsExpanded = isExpanded,
				ExpandToggleButton = expandToggleButton,
				ExpandedContent = (IContainer)(object)expandedContent,
				HeaderContent = (IContainer)(object)headerContent
			};
			Ergeebnis = ergeebnis;
		}
	}
}
