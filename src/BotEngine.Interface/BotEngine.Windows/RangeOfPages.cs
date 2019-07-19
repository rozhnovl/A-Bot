namespace BotEngine.Windows
{
	public class RangeOfPages
	{
		public MEMORY_BASIC_INFORMATION BasicInfo;

		public byte[] MemoryFromBaseAddressListOctet;

		public RangeOfPages(MEMORY_BASIC_INFORMATION basicInfo, byte[] memoryFromBaseAddressListOctet = null)
		{
			BasicInfo = basicInfo;
			MemoryFromBaseAddressListOctet = memoryFromBaseAddressListOctet;
		}
	}
}
