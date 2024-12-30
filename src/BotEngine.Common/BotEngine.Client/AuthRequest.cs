using System;
using System.Linq;

namespace BotEngine.Client;

public class AuthRequest
{
	public string ServiceId;

	public string ServiceInterfaceId;

	public string LicenseKey;

	public bool Consume;

	public string SessionId;

	public byte[] ProofOfWork;

	public string ReffererId;

	public const int ProofOfWorkAmountMin = 10000;

	public static byte[] ProofOfWorkConstruct(int amount)
	{
		return Enumerable.Range(0, amount).Select((Func<int, byte>)((int _) => 0)).ToArray();
	}

	public static byte[] ProofOfWorkConstruct()
	{
		return ProofOfWorkConstruct(10000);
	}
}
