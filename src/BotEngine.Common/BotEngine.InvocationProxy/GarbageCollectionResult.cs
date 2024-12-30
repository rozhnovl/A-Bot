using BotEngine.InvocationProxy.SerialStruct;

namespace BotEngine.InvocationProxy;

public class GarbageCollectionResult : BotEngine.InvocationProxy.SerialStruct.GarbageCollectionResult
{
	public GarbageCollectionParam ToHostParam;

	public BotEngine.InvocationProxy.SerialStruct.GarbageCollectionResult FromHostResult;
}
