using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public interface IOverviewEntry
	{
		string Type { get; }
		string Name { get; }
		bool IsEnemy { get; }
		int Distance { get; }
		bool MeTargeted { get; }
		bool MeActiveTarget { get; }
		long Id { get; }
		ISerializableBotTask ClickMenuEntryByRegexPattern(string path1, string path2 = null);
		ISerializableBotTask GetSelectTask();
		/// <summary>
		/// The clickable overview row, so several entries can be locked in ONE ctrl-held batch instead of
		/// one lock per tick (fewer mouse round-trips, fewer chances for the distance-sorted overview to
		/// shuffle a row under the cursor). Null when this entry has no backing UI element.
		/// </summary>
		IUIElement? SelectElement { get; }
		public OverviewWindowEntryCommonIndications CommonIndications { get; }
		ISerializableBotTask GetApproachTask();
	}
}