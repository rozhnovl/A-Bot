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
	}
}