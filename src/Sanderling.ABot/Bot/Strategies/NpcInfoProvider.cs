using System.Collections.Generic;
using System.Linq;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class NpcInfoProvider
	{
		private readonly Dictionary<string, double> DpsPerEntry = new Dictionary<string, double>()
		{
			{"Sparkneedle Tessella", 25},
			{"Emberneedle Tessella", 25},
			{"Strikeneedle Tessella", 25},
			{"Blastneedle Tessella", 25},
			{"Snarecaster Tessella", 10},
			{"Spotlighter Tessella", 10},
			{"Fogcaster Tessella", 10},
			{"Gazedimmer Tessella", 10},
			{"Sparklance Tessella", 50},
			{"Emberlance Tessella", 50},
			{"Strikelance Tessella", 50},
			{"Blastlance Tessella", 50},
			{"Fieldweaver Tessella", 0},
			{"Plateforger Tessella", 0},
			{"Sparkgrip Tessera", 191},
			{"Embergrip Tessera", 191},
			{"Strikegrip Tessera", 191},
			{"Blastgrip Tessera", 191},
			{"Photic Abyssal Overmind", 108.6419753},
			{"Twilit Abyssal Overmind", 264},
			{"Bathyic Abyssal Overmind", 375.3084112},
			{"Hadal Abyssal Overmind", 457.5575221},
			{"Benthic Abyssal Overmind", 594.861461},
			{"Drifter Foothold Battleship", 100},
			{"Drifter Rearguard Battleship", 200},
			{"Drifter Frontline Battleship", 300},
			{"Drifter Vanguard Battleship", 400},
			{"Drifter Assault Battleship", 500},
			{"Drifter Entanglement Cruiser", 40},
			{"Drifter Nullwarp Cruiser", 40},
			{"Drifter Nullcharge Cruiser", 40},
			{"Ghosting Damavik", 36},
			{"Tangling Damavik", 36},
			{"Anchoring Damavik", 36},
			{"Starving Damavik", 36},
			{"Striking Damavik", 36},
			{"Striking Vila Damavik", 9 + 40},
			{"Tangling Vila Damavik", 9 + 40},
			{"Anchoring Vila Damavik", 9 + 40},
			{"Shining Vila Damavik", 9 + 40},
			{"Blinding Vila Damavik", 9 + 40},
			{"Ghosting Vila Damavik", 9 + 40},
			{"Starving Vedmak", 237.6},
			{"Harrowing Vedmak", 237.6},
			{"Harrowing Vila Vedmak", 118.8 + 40},
			{"Striking Leshak", 147.84},
			{"Renewing Leshak", 147.84},
			{"Tangling Leshak", 147.84},
			{"Starving Leshak", 147.84},
			{"Warding Leshak", 147.84},
			{"Blinding Leshak", 147.84},
			{"Lucid Escort", 24},
			{"Lucid Warden", 20},
			{"Lucid Aegis", 36},
			{"Lucid Firewatcher", 30},
			{"Lucid Preserver", 0},
			{"Lucid Watchman", 48},
			{"Lucid Upholder", 40},
			{"Lucid Sentinel", 40},
			{"Lucid Deepwatcher", 160},
			{"Ephialtes Lancer", 30},
			{"Ephialtes Entangler", 20},
			{"Ephialtes Spearfisher", 20},
			{"Ephialtes Illuminator", 20},
			{"Ephialtes Dissipator", 20},
			{"Ephialtes Obfuscator", 20},
			{"Ephialtes Confuser", 20},
			{"Vila Swarmer", 0},
			{"Triglavian Bioadaptive Cache", 0},
			{"Triglavian Extraction Node", 0},
			{"Triglavian Extraction SubNode", 0},
		};

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
			if (entry.Name.Contains("Anchoring"))
				return 1;

			if (entry.Name.Contains("Firewatcher"))
				return 2;

			if (entry.Name.Contains("Renewing") || entry.Name.Contains("Plateforger") ||
			    entry.Name.Contains("Fieldweaver"))
				return 3;
			if (entry.Name.Contains("Entangler") || entry.Name.Contains("Snarecaster"))
				return 6;
			if (entry.Name.Contains("Scylla"))
				return 8;
			if (entry.Name.Contains("Tyrannos"))
				return 8;
			if (entry.Name.Contains("Extraction"))
				return 10;
			if (entry.Name.Contains("Bioadaptive"))
				return 1000;
			if (entry.Type.Contains("Battleship") && entry.Type.Contains("Drifter"))
				return 9000;

			return 800 - (int)(CalculateApproximateDps(new List<IOverviewEntry>() { entry }));
		}

		public bool IsOrbitBeacon(IOverviewEntry entry)
		{
			if (entry.Name.Contains("Leshak"))
				return true;

			if (entry.Name.Contains("Overmind"))
				return true;

			if (entry.Name.Contains("Battleship"))
				return true;
			return false;
		}
	}
}