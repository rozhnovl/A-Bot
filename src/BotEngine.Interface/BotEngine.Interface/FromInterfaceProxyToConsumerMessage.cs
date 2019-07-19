namespace BotEngine.Interface
{
	public class FromInterfaceProxyToConsumerMessage
	{
		public byte[] AppSpecific;

		public FromInterfaceProxyToConsumerMessage()
		{
		}

		public FromInterfaceProxyToConsumerMessage(byte[] appSpecific)
		{
			AppSpecific = appSpecific;
		}
	}
}
