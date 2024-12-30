using System.Collections.Generic;
using System.Linq;
using Bib3;
using Bib3.Geometrik;
using BotEngine;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public static class UINodeExtension
{
	public static IUIElement AsUIElementIfVisible(this UINodeInfoInTree uiNode)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		if ((!(uiNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		return (IUIElement)new UIElement((IObjectIdInMemory)new ObjectIdInMemory(uiNode.PyObjAddress ?? 0))
		{
			Region = (Glob.FläceAusGbsAstInfoMitVonParentErbe(uiNode) ?? RectInt.Empty),
			InTreeIndex = uiNode.InTreeIndex,
			ChildLastInTreeIndex = uiNode.ChildLastInTreeIndex
		};
	}

	public static bool AstMitHerkunftAdreseEnthaltAstMitHerkunftAdrese(this UINodeInfoInTree suuceWurzel, long enthaltendeAstHerkunftAdrese, long enthalteneAstHerkunftAdrese)
	{
		if (enthaltendeAstHerkunftAdrese == enthalteneAstHerkunftAdrese)
		{
			return true;
		}
		if (suuceWurzel == null)
		{
			return false;
		}
		return suuceWurzel.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(enthaltendeAstHerkunftAdrese)?.EnthaltAstMitHerkunftAdrese(enthalteneAstHerkunftAdrese) ?? false;
	}

	public static bool EnthaltAstMitHerkunftAdrese(this UINodeInfoInTree suuceWurzel, long astHerkunftAdrese)
	{
		return suuceWurzel?.AstEnthalteInBaum((UINodeInfoInTree kandidaatAst) => kandidaatAst != null && kandidaatAst.PyObjAddress == astHerkunftAdrese, (UINodeInfoInTree zuZerleegende) => zuZerleegende.ListChild) ?? false;
	}

	public static bool EnthaltAst(this UINodeInfoInTree suuceWurzel, UINodeInfoInTree node)
	{
		if (node == null)
		{
			return false;
		}
		return suuceWurzel?.AstEnthalteInBaum(node, (UINodeInfoInTree zuZerleegende) => zuZerleegende.ListChild) ?? false;
	}

	public static IEnumerable<T> TailmengeUnterste<T>(this IEnumerable<T> mengeAstRepr, UINodeInfoInTree uiTree) where T : class, IUIElement
	{
		if (mengeAstRepr == null)
		{
			return null;
		}
		if (uiTree == null)
		{
			return null;
		}
		return mengeAstRepr?.Where(delegate(T astRepr)
		{
			UINodeInfoInTree Ast = uiTree.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(((IObjectIdInt64)astRepr).Id);
			return Ast != null && !mengeAstRepr.Any(delegate(T andereAstRepr)
			{
				if (andereAstRepr == null)
				{
					return false;
				}
				if (andereAstRepr == astRepr)
				{
					return false;
				}
				UINodeInfoInTree uINodeInfoInTree = uiTree.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(((IObjectIdInt64)andereAstRepr).Id);
				return uINodeInfoInTree != Ast && Ast.EnthaltAst(uINodeInfoInTree);
			});
		});
	}

	public static float? LaagePlusVonParentErbeLaageA(this UINodeInfoInTree uiNode)
	{
		return uiNode?.LaagePlusVonParentErbeLaage()?.A;
	}

	public static float? LaagePlusVonParentErbeLaageB(this UINodeInfoInTree uiNode)
	{
		return uiNode?.LaagePlusVonParentErbeLaage()?.B;
	}

	public static MenuEntry MenuEntry(this UINodeInfoInTree menuEntryAst, RectInt regionConstraint, bool? highlight = null)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		Container val = menuEntryAst.AlsContainer(treatIconAsSprite: false, regionConstraint);
		if (val == null)
		{
			return null;
		}
		return new MenuEntry((IUIElement)(object)val)
		{
			HighlightVisible = highlight
		};
	}

	public static Window Window(this UINodeInfoInTree uiNode, bool? isModal, string caption, bool? headerButtonsVisible = null, Sprite[] headerButton = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		string text = null;
		if (uiNode != null)
		{
			text = uiNode.PyObjTypName;
		}
		return new Window(uiNode.AsUIElementIfVisible())
		{
			isModal = isModal,
			Caption = caption,
			HeaderButtonsVisible = headerButtonsVisible,
			HeaderButton = (ISprite[])(object)headerButton
		};
	}
}
