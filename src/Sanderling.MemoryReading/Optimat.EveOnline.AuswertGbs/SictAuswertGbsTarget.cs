using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsTarget
{
	public readonly UINodeInfoInTree TargetAst;

	public UINodeInfoInTree AstBarAndImageCont { get; private set; }

	public UINodeInfoInTree AstLabelContainer { get; private set; }

	public UINodeInfoInTree AstAssignedPar { get; private set; }

	public UINodeInfoInTree AssignedContainerAst { get; private set; }

	public UINodeInfoInTree[] MengeAssignedModuleOderDroneGrupeAst { get; private set; }

	public UINodeInfoInTree AstSymboolActive { get; private set; }

	public UINodeInfoInTree AstTargetHealthBars { get; private set; }

	public UINodeInfoInTree AstTargetHealthBarsShield { get; private set; }

	public UINodeInfoInTree AstTargetHealthBarsArmor { get; private set; }

	public UINodeInfoInTree AstTargetHealthBarsHull { get; private set; }

	public ShipUiTarget Ergeebnis { get; private set; }

	public SictAuswertGbsTarget(UINodeInfoInTree targetElement)
	{
		TargetAst = targetElement;
	}

	private static string AusTargetLabelStringEntferneFormiirung(string stringFormiirt)
	{
		return stringFormiirt?.RemoveXmlTag();
	}

	public static int? AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(UINodeInfoInTree uiElement)
	{
		if (uiElement == null)
		{
			return null;
		}
		double? lastStateFloat = uiElement.LastStateFloat;
		if (!lastStateFloat.HasValue)
		{
			return null;
		}
		return (int)(lastStateFloat.Value * 1000.0);
	}

	private static ShipUiTargetAssignedGroup AusAstBerecneAssignedModuleOderDroneGrupe(UINodeInfoInTree ast)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		if (ast == null)
		{
			return null;
		}
		bool? flag = null;
		UINodeInfoInTree uINodeInfoInTree = ast.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Sprite", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		UINodeInfoInTree uINodeInfoInTree2 = ast.FirstMatchingNodeFromSubtreeBreadthFirst(Glob.GbsAstTypeIstEveIcon, 2, 1);
		if (uINodeInfoInTree != null)
		{
			string hint = uINodeInfoInTree.Hint;
			if (hint != null)
			{
				flag = Regex.Match(hint, "Drone", RegexOptions.IgnoreCase).Success;
			}
		}
		long? num = uINodeInfoInTree2?.TextureIdent0;
		return new ShipUiTargetAssignedGroup(ast.AsUIElementIfVisible())
		{
			IconTexture = (num.HasValue ? num.GetValueOrDefault().AsObjectIdInMemory() : null)
		};
	}

	public void Berecne()
	{
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Expected O, but got Unknown
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Expected O, but got Unknown
		UINodeInfoInTree targetAst = TargetAst;
		if (targetAst == null)
		{
			return;
		}
		AstBarAndImageCont = targetAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("barAndImageCont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstLabelContainer = targetAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("labelContainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstAssignedPar = targetAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("assignedPar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AssignedContainerAst = targetAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("assigned", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);
		MengeAssignedModuleOderDroneGrupeAst = AssignedContainerAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree c) => c.PyObjTypNameIsContainer() || c.PyObjTypNameMatchesRegexPatternIgnoreCase("Weapon"), null, 1, 1, omitNodesBelowNodesMatchingPredicate: true);
		AstSymboolActive = AstBarAndImageCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("ActiveTargetOnBracket", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);
		AstTargetHealthBars = AstBarAndImageCont.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("TargetHealthBars", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);
		AstTargetHealthBarsShield = AstTargetHealthBars.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("shieldBar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstTargetHealthBarsArmor = AstTargetHealthBars.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("armorBar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		AstTargetHealthBarsHull = AstTargetHealthBars.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("hullBar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);
		UINodeInfoInTree[] array = AstLabelContainer.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2);
		if (array == null)
		{
			return;
		}
		ShipHitpointsAndEnergy hitpoints = new ShipHitpointsAndEnergy
		{
			Struct = AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(AstTargetHealthBarsHull),
			Armor = AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(AstTargetHealthBarsArmor),
			Shield = AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(AstTargetHealthBarsShield)
		};
		KeyValuePair<UINodeInfoInTree, string>[] source = (from kandidaat in array
			select new KeyValuePair<UINodeInfoInTree, string>(kandidaat, kandidaat.SetText) into kandidaat
			where kandidaat.Value != null && kandidaat.Key.LaagePlusVonParentErbeLaage().HasValue
			select kandidaat).ToArray();
		KeyValuePair<UINodeInfoInTree, string>[] array2 = source.OrderBy((KeyValuePair<UINodeInfoInTree, string> kandidaat) => kandidaat.Key.LaagePlusVonParentErbeLaage().Value.B).ToArray();
		if (array2.Length < 2 || 4 < array2.Length)
		{
			return;
		}
		int index = array2.Length - 1;
		string value = array2.ElementAtOrDefault(index).Value;
		string[] array3 = (from zaileStringFormiirt in array2.Take(array2.Length - 1)
			select AusTargetLabelStringEntferneFormiirung(zaileStringFormiirt.Value) into zaileBescriftung
			where !zaileBescriftung.IsNullOrEmpty()
			select zaileBescriftung).ToArray();
		bool? isSelected = null;
		if (AstSymboolActive != null)
		{
			isSelected = AstSymboolActive.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => true == kandidaat.VisibleIncludingInheritance, 2, 1) != null;
		}
		ShipUiTargetAssignedGroup[] assigned = MengeAssignedModuleOderDroneGrupeAst?.Select((UINodeInfoInTree modulOderDroneGrupeAssignedAst) => AusAstBerecneAssignedModuleOderDroneGrupe(modulOderDroneGrupeAssignedAst))?.Where((ShipUiTargetAssignedGroup modulOderDroneGrupeAssigned) => modulOderDroneGrupeAssigned != null)?.ToArrayIfNotEmpty();
		IUIElement val = AstBarAndImageCont?.AsUIElementIfVisible();
		val = ((val != null) ? val.WithRegionSizePivotAtCenter(val.Region.Value.Size() * 7L / 10L) : null);
		Ergeebnis = new ShipUiTarget(targetAst.AsUIElementIfVisible())
		{
			LabelText = targetAst?.ExtraktMengeLabelString()?.OrdnungLabel<IUIElementText>()?.ToArray(),
			IsSelected = isSelected,
			Hitpoints = (IShipHitpointsAndEnergy)(object)hitpoints,
			RegionInteractionElement = val,
			Assigned = assigned
		};
	}
}
