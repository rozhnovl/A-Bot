using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsMenu
{
	public const string MenuEntryPyTypeName = "MenuEntryView";

	public static Menu ReadMenu(UINodeInfoInTree menuNode)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		if ((!(menuNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		UINodeInfoInTree[] array = menuNode.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat?.PyObjTypNameMatchesRegexPatternIgnoreCase("MenuEntryView") ?? false, null, 3, 1);
		IUIElement baseElement = menuNode.AsUIElementIfVisible();
		MenuEntry[] entry = (array?.Select(delegate(UINodeInfoInTree kandidaatAst)
		{
			IUIElement obj = baseElement;
			return ReadMenuEntry(kandidaatAst, (obj != null) ? obj.Region.Value : RectInt.Empty);
		}).ToArray())?.OrdnungLabel<MenuEntry>()?.ToArray();
		return new Menu(baseElement)
		{
			Entry = (IMenuEntry[])(object)entry
		};
	}

	public static MenuEntry ReadMenuEntry(UINodeInfoInTree entryNode, RectInt regionConstraint)
	{
		if ((!(entryNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		UINodeInfoInTree uINodeInfoInTree = entryNode.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("Fill", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1) ?? entryNode.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => Regex.Match(kandidaat.PyObjTypName ?? "", "Underlay", RegexOptions.IgnoreCase).Success, 2, 1);
		ColorORGB val = ((uINodeInfoInTree == null) ? null : ColorORGB.VonVal(uINodeInfoInTree.Color));
		bool? highlight = ((val != null) ? new bool?(200 < val.OMilli) : null);
		return entryNode.MenuEntry(regionConstraint, highlight);
	}
}
