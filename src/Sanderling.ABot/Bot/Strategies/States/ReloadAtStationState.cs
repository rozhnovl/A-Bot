using System;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;
using Bib3;
using BotEngine.Motor;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using ListEntryExtension = Sanderling.Parse.ListEntryExtension;

namespace Sanderling.ABot.Bot.Strategies
{
	internal class ReloadAtStationState : IStragegyState
	{
		private IList<(string, int)> keptInventoryItems;
		private ReloadStage Stage;
		private IList<(string, int)> currentInventoryItems;

		private Dictionary<string, int> ItemsRequiredToRefill
		{
			get
			{
				var result = new Dictionary<string, int>();
				foreach (var item in keptInventoryItems)
				{
					int requiredQuantity = item.Item2 -
					                       currentInventoryItems.Where(i => i.Item1 == item.Item1).Sum(i => i.Item2);
					if (requiredQuantity > 0)
						result.Add(item.Item1, requiredQuantity);
				}

				return result;
			}
		}

		public ReloadAtStationState(params (string, int)[] keptInventoryItems)
		{
			this.keptInventoryItems = keptInventoryItems.ToList();
			Stage = ReloadStage.UnloadLoot;
		}

		private (string, int) GetNameAndQuantity(IListEntry entry)
		{
			var quantityString = ListEntryExtension.CellValueFromColumnHeader(entry, "Quantity")?.Replace(",", "");
			var name = ListEntryExtension.CellValueFromColumnHeader(entry, "Name");
			if (quantityString.IsNullOrEmpty())
				return (name, 1);
			return (name, int.Parse(quantityString));
		}

		private int currentItemQuantity;
		public IBotTask GetStateActions(Bot bot)
		{
			var task = new DynamicTask();
			throw new NotImplementedException();/*
			var inventoryWindow = bot.MemoryMeasurementAtTime.Value.WindowInventory?.SingleOrDefault();
			//if (inventoryWindow == null)
			//	return bot.MemoryMeasurementAtTime.Value.Neocom?.InventoryButton?.ClickTask();
			switch (Stage)
			{
				case ReloadStage.UnloadLoot:
				{ 
					if (inventoryWindow.ActiveShipSelectedCargoSpaceTypeEnum != ShipCargoSpaceTypeEnum.General)
						return inventoryWindow.ActiveShipEntry.ClickTask();

					var notSelectedItems =
						inventoryWindow.SelectedContainerInventory.ItemsView.Where(e => !(e.IsSelected ?? false));

					foreach (var item in notSelectedItems.Where(i =>
						!keptInventoryItems.Any(ki => ListEntryExtension.CellValueFromColumnHeader(i, "Name") == ki.Item1)))
						return item.ClickMenuEntryWithModifierKey(bot, VirtualKeyCode.CONTROL);

					if (inventoryWindow.SelectedContainerInventory.ItemsView.Any(e => (e.IsSelected ?? false)))
					{
						var lootMoveDestination =
							inventoryWindow.LeftTreeListEntry.SelectMany(e => e.Child ?? new ITreeViewEntry[0])
								.FirstOrDefault(e => e.Text == "Abyssal loot")
							?? inventoryWindow.ItemHangarEntry;

						//TODO Change view mode to list
						//var selectedItem =
						//	inventoryWindow.SelectedRightInventory.ListView.Entry.FirstOrDefault(e => e.IsSelected ?? false);
						//if (selectedItem == null)
						//	return inventoryWindow.SelectedRightInventory.ListView.Entry.FirstOrDefault().ClickTask();
						return new BotTask("test")
						{
							ClientActions = new[]
							{
								inventoryWindow.SelectedContainerInventory.ItemsView
									.FirstOrDefault(e => e.IsSelected ?? false)
									.MouseDragAndDropOn(lootMoveDestination.RegionInteraction, MouseButtonIdEnum.Left)
									.AsRecommendation()
							}
						};
					}

					currentInventoryItems = inventoryWindow.SelectedContainerInventory.ItemsView.Select(GetNameAndQuantity)
						.ToList();
					Stage = ReloadStage.Refill;
					break;
				}

				case ReloadStage.Refill:
				{
					task.With("Items required to refill: " +
					          string.Join("", ItemsRequiredToRefill.Select(i => $"\n\t{i.Key}:{i.Value}")));

					var setQuantityWindow =
						bot.MemoryMeasurementAtTime.Value.WindowOther?.FirstOrDefault(w => w.Caption == "Set Quantity");
					if (setQuantityWindow != null)
					{
						if (setQuantityWindow.InputText.SingleOrDefault(t =>
							    t.Text != currentItemQuantity.ToString()) != null)
							return new EnterTextTask(currentItemQuantity.ToString());
						else
						{
							return task.With(
								setQuantityWindow.ButtonText.Single(bt => bt.Text == "OK").ClickTask());
						}
					}


					if (inventoryWindow.ActiveShipSelectedCargoSpaceTypeEnum != null)
						return inventoryWindow.ItemHangarEntry.LabelText.Single(e => e.Text == "Item hangar")
							.ClickTask();

					var notSelectedItems =
						inventoryWindow.SelectedRightInventory.ListView.Entry.Where(e => !(e.IsSelected ?? false));

					foreach (var requiredItem in ItemsRequiredToRefill)
					{
						var itemInHangar = inventoryWindow.SelectedRightInventory.ListView.Entry
							.FirstOrDefault(e => ListEntryExtension.CellValueFromColumnHeader(e, "Name") == requiredItem.Key);
						//TODO Critical state
						if (itemInHangar == null)
							return task.With($"Could not find '{requiredItem.Key}'");
						currentItemQuantity = requiredItem.Value;
						currentInventoryItems.Add((requiredItem.Key, requiredItem.Value));
						return task.With(itemInHangar.DragElementTo(inventoryWindow.ActiveShipEntry,
							VirtualKeyCode.SHIFT));
					}

					MoveToNext = true;
					break;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}

			return null;*/
		}

		public IBotTask GetStateExitActions(Bot bot)
		{
			var inventoryWindow = bot.MemoryMeasurementAtTime.Value.WindowInventory?.SingleOrDefault();
			if (inventoryWindow != null)
				return bot.MemoryMeasurementAtTime.Value.Neocom.InventoryButton.ClickTask();
			return null;
		}

		public bool MoveToNext { get; private set; }

		private enum ReloadStage
		{
			UnloadLoot,
			Refill,
		}
	}
}