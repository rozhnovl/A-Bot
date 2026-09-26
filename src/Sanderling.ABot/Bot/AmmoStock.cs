using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.ABot.Bot
{
	/// <summary>
	/// How many rounds of each charge one client still has: what the hold holds plus what sits in
	/// the launchers. The hold count is exact whenever the ship-cargo window is readable (docked
	/// readiness check, looting) and is carried forward by reload arithmetic in between — a reload
	/// of the same charge pulls the top-up out of the hold, a swap returns the old rounds to the hold
	/// and pulls the new ones out. Firing only drains the launchers, which the HUD reports directly.
	/// Nothing here throws; unknown stays unknown (null) rather than guessed.
	/// </summary>
	public sealed class AmmoStock
	{
		private readonly Dictionary<string, int> hold = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>True once the hold has been read at least once; before that every count is null.</summary>
		public bool HoldKnown { get; private set; }
		public DateTime HoldReadUtc { get; private set; }
		/// <summary>Charge name the launcher group reports, or null when it shows no charge.</summary>
		public string? LoadedName { get; private set; }
		/// <summary>Rounds in the launcher group (summed), or null when the HUD does not expose it.</summary>
		public int? LoadedQuantity { get; private set; }
		/// <summary>Largest launcher-group count ever seen: the group's magazine, learned live.</summary>
		public int ObservedMagazine { get; private set; }

		/// <summary>Exact hold snapshot (item name → quantity); null = no readable cargo window this tick.</summary>
		public void ObserveCargo(IReadOnlyDictionary<string, int>? items)
		{
			if (items == null) return;
			hold.Clear();
			foreach (var (name, qty) in items)
				hold[name] = Math.Max(0, qty);
			HoldKnown = true;
			HoldReadUtc = DateTime.UtcNow;
		}

		/// <summary>What the launcher group shows this tick: the loaded charge (by plan name) and its count.</summary>
		public void ObserveLauncher(string? chargeName, int? quantity)
		{
			if (quantity is int seen)
				ObservedMagazine = Math.Max(ObservedMagazine, seen);

			if (HoldKnown && chargeName != null && quantity is int qty)
			{
				if (LoadedName != null && !LoadedName.Equals(chargeName, StringComparison.OrdinalIgnoreCase))
				{
					// A swap: the previous rounds went back into the hold, the new ones came out of it.
					if (LoadedQuantity is int returned)
						Adjust(LoadedName, returned);
					Adjust(chargeName, -qty);
				}
				else if (LoadedQuantity is int before && qty > before)
				{
					// Same charge, more rounds than before: a reload topped the group up from the hold.
					Adjust(chargeName, -(qty - before));
				}
			}

			LoadedName = chargeName;
			LoadedQuantity = quantity;
		}

		private void Adjust(string name, int delta)
		{
			hold.TryGetValue(name, out var current);
			hold[name] = Math.Max(0, current + delta);
		}

		/// <summary>Rounds still in the hold for a charge; null until the hold has been read.</summary>
		public int? InHold(string name) =>
			HoldKnown ? (hold.TryGetValue(name, out var qty) ? qty : 0) : null;

		/// <summary>
		/// Rounds we can still fire with this charge: hold plus launchers when it is the loaded one.
		/// Null until the hold has been read — the planner treats that as "plenty" for required
		/// charges and as "absent" for optional ones.
		/// </summary>
		public int? AvailableRounds(string name)
		{
			var inHold = InHold(name);
			if (inHold is not int total) return null;
			if (LoadedName != null && LoadedName.Equals(name, StringComparison.OrdinalIgnoreCase) && LoadedQuantity is int loaded)
				total += loaded;
			return total;
		}

		public string Describe(IEnumerable<string> chargeNames)
		{
			if (!HoldKnown) return "hold not read yet";
			var parts = chargeNames.Select(n =>
			{
				var loadedMark = LoadedName != null && LoadedName.Equals(n, StringComparison.OrdinalIgnoreCase) ? "*" : "";
				return $"{n}{loadedMark} {AvailableRounds(n)}";
			});
			return string.Join(", ", parts);
		}
	}
}
