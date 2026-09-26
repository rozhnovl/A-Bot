using System.Diagnostics.CodeAnalysis;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public interface IShipState
	{
		bool ManeuverStartPossible { get; }
		/// <summary>Shield charge 0–100%, or null when the gauge is unreadable this tick.</summary>
		int? ShieldPercent { get; }
		/// <summary>Capacitor charge 0–100%, or null when the gauge is unreadable this tick.</summary>
		int? CapacitorPercent { get; }
		ShipManeuverType Maneuver { get; }
		/// <summary>What the HUD says the maneuver is aimed at, or null.</summary>
		string? ManeuverTarget { get; }
		/// <summary>True only when the ship is approaching this object (type AND target).</summary>
		bool IsApproaching(IOverviewEntry? entry);
		[NotNull]
		DronesContoller Drones { get; }
		[NotNull]
		ActiveTargetsContoller ActiveTargets { get; }
		bool IsInAbyss { get; }
		int AttackRange { get; }
		ShipFit Fit { get; }
		bool ShouldUseTractorForLooting { get; }
		ISerializableBotTask? GetTurnOnAlwaysActiveModulesTask();
		ISerializableBotTask? GetSetModuleActiveTask(ShipFit.ModuleType type, bool shouldBeActive);
		/// <summary>Whether the Selected Item panel currently shows this overview entry.</summary>
		bool SelectedItemPanelShows(IOverviewEntry entry);
		ISerializableBotTask? GetAttackTasks();
		ISerializableBotTask GetNextTankingModulesTask(double estimatedIncomingDps, double enemyNeutGjPerSec = 0);
		ISerializableBotTask? GetReloadTask();
		/// <summary>Act on an overview object through the Selected Item panel (select, then its button).</summary>
		ISerializableBotTask? GetSelectedItemActionTask(IOverviewEntry entry, string buttonNamePattern);
		ISerializableBotTask? GetPopupButtonTask(string buttonText);
	}
}