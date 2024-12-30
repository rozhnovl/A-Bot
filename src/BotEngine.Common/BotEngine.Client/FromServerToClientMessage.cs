namespace BotEngine.Client;

public class FromServerToClientMessage
{
	public FromServerToInterfaceAppManagerMessage Interface;

	public string Consumer;

	public FromServerToClientMessage()
	{
	}

	public FromServerToClientMessage(FromServerToInterfaceAppManagerMessage @interface, string consumer)
	{
		Interface = @interface;
		Consumer = consumer;
	}
}
