namespace BotEngine.Interface
{
	public interface IMemoryReader
	{
		int ReadBytes(long address, int bytesCount, byte[] destinationArray);

		MemoryReaderModuleInfo[] Modules();
	}
}
