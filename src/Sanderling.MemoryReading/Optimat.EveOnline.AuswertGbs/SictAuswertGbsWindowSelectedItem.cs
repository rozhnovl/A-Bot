using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowSelectedItem : SictAuswertGbsWindow
{
	public UINodeInfoInTree AstMainContainerMainToparea { get; private set; }

	public UINodeInfoInTree AstMainContainerMainTopareaLabelNameUndDistance { get; private set; }

	public string SelectedItemTextNameUndDistance { get; private set; }

	public string[] SelectedItemTextNameUndDistanceListeZaile { get; private set; }

	public string SelectedItemName { get; private set; }

	public string SelectedItemDistanceSictString { get; private set; }

	public long? SelectedItemDistanceScrankeMin { get; private set; }

	public long? SelectedItemDistanceScrankeMax { get; private set; }

	public WindowSelectedItemView ErgeebnisScpez { get; private set; }

	public new static WindowSelectedItemView BerecneFürWindowAst(UINodeInfoInTree windowAst)
	{
		if (windowAst == null)
		{
			return null;
		}
		SictAuswertGbsWindowSelectedItem sictAuswertGbsWindowSelectedItem = new SictAuswertGbsWindowSelectedItem(windowAst);
		sictAuswertGbsWindowSelectedItem.Berecne();
		return sictAuswertGbsWindowSelectedItem.ErgeebnisScpez;
	}

	public SictAuswertGbsWindowSelectedItem(UINodeInfoInTree windowSelectedItem)
		: base(windowSelectedItem)
	{
	}

	public override void Berecne()
	{
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		base.Berecne();
		Window ergeebnis = base.Ergeebnis;
		if (ergeebnis != null)
		{
			AstMainContainerMainToparea = base.AstMainContainerMain.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("toparea", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
			AstMainContainerMainTopareaLabelNameUndDistance = AstMainContainerMainToparea.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2);
			if (AstMainContainerMainTopareaLabelNameUndDistance != null)
			{
				SelectedItemTextNameUndDistance = AstMainContainerMainTopareaLabelNameUndDistance.SetText;
			}
			UINodeInfoInTree uINodeInfoInTree = base.AstMainContainerMain?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("action"));
			UINodeInfoInTree[][] array = base.AstMainContainerMain?.ListPathToNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameIsSprite())?.ToArray();
			if (SelectedItemTextNameUndDistance != null)
			{
				SelectedItemTextNameUndDistanceListeZaile = SelectedItemTextNameUndDistance.Split(new string[1] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
				SelectedItemName = SelectedItemTextNameUndDistanceListeZaile.ElementAtOrDefault(0);
				SelectedItemDistanceSictString = SelectedItemTextNameUndDistanceListeZaile.ElementAtOrDefault(1);
			}
			WindowSelectedItemView ergeebnisScpez = new WindowSelectedItemView((IWindow)(object)ergeebnis);
			ErgeebnisScpez = ergeebnisScpez;
		}
	}
}
