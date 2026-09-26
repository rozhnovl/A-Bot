namespace Sanderling.ABot.Bot
{
	public interface ITarget
	{
		int Distance { get; }
		int AssignedEffectsCount { get; }
		string Name { get; }
		/// <summary>
		/// How much of the target is left, 0..1, averaged over shield/armor/hull (the target readout is
		/// per-mille per layer). Null when the client does not report it. Shared with the other windows
		/// so nobody wastes volleys on a rat that is already dying.
		/// </summary>
		double? RemainingHitpointsFraction { get; }
		ISerializableBotTask GetUnlockTask();
		ISerializableBotTask GetOrbitTask();
		bool WeaponAssigned { get; }
		bool DroneAssigned { get; }
	}
}