using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public class MemoryProxyInventoryProvider : IInventoryProvider
	{
		private readonly Bot bot;
		private readonly Sanderling.Parse.IMemoryMeasurement memoryMeasurement;
		private IWindowInventory? selectedWindowInventory;

		public MemoryProxyInventoryProvider(Bot bot, IWindowInventory? selectedWindowInventory = null)
		{
			this.bot = bot;
			memoryMeasurement = bot.MemoryMeasurementAtTime.Value;
			this.selectedWindowInventory = selectedWindowInventory ?? memoryMeasurement?.WindowInventory?.FirstOrDefault();
		}

		public ISerializableBotTask? GetCloseWindowTask()
		{
			if ((memoryMeasurement?.WindowInventory?.Any() ?? false))
				return memoryMeasurement.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public ISerializableBotTask? GetOpenWindowTask()
		{
			if (!(memoryMeasurement?.WindowInventory?.Any() ?? false))
				return memoryMeasurement.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public ISerializableBotTask? GetActvateItemIfPresentTask(string mobileTractorUnit, string launch)
		{
			var tractorInCargo = selectedWindowInventory.SelectedContainerInventory.ItemsView
				.FirstOrDefault(lt => lt.CellsTexts.ContainsValue(mobileTractorUnit));
			return tractorInCargo != null ? tractorInCargo.ClickMenuEntryByRegexPattern(bot, launch) : null;
		}

		public IInventoryProvider? GetLootableWindow()
		{
			var lootWindow = memoryMeasurement?.WindowInventory
				?.FirstOrDefault(wi => wi?.LootAllButton !=null);
			return lootWindow != null ? new MemoryProxyInventoryProvider(bot, lootWindow) : null;
		}

		public bool IsEmpty => !(selectedWindowInventory?.SelectedContainerInventory?.ItemsView?.Any() ?? false);

		/// <summary>
		/// When the selected container has items but they carry no readable name (ICON view — only a
		/// quantity badge is in memory, the name lives in a hover tooltip we can't read), returns a click
		/// on the "switch to List view" button so cargo becomes name-readable. Null when already readable
		/// or empty. Makes the readiness/cargo checks robust to the player's inventory view setting.
		/// </summary>
		public ISerializableBotTask? GetSwitchToListViewTaskIfNeeded()
		{
			var items = selectedWindowInventory?.SelectedContainerInventory?.ItemsView;
			if (items == null || items.Count == 0) return null;

			var anyNamed = items.Any(it => it.CellsTexts != null &&
			                                (it.CellsTexts.ContainsKey("Name") || it.CellsTexts.ContainsKey("Hint")));
			if (anyNamed) return null; // already List view / names available

			var viewModeButton = selectedWindowInventory?.SwitchToListViewButton;
			if (viewModeButton == null) return null;

			// Left-click the dropdown, then pick a view that puts item NAMES in the tree. "Details" is the
			// column table the item parser reads; "List" is the acceptable fallback.
			return new Task.MenuPathTask
			{
				Bot = bot,
				RootUIElement = viewModeButton,
				OpenWithLeftClick = true,
				ListMenuListPriorityEntryRegexPattern = new[] { new[] { "^Details$", "^List$" } },
			};
		}

		/// <summary>
		/// Check the required cargo (ammo/consumables) against the open inventory. Drone-bay items are
		/// skipped (verified via the drone window instead). Requires the inventory window open; when it
		/// isn't, every non-drone requirement reports not-ok with "cargo not readable".
		/// </summary>
		public IReadOnlyList<(string Item, bool Ok, string Detail)> CheckCargo(
			IEnumerable<Configuration.CargoRequirement> requirements)
		{
			// Read OUR hold, not merely whatever container happens to be selected: with a wreck open the
			// selected container is the wreck, and the gate then reports every required item as missing
			// (observed live 2026-09-18). A loot window is the one carrying a "Loot All" button.
			var shipCargoWindow = memoryMeasurement?.WindowInventory?.FirstOrDefault(w => w != null && w.LootAllButton == null)
			                      ?? selectedWindowInventory;
			var items = shipCargoWindow?.SelectedContainerInventory?.ItemsView;
			var result = new List<(string, bool, string)>();
			foreach (var req in requirements)
			{
				if (req.InDroneBay) continue;
				if (items == null) { result.Add((req.Item, false, "cargo not readable — open inventory")); continue; }

				var match = items.FirstOrDefault(it =>
					it.CellsTexts != null &&
					it.CellsTexts.Values.Any(v => v != null && v.Contains(req.Item, StringComparison.OrdinalIgnoreCase)));
				if (match == null) { result.Add((req.Item, false, "missing from cargo")); continue; }

				// Quantity lives in a separate cell; take the largest integer among the row's cells.
				var qty = match.CellsTexts.Values
					.Select(v => int.TryParse(new string((v ?? "").Where(char.IsDigit).ToArray()), out var n) ? n : 0)
					.DefaultIfEmpty(0).Max();
				var ok = qty == 0 || qty >= req.Quantity; // qty unreadable => accept presence
				result.Add((req.Item, ok, qty > 0 ? $"{qty} (need {req.Quantity})" : "present"));
			}
			return result;
		}

		public ISerializableBotTask? GetClickLootButtonTask()
		{
			var lootButton = selectedWindowInventory?.LootAllButton;
			return lootButton?.ClickTask();
		}

		/// <summary>
		/// Snapshot of the SHIP cargo as item name → quantity, or null when no readable ship-cargo
		/// window is open (loot windows — the ones with a LootAllButton — are skipped so a wreck's
		/// contents never masquerade as our hold). Feeds the loot ledger: successive snapshots are
		/// diffed to record what a run picked up and burned (see RoomStatsRecorder.ObserveCargo).
		/// Name comes from the List-view "Name" column or the ICON-view hint's first line; quantity
		/// from the "Quantity" cell or the icon badge (a bare-integer text), defaulting to 1.
		/// </summary>
		public static Dictionary<string, int>? TryReadShipCargoItems(Sanderling.Parse.IMemoryMeasurement? memoryMeasurement)
		{
			var window = memoryMeasurement?.WindowInventory?.FirstOrDefault(w => w != null && w.LootAllButton == null);
			var items = window?.SelectedContainerInventory?.ItemsView;
			if (items == null)
				return null;

			var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			foreach (var item in items)
			{
				var cells = item?.CellsTexts;
				if (cells == null || cells.Count == 0) continue;

				string? name = null;
				if (cells.TryGetValue("Name", out var n) && !string.IsNullOrWhiteSpace(n))
					name = n.Trim();
				else if (cells.TryGetValue("Hint", out var h) && !string.IsNullOrWhiteSpace(h))
					name = h.Split('\n', '\r')[0].Trim();
				if (name == null) continue;

				var qty = 1;
				if (cells.TryGetValue("Quantity", out var q) &&
				    int.TryParse(new string(q.Where(char.IsDigit).ToArray()), out var qn) && qn > 0)
					qty = qn;
				else
				{
					// ICON view: the stack size is a bare-integer badge among the raw texts (_tN).
					var badge = cells.Where(kv => kv.Key.StartsWith("_t"))
						.Select(kv => int.TryParse(kv.Value.Replace(" ", "").Replace(",", "").Replace(".", ""), out var b) ? b : 0)
						.Where(b => b > 0).DefaultIfEmpty(0).Max();
					if (badge > 0) qty = badge;
				}

				result[name] = result.TryGetValue(name, out var existing) ? existing + qty : qty;
			}
			return result;
		}
	}
}