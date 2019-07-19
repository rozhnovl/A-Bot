namespace BotEngine.Interface
{
	public interface IInterfaceKomponente
	{
		void VonServerZuusctandGeändert(VonServerNaacInterfaceZuusctand vonServerZuusctand);

		object NaacServerZuusctand();

		void VonKonsumentAingang(FromConsumerToInterfaceProxyMessage vonKonsument);

		FromInterfaceProxyToConsumerMessage NaacKonsumentAusgang();

		string ClientRequest(string request);
	}
}
