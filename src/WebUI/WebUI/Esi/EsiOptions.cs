namespace WebUI.Esi;

public sealed class EsiOptions
{
    public const string SectionName = "Esi";

    // Set these from server-side secrets, never from a browser or checked-in settings.
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public long CharacterId { get; set; }
    public string RefreshToken { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string OperatorEmail { get; set; } = "";
    public string UserAgent { get; set; } = "A-Bot-ESI/0.1";
    public string CompatibilityDate { get; set; } = "2026-09-13";
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxPagesPerResource { get; set; } = 100;
    public int MaxWalletPages { get; set; } = 10;

    public bool HasCredentials =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
