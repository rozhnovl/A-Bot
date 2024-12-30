using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsShipUiEWarElement
{
	public readonly UINodeInfoInTree EWarElementAst;

	public UINodeInfoInTree EWarButtonAst { get; private set; }

	public UINodeInfoInTree IconAst { get; private set; }

	public ShipUiEWarElement Ergeebnis { get; private set; }

	public SictAuswertGbsShipUiEWarElement(UINodeInfoInTree eWarElementNode)
	{
		EWarElementAst = eWarElementNode;
	}

	public void Berecne()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		IUIElement val = EWarElementAst?.AsUIElementIfVisible();
		if (val == null)
		{
			return;
		}
		EWarButtonAst = EWarElementAst;
		IconAst = EWarElementAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) || string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		if (IconAst?.VisibleIncludingInheritance ?? false)
		{
			string eWarType = EWarElementAst?.Name;
			ShipUiEWarElement val2 = new ShipUiEWarElement(val)
			{
				EWarType = eWarType
			};
			UINodeInfoInTree iconAst = IconAst;
			object iconTexture;
			if (iconAst == null)
			{
				iconTexture = null;
			}
			else
			{
				ref long? textureIdent = ref iconAst.TextureIdent0;
				iconTexture = (textureIdent.HasValue ? textureIdent.GetValueOrDefault().AsObjectIdInMemory() : null);
			}
			val2.IconTexture = (IObjectIdInMemory)iconTexture;
			Ergeebnis = val2;
		}
	}
}
