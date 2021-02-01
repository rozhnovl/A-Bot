namespace Sanderling.ABot.Bot
{
	public interface IInventoryProvider
	{
		bool IsEmpty { get; }
		ISerializableBotTask GetClickLootButtonTask();
		ISerializableBotTask GetOpenWindowTask();
		ISerializableBotTask GetActvateItemIfPresentTask(string ragingExoticFilament, string use);
		ISerializableBotTask GetCloseWindowTask();
		IInventoryProvider GetLootableWindow();
	}
}