namespace BotEngine.InvocationProxy.SerialStruct;

public class Invocation
{
	public long objectId;

	public MethodId method;

	public RemoteObject[] listArgument;
}
