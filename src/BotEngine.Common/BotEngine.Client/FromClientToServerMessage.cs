namespace BotEngine.Client;

public class FromClientToServerMessage : IRequestInSession
{
	public const int PayloadProofOfWorkAmountMin = 4000;

	public byte[] ProofOfWork;

	public string SessionId;

	public long? Time;

	public FromInterfaceAppManagerToServerMessage Interface;

	public string Consumer;

	public string SessionIdAsString()
	{
		return SessionId;
	}
}
