using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsListEntry
{
	public readonly IColumnHeader[] ListeColumnHeader;

	public readonly UINodeInfoInTree EntryAst;

	public readonly RectInt? RegionConstraint;

	public readonly ListEntryTrenungZeleTypEnum? TrenungZeleTyp;

	private IUIElementText Label;

	private static readonly KeyValuePair<string, bool>[] MengeZuTexturePathBedoitungIsExpanded = new KeyValuePair<string, bool>[2]
	{
		new KeyValuePair<string, bool>("res:/UI/Texture/Icons/38_16_228.png", value: false),
		new KeyValuePair<string, bool>("res:/UI/Texture/Icons/38_16_229.png", value: true)
	};

	private const string RegexGrupeInKlamerNaame = "inklamer";

	private const string GroupBescriftungZerleegungRegexPattern = "([^\\(\\[]+)((\\(|\\[)(?<inklamer>[^\\)\\]]+)(\\)|\\])|)";

	public UINodeInfoInTree LabelAst { get; private set; }

	public int? LabelGrenzeLinx { get; private set; }

	public UINodeInfoInTree ExpanderAst { get; private set; }

	public UINodeInfoInTree FläceFürMenuAst { get; private set; }

	public bool? IstGroup { get; private set; }

	public bool? IstItem { get; private set; }

	public int? InhaltGrenzeLinx { get; private set; }

	public string BescriftungTailTitel { get; private set; }

	public int? BescriftungQuantitäät { get; private set; }

	public string[] ItemListeZeleTextMitFormat { get; private set; }

	public KeyValuePair<IColumnHeader, string>[] ListeZuHeaderZeleString { get; private set; }

	public IListEntry ErgeebnisListEntry { get; private set; }

	public virtual ListEntryTrenungZeleTypEnum ListEntryInZeleTrenung()
	{
		return TrenungZeleTyp ?? ListEntryTrenungZeleTypEnum.InLabelTab;
	}

	public string ZeleStringZuHeaderMitBescriftungRegexPattern(string headerBescriftungRegexPattern, RegexOptions regexOptions = RegexOptions.None)
	{
		return ZeleStringZuHeaderMitBescriftungRegex(new Regex(headerBescriftungRegexPattern, regexOptions));
	}

	public string ZeleStringZuHeaderMitBescriftungRegex(Regex headerBescriftungRegex)
	{
		if (headerBescriftungRegex == null)
		{
			return null;
		}
		KeyValuePair<IColumnHeader, string>[] listeZuHeaderZeleString = ListeZuHeaderZeleString;
		if (listeZuHeaderZeleString == null)
		{
			return null;
		}
		KeyValuePair<IColumnHeader, string>[] array = listeZuHeaderZeleString;
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<IColumnHeader, string> keyValuePair = array[i];
			if (((IUIElementText)keyValuePair.Key).Text != null)
			{
				Match match = headerBescriftungRegex.Match(((IUIElementText)keyValuePair.Key).Text);
				if (match.Success)
				{
					return keyValuePair.Value;
				}
			}
		}
		return null;
	}

	private static bool? ExpanderSpriteIsExpanded(UINodeInfoInTree expanderSprite)
	{
		if (expanderSprite == null)
		{
			return null;
		}
		string texturePath = expanderSprite.texturePath;
		if (texturePath.IsNullOrEmpty())
		{
			return null;
		}
		Func<KeyValuePair<string, bool>, bool> func = (KeyValuePair<string, bool> kandidaat) => string.Equals(kandidaat.Key, texturePath, StringComparison.InvariantCultureIgnoreCase);
		KeyValuePair<string, bool> arg = MengeZuTexturePathBedoitungIsExpanded.FirstOrDefault(func);
		if (func(arg))
		{
			return arg.Value;
		}
		return null;
	}

	public static string ZeleTextAusZeleTextMitFormat(string zeleTextMitFormat)
	{
		return zeleTextMitFormat?.RemoveXmlTag();
	}

	public SictAuswertGbsListEntry(UINodeInfoInTree entryAst, IColumnHeader[] listeScrollHeader, RectInt? regionConstraint, ListEntryTrenungZeleTypEnum? trenungZeleTyp = null)
	{
		EntryAst = entryAst;
		ListeColumnHeader = listeScrollHeader;
		RegionConstraint = regionConstraint;
		TrenungZeleTyp = trenungZeleTyp;
	}

	public static long ÜberlapungBetraag(IUIElement uIElement, IUIElement header)
	{
		return Math.Min((uIElement != null) ? uIElement.Region.Value.Max0 : int.MinValue, (header != null) ? header.Region.Value.Max0 : int.MinValue) - Math.Max((uIElement != null) ? uIElement.Region.Value.Min0 : int.MaxValue, (header != null) ? header.Region.Value.Min0 : int.MaxValue);
	}

	public static T HeaderBestFit<T>(IUIElement zeleUIElement, IEnumerable<T> mengeKandidaatHeader) where T : class, IUIElement
	{
		object result;
		if (mengeKandidaatHeader == null)
		{
			result = null;
		}
		else
		{
			var enumerable = mengeKandidaatHeader.Select((T kandidaat) => new
			{
				Kandidaat = kandidaat,
				ÜberlapungBetraag = ÜberlapungBetraag(zeleUIElement, (IUIElement)(object)kandidaat)
			});
			if (enumerable == null)
			{
				result = null;
			}
			else
			{
				var enumerable2 = enumerable.Where(kandidaatUndÜberlapungBetraag => 3 < kandidaatUndÜberlapungBetraag.ÜberlapungBetraag);
				if (enumerable2 == null)
				{
					result = null;
				}
				else
				{
					var orderedEnumerable = enumerable2.OrderByDescending(kandidaatUndÜberlapungBetraag => kandidaatUndÜberlapungBetraag.ÜberlapungBetraag);
					if (orderedEnumerable == null)
					{
						result = null;
					}
					else
					{
						var anon = orderedEnumerable.FirstOrDefault();
						result = ((anon != null) ? anon.Kandidaat : null);
					}
				}
			}
		}
		return (T)result;
	}

	public virtual void Berecne()
	{
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_054f: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Expected O, but got Unknown
		Container val = EntryAst.AlsContainer(treatIconAsSprite: false, RegionConstraint);
		if (val == null)
		{
			return;
		}
		LabelAst = EntryAst.LargestLabelInSubtree();
		FläceFürMenuAst = LabelAst;
		LabelGrenzeLinx = (int?)LabelAst.LaagePlusVonParentErbeLaageA();
		ExpanderAst = EntryAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => (Glob.GbsAstTypeIstEveIcon(kandidaat) || Glob.GbsAstTypeIstSprite(kandidaat)) && Regex.Match(kandidaat.Name ?? "", "expander", RegexOptions.IgnoreCase).Success);
		IstGroup = ((ExpanderAst == null) ? null : ExpanderAst.VisibleIncludingInheritance) ?? false;
		UINodeInfoInTree[] array = EntryAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => (Glob.GbsAstTypeIstEveIcon(kandidaat) || Regex.Match(kandidaat.PyObjTypName ?? "", "sprite", RegexOptions.IgnoreCase).Success) && (kandidaat.VisibleIncludingInheritance ?? false));
		IstItem = (!array.IsNullOrEmpty() || LabelAst != null) && !(IstGroup ?? false);
		float[] array2 = array?.Select((UINodeInfoInTree iconAst) => iconAst.LaagePlusVonParentErbeLaageA())?.Where((float? kandidaat) => kandidaat.HasValue)?.Select((float? iconAstGrenzeLinx) => iconAstGrenzeLinx.Value)?.ToArray();
		if (!array2.IsNullOrEmpty())
		{
			InhaltGrenzeLinx = (int)Math.Min(InhaltGrenzeLinx ?? int.MaxValue, array2.Min());
		}
		if (LabelGrenzeLinx.HasValue)
		{
			InhaltGrenzeLinx = Math.Min(InhaltGrenzeLinx ?? int.MaxValue, LabelGrenzeLinx.Value);
		}
		ListEntryTrenungZeleTypEnum listEntryTrenungZeleTypEnum = ListEntryInZeleTrenung();
		if (ListEntryTrenungZeleTypEnum.Ast == listEntryTrenungZeleTypEnum)
		{
			ListeZuHeaderZeleString = (EntryAst.ExtraktMengeLabelString()?.ToArray())?.Select((IUIElementText zeleLabel) => new KeyValuePair<IColumnHeader, string>(SictAuswertGbsListEntry.HeaderBestFit<IColumnHeader>((IUIElement)(object)zeleLabel, (IEnumerable<IColumnHeader>)ListeColumnHeader), (zeleLabel != null) ? zeleLabel.Text : null))?.ToArray();
		}
		else
		{
			Label = LabelAst.AsUIElementTextIfTextNotEmpty();
			IUIElementText label = Label;
			string text = ((label != null) ? label.Text : null);
			if (text != null && ((!IstGroup) ?? true))
			{
				ItemListeZeleTextMitFormat = text.Split(new string[1] { "<t>" }, StringSplitOptions.None);
				ListeZuHeaderZeleString = (ItemListeZeleTextMitFormat?.Select((string zeleTextMitFormat) => ZeleTextAusZeleTextMitFormat(zeleTextMitFormat))?.ToArray())?.Select((string zeleText, int index) => new KeyValuePair<IColumnHeader, string>(ListeColumnHeader?.FirstOrDefault((IColumnHeader kandidaat) => kandidaat.ColumnIndex == index), zeleText))?.ToArray();
			}
		}
		UINodeInfoInTree uINodeInfoInTree = EntryAst?.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree node) => node != null && node.PyObjTypNameMatchesRegexPatternIgnoreCase("fill") && (node?.Name?.RegexMatchSuccessIgnoreCase("bgColor") ?? false));
		ColorORGB[] listBackgroundColor = EntryAst?.BackgroundList?.Select((GbsAstInfo background) => background.Color.AlsColorORGB()).ConcatNullable((IEnumerable<ColorORGB>)(object)new ColorORGB[1] { uINodeInfoInTree?.Color.AlsColorORGB() })?.WhereNotDefault()?.ToArrayIfNotEmpty();
		ISprite[] setSprite = EntryAst.SetSpriteFromChildren()?.ToArrayIfNotEmpty();
		ErgeebnisListEntry = (IListEntry)new ListEntry((IUIElement)(object)val)
		{
			ContentBoundLeft = InhaltGrenzeLinx,
			ListColumnCellLabel = ListeZuHeaderZeleString,
			GroupExpander = ExpanderAst?.AsUIElementIfVisible(),
			IsGroup = IstGroup,
			IsExpanded = ExpanderSpriteIsExpanded(ExpanderAst),
			IsSelected = EntryAst.isSelected,
			ListBackgroundColor = listBackgroundColor,
			SetSprite = setSprite
		};
	}
}
