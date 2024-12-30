using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsLayerUtilmenu
{
	public readonly UINodeInfoInTree AstLayerUtilmenu;

	public UINodeInfoInTree AstHeader { get; private set; }

	public UINodeInfoInTree AstHeaderLabel { get; private set; }

	public IUIElementText Header { get; private set; }

	public string MenuTitel { get; private set; }

	public UINodeInfoInTree AstExpandedUtilMenu { get; private set; }

	public SictAuswertGbsLayerUtilmenu(UINodeInfoInTree AstLayerUtilmenu)
	{
		this.AstLayerUtilmenu = AstLayerUtilmenu;
	}

	public static bool KandidaatUtilmenuLaagePasendZuExpandedUtilmenu(UINodeInfoInTree ExpandedUtilMenuAst, UINodeInfoInTree KandidaatUtilmenuAst)
	{
		if (ExpandedUtilMenuAst == null || KandidaatUtilmenuAst == null)
		{
			return false;
		}
		Vektor2DSingle? vektor2DSingle = ExpandedUtilMenuAst.LaagePlusVonParentErbeLaage();
		if (!vektor2DSingle.HasValue)
		{
			return false;
		}
		Vektor2DSingle? vektor2DSingle2 = KandidaatUtilmenuAst.LaagePlusVonParentErbeLaage();
		Vektor2DSingle? grööse = KandidaatUtilmenuAst.Grööse;
		if (!grööse.HasValue)
		{
			return false;
		}
		if (!vektor2DSingle2.HasValue)
		{
			return false;
		}
		Vektor2DSingle vektor2DSingle3 = vektor2DSingle2.Value + new Vektor2DSingle(0f, grööse.Value.B);
		if (4.0 < (vektor2DSingle.Value + new Vektor2DSingle(0f, 1f) - vektor2DSingle3).Betraag)
		{
			return false;
		}
		return true;
	}

	public virtual void Berecne(UINodeInfoInTree[] MengeKandidaatUtilmenuAst)
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		if (AstLayerUtilmenu == null || true != AstLayerUtilmenu.VisibleIncludingInheritance)
		{
			return;
		}
		AstHeader = AstLayerUtilmenu.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => Kandidaat.PyObjTypNameIsContainer(), 2, 1);
		AstExpandedUtilMenu = AstLayerUtilmenu.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("ExpandedUtilMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		if (AstExpandedUtilMenu == null || !AstExpandedUtilMenu.LaagePlusVonParentErbeLaage().HasValue)
		{
			return;
		}
		UINodeInfoInTree rootNode = null;
		if (MengeKandidaatUtilmenuAst != null)
		{
			rootNode = MengeKandidaatUtilmenuAst.FirstOrDefault((UINodeInfoInTree Kandidaat) => KandidaatUtilmenuLaagePasendZuExpandedUtilmenu(AstExpandedUtilMenu, Kandidaat));
		}
		AstHeaderLabel = rootNode.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree Kandidaat) => string.Equals("EveLabelMedium", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		if (AstHeaderLabel != null)
		{
			Header = (IUIElementText)new UIElementText(AstHeaderLabel.AsUIElementIfVisible(), AstHeaderLabel.LabelText());
		}
		if (Header != null)
		{
			MenuTitel = Header.Text;
		}
	}
}
