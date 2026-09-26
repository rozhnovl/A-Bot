using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.Bot
{
	/// <summary>
	/// Picks the charge the launchers should carry and issues the load command when it differs from
	/// what is loaded.
	///
	/// The decision is a plan over the kill QUEUE — not a hull list, and not a whole-room average.
	/// The enemies arrive already sorted by the target-priority scorer; for that order the planner
	/// finds the cheapest sequence of charges by dynamic programming: per target, EVE's missile
	/// application formula (signature vs explosion radius, speed vs explosion velocity, drf) times
	/// the target's EHP against the charge's damage type, plus 10 s of silence for every swap. Only
	/// the FIRST step is acted on; the rest is lookahead, which is what stops us from loading Fury
	/// for two Leshaks while a Rodiva still has to die first.
	///
	/// Charges are finite. <see cref="AmmoStock"/> tracks what is left in the hold (exact whenever
	/// the cargo window is readable, reload arithmetic in between) plus what sits in the launchers,
	/// and the planner never assigns more rounds to a charge than it has: a charge that runs dry
	/// mid-queue is dropped for the rest of the queue and the next best takes over — that is the
	/// fallback. A charge marked optional is loaded only after it has been SEEN in the hold.
	/// </summary>
	public static class AmmoController
	{
		/// <summary>Seconds of zero DPS a charge swap costs (EVE's fixed reload).</summary>
		public const double ReloadSeconds = 10;
		/// <summary>Only swap when the plan saves more than this on top of the reload — anti-flapping.</summary>
		public const double MinSavedSeconds = 3;
		private const int StableTicksRequired = 2;
		private const int CommandCooldownMs = 15000;
		/// <summary>EHP assumed for an enemy the stat DB doesn't know (it still gets a vote, just a blind one).</summary>
		private const double UnknownEhp = 8000;

		private sealed class ClientState
		{
			public int DesiredTypeId;
			public int StableTicks;
			public long LastCommandTick;
			public readonly AmmoStock Stock = new();
		}

		private static readonly object Gate = new();
		private static readonly Dictionary<int, ClientState> ByPid = new();

		private static ClientState StateFor(int pid)
		{
			lock (Gate)
			{
				if (!ByPid.TryGetValue(pid, out var state))
					ByPid[pid] = state = new ClientState();
				return state;
			}
		}

		/// <summary>The stock tracker of one client (diagnostics, dashboards, scenarios).</summary>
		public static AmmoStock StockFor(int pid) => StateFor(pid).Stock;

		/// <summary>
		/// EVE's missile application: <c>min(1, S/E, (S/E · Ve/Vt)^drf)</c> — signature radius S over
		/// explosion radius E, explosion velocity Ve over target velocity Vt, raised to the charge's
		/// damage reduction factor (lower drf = better application; Navy 0.604 vs Fury 0.682).
		/// Returns 1 when the target's size/speed is unknown, so a missing stat never biases the choice.
		/// </summary>
		public static double ApplicationFactor(ShipFit.AmmoCharge charge, NpcStat? stat, double velocityMultiplier)
		{
			if (charge.ExplosionRadiusM <= 0 || stat?.Sig is not int sig || sig <= 0)
				return 1;

			var sigTerm = sig / charge.ExplosionRadiusM;
			var velocity = (stat.Vmax ?? 0) * velocityMultiplier;
			if (velocity <= 0 || charge.ExplosionVelocityMs <= 0)
				return Math.Min(1, sigTerm);

			var speedTerm = Math.Pow(sigTerm * (charge.ExplosionVelocityMs / velocity), charge.Drf);
			return Math.Min(1, Math.Min(sigTerm, speedTerm));
		}

		/// <summary>
		/// Seconds this charge needs on ONE enemy: its EHP against the charge's damage type over our
		/// applied DPS (fleet-shared when <see cref="ShipFit.AmmoPlan.SharedShooters"/> &gt; 1).
		/// <see cref="double.PositiveInfinity"/> when the enemy is outside the charge's flight range.
		/// </summary>
		public static double SecondsToKill(ShipFit.AmmoCharge charge, IOverviewEntry enemy, ShipFit.AmmoPlan plan)
		{
			if (charge.MaxRangeM > 0 && enemy.Distance > charge.MaxRangeM)
				return double.PositiveInfinity;

			var dpsPerFactor = charge.DamagePerMissile * plan.LauncherCount / Math.Max(plan.CycleTimeSec, 0.1)
			                   * Math.Max(plan.SharedShooters, 1);
			if (dpsPerFactor <= 0)
				return double.PositiveInfinity;

			var stat = NpcStats.Lookup(enemy.Type) ?? NpcStats.Lookup(enemy.Name);
			var ehp = stat?.EhpFor(charge.DamageType) ?? UnknownEhp;
			var dps = dpsPerFactor * ApplicationFactor(charge, stat, plan.NpcVelocityMultiplier);
			return dps <= 0 ? double.PositiveInfinity : ehp / dps;
		}

		/// <summary>Seconds to chew through every enemy with one charge, one target at a time.</summary>
		public static double SecondsToClear(ShipFit.AmmoCharge charge, IReadOnlyList<IOverviewEntry> enemies, ShipFit.AmmoPlan plan) =>
			enemies.Sum(e => SecondsToKill(charge, e, plan));

		/// <summary>Rounds OUR launchers spend firing for that long (fleet share does not change our cadence).</summary>
		private static int RoundsFor(double seconds, ShipFit.AmmoPlan plan) =>
			double.IsInfinity(seconds) || double.IsNaN(seconds)
				? int.MaxValue
				: (int)Math.Min(int.MaxValue / 2, Math.Ceiling(seconds / Math.Max(plan.CycleTimeSec, 0.1)) * Math.Max(plan.LauncherCount, 1));

		/// <summary>A point in the kill queue where the plan changes charge.</summary>
		public sealed record PlanStep(int Index, ShipFit.AmmoCharge Charge, string Target);

		/// <summary>The planner's answer for one room state.</summary>
		public sealed record RoomPlan
		{
			public ShipFit.AmmoCharge? Loaded { get; init; }
			/// <summary>Charge the plan wants loaded for the FIRST target; null when nothing usable reaches anything.</summary>
			public ShipFit.AmmoCharge? First { get; init; }
			/// <summary>Total seconds for the whole queue, swaps included.</summary>
			public double Seconds { get; init; }
			/// <summary>Best total if we keep the loaded charge for the first target (∞ when it can't).</summary>
			public double SecondsIfStaying { get; init; }
			public IReadOnlyList<PlanStep> Swaps { get; init; } = Array.Empty<PlanStep>();
			public IReadOnlyDictionary<string, int?> Available { get; init; } = new Dictionary<string, int?>();

			/// <summary>Swap now: a different first charge that saves more than the anti-flap margin.</summary>
			public bool WantsSwap =>
				First != null && (Loaded == null || First.TypeId != Loaded.TypeId) &&
				SecondsIfStaying - Seconds >= MinSavedSeconds;

			public string Describe()
			{
				var loadedName = Loaded?.Name ?? "unknown charge";
				if (First == null)
					return $"no usable charge reaches the queue (loaded {loadedName})";
				var swaps = Swaps.Count == 0
					? "no swaps"
					: "swaps " + string.Join(", ", Swaps.Select(s => $"[{s.Index}] {s.Charge.Name} for {s.Target}"));
				var stay = double.IsInfinity(SecondsIfStaying) ? "∞" : $"{SecondsIfStaying:F0}s";
				var rounds = string.Join(", ", Available.Select(kv => $"{kv.Key} {(kv.Value?.ToString() ?? "?")}"));
				return $"{First.Name} first: {Seconds:F0}s for the queue vs {stay} staying on {loadedName}; {swaps}; rounds: {rounds}";
			}
		}

		/// <summary>
		/// Cheapest charge sequence for the kill queue (already priority-ordered), honouring what is
		/// left of each charge. Dynamic programme over (queue position, charge) with a reload per
		/// change, repaired iteratively for stock: when the assignment would fire more rounds of a
		/// charge than it has, that charge is barred from the queue position where it runs dry
		/// onwards and the programme is re-solved, so the remaining targets fall to the next best.
		/// </summary>
		public static RoomPlan PlanRoom(ShipFit.AmmoPlan plan, IReadOnlyList<IOverviewEntry> killOrder, int? loadedTypeId, AmmoStock stock)
		{
			var charges = plan.Charges.ToList();
			var loaded = loadedTypeId is int id ? charges.FirstOrDefault(c => c.TypeId == id) : null;
			var loadedIdx = loaded == null ? -1 : charges.IndexOf(loaded);
			var k = charges.Count;
			var n = killOrder.Count;
			var available = charges.ToDictionary(c => c.Name, c => stock.AvailableRounds(c.Name), StringComparer.OrdinalIgnoreCase);

			int Budget(ShipFit.AmmoCharge c)
			{
				if (available[c.Name] is int rounds) return rounds;
				// Hold never read: a required charge is assumed aboard (the readiness gate demanded it),
				// an optional one is not — we never click a charge we have not seen.
				return c.Optional ? 0 : int.MaxValue;
			}

			// A charge we would have to load needs at least one full volley in the hold to be worth it.
			bool Usable(int ci)
			{
				var budget = Budget(charges[ci]);
				return ci == loadedIdx ? budget > 0 : budget >= Math.Max(1, plan.LauncherCount);
			}

			var time = new double[k, Math.Max(n, 1)];
			var rounds = new int[k, Math.Max(n, 1)];
			for (var ci = 0; ci < k; ci++)
			{
				var usable = Usable(ci);
				for (var i = 0; i < n; i++)
				{
					var t = usable ? SecondsToKill(charges[ci], killOrder[i], plan) : double.PositiveInfinity;
					time[ci, i] = t;
					rounds[ci, i] = RoundsFor(t, plan);
				}
			}

			// blockedFrom[ci] = first queue position the charge may no longer be used from (stock ran dry).
			var blockedFrom = Enumerable.Repeat(n, k).ToArray();

			(double cost, int[] assign) Solve(bool forceLoadedFirst)
			{
				var best = new double[k];
				for (var ci = 0; ci < k; ci++)
					best[ci] = loadedIdx < 0 ? ReloadSeconds : (ci == loadedIdx ? 0 : double.PositiveInfinity);
				var prev = new int[k, Math.Max(n, 1)];

				for (var i = 0; i < n; i++)
				{
					var next = new double[k];
					for (var ci = 0; ci < k; ci++)
					{
						var own = i < blockedFrom[ci] ? time[ci, i] : double.PositiveInfinity;
						if (forceLoadedFirst && i == 0 && ci != loadedIdx)
							own = double.PositiveInfinity;

						var bestPrev = double.PositiveInfinity;
						var arg = ci;
						for (var pi = 0; pi < k; pi++)
						{
							var c = best[pi] + (pi == ci ? 0 : ReloadSeconds);
							if (c < bestPrev)
							{
								bestPrev = c;
								arg = pi;
							}
						}
						next[ci] = bestPrev + own;
						prev[ci, i] = arg;
					}
					best = next;
				}

				var end = 0;
				for (var ci = 1; ci < k; ci++)
					if (best[ci] < best[end]) end = ci;

				var assign = new int[n];
				var cur = end;
				for (var i = n - 1; i >= 0; i--)
				{
					assign[i] = cur;
					cur = prev[cur, i];
				}
				return (n == 0 ? 0 : best[end], assign);
			}

			if (k == 0 || n == 0)
				return new RoomPlan { Loaded = loaded, First = loaded, Seconds = 0, SecondsIfStaying = 0, Available = available };

			// Solve, then repair for stock until the assignment fits what we actually carry.
			var (cost, assign) = Solve(false);
			var lastFeasible = (cost, assign);
			for (var iteration = 0; iteration <= n * k && !double.IsInfinity(cost); iteration++)
			{
				var used = new long[k];
				var violation = -1;
				for (var i = 0; i < n; i++)
				{
					var ci = assign[i];
					used[ci] = rounds[ci, i] == int.MaxValue ? long.MaxValue : used[ci] + rounds[ci, i];
					if (used[ci] > Budget(charges[ci]))
					{
						violation = i;
						blockedFrom[ci] = Math.Min(blockedFrom[ci], i);
						break;
					}
				}
				if (violation < 0)
				{
					lastFeasible = (cost, assign);
					break;
				}
				(cost, assign) = Solve(false);
				if (!double.IsInfinity(cost))
					lastFeasible = (cost, assign);
			}
			(cost, assign) = lastFeasible;

			var (stayCost, _) = Solve(true);

			var first = double.IsInfinity(cost) ? null : charges[assign[0]];
			var swaps = new List<PlanStep>();
			if (!double.IsInfinity(cost))
			{
				var current = loadedIdx;
				for (var i = 0; i < n; i++)
				{
					if (assign[i] == current) continue;
					current = assign[i];
					swaps.Add(new PlanStep(i, charges[current], killOrder[i].Name ?? killOrder[i].Type ?? "?"));
				}
			}

			return new RoomPlan
			{
				Loaded = loaded,
				First = first,
				Seconds = cost,
				SecondsIfStaying = stayCost,
				Swaps = swaps,
				Available = available,
			};
		}

		/// <summary>Live enemies worth modelling: no caches, no wrecks, no structures we never shoot.</summary>
		private static List<IOverviewEntry> LiveEnemies(IEnumerable<IOverviewEntry>? killOrder) =>
			(killOrder ?? Enumerable.Empty<IOverviewEntry>())
			.Where(e => e != null && e.IsEnemy && !NpcInfoProvider.IsAbyssalCache(e) &&
			            !NpcInfoProvider.IsWreckName(e.Name) && !NpcInfoProvider.IsWreckName(e.Type))
			.ToList();

		/// <summary>
		/// The charge the room calls for given what is loaded now and what is left in stock. An empty
		/// room wants the first charge of the fallback order that still has rounds (reloading between
		/// rooms is free); otherwise the plan's first step, if it beats staying by the margin.
		/// </summary>
		public static ShipFit.AmmoCharge? DesiredCharge(
			ShipFit.AmmoPlan plan,
			IReadOnlyList<IOverviewEntry>? killOrder,
			int? loadedTypeId,
			AmmoStock stock,
			out string reason)
		{
			var enemies = LiveEnemies(killOrder);
			var names = plan.Charges.Select(c => c.Name).ToList();
			if (enemies.Count == 0)
			{
				var fallback = plan.FallbackOrder.FirstOrDefault(c =>
					stock.AvailableRounds(c.Name) is int rounds ? rounds >= Math.Max(1, plan.LauncherCount) : !c.Optional);
				reason = fallback == null
					? $"room clear — nothing left to load ({stock.Describe(names)})"
					: $"room clear — back to {fallback.Name} ({stock.Describe(names)})";
				return fallback;
			}

			var room = PlanRoom(plan, enemies, loadedTypeId, stock);
			reason = room.Describe();
			return room.WantsSwap ? room.First : room.Loaded;
		}

		/// <summary>
		/// Feed the stock tracker from this tick's HUD and cargo window. Called every combat tick by
		/// <see cref="GetSwitchTask"/> and by the abyss state outside combat (docked checks, looting),
		/// so hold reads taken while the launchers are idle still count.
		/// </summary>
		public static void Observe(Bot bot, ShipFit fit)
		{
			var plan = fit?.Ammo;
			if (plan is null || bot is null)
				return;

			var state = StateFor(bot.Pid);
			var info = fit.GetWeapon()?.UiModule?.ModuleInfo;
			string? loadedName = null;
			if (info?.ChargeTypeId is int typeId)
				loadedName = plan.Charges.FirstOrDefault(c => c.TypeId == typeId)?.Name ?? $"type {typeId}";

			Dictionary<string, int>? hold = null;
			try
			{
				hold = MemoryProxyInventoryProvider.TryReadShipCargoItems(bot.MemoryMeasurementAtTime?.Value);
			}
			catch
			{
				// A half-parsed inventory window must never stall combat; we just keep the last estimate.
			}

			lock (Gate)
			{
				state.Stock.ObserveCargo(hold);
				state.Stock.ObserveLauncher(loadedName, info?.ChargeQuantity);
			}
		}

		/// <summary>
		/// Right-click the weapon group and load the charge the queue calls for, or null when nothing
		/// needs doing. Returns null while the loaded charge is unknown — we never click blind.
		/// </summary>
		public static ISerializableBotTask? GetSwitchTask(
			Bot bot,
			ShipFit fit,
			IReadOnlyList<IOverviewEntry>? killOrder,
			out string reason)
		{
			reason = "";
			var plan = fit?.Ammo;
			if (plan is null || bot is null || plan.Charges.Count == 0)
				return null;

			Observe(bot, fit);

			var weapon = fit.GetWeapon()?.UiModule;
			var loaded = weapon?.ModuleInfo?.ChargeTypeId;
			if (weapon?.UINode is null || loaded is not int loadedTypeId)
				return null;

			var state = StateFor(bot.Pid);
			ShipFit.AmmoCharge? desired;
			lock (Gate)
				desired = DesiredCharge(plan, killOrder, loadedTypeId, state.Stock, out reason);

			if (desired is null || desired.TypeId == loadedTypeId || string.IsNullOrEmpty(desired.Name))
				return null;

			var now = Environment.TickCount64;
			lock (Gate)
			{
				if (state.DesiredTypeId != desired.TypeId)
				{
					state.DesiredTypeId = desired.TypeId;
					state.StableTicks = 0;
				}

				// The queue changes every volley, so a swap only fires once the plan has asked for it
				// on consecutive ticks and the previous reload has had time to finish.
				if (++state.StableTicks < StableTicksRequired)
					return null;

				if (now - state.LastCommandTick < CommandCooldownMs)
					return null;

				state.LastCommandTick = now;
			}

			// The launcher group's context menu lists every charge in the cargo hold by its full name;
			// clicking one loads it into the whole group.
			return weapon.UINode.ClickMenuEntryByRegexPattern(bot, Regex.Escape(desired.Name));
		}

		/// <summary>Drops the per-client switch state and stock (new run / client re-attach).</summary>
		public static void Reset(int pid)
		{
			lock (Gate)
				ByPid.Remove(pid);
		}
	}
}
