using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.ABot.Bot
{
	class ShipFit
	{
		private ModuleInfo[] High { get; }
		private ModuleInfo[] Mid { get; }
		private ModuleInfo[] Low { get; }

		public ShipFit(IEnumerable<Accumulation.IShipUiModule> memoryModules, ModuleInfo[][] fitInfo)
		{
			var modulesByY = memoryModules.GroupBy(m => m.Region.Min1).OrderBy(g=>g.Key).ToArray();
			if (modulesByY.Count()!=3)
				throw new ArgumentException("Couldn't determine 3 module groups");
			High = modulesByY[0].Select((m, i) =>
			{
				fitInfo[0][i].UiModule = m;
				return fitInfo[0][i];
			}).ToArray();
			Mid = modulesByY[1].Select((m, i) =>
			{
				fitInfo[1][i].UiModule = m;
				return fitInfo[1][i];
			}).ToArray();
			Low = modulesByY[2].Select((m, i) =>
			{
				fitInfo[2][i].UiModule = m;
				return fitInfo[2][i];
			}).ToArray();
		}

		public IEnumerable<Accumulation.IShipUiModule> GetAlwaysActiveModules()
		{
			foreach (var moduleInfo in High.Union(Mid).Union(Low))
			{
				if (moduleInfo.Type == ModuleType.Hardener)
					yield return moduleInfo.UiModule;
			}
		}

		public class ModuleInfo
		{
			public ModuleInfo(ModuleType type)
			{
				Type = type;
			}

			public ModuleType Type { get; }

			public Accumulation.IShipUiModule UiModule { get; set; }
		}
		public enum ModuleType
		{
			Hardener,
			Weapon, 
			Etc,
		}
	}
}