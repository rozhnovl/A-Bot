using System;

namespace BotEngine.Client;

public class AuthResponse
{
	public DateTime? NowTimeCal;

	public bool LicenseKeyValid;

	public bool LicenseTimeframeMet;

	public DateTime? LicenseReplenishTimeCal;

	public bool LicenseReplenishIntervalExhaustedNot;

	public bool LicenseSessionConcurrencyLimitExhaustedNot;

	public bool ServiceSelectValid;

	public bool ServiceTimeframeMet;

	public DateTime? LicenseConsumeStartTimeCal;

	public DateTime? LicenseStartTimeCal;

	public DateTime? LicenseEndTimeCal;

	public DateTime? ServiceStartTimeCal;

	public DateTime? ServiceEndTimeCal;

	public string ServiceId;

	public string SessionId;

	public DateTime? SessionEndTimeCal;

	public int RequestTimeDistanceMaxMilli;
}
