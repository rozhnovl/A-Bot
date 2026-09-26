using Sanderling.Interface.MemoryStruct;

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
		public virtual string? Tag => null;
		public virtual IReadOnlyCollection<string> IconNames => System.Array.Empty<string>();
		public virtual string? IconTexturePath => null;
		public virtual bool IsEmptyWreck => false;
		public abstract ISerializableBotTask ClickMenuEntryByRegexPattern(string orbit, string km);
		public abstract ISerializableBotTask GetSelectTask();
		/// <summary>Never serialized: UIElement holds parent back-references (self-referencing loop).</summary>
		[Newtonsoft.Json.JsonIgnore]
		public virtual IUIElement? SelectElement => null;
		public abstract OverviewWindowEntryCommonIndications CommonIndications { get; }
		public abstract ISerializableBotTask GetApproachTask();

		public override string ToString()
		{
			return $"SimpleOverviewEntry ({Name})";
		}
	}
}