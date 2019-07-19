using Bib3;
using BotEngine.Client;
using BotEngine.Common;
using System;
using System.Linq;

namespace BotEngine.Interface
{
	public class InterfaceAppManager
	{
		private readonly Type AppDomainSetupType;

		private readonly bool AppDomainSetupTypeLoadFromMainModule;

		private WertZuZaitpunktStruct<InterfaceAppDomainProxy> AppDomain;

		public long? InterfaceAppDomainMonitoringSurvivedMemorySize => AppDomain.Wert?.InterfaceAppDomainMonitoringSurvivedMemorySize;

		public TimeSpan? InterfaceAppDomainMonitoringTotalProcessorTime => AppDomain.Wert?.InterfaceAppDomainMonitoringTotalProcessorTime;

		public InterfaceAppManager()
			: this(null)
		{
		}

		public InterfaceAppManager(Type appDomainSetupType, bool appDomainSetupTypeLoadFromMainModule = false)
		{
			AppDomainSetupType = (appDomainSetupType ?? typeof(InterfaceAppDomainSetup));
			AppDomainSetupTypeLoadFromMainModule = appDomainSetupTypeLoadFromMainModule;
		}

		public void AppDomainConstruct()
		{
			AppDomain.Wert?.Dispose();
			AppDomain = new WertZuZaitpunktStruct<InterfaceAppDomainProxy>(new InterfaceAppDomainProxy(AppDomainSetupType, AppDomainSetupTypeLoadFromMainModule), Glob.StopwatchZaitMiliSictInt());
		}

		public void FromServer(FromServerToInterfaceAppManagerMessage message)
		{
			if (message != null)
			{
				long? num = message?.AppDomainConstructionTimeMin;
				if (num.HasValue && AppDomain.Zait < num)
				{
					AppDomainConstruct();
				}
				message?.ToInterfaceApp?.ForEach(delegate(byte[] toInterfaceApp)
				{
					AppDomain.Wert?.FromServerInput(InterfaceProxyMessage.ServerMessage(toInterfaceApp));
				});
			}
		}

		public FromInterfaceAppManagerToServerMessage ToServer()
		{
			return new FromInterfaceAppManagerToServerMessage(AppDomain.Zait, AppDomain.Wert?.ToServerOutput()?.Server);
		}

		public FromInterfaceProxyToConsumerMessage ConsumerExchange(FromConsumerToInterfaceProxyMessage message)
		{
			return AppDomain.Wert?.ConsumerExchange(InterfaceProxyMessage.ConsumerMessage(message.ProtobufSerialize()))?.Consumer?.LastOrDefault().ProtobufDeserialize<FromInterfaceProxyToConsumerMessage>();
		}

		public object AppImplementationOfType(Type typeToAssignTo)
		{
			return AppDomain.Wert?.AppImplementationOfType(typeToAssignTo);
		}

		public T AppImplementationOfType<T>()
		{
			return (T)AppImplementationOfType(typeof(T));
		}

		public string ClientRequest(string request)
		{
			return AppDomain.Wert?.ClientRequest(request);
		}
	}
}
