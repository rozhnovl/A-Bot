namespace Sanderling.ABot.Bot
{
	public interface IInventoryProvider
	{
		bool IsEmpty { get; }
		ISerializableBotTask GetClickLootButtonTask();
	}
}