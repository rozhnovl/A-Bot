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
		/// <summary>The FC's tag drawn on the target icon ("1".."9", "A".."Z"), when the label carries one.</summary>
		string? Tag { get; }
		ISerializableBotTask GetUnlockTask();
		ISerializableBotTask GetOrbitTask();
		/// <summary>Plain click on the target icon: makes this locked target the client's active one.</summary>
		ISerializableBotTask GetMakeActiveTask();
		bool WeaponAssigned { get; }
		bool DroneAssigned { get; }
	}
}