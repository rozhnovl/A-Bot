namespace BotEngine.Interface
{
	public interface IAppDomainProxyByte
	{
		byte[] AustauscKonsument(byte[] aingang);

		void VonServerAingang(byte[] aingang);

		byte[] NaacServerAusgang();
	}
}
