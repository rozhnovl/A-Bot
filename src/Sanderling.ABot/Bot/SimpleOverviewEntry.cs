namespace Sanderling.ABot.Bot
{
	public abstract class SimpleOverviewEntry : IOverviewEntry
	{
		public SimpleOverviewEntry(string type, string name, bool isEnemy, int distance, bool meTargeted,
			bool meActiveTarget, long id)
		{
			Type = type;
			Name = name??type;
			IsEnemy = isEnemy;
			Distance = distance;
			MeTargeted = meTargeted;
			MeActiveTarget = meActiveTarget;
			Id = id;
		}

		public string Type { get; private set; }
		public string Name { get; private set; }
		public bool IsEnemy { get; }
		public int Distance { get; }
		public bool MeTargeted { get; }
		public bool MeActiveTarget { get; }
		public long Id { get; }
		public abstract ISerializableBotTask ClickMenuEntryByRegexPattern(string orbit, string km);
		public abstract ISerializableBotTask GetSelectTask();
	}
}