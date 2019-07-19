namespace BotEngine.Interface
{
	public class MemoryReaderModuleInfo
	{
		public readonly string ModuleName;

		public readonly long BaseAddress;

		public MemoryReaderModuleInfo(string moduleName, long baseAddress)
		{
			ModuleName = moduleName;
			BaseAddress = baseAddress;
		}
	}
}
