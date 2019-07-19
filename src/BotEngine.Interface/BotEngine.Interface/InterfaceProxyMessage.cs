namespace BotEngine.Interface
{
	public class InterfaceProxyMessage
	{
		public byte[][] Server;

		public byte[][] Consumer;

		public InterfaceProxyMessage()
		{
		}

		public InterfaceProxyMessage(byte[][] server, byte[][] consumer = null)
		{
			Server = server;
			Consumer = consumer;
		}

		public static InterfaceProxyMessage ServerMessage(byte[][] listeMessage)
		{
			return new InterfaceProxyMessage(listeMessage);
		}

		public static InterfaceProxyMessage ConsumerMessage(byte[][] listeMessage)
		{
			return new InterfaceProxyMessage(null, listeMessage);
		}

		public static InterfaceProxyMessage ServerMessage(byte[] message)
		{
			return ServerMessage(new byte[1][]
			{
				message
			});
		}

		public static InterfaceProxyMessage ConsumerMessage(byte[] message)
		{
			return ConsumerMessage(new byte[1][]
			{
				message
			});
		}
	}
}
