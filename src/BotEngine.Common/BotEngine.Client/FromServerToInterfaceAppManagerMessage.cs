namespace BotEngine.Client;

public class FromServerToInterfaceAppManagerMessage
{
	public long? AppDomainConstructionTimeMin;

	public byte[][] ToInterfaceApp;

	public FromServerToInterfaceAppManagerMessage()
	{
	}

	public FromServerToInterfaceAppManagerMessage(long? appDomainConstructionTimeMin, byte[][] toInterfaceApp)
	{
		AppDomainConstructionTimeMin = appDomainConstructionTimeMin;
		ToInterfaceApp = toInterfaceApp;
	}
}
