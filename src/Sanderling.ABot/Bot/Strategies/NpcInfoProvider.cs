using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Configuration;

namespace Sanderling.ABot.Bot.Strategies
{
	public class NpcInfoProvider
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
			{"Guristas Despoiler", 29},
			{"Triglavian Biocombinative Cache", 0},
			{"Devoted Knight", 100},
		};

		/// <summary>
		/// Approximate DPS one enemy applies. Prefers the hand-tuned effective-DPS table (accounts for
		/// application/ramp), falls back to the bundled SDE stat DB (base weapon DPS, with a ramp uplift
		/// for disintegrator rats), and finally to 0. Never throws on an unknown type — a new rat that
		/// isn't in either source contributes 0 rather than crashing the whole DPS estimate.
		/// </summary>
		public static bool IsAbyssalCache(IOverviewEntry? entry) =>
			// "Triglavian Biocombinative Cache Wreck" also contains "Biocombinative": without this guard
			// the already-popped cache's WRECK was appended to the combat candidates, locked and shot at
			// (seen live 2026-09-18) instead of being left alone for looting.
			!IsWreckName(entry?.Name) && !IsWreckName(entry?.Type) &&
			(IsAbyssalCacheName(entry?.Name) || IsAbyssalCacheName(entry?.Type));

		public static bool IsAbyssalCacheName(string? name) =>
			!string.IsNullOrEmpty(name) &&
			(name.Contains("Bioadaptive", StringComparison.OrdinalIgnoreCase) ||
			 name.Contains("Biocombinative", StringComparison.OrdinalIgnoreCase));

		public static bool IsWreckName(string? name) =>
			name?.Contains("Wreck", StringComparison.OrdinalIgnoreCase) == true;

		public double DpsFor(IOverviewEntry entry)
		{
			var type = entry.Type?.Trim() ?? "";
			if (DpsPerEntry.TryGetValue(type, out var tuned))
				return tuned;

			var stat = NpcStats.Lookup(type);
			if (stat?.Dps is > 0)
				return stat.Dps.Value * (stat.Ramp ? 2.0 : 1.0); // disintegrators ~double at full ramp
			return 0;
		}

		public double CalculateApproximateDps(IList<IOverviewEntry> entries) =>
			entries.Sum(DpsFor);

		/// <summary>
		/// Approximate capacitor drain (GJ/s) ONE enemy applies to us with its neutralizers. The stat DB
		/// only flags WHETHER a rat neuts (ewar "neut" / the Starving name prefix / EDENCOM Dissipators),
		/// not how hard, so the amount is bucketed by hull size (via EHP) — rough numbers in line with
		/// Triglavian frig/cruiser/battleship neut pressure. Enemies beyond typical neut range contribute 0.
		/// </summary>
		public double NeutGjPerSecFor(IOverviewEntry entry)
		{
			var name = entry.Name ?? "";
			var type = entry.Type?.Trim() ?? "";
			var stat = NpcStats.Lookup(type);
			var neuts = HasEwar(stat, "neut") || name.Contains("Starving") || name.Contains("Dissipator");
			if (!neuts)
				return 0;
			if (entry.Distance > 20000)
				return 0; // out of typical NPC neut range
			var ehp = EhpFor(entry) ?? 10000;
			return ehp > 150000 ? 45 : ehp > 20000 ? 20 : 6; // battleship / cruiser / frigate buckets
		}

		/// <summary>Total enemy neut pressure on us right now, GJ/s (see <see cref="NeutGjPerSecFor"/>).</summary>
		public double CalculateNeutPressure(IOverviewProvider overviewProvider) =>
			(overviewProvider.Entries?.Where(e => e.IsEnemy) ?? Enumerable.Empty<IOverviewEntry>())
			.Sum(NeutGjPerSecFor);

		public double CalculateApproximateDps(IOverviewProvider overviewProvider) =>
			(overviewProvider.Entries?.Where(e => e.IsEnemy) ?? Enumerable.Empty<IOverviewEntry>()).Sum(DpsFor);

		/// <summary>Effective HP of an enemy vs kinetic damage (our missile-boat profile), or null if unknown.</summary>
		public long? EhpFor(IOverviewEntry entry) => NpcStats.EhpKinetic(entry.Type);

		/// <summary>
		/// Room-aware kill-order priority (LOWER = shoot first) for the scorer produced by
		/// <see cref="TargetPriorityScorer"/>. This parameterless overload uses a typical-room DPS
		/// estimate — only for callers with no overview at hand (e.g. picking an orbit anchor).
		/// </summary>
		public int CalcTargetPriority(IOverviewEntry entry) => CalcTargetPriority(entry, 300);

		/// <summary>
		/// Build a priority function with the CURRENT room folded in (total room DPS drives how
		/// valuable killing a webber is). Use this for kill ordering instead of the bare
		/// <see cref="CalcTargetPriority(IOverviewEntry)"/>.
		/// </summary>
		public System.Func<IOverviewEntry, int> TargetPriorityScorer(IOverviewProvider overviewProvider)
		{
			var roomTotalDps = CalculateApproximateDps(overviewProvider);
			return entry => CalcTargetPriority(entry, roomTotalDps);
		}

		/// <summary>
		/// Shield HP/s our booster produces per GJ/s of capacitor (C5-L on the Hawk: 20.6 HP/s per
		/// 9 GJ/s ≈ 2.3): converts enemy neut GJ/s into the tank HP/s it effectively costs us.
		/// </summary>
		private const double ShieldHpPerGj = 2.3;

		/// <summary>
		/// Kill-order priority for one enemy (LOWER = shoot first), given the room's total DPS.
		///
		/// This is weighted-shortest-job-first (Smith's rule) instead of fixed EWAR bands: the total
		/// damage we absorb while clearing a room is Σ danger_i × (time until i dies), and that sum is
		/// minimized by shooting in descending danger / time-to-kill order — no kill-order simulation
		/// needed, the greedy ratio IS the optimum under additive damage. TTK is proportional to EHP
		/// for a fixed fleet DPS, so the ratio needs no own-DPS input: danger-per-EHP-point already
		/// yields the same ordering.
		///
		/// Danger of a rat = its applied DPS (ramping disintegrators ×1.5 — they only get worse), plus
		/// what its EWAR costs US, valued in this room:
		///  - webber/jammer: +35% of the room's TOTAL DPS. A web on our AB frigate turns everyone
		///    else's poor application into full hits (double-webbed under Lucifer Dramiels is what
		///    killed the first Hawk); a jam denies our own DPS for its cycle. Scales with the room —
		///    a webber alone is harmless, a webber in a 500-DPS room is the primary.
		///  - neuter: its GJ/s converted to the shield HP/s our boosters lose without that cap
		///    (<see cref="ShieldHpPerGj"/>), range-gated inside <see cref="NeutGjPerSecFor"/>.
		///  - dedicated healer: flat bump (it subtracts from OUR effective DPS across every kill).
		///  - damp/TD/paint: small bump — application denial, annoying but survivable.
		/// Scram is deliberately NOT weighted (nothing to warp to in the abyss anyway) — Anchoring
		/// rats compete on their damage alone.
		/// </summary>
		public int CalcTargetPriority(IOverviewEntry entry, double roomTotalDps)
		{
			var name = entry.Name ?? "";
			var type = entry.Type?.Trim() ?? "";
			var stat = NpcStats.Lookup(type);

			// --- structures / do-not-shoot: always last -------------------------------------------
			if (name.Contains("Bioadaptive") || name.Contains("Biocombinative"))
				return 100000;                                   // the loot cache — never a target
			if (type.Contains("Extraction") || type.Contains("Cache"))
				return 90000;
			if (type.Contains("Drifter") && type.Contains("Battleship"))
				return 80000;                                    // Drifter BS: avoid, don't tank-race it

			var danger = DpsFor(entry);
			if (stat?.Ramp ?? false)
				danger *= 1.5;                                   // disintegrators only get worse

			if (HasEwar(stat, "web") || HasEwar(stat, "ecm") ||
			    name.Contains("Tangling") || name.Contains("Snarecaster") || name.Contains("Entangler"))
				danger += 0.35 * roomTotalDps;

			danger += NeutGjPerSecFor(entry) * ShieldHpPerGj;

			if (IsDedicatedHealer(name))
				danger += 100;

			if (HasEwar(stat, "damp") || HasEwar(stat, "td") || HasEwar(stat, "paint") ||
			    name.Contains("Blinding") || name.Contains("Ghosting") || name.Contains("Harrowing"))
				danger += 30;

			var ehp = System.Math.Max(EhpFor(entry) ?? 6000, 1000);
			// Rank by danger removed per EHP point we must chew through (Smith's ratio), scaled to int.
			return 50000 - (int)(danger / ehp * 1e6);
		}

		private static bool HasEwar(NpcStat stat, string kind) => stat?.HasEwar(kind) ?? false;

		/// <summary>
		/// A rat whose PRIMARY job is remote-repping its friends (spider-tank), worth focusing to break a
		/// room's regen — as opposed to the incidental 'rep' nearly every Triglavian carries.
		/// </summary>
		public static bool IsDedicatedHealer(string name) =>
			name.Contains("Renewing") || name.Contains("Rodiva") || name.Contains("Preserver") ||
			name.Contains("Firewatcher") || name.Contains("Deepwatcher");

		/// <summary>How to hold range on a target, from the Abyssal Lurkers consensus (see ABYSS-INTEL.md §4).</summary>
		public enum EngageRange
		{
			/// <summary>Point-blank orbit (~500 m): angular-tank high-EHP / poor-tracking / catch-us-anyway hulls.</summary>
			Brawl,
			/// <summary>Mid kite (~20 km): fast ramping cruisers/frigs (Kiki, Vedmak, Damavik, Tessera).</summary>
			Kite,
			/// <summary>Stay outside the enemy's gun/missile envelope (long-range battleships).</summary>
			Standoff,
		}

		/// <summary>
		/// Pick the range archetype for a primary target. Standoff for the long-range battleships you must
		/// not sit inside the guns of; brawl for high-EHP / poor-tracking hulls where a tight orbit's angular
		/// velocity is the tank; kite for everything else (the default fast damage dealers).
		/// </summary>
		public EngageRange RangeArchetypeFor(IOverviewEntry entry)
		{
			var name = entry.Name ?? "";
			var type = entry.Type?.Trim() ?? "";

			if (name.Contains("Leshak") || name.Contains("Marshal") ||
			    (type.Contains("Drifter") && type.Contains("Battleship")))
				return EngageRange.Standoff;

			if (name.Contains("Tyrannos") || name.Contains("Overmind") || name.Contains("Karybdis") ||
			    name.Contains("Cynabal") || name.Contains("Knight") || name.Contains("Drekavac") ||
			    name.Contains("Hunter"))
				return EngageRange.Brawl;

			return EngageRange.Kite;
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