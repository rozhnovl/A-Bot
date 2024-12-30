using System;
using System.Collections.Generic;
using System.IO;
using BotEngine;
using Newtonsoft.Json;
using NUnit.Framework;
using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.Interface.MemoryStruct;
using IOverviewEntry = Sanderling.ABot.Bot.IOverviewEntry;

namespace Sanderling.ABot.Test.Exe.Abyssal
{
	[TestFixture]
	class AbyssalStrategyTest
	{
		private AbyssalRunner runner;

		[Test]
		public void Setup()
		{
			var fileName = "R:\\AbyssalRun_25_11_02_26_476.log";
			var state = new AbyssalFightState();
			var fileContent = File.ReadAllLines(fileName);
			for (int i = 0; i < fileContent.Length; i += 2)
			{
				var inputString = fileContent[i].Substring(31);
				var resultString = fileContent[i + 1].Substring(30);
				var stateInput = inputString.DeserializeFromString<MockedStateInput>();
				var receivedResult = state.GetActions(stateInput.ShipState, stateInput.OverviewProvider,
					stateInput.InventoryProvider);
			}
		}

        [JsonObject]
		public class MockedStateInput
		{
			public MockedStateInput(MockedShipState shipState, MockedOverviewProvider overviewProvider, MockedInventoryProvider inventoryProvider)
			{
				ShipState = shipState;
				OverviewProvider = overviewProvider;
				InventoryProvider = inventoryProvider;
			}
            [JsonProperty]
			public MockedShipState ShipState { get; set; }
			[JsonProperty]
			public MockedOverviewProvider OverviewProvider { get; set; }
			[JsonProperty]
			public MockedInventoryProvider InventoryProvider { get; set; }
		}

		[JsonObject]
		public class MockedShipState : IShipState
		{
			[JsonProperty]
			public bool ManeuverStartPossible { get; set; }

			IShipHitpointsAndEnergy IShipState.HitpointsAndEnergy => HitpointsAndEnergy;

			[JsonProperty]
			public ShipHitpointsAndEnergy HitpointsAndEnergy { get; }
			[JsonProperty]
			public ShipManeuverTypeEnum Maneuver { get; set; }
			[JsonProperty]
			public DronesContoller Drones { get; set; }
			[JsonProperty]
			public ActiveTargetsContoller ActiveTargets { get; set; }
            [JsonProperty]
			public bool IsInAbyss { get; set; }
			public ISerializableBotTask GetTurnOnAlwaysActiveModulesTask()
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetSetModuleActiveTask(ShipFit.ModuleType type, bool shouldBeActive)
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetNextTankingModulesTask(double estimatedIncomingDps)
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetReloadTask()
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetPopupButtonTask(string buttonText)
			{
				throw new NotImplementedException();
			}
		}

        [JsonObject]
		public class MockedInventoryProvider : IInventoryProvider
		{
			[JsonProperty]
			public bool IsEmpty { get; set; }
			public ISerializableBotTask GetClickLootButtonTask()
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetOpenWindowTask()
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetActvateItemIfPresentTask(string ragingExoticFilament, string use)
			{
				throw new NotImplementedException();
			}

			public ISerializableBotTask GetCloseWindowTask()
			{
				throw new NotImplementedException();
			}

			public IInventoryProvider GetLootableWindow()
			{
				throw new NotImplementedException();
			}
		}
        [JsonObject]
		public class MockedOverviewProvider : IOverviewProvider
		{
			[JsonProperty]
			public IList<IOverviewEntry> Entries { get; set; }

			public int CalculateApproximateDps(NpcInfoProvider npcInfoProvider)
			{
				return 0;
			}
		}
	}
}
