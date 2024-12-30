namespace BotEngine.InvocationProxy;

public interface IHost
{
	string Invoke(string invocationSerial);

	string GetAppObject();

	string GarbageCollect(string collectionSerial);
}
