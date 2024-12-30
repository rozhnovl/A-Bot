using System;
using System.Collections.Generic;
using System.Linq;
using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs;

public class SictAuswertGbsInfoPanelMissions : SictAuswertGbsInfoPanelGen
{
	public UINodeInfoInTree[] MengeAstKandidaatMission { get; private set; }

	public SictAuswertGbsInfoPanelMissionsMission[] MengeMissionAuswert { get; private set; }

	public InfoPanelMissions ErgeebnisScpez { get; private set; }

	public SictAuswertGbsInfoPanelMissions(UINodeInfoInTree astInfoPanelMissions)
		: base(astInfoPanelMissions)
	{
	}

	public override void Berecne()
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		base.Berecne();
		MengeAstKandidaatMission = base.MainContAst.MatchingNodesFromSubtreeBreadthFirst((UINodeInfoInTree kandidaat) => string.Equals("UtilMenu", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1);
		MengeMissionAuswert = MengeAstKandidaatMission?.Select(delegate(UINodeInfoInTree astKandidaatMission)
		{
			SictAuswertGbsInfoPanelMissionsMission sictAuswertGbsInfoPanelMissionsMission = new SictAuswertGbsInfoPanelMissionsMission(astKandidaatMission);
			sictAuswertGbsInfoPanelMissionsMission.Berecne();
			return sictAuswertGbsInfoPanelMissionsMission;
		}).ToArray();
		if (MengeMissionAuswert != null)
		{
			IUIElementText[] listMissionButton = (from kandidaat in MengeMissionAuswert.Select(delegate(SictAuswertGbsInfoPanelMissionsMission auswert)
				{
					IUIElementText ergeebnis = auswert.Ergeebnis;
					RectInt value = ((ergeebnis == null) ? RectInt.Empty : ((IUIElement)ergeebnis).Region.Value);
					return new KeyValuePair<IUIElementText, RectInt>(ergeebnis, value);
				}).Where(delegate(KeyValuePair<IUIElementText, RectInt> kandidaat)
				{
					_ = kandidaat.Value;
					return true;
				})
				orderby kandidaat.Value.Center().B
				select kandidaat.Key).ToArray();
			ErgeebnisScpez = new InfoPanelMissions((IInfoPanel)(object)base.Ergeebnis)
			{
				ListMissionButton = listMissionButton
			};
		}
	}
}
