namespace Sanderling.ABot.Bot;

public record Config
{
	public string? RetreatBookmark { get; init; }

	public string[]? ModuleActivePermanentSetTitlePattern { get; init; }
}
