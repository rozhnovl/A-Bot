namespace BotEngine.Client;

public class FromInterfaceAppManagerToServerMessage
{
	public long? AppDomainConstructionTime;

	public byte[][] FromInterfaceApp;

	public FromInterfaceAppManagerToServerMessage()
	{
	}

	public FromInterfaceAppManagerToServerMessage(long? appDomainConstructionTime, byte[][] fromInterfaceApp)
	{
		AppDomainConstructionTime = appDomainConstructionTime;
		FromInterfaceApp = fromInterfaceApp;
	}
}
