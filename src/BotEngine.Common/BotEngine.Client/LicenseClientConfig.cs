namespace BotEngine.Client;

public record LicenseClientConfig
{
	public string? ApiVersionAddress { get; init; }

	public string? ApiOverviewAddress { get; init; }

	public string? VersionId { get; init; }

	public AuthRequest? Request { get; init; }
}
