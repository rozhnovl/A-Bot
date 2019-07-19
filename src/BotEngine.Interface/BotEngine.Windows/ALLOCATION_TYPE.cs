namespace BotEngine.Windows
{
	public enum ALLOCATION_TYPE : uint
	{
		MEM_COMMIT = 0x1000,
		MEM_RESERVE = 0x2000,
		MEM_FREE = 0x10000,
		MEM_RESET = 0x80000
	}
}
