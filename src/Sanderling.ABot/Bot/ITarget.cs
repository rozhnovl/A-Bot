namespace Sanderling.ABot.Bot
{
	public interface ITarget
	{
		int Distance { get; }
		int AssignedEffectsCount { get; }
		string Name { get; }
		ISerializableBotTask GetUnlockTask();
		ISerializableBotTask GetOrbitTask();
		bool WeaponAssigned { get; }
		bool DroneAssigned { get; }
	}
}