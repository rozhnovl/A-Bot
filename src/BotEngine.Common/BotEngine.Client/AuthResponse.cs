namespace BotEngine.Client;

public record AuthResponse
{
	public DateTime? NowTimeCal { get; init; }

	public bool LicenseKeyValid { get; init; }

	public bool LicenseTimeframeMet { get; init; }

	public DateTime? LicenseReplenishTimeCal { get; init; }

	public bool LicenseReplenishIntervalExhaustedNot { get; init; }

	public bool LicenseSessionConcurrencyLimitExhaustedNot { get; init; }

	public bool ServiceSelectValid { get; init; }

	public bool ServiceTimeframeMet { get; init; }

	public DateTime? LicenseConsumeStartTimeCal { get; init; }

	public DateTime? LicenseStartTimeCal { get; init; }

	public DateTime? LicenseEndTimeCal { get; init; }

	public DateTime? ServiceStartTimeCal { get; init; }

	public DateTime? ServiceEndTimeCal { get; init; }

	public string? ServiceId { get; init; }

	public string? SessionId { get; init; }

	public DateTime? SessionEndTimeCal { get; init; }

	public int RequestTimeDistanceMaxMilli { get; init; }
}
