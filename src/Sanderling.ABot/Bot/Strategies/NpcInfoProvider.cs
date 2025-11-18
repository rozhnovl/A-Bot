using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Configuration;

namespace Sanderling.ABot.Bot.Strategies
{
	public class NpcInfoProvider : INpcInfoProvider
	{
		private readonly CombatConfiguration _config;
		private readonly Dictionary<string, double> DpsPerEntry;

		public NpcInfoProvider(CombatConfiguration config)
		{
			_config = config;
			DpsPerEntry = config.NpcDpsValues;
		}

		// For backward compatibility - uses default configuration
		public NpcInfoProvider() : this(ConfigurationLoader.LoadConfigurationsStandalone().Combat)
		{
		}

		public double CalculateApproximateDps(IList<IOverviewEntry> entries)
		{
			return entries.Select(e =>
					DpsPerEntry.TryGetValue(e.Type.Trim(), out var dpsValue)
						? dpsValue
						: throw new KeyNotFoundException(e.Type))
				.Sum();
		}

		public double CalculateApproximateDps(IOverviewProvider overviewProvider)
		{
			var entries = overviewProvider.Entries
				?.Where(entry => entry.IsEnemy)
				?.ToList();

			return entries.Select(e =>
					DpsPerEntry.TryGetValue(e.Type.Trim(), out var dpsValue)
						? dpsValue
						: throw new KeyNotFoundException(e.Type))
				.Sum();
		}

		public int CalcTargetPriority(IOverviewEntry entry)
		{
			if (entry.Name.Contains("Guristas"))
				return _config.Priorities.Guristas;
			if (entry.Name.Contains("Anchoring"))
				return _config.Priorities.Anchoring;

			if (entry.Name.Contains("Firewatcher"))
				return _config.Priorities.Firewatcher;

			if (entry.Name.Contains("Renewing") || entry.Name.Contains("Plateforger") ||
			    entry.Name.Contains("Fieldweaver"))
				return _config.Priorities.Renewing;
			if (entry.Name.Contains("Entangler") || entry.Name.Contains("Snarecaster"))
				return _config.Priorities.Entangler;
			if (entry.Name.Contains("Scylla"))
				return _config.Priorities.Scylla;
			if (entry.Name.Contains("Tyrannos"))
				return _config.Priorities.Tyrannos;
			if (entry.Name.Contains("Extraction"))
				return _config.Priorities.Extraction;
			if (entry.Name.Contains("Bioadaptive"))
				return _config.Priorities.Bioadaptive;
			if (entry.Type.Contains("Battleship") && entry.Type.Contains("Drifter"))
				return _config.Priorities.DrifterBattleship;

			return _config.Priorities.DefaultBasePriority - (int)(CalculateApproximateDps(new List<IOverviewEntry>() { entry }));
		}

		public bool IsOrbitBeacon(IOverviewEntry entry)
		{
			foreach (var beaconName in _config.OrbitBeacons.BeaconNames)
			{
				if (entry.Name.Contains(beaconName))
					return true;
			}
			return false;
		}
	}
}
