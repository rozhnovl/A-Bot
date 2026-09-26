using BotEngine.Client;
using Timer = System.Timers.Timer;

namespace BotEngine.Interface
{
	public class SimpleInterfaceServerDispatcher
	{
		private readonly object @lock = new object();

		private readonly Timer exchangeTimer = new Timer();

		public LicenseClientConfig LicenseClientConfig;

		public LicenseClient LicenseClient;

		public Type InterfaceAppDomainSetupType;

		public bool InterfaceAppDomainSetupTypeLoadFromMainModule;

		public int ExchangeAuthTimeDistanceMinMilli = 4000;

		public void CyclicExchangeStart()
		{
			exchangeTimer.Start();
			//Exchange();
		}

	}
}
