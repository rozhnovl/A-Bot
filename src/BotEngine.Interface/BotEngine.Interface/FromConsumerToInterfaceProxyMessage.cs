namespace BotEngine.Interface
{
	public class FromConsumerToInterfaceProxyMessage
	{
		public byte[] AppSpecific;

		public FromConsumerToInterfaceProxyMessage()
		{
		}

		public FromConsumerToInterfaceProxyMessage(byte[] appSpecific)
		{
			AppSpecific = appSpecific;
		}
	}
}
