using System.Diagnostics.CodeAnalysis;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public interface IShipState
	{
		bool ManeuverStartPossible { get; }
		IShipHitpointsAndEnergy HitpointsAndEnergy { get; }
		ShipManeuverType Maneuver { get; }
		[NotNull]
		DronesContoller Drones { get; }
		[NotNull]
		ActiveTargetsContoller ActiveTargets { get; }
		bool IsInAbyss { get; }
		int AttackRange { get; }
		ISerializableBotTask? GetTurnOnAlwaysActiveModulesTask();
		ISerializableBotTask? GetSetModuleActiveTask(ShipFit.ModuleType type, bool shouldBeActive);
		ISerializableBotTask? GetAttackTasks();
		ISerializableBotTask GetNextTankingModulesTask(double estimatedIncomingDps);
		ISerializableBotTask? GetReloadTask();
		ISerializableBotTask GetPopupButtonTask(string buttonText);
	}
}