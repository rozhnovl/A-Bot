namespace BotEngine.Client;

public class AuthRequest
{
	public string? ServiceId { get; init; }

	public string? ServiceInterfaceId { get; init; }

	public string? LicenseKey { get; init; }

	public bool Consume { get; init; }

	public string? SessionId { get; init; }

	public byte[]? ProofOfWork { get; init; }

	public string? ReffererId { get; init; }

	public const int ProofOfWorkAmountMin = 10000;

	public static byte[] ProofOfWorkConstruct(int amount) =>
		Enumerable.Range(0, amount).Select(_ => (byte)0).ToArray();

	public static byte[] ProofOfWorkConstruct() =>
		ProofOfWorkConstruct(ProofOfWorkAmountMin);
}
