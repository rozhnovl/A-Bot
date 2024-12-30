using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine.Interface;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsWindowFittingWindowDefenceStatsRowCellDamageType
{
	public readonly UINodeInfoInTree CellAst;

	private static readonly string ResistanceRegexPattern = "([\\d]+)\\s*" + Regex.Escape("%");

	public UINodeInfoInTree LabelAst { get; private set; }

	public string LabelAstText { get; private set; }

	public long? ResistanceMili { get; private set; }

	public ColorORGB DamageTypColor { get; private set; }

	public SictAuswertGbsWindowFittingWindowDefenceStatsRowCellDamageType(UINodeInfoInTree cellAst)
	{
		CellAst = cellAst;
	}

	public void Berecne()
	{
		LabelAst = CellAst.FirstMatchingNodeFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);
		if (LabelAst != null)
		{
			LabelAstText = LabelAst.SetText;
		}
		Match match = Regex.Match(LabelAstText ?? "", ResistanceRegexPattern);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			ResistanceMili = ((value == null) ? null : (value.TryParseInt64(Bib3.Glob.NumberFormat) * 10));
		}
		UINodeInfoInTree[] array = CellAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("PyFill", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		if (array == null)
		{
			return;
		}
		ColorORGBVal?[] source = array.Select((UINodeInfoInTree ast) => ast.Color).Where(delegate(ColorORGBVal? color)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			int result;
			throw new NotImplementedException();
			if (color.HasValue)
			{
				ColorORGBVal value2 = color.Value;
				//TODO result = (((ColorORGBVal)(ref value2)).AleUnglaicNul() ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}).ToArray();
		DamageTypColor = ColorORGB.VonVal(source.OrderBy((ColorORGBVal? color) => color.Value.OMilli ?? 0).LastOrDefault());
	}
}
