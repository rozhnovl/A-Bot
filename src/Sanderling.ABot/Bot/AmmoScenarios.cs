using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.ABot.Bot.Configuration;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	/// <summary>
	/// Offline scenarios for the charge planner: hand-built kill queues and stock levels run through
	/// <see cref="AmmoController.PlanRoom"/> / <see cref="AmmoController.DesiredCharge"/>, printing the
	/// decision and checking the ones we rely on. Doubles as a smoke test and as living documentation
	/// of the ammo logic. No live client involved: <c>FleetOrchestrator --brain-selftest</c>.
	/// </summary>
	public static class AmmoScenarios
	{
		/// <summary>An overview row with just what the planner reads: type, distance, enemy flag.</summary>
		private sealed class Rat : IOverviewEntry
		{
			private static long nextId = 1;

			public Rat(string type, int distance = 15000)
			{
				Type = type;
				Name = type;
				Distance = distance;
				Id = nextId++;
			}

			public string Type { get; }
			public string Name { get; }
			public bool IsEnemy => true;
			public int Distance { get; }
			public bool MeTargeted => false;
			public bool MeActiveTarget => false;
			public long Id { get; }
			public ISerializableBotTask ClickMenuEntryByRegexPattern(string path1, string path2 = null) => null!;
			public ISerializableBotTask GetSelectTask() => null!;
			public IUIElement? SelectElement => null;
			public OverviewWindowEntryCommonIndications CommonIndications => default!;
			public ISerializableBotTask GetApproachTask() => null!;
			public override string ToString() => Type;
		}

		private const string Nova = "Caldari Navy Nova Light Missile";
		private const string FuryNova = "Nova Fury Light Missile";
		private const string Inferno = "Caldari Navy Inferno Light Missile";
		private const int NovaId = 27381, FuryNovaId = 24497, InfernoId = 27371;

		private static List<IOverviewEntry> Queue(params string[] types) =>
			types.Select(t => (IOverviewEntry)new Rat(t)).ToList();

		/// <summary>Stock with a known hold and a launcher group showing the loaded charge.</summary>
		private static AmmoStock Stock(Dictionary<string, int>? hold, string? loaded, int? loadedQty)
		{
			var stock = new AmmoStock();
			stock.ObserveCargo(hold);
			stock.ObserveLauncher(loaded, loadedQty);
			return stock;
		}

		private static Dictionary<string, int> Hold(int nova, int furyNova, int inferno = 0) =>
			new(StringComparer.OrdinalIgnoreCase)
			{
				[Nova] = nova,
				[FuryNova] = furyNova,
				[Inferno] = inferno,
			};

		/// <summary>Runs every scenario, logs decisions, returns the number of failed checks.</summary>
		public static int Run(Action<string> log)
		{
			var plan = FitsRegistry.HawkAmmoPlan();
			var failures = 0;

			void Check(string what, bool ok, string detail)
			{
				log($"    [{(ok ? " ok " : "FAIL")}] {what}: {detail}");
				if (!ok) failures++;
			}

			AmmoController.RoomPlan Scenario(string title, List<IOverviewEntry> queue, AmmoStock stock, int loadedTypeId, ShipFit.AmmoPlan? withPlan = null)
			{
				var p = withPlan ?? plan;
				var room = AmmoController.PlanRoom(p, queue, loadedTypeId, stock);
				var desired = AmmoController.DesiredCharge(p, queue, loadedTypeId, stock, out var reason);
				log($"  {title}");
				log($"    queue: {string.Join(", ", queue.Select(e => e.Type))}");
				log($"    -> {reason}");
				log($"    -> desired now: {desired?.Name ?? "(nothing)"}");
				return room;
			}

			log("=== Ammo planner scenarios (Hawk plan, single ship, Dark ×1.3) ===");

			// 1. Mixed room: frigates first, a battleship last — swap only once the frigates are gone.
			{
				var room = Scenario("1) Kikimora ×3 then a Leshak, Navy Nova loaded, hold full",
					Queue("Anchoring Kikimora", "Anchoring Kikimora", "Anchoring Kikimora", "Striking Leshak"),
					Stock(Hold(600, 200), Nova, 212), NovaId);
				Check("no swap now", !room.WantsSwap && room.First?.TypeId == NovaId, $"first={room.First?.Name}");
				Check("Fury Nova planned for the Leshak", room.Swaps.Count == 1 && room.Swaps[0].Index == 3 && room.Swaps[0].Charge.TypeId == FuryNovaId,
					string.Join("; ", room.Swaps.Select(s => $"[{s.Index}] {s.Charge.Name}")));
			}

			// 2. Battleships only: Fury pays for its reload immediately (plenty of Fury aboard).
			{
				var room = Scenario("2) Leshak pair, Navy Nova loaded, 1000 Fury Nova aboard",
					Queue("Striking Leshak", "Striking Leshak"), Stock(Hold(600, 1000), Nova, 212), NovaId);
				Check("swap to Fury Nova now", room.WantsSwap && room.First?.TypeId == FuryNovaId && room.Swaps.Count == 1,
					$"{room.Seconds:F0}s vs {room.SecondsIfStaying:F0}s staying, swaps {room.Swaps.Count}");
			}

			// 2b. Same pair with the per-run minimum of 200 Fury: 4 launchers at a 4.8 s cycle burn 50 rounds a
			//     minute, so 200 rounds are ~4 min of fire — one Leshak. The plan hands the second one back to Navy.
			{
				var room = Scenario("2b) Leshak pair, Navy Nova loaded, only 200 Fury Nova aboard",
					Queue("Striking Leshak", "Striking Leshak"), Stock(Hold(600, 200), Nova, 212), NovaId);
				Check("Fury runs dry after one Leshak — plan returns to Navy", room.Swaps.Count == 2 && room.Swaps[1].Charge.TypeId == NovaId,
					string.Join("; ", room.Swaps.Select(s => $"[{s.Index}] {s.Charge.Name}")) + $" ({room.Seconds:F0}s vs {room.SecondsIfStaying:F0}s)");
			}

			// 3. Healer first: the lookahead keeps Navy for the Rodiva, Fury comes after it.
			{
				var room = Scenario("3) Rodiva then Leshak pair, Navy Nova loaded, 1000 Fury Nova aboard",
					Queue("Renewing Rodiva", "Striking Leshak", "Striking Leshak"), Stock(Hold(600, 1000), Nova, 212), NovaId);
				Check("kill the Rodiva on Navy first", !room.WantsSwap && room.First?.TypeId == NovaId, $"first={room.First?.Name}");
				Check("Fury Nova planned from queue position 1", room.Swaps.Count == 1 && room.Swaps[0].Index == 1 && room.Swaps[0].Charge.TypeId == FuryNovaId,
					string.Join("; ", room.Swaps.Select(s => $"[{s.Index}] {s.Charge.Name}")));
			}

			// 4. Fury ran out: the queue falls back to Navy without a pointless reload.
			{
				var room = Scenario("4) Leshak pair, Navy Nova loaded, NO Fury left in the hold",
					Queue("Striking Leshak", "Striking Leshak"), Stock(Hold(600, 0), Nova, 212), NovaId);
				Check("stay on Navy Nova", !room.WantsSwap && room.First?.TypeId == NovaId, $"available: {room.Available[FuryNova]} Fury");
			}

			// 5. Fury is aboard but only a handful: the budget repair keeps it out of a job it can't finish.
			{
				var room = Scenario("5) Leshak pair, Navy Nova loaded, only 40 Fury Nova in the hold",
					Queue("Striking Leshak", "Striking Leshak"), Stock(Hold(600, 40), Nova, 212), NovaId);
				Check("40 rounds can't clear a Leshak — stay on Navy", !room.WantsSwap && room.First?.TypeId == NovaId, $"first={room.First?.Name}");
			}

			// 6. Loaded charge exhausted mid-room (launchers empty, none in the hold): immediate fallback.
			{
				var room = Scenario("6) Vedmak trio, Navy Nova loaded but empty and none in the hold",
					Queue("Liminal Vedmak", "Liminal Vedmak", "Liminal Vedmak"), Stock(Hold(0, 200), Nova, 0), NovaId);
				Check("fall back to Fury Nova", room.WantsSwap && room.First?.TypeId == FuryNovaId,
					$"staying={(double.IsInfinity(room.SecondsIfStaying) ? "impossible" : room.SecondsIfStaying.ToString("F0"))}");
			}

			// 7. Optional charge: ignored until seen in the hold, used once it is (Damavik/Kiki like thermal a bit more).
			{
				var queue = Queue("Tangling Damavik", "Tangling Damavik", "Tangling Damavik", "Anchoring Kikimora", "Anchoring Kikimora");
				var unseen = new AmmoStock();
				unseen.ObserveLauncher(Nova, 212);
				var roomUnseen = Scenario("7a) Damavik/Kiki mix, hold never read (Inferno unseen)", queue, unseen, NovaId);
				Check("Inferno never loaded blind", roomUnseen.First?.TypeId == NovaId && !roomUnseen.WantsSwap, $"first={roomUnseen.First?.Name}");
				var roomFew = Scenario("7b) same room, hold read: 300 Inferno aboard (covers ~40% of the room)", queue, Stock(Hold(600, 200, inferno: 300), Nova, 212), NovaId);
				Check("300 Inferno can't pay for two reloads — stay on Nova", roomFew.First?.TypeId == NovaId && !roomFew.WantsSwap,
					$"{roomFew.Seconds:F0}s vs {roomFew.SecondsIfStaying:F0}s on Nova");
				var roomSeen = Scenario("7c) same room, hold read: 1000 Inferno aboard", queue, Stock(Hold(600, 200, inferno: 1000), Nova, 212), NovaId);
				Check("Inferno chosen once seen in quantity", roomSeen.First?.TypeId == InfernoId && roomSeen.WantsSwap,
					$"{roomSeen.Seconds:F0}s vs {roomSeen.SecondsIfStaying:F0}s on Nova (must beat it by > {AmmoController.MinSavedSeconds:F0}s after the {AmmoController.ReloadSeconds:F0}s reload)");
			}

			// 8. Room clear: back to the default, or down the fallback order when the default is gone.
			{
				var reason = "";
				var back = AmmoController.DesiredCharge(plan, Queue(), FuryNovaId, Stock(Hold(600, 100), FuryNova, 100), out reason);
				log($"  8a) room clear, Fury loaded -> {back?.Name} ({reason})");
				Check("back to Navy Nova", back?.TypeId == NovaId, back?.Name ?? "null");
				var noNova = AmmoController.DesiredCharge(plan, Queue(), FuryNovaId, Stock(Hold(0, 100), FuryNova, 100), out reason);
				log($"  8b) room clear, Navy Nova gone -> {noNova?.Name} ({reason})");
				Check("keep Fury when Navy is gone", noNova?.TypeId == FuryNovaId, noNova?.Name ?? "null");
				var onlyInferno = AmmoController.DesiredCharge(plan, Queue(), FuryNovaId, Stock(Hold(0, 0, inferno: 300), FuryNova, 0), out reason);
				log($"  8c) room clear, only Inferno left -> {onlyInferno?.Name} ({reason})");
				Check("optional Inferno is the last resort", onlyInferno?.TypeId == InfernoId, onlyInferno?.Name ?? "null");
			}

			// 9. Stock arithmetic: firing drains launchers only, reloads and swaps move rounds hold <-> launchers.
			{
				var stock = new AmmoStock();
				stock.ObserveCargo(Hold(600, 200));
				stock.ObserveLauncher(Nova, 212);
				Check("hold + launchers", stock.AvailableRounds(Nova) == 812, $"Nova {stock.AvailableRounds(Nova)}");
				stock.ObserveLauncher(Nova, 100);                    // fired 112
				Check("firing drains the group only", stock.AvailableRounds(Nova) == 700 && stock.InHold(Nova) == 600, $"Nova {stock.AvailableRounds(Nova)}, hold {stock.InHold(Nova)}");
				stock.ObserveLauncher(Nova, 212);                    // reloaded from the hold
				Check("reload comes out of the hold", stock.InHold(Nova) == 488 && stock.AvailableRounds(Nova) == 700, $"hold {stock.InHold(Nova)}, avail {stock.AvailableRounds(Nova)}");
				stock.ObserveLauncher(FuryNova, 200);                // swapped: Nova back to hold, Fury out
				Check("swap returns the old rounds", stock.InHold(Nova) == 700 && stock.InHold(FuryNova) == 0 && stock.AvailableRounds(FuryNova) == 200,
					$"hold Nova {stock.InHold(Nova)}, Fury {stock.InHold(FuryNova)}, avail Fury {stock.AvailableRounds(FuryNova)}");
				log($"  9) stock arithmetic: {stock.Describe(new[] { Nova, FuryNova })}, magazine seen {stock.ObservedMagazine}");
			}

			// 10. Trio: the same mixed room with three Hawks on the target — reload weighs three times more.
			{
				var trio = plan with { SharedShooters = 3 };
				var room = Scenario("10) Kikimora ×3 then a Leshak, three Hawks sharing targets",
					Queue("Anchoring Kikimora", "Anchoring Kikimora", "Anchoring Kikimora", "Striking Leshak"),
					Stock(Hold(600, 200), Nova, 212), NovaId, trio);
				Check("no swap now for the trio", !room.WantsSwap && room.First?.TypeId == NovaId, $"first={room.First?.Name}, {room.Seconds:F0}s");
			}

			log(failures == 0 ? "=== Ammo scenarios: all checks passed ===" : $"=== Ammo scenarios: {failures} check(s) FAILED ===");
			return failures;
		}
	}
}
