using System.Collections.Generic;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Strategies;

namespace Sanderling.ABot.Bot
{
	/// <summary>
	/// In-process fire-control board shared by every <see cref="Bot"/> in this process (AbotMcp's
	/// three ClientAgents). Ships claim targets instead of using a fixed role: whoever is free
	/// takes a one-volley kill; the rest skip it and move down the priority list.
	/// </summary>
	public static class FleetFireBoard
	{
		private const int StaleMs = 8000;
		private const int AssumedRepPerCycle = 400;
		/// <summary>A target this close to death is left to the window already shooting it.</summary>
		private const int VolleysToCallItDead = 2;
		/// <summary>Reported remaining-HP readings older than this are ignored.</summary>
		private const int HealthStaleMs = 5000;
		private static readonly object Gate = new();
		private static readonly Dictionary<string, List<Claim>> Claims = new(StringComparer.Ordinal);
		private static readonly Dictionary<string, (long Ehp, long Tick)> Health = new(StringComparer.Ordinal);

		private readonly struct Claim
		{
			public Claim(int pid, long tick) { Pid = pid; Tick = tick; }
			public int Pid { get; }
			public long Tick { get; }
		}

		/// <summary>
		/// Identity of an overview enemy shared across the three clients — or null when the grid cannot
		/// name it unambiguously. Duplicate names USED to be disambiguated by distance order, but each
		/// ship sees its own distances, so "#2" meant a different rat in every window. When a name is
		/// duplicated we therefore claim nothing and everyone shoots freely.
		/// </summary>
		public static string? TargetKey(IOverviewEntry e, IReadOnlyList<IOverviewEntry> peers)
		{
			if (NpcInfoProvider.IsAbyssalCache(e)) return "abyssal-cache";
			var type = e.Type ?? "";
			var name = e.Name ?? "";
			var sameCount = peers.Count(x => (x.Type ?? "") == type && (x.Name ?? "") == name);
			return sameCount <= 1 ? $"{type}|{name}" : null;
		}

		/// <summary>
		/// What one window knows about a target's remaining EHP, so the others can see it is already
		/// about to die and go shoot something else instead of overkilling it.
		/// </summary>
		public static void ReportRemainingEhp(string? key, long remainingEhp)
		{
			if (string.IsNullOrEmpty(key)) return;
			lock (Gate)
				Health[key!] = (remainingEhp, Environment.TickCount64);
		}

		private static long? KnownRemainingEhp(string? key)
		{
			if (string.IsNullOrEmpty(key)) return null;
			lock (Gate)
			{
				if (!Health.TryGetValue(key!, out var entry)) return null;
				return Environment.TickCount64 - entry.Tick > HealthStaleMs ? null : entry.Ehp;
			}
		}

		/// <summary>
		/// How many ships should dump onto this target. A target that dies inside
		/// <see cref="VolleysToCallItDead"/> volleys of the ship already on it gets a single shooter —
		/// extra windows would only overkill it, and it dies at the same moment either way. Remaining
		/// EHP reported by whoever has it locked beats the type's book value; the cache is always one.
		/// </summary>
		public static int MaxShooters(
			IOverviewEntry target,
			IOverviewProvider overview,
			int volleyDamage,
			NpcInfoProvider npc,
			string? key = null)
		{
			if (NpcInfoProvider.IsAbyssalCache(target)) return 1;
			if (volleyDamage <= 0) return int.MaxValue;

			var ehp = KnownRemainingEhp(key) ?? npc.EhpFor(target);
			if (ehp is not long hp) return int.MaxValue;

			var budget = volleyDamage * VolleysToCallItDead;
			if (hp > budget) return int.MaxValue;

			var healers = (overview.Entries ?? Array.Empty<IOverviewEntry>())
				.Count(e => e.IsEnemy && NpcInfoProvider.IsDedicatedHealer(e.Name ?? e.Type ?? ""));
			if (healers == 0) return 1;
			return hp + healers * AssumedRepPerCycle <= budget ? 1 : int.MaxValue;
		}

		/// <summary>
		/// Claim a slot on <paramref name="key"/> for <paramref name="pid"/>. Refreshes an existing
		/// claim; rejects if the target already has <paramref name="maxShooters"/> live claimants.
		/// </summary>
		public static bool ShouldShoot(int pid, string? key, int maxShooters)
		{
			// No shared identity for this rat (duplicate name): coordination is impossible, so never
			// hold anyone back — a missing key must not mean "don't shoot".
			if (string.IsNullOrEmpty(key)) return true;
			if (maxShooters <= 0) return false;
			var now = Environment.TickCount64;
			lock (Gate)
			{
				if (!Claims.TryGetValue(key, out var list))
					Claims[key] = list = new List<Claim>();
				list.RemoveAll(c => now - c.Tick > StaleMs);
				var mine = list.FindIndex(c => c.Pid == pid);
				if (mine >= 0)
				{
					list[mine] = new Claim(pid, now);
					return true;
				}
				if (list.Count >= maxShooters) return false;
				list.Add(new Claim(pid, now));
				return true;
			}
		}

		public static bool NamesMatch(string? selectedName, string? overviewName)
		{
			if (string.IsNullOrWhiteSpace(selectedName) || string.IsNullOrWhiteSpace(overviewName))
				return false;

			// The target label pads its columns ("Striking Vila  Damavik  25 km") while the overview name
			// is single-spaced, so a raw substring test misses every multi-word rat — which, with the
			// friendly-fire allowlist, silently stopped the ship from firing at all (live 2026-09-18).
			var selected = Normalize(selectedName);
			var overview = Normalize(overviewName);
			return selected.IndexOf(overview, StringComparison.OrdinalIgnoreCase) >= 0 ||
			       overview.IndexOf(selected, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static string Normalize(string value) =>
			System.Text.RegularExpressions.Regex.Replace(value, @"\s+", " ").Trim();
	}
}
