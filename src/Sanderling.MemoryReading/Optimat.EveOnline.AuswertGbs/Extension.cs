using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using Bib3.RefBaumKopii;
using Bib3.RefNezDiferenz;
using Bib3.RefNezDiferenz.NewtonsoftJson;
using BotEngine;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs;

public static class Extension
{
	private static readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer KonvertGbsAstInfoRictliinieMitScatescpaicer = new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie(new KeyValuePair<Type, Type>[2]
	{
		new KeyValuePair<Type, Type>(typeof(GbsAstInfo), typeof(UINodeInfoInTree)),
		new KeyValuePair<Type, Type>(typeof(GbsAstInfo[]), typeof(UINodeInfoInTree[]))
	}));

	private static string[] UIRootVorgaabeGrööseListeName = new string[2] { "l_main", "l_viewstate" };

	public static Func<long, IEnumerable<KeyValuePair<string, SictAuswertPythonObj>>> FunkEnumDictEntry;

	public static Vektor2DInt AlsVektor2DInt(this Vektor2DSingle vektor2DSingle)
	{
		return new Vektor2DInt((long)vektor2DSingle.A, (long)vektor2DSingle.B);
	}

	public static UINodeInfoInTree SictAuswert(this GbsAstInfo gbsBaum)
	{
		if (gbsBaum == null)
		{
			return null;
		}
		if (!(SictRefNezKopii.ObjektKopiiErsctele(gbsBaum, null, new Param(null, KonvertGbsAstInfoRictliinieMitScatescpaicer), null) is UINodeInfoInTree uINodeInfoInTree))
		{
			return null;
		}
		int inBaumAstIndexZääler = 0;
		uINodeInfoInTree.AbgelaiteteAigescafteBerecne(ref inBaumAstIndexZääler);
		return uINodeInfoInTree;
	}

	public static IMemoryMeasurement SensorikScnapscusKonstrukt(this GbsAstInfo gbsBaum, int? sessionDurationRemaining)
	{
		UINodeInfoInTree gbsBaumWurzel = gbsBaum.SictAuswert();
		SictAuswertGbsAgr sictAuswertGbsAgr = new SictAuswertGbsAgr(gbsBaumWurzel);
		sictAuswertGbsAgr.Berecne(sessionDurationRemaining);
		return sictAuswertGbsAgr.AuswertErgeebnis;
	}

	public static IEnumerable<UINodeInfoInTree> BaumEnumFlacListeKnoote(this UINodeInfoInTree suuceWurzel, int? tiifeMax = null, int? tiifeMin = null)
	{
		return suuceWurzel.EnumerateNodeFromTreeBFirst((UINodeInfoInTree node) => node?.GetListChild()?.OfType<UINodeInfoInTree>(), tiifeMax, tiifeMin);
	}

	public static Vektor2DSingle? LaagePlusVonParentErbeLaage(this UINodeInfoInTree node)
	{
		Vektor2DSingle? vektor2DSingle = node?.FromParentLocation;
		if (!vektor2DSingle.HasValue)
		{
			return node.LaageInParent;
		}
		return node.LaageInParent + vektor2DSingle;
	}

	public static string LabelText(this UINodeInfoInTree node)
	{
		return node?.SetText;
	}

	public static void AbgelaiteteAigescafteBerecne(this UINodeInfoInTree node, ref int inBaumAstIndexZääler, int? tiifeMax = null, Vektor2DSingle? vonParentErbeLaage = null, float? vonParentErbeClippingFläceLinx = null, float? vonParentErbeClippingFläceOobn = null, float? vonParentErbeClippingFläceRecz = null, float? vonParentErbeClippingFläceUntn = null)
	{
		if (node == null || tiifeMax < 0)
		{
			return;
		}
		node.InTreeIndex = (inBaumAstIndexZääler += 1);
		node.FromParentLocation = vonParentErbeLaage;
		Vektor2DSingle? vonParentErbeLaage2 = node.LaageInParent;
		Vektor2DSingle? vektor2DSingle = node.LaagePlusVonParentErbeLaage();
		Vektor2DSingle? grööse = node.Grööse;
		float? num = vonParentErbeClippingFläceLinx;
		float? num2 = vonParentErbeClippingFläceOobn;
		float? num3 = vonParentErbeClippingFläceRecz;
		float? num4 = vonParentErbeClippingFläceUntn;
		if (vektor2DSingle.HasValue && grööse.HasValue)
		{
			num = Bib3.Glob.Max(num, vektor2DSingle.Value.A);
			num3 = Bib3.Glob.Min(num3, vektor2DSingle.Value.A);
			num2 = Bib3.Glob.Max(num2, vektor2DSingle.Value.B);
			num4 = Bib3.Glob.Min(num4, vektor2DSingle.Value.B);
		}
		if (vonParentErbeLaage.HasValue)
		{
			vonParentErbeLaage2 = ((!vonParentErbeLaage2.HasValue) ? vonParentErbeLaage : new Vektor2DSingle?(vonParentErbeLaage2.Value + vonParentErbeLaage.Value));
		}
		UINodeInfoInTree[] listChild = node.ListChild;
		for (int i = 0; i < listChild?.Length; i++)
		{
			UINodeInfoInTree uINodeInfoInTree = listChild[i];
			if (uINodeInfoInTree != null)
			{
				uINodeInfoInTree.InParentListChildIndex = i;
				uINodeInfoInTree.AbgelaiteteAigescafteBerecne(ref inBaumAstIndexZääler, tiifeMax - 1, vonParentErbeLaage2, num, num2, num3, num4);
			}
		}
		int?[] array = listChild?.Select((UINodeInfoInTree child) => child?.ChildLastInTreeIndex ?? child?.InTreeIndex)?.WhereNotDefault()?.ToArray();
		if (0 < array?.Length)
		{
			node.ChildLastInTreeIndex = array.Max();
		}
	}

	public static UINodeInfoInTree SuuceFlacMengeAstFrüheste(this UINodeInfoInTree[] suuceMengeWurzel, Func<UINodeInfoInTree, bool> prädikaat, int? tiifeMax = null, int? tiifeMin = null)
	{
		foreach (UINodeInfoInTree item in suuceMengeWurzel.EmptyIfNull())
		{
			UINodeInfoInTree uINodeInfoInTree = item.FirstMatchingNodeFromSubtreeBreadthFirst(prädikaat, tiifeMax, tiifeMin);
			if (uINodeInfoInTree != null)
			{
				return uINodeInfoInTree;
			}
		}
		return null;
	}

	public static T Grööste<T>(this IEnumerable<T> source) where T : class, IUIElement
	{
		object result;
		if (source == null)
		{
			result = null;
		}
		else
		{
			IOrderedEnumerable<T> orderedEnumerable = source.OrderByDescending((T element) => ((IUIElement)element).Region?.Area());
			result = ((orderedEnumerable != null) ? orderedEnumerable.FirstOrDefault() : null);
		}
		return (T)result;
	}

	public static T LargestNodeInSubtree<T>(this IEnumerable<T> source) where T : GbsAstInfo
	{
		object result;
		if (source == null)
		{
			result = null;
		}
		else
		{
			IOrderedEnumerable<T> orderedEnumerable = source.OrderByDescending((T element) => (element.GrööseA * element.GrööseB) ?? (-2.1474836E+09f));
			result = ((orderedEnumerable != null) ? orderedEnumerable.FirstOrDefault() : null);
		}
		return (T)result;
	}

	public static T GröösteSpriteAst<T>(this IEnumerable<T> source) where T : GbsAstInfo
	{
		object result;
		if (source == null)
		{
			result = null;
		}
		else
		{
			IEnumerable<T> enumerable = source.Where((T k) => k.PyObjTypNameIsSprite());
			result = ((enumerable != null) ? enumerable.LargestNodeInSubtree() : null);
		}
		return (T)result;
	}

	public static UINodeInfoInTree LargestLabelInSubtree(this UINodeInfoInTree rootNode, int? tiifeMax = null)
	{
		UINodeInfoInTree[] source = rootNode.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.GbsAstTypeIstLabel(), null, tiifeMax);
		UINodeInfoInTree uINodeInfoInTree = null;
		foreach (UINodeInfoInTree item in source.EmptyIfNull())
		{
			Vektor2DSingle? vektor2DSingle = item?.Grööse;
			if (vektor2DSingle.HasValue && (uINodeInfoInTree?.Grööse.Value.BetraagQuadriirt ?? (-1.0)) < vektor2DSingle.Value.BetraagQuadriirt)
			{
				uINodeInfoInTree = item;
			}
		}
		return uINodeInfoInTree;
	}

	public static UINodeInfoInTree[] MatchingNodesFromSubtreeBreadthFirst(this UINodeInfoInTree rootNode, Func<UINodeInfoInTree, bool> predicate, int? resultCountMax = null, int? depthBoundMax = null, int? depthBoundMin = null, bool omitNodesBelowNodesMatchingPredicate = false)
	{
		return rootNode.ListPathToNodeFromSubtreeBreadthFirst(predicate, resultCountMax, depthBoundMax, depthBoundMin, omitNodesBelowNodesMatchingPredicate)?.Select((UINodeInfoInTree[] astMitPfaad) => astMitPfaad.LastOrDefault()).ToArray();
	}

	public static UINodeInfoInTree FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(this UINodeInfoInTree node, long? pyObjAddress, int? depthBoundMax = null, int? depthBoundMin = null)
	{
		return node.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat.PyObjAddress == pyObjAddress, depthBoundMax, depthBoundMin);
	}

	public static UINodeInfoInTree FirstMatchingNodeFromSubtreeBreadthFirst(this UINodeInfoInTree rootNode, Func<UINodeInfoInTree, bool> predicate, int? depthBoundMax = null, int? depthBoundMin = null)
	{
		return rootNode?.MatchingNodesFromSubtreeBreadthFirst(predicate, 1, depthBoundMax, depthBoundMin, omitNodesBelowNodesMatchingPredicate: true)?.FirstOrDefault();
	}

	public static UINodeInfoInTree[][] ListPathToNodeFromSubtreeBreadthFirst(this UINodeInfoInTree rootNode, Func<UINodeInfoInTree, bool> predicate, int? ListeFundAnzaalScrankeMax = null, int? depthBoundMax = null, int? depthBoundMin = null, bool omitNodesBelowNodesMatchingPredicate = false)
	{
		if (rootNode == null)
		{
			return null;
		}
		return Bib3.Glob.SuuceFlacMengeAstMitPfaad(rootNode, predicate, (UINodeInfoInTree node) => node.ListChild, ListeFundAnzaalScrankeMax, depthBoundMax, depthBoundMin, omitNodesBelowNodesMatchingPredicate);
	}

	public static Vektor2DSingle? GrööseMaxAusListeChild(this UINodeInfoInTree Ast)
	{
		if (Ast == null)
		{
			return null;
		}
		Vektor2DSingle? result = null;
		Vektor2DSingle? grööse = Ast.Grööse;
		if (grööse.HasValue)
		{
			result = grööse;
		}
		UINodeInfoInTree[] listChild = Ast.ListChild;
		if (listChild != null)
		{
			UINodeInfoInTree[] array = listChild;
			foreach (UINodeInfoInTree uINodeInfoInTree in array)
			{
				Vektor2DSingle? grööse2 = uINodeInfoInTree.Grööse;
				if (grööse2.HasValue)
				{
					result = ((!result.HasValue) ? grööse2 : new Vektor2DSingle?(new Vektor2DSingle(Math.Max(result.Value.A, grööse2.Value.A), Math.Max(result.Value.B, grööse2.Value.B))));
				}
			}
		}
		return result;
	}

	public static Vektor2DSingle? GrööseAusListeChildFürScpezUIRootBerecne(this UINodeInfoInTree Ast)
	{
		if (Ast == null)
		{
			return null;
		}
		UINodeInfoInTree[] listChild = Ast.ListChild;
		if (listChild != null)
		{
			UINodeInfoInTree[] array = listChild;
			foreach (UINodeInfoInTree Child in array)
			{
				Vektor2DSingle? grööse = Child.Grööse;
				if (grööse.HasValue && UIRootVorgaabeGrööseListeName.Any((string AstNaame) => string.Equals(AstNaame, Child.Name)))
				{
					return grööse;
				}
			}
		}
		return null;
	}

	public static IUIElementText AsUIElementText(this UINodeInfoInTree GbsAst)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		return (IUIElementText)(((!(GbsAst?.VisibleIncludingInheritance)) ?? true) ? ((UIElementText)null) : new UIElementText(GbsAst.AsUIElementIfVisible(), GbsAst.LabelText() ?? GbsAst.Text));
	}

	public static IUIElementInputText AsUIElementInputText(this UINodeInfoInTree GbsAst)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		IUIElementText val = GbsAst?.AsUIElementText();
		return (IUIElementInputText)((val == null) ? ((UIElementInputText)null) : new UIElementInputText(val));
	}

	public static IUIElementText AsUIElementTextIfTextNotEmpty(this UINodeInfoInTree GbsAst)
	{
		IUIElementText val = GbsAst?.AsUIElementText();
		if (((val != null) ? val.Text : null).IsNullOrEmpty())
		{
			return null;
		}
		return val;
	}

	public static IEnumerable<IUIElementText> ExtraktMengeLabelString(this UINodeInfoInTree GbsAst)
	{
		return GbsAst?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => kandidaat?.VisibleIncludingInheritance ?? false)?.Select(AsUIElementTextIfTextNotEmpty)?.WhereNotDefault();
	}

	public static IEnumerable<IUIElementText> ExtraktMengeButtonLabelString(this UINodeInfoInTree GbsAst)
	{
		return (IEnumerable<IUIElementText>)(GbsAst?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => (kandidaat?.VisibleIncludingInheritance ?? false) && Regex.Match(kandidaat?.PyObjTypName ?? "", "button", RegexOptions.IgnoreCase).Success)?.Select((UINodeInfoInTree kandidaatButtonAst) => new
		{
			ButtonAst = kandidaatButtonAst,
			LabelAst = kandidaatButtonAst.LargestLabelInSubtree()
		})?.GroupBy(buttonAstUndLabelAst => buttonAstUndLabelAst.LabelAst)?.Select(GroupLabelAst => new
		{
			ButtonAst = (from buttonAstUndLabelAst in GroupLabelAst
				select buttonAstUndLabelAst.ButtonAst into kandidaatButtonAst
				orderby kandidaatButtonAst.InTreeIndex
				select kandidaatButtonAst).LastOrDefault(),
			LabelAst = GroupLabelAst.Key
		})?.Select(buttonAstUndLabelAst => new UIElementText(buttonAstUndLabelAst.ButtonAst.AsUIElementIfVisible(), buttonAstUndLabelAst?.LabelAst?.LabelText()))?.Where((UIElementText kandidaat) => !((kandidaat != null) ? kandidaat.Text : null).IsNullOrEmpty()));
	}

	public static IEnumerable<T> OrdnungLabel<T>(this IEnumerable<T> Menge) where T : IUIElement
	{
		return Menge?.OrderBy((T element) => ((((IUIElement)(element)).Region)?.Center())?.B ?? int.MaxValue)?.ThenBy((T element) => ((((IUIElement)(element)).Region)?.Center())?.A ?? int.MaxValue);
	}

	public static Sprite AlsSprite(this UINodeInfoInTree GbsAst)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		object result;
		if (GbsAst?.VisibleIncludingInheritance ?? false)
		{
			throw new NotImplementedException();
			/*Sprite val = new Sprite(GbsAst.AsUIElementIfVisible())
			{
				Name = GbsAst?.Name,
				Color = ((GbsAst != null) ? Extension.AsColorORGBIfAnyHasValue(GbsAst.Color) : null)
			};
			object texture0Id;
			if (GbsAst == null)
			{
				texture0Id = null;
			}
			else
			{
				ref long? textureIdent = ref GbsAst.TextureIdent0;
				texture0Id = (textureIdent.HasValue ? Extension.AsObjectIdInMemory(textureIdent.GetValueOrDefault()) : null);
			}
			val.Texture0Id = (IObjectIdInMemory)texture0Id;
			val.HintText = GbsAst?.Hint;
			val.TexturePath = GbsAst?.texturePath;
			result = (object)val;*/
		}
		else
		{
			result = null;
		}
		return (Sprite)result;
	}

	public static ListViewAndControl<EntryT> AlsListView<EntryT>(this UINodeInfoInTree ListViewportAst, Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> CallbackListEntryConstruct = null, ListEntryTrenungZeleTypEnum? InEntryTrenungZeleTyp = null) where EntryT : class, IListEntry
	{
		SictAuswertGbsListViewport<EntryT> sictAuswertGbsListViewport = new SictAuswertGbsListViewport<EntryT>(ListViewportAst, CallbackListEntryConstruct, InEntryTrenungZeleTyp);
		sictAuswertGbsListViewport.Read();
		return sictAuswertGbsListViewport?.Result;
	}

	public static IUIElement WithRegionConstrainedToIntersection(this IUIElement original, RectInt constraint)
	{
		throw new NotImplementedException();
		//return (original != null) ? Extension.WithRegion(original, original.Region.Intersection(constraint)) : null;
	}

	public static Container AlsContainer(this UINodeInfoInTree containerNode, bool treatIconAsSprite = false, RectInt? regionConstraint = null)
	{
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected O, but got Unknown
		if ((!(containerNode?.VisibleIncludingInheritance)) ?? true)
		{
			return null;
		}
		UIElementInputText[] array = (containerNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameMatchesRegexPatternIgnoreCase("SinglelineEdit|QuickFilterEdit")))?.Select((Func<UINodeInfoInTree, UIElementInputText>)delegate(UINodeInfoInTree textBoxAst)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			UINodeInfoInTree uINodeInfoInTree = textBoxAst.LargestLabelInSubtree();
			if (uINodeInfoInTree == null)
			{
				return (UIElementInputText)null;
			}
			string text = uINodeInfoInTree?.LabelText();
			return new UIElementInputText(textBoxAst.AsUIElementIfVisible(), text);
		})?.WhereNotDefault()?.OrdnungLabel<UIElementInputText>()?.ToArrayIfNotEmpty();
		IUIElementText[] array2 = containerNode?.ExtraktMengeButtonLabelString()?.OrdnungLabel<IUIElementText>()?.ToArrayIfNotEmpty();
		UINodeInfoInTree[] array3 = array2?.Select((IUIElementText button) => containerNode.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(((IObjectIdInt64)button).Id))?.ToArray();
		UINodeInfoInTree[] array4 = array?.Select((UIElementInputText textBox) => containerNode.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(((ObjectIdInt64)(object)textBox).Id))?.ToArray();
		UINodeInfoInTree[] mengeContainerZuMaide = new UINodeInfoInTree[2][] { array3, array4 }.ConcatNullable().ToArray();
		IUIElementText[] labelText = containerNode?.ExtraktMengeLabelString()?.WhereNitEnthalte<IUIElementText, UINodeInfoInTree>((IEnumerable<UINodeInfoInTree>)mengeContainerZuMaide)?.OrdnungLabel<IUIElementText>()?.ToArrayIfNotEmpty();
		ISprite[] sprite = containerNode.SetSpriteFromChildren(treatIconAsSprite)?.OrdnungLabel<ISprite>()?.ToArrayIfNotEmpty();
		IUIElement val = containerNode.AsUIElementIfVisible();
		if (regionConstraint.HasValue)
		{
			val = val.WithRegionConstrainedToIntersection(regionConstraint.Value);
		}
		return new Container(val)
		{
			ButtonText = array2,
			InputText = (IEnumerable<IUIElementInputText>)(object)array,
			LabelText = labelText,
			Sprite = sprite
		};
	}

	public static IEnumerable<ISprite> SetSpriteFromChildren(this UINodeInfoInTree uiNode, bool treatIconAsSprite = false)
	{
		return (IEnumerable<ISprite>)(uiNode?.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree c) => (c != null && c.PyObjTypNameIsSprite()) || (treatIconAsSprite && (c?.PyObjTypNameIsIcon() ?? false)), null, null, null, omitNodesBelowNodesMatchingPredicate: true)?.Select((UINodeInfoInTree spriteNode) => spriteNode?.AlsSprite())?.WhereNotDefault());
	}

	public static IInSpaceBracket AsInSpaceBracket(this UINodeInfoInTree node)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Container val = node?.AlsContainer();
		if (val == null)
		{
			return null;
		}
		return (IInSpaceBracket)new InSpaceBracket((IUIElement)(object)val)
		{
			Name = node.Name
		};
	}

	public static IContainer AlsUtilmenu(this UINodeInfoInTree GbsAst)
	{
		return (IContainer)(object)(GbsAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree k) => k.PyObjTypNameMatchesRegexPatternIgnoreCase("ExpandedUtilMenu")))?.AlsContainer();
	}

	public static IEnumerable<T> WhereNitEnthalte<T, AstT>(this IEnumerable<T> MengeKandidaat, IEnumerable<AstT> MengeContainerZuMaide) where T : IObjectIdInMemory where AstT : GbsAstInfo
	{
		return MengeKandidaat?.Where((T Kandidaat) => !(MengeContainerZuMaide?.Any((AstT ContainerZuMaide) => new AstT[1] { ContainerZuMaide }.ConcatNullable(ContainerZuMaide.MengeChildAstTransitiiveHüle()).Any((GbsAstInfo ContainerZuMaideChild) => ContainerZuMaideChild.PyObjAddress == ((IObjectIdInt64)Kandidaat).Id)) ?? false));
	}

	public static ColorORGB AlsColorORGB(this ColorORGBVal? Color)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return (!Color.HasValue) ? ((ColorORGB)null) : new ColorORGB(Color);
	}

	public static IEnumerable<NodeT> OrderByRegionSizeDescending<NodeT>(this IEnumerable<NodeT> seq) where NodeT : GbsAstInfo
	{
		return seq?.OrderByDescending((NodeT node) => node?.Grööse?.Betraag ?? (-1.0));
	}
}
