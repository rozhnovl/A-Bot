using BotEngine.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace BotEngine.Interface
{
	public class InterfaceAppDomainProxy : IDisposable
	{
		private readonly Type AppDomainSetupType;

		private readonly AppDomain InterfaceAppDomain;

		private readonly AppDomainProxyByte Proxy;

		public long? InterfaceAppDomainMonitoringSurvivedMemorySize => InterfaceAppDomain?.MonitoringSurvivedMemorySize;

		public TimeSpan? InterfaceAppDomainMonitoringTotalProcessorTime => InterfaceAppDomain?.MonitoringTotalProcessorTime;

		public static bool IsRunningInHostingProcess => AppDomain.CurrentDomain?.FriendlyName?.EndsWith("vshost.exe") ?? false;

		public void Dispose()
		{
			AppDomain.Unload(InterfaceAppDomain);
		}

		public static void ConfigureAppDomain(string[] args)
		{
			string assemblyFile = args?.FirstOrDefault();
			Assembly assembly = Assembly.LoadFrom(assemblyFile);
		}

		public InterfaceAppDomainProxy(Type appDomainSetupType, bool appDomainSetupTypeLoadFromMainModule = false)
		{
			AppDomain.MonitoringIsEnabled = true;
			AppDomainSetupType = appDomainSetupType;
			PermissionSet grantSet = new PermissionSet(PermissionState.Unrestricted);
			AppDomain appDomain = AppDomain.CreateDomain("Interface", null, new AppDomainSetup
			{
				ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
			}, grantSet);
			string assemblyFile = System.Diagnostics.Process.GetCurrentProcess()?.MainModule?.FileName;
			if (null != appDomainSetupType)
			{
				if (appDomainSetupTypeLoadFromMainModule && !IsRunningInHostingProcess)
				{
					appDomain.CreateInstanceFrom(assemblyFile, appDomainSetupType.FullName);
				}
				else
				{
					appDomain.CreateInstance(appDomainSetupType.Assembly.FullName, appDomainSetupType.FullName);
				}
			}
			Type typeFromHandle = typeof(AppDomainProxyByte);
			InterfaceAppDomainSetup.Setup();
			Proxy = (AppDomainProxyByte)appDomain.CreateInstanceAndUnwrap(typeFromHandle.Assembly.FullName, typeFromHandle.FullName);
			InterfaceAppDomain = appDomain;
		}

		private byte[] ToServerOutputListeOktet()
		{
			return Proxy.NaacServerAusgang();
		}

		public InterfaceProxyMessage ToServerOutput()
		{
			return ToServerOutputListeOktet().ProtobufDeserialize<InterfaceProxyMessage>();
		}

		private void FromServerInput(byte[] input)
		{
			Proxy.VonServerAingang(input);
		}

		public void FromServerInput(InterfaceProxyMessage message)
		{
			FromServerInput(message.ProtobufSerialize());
		}

		public InterfaceProxyMessage ConsumerExchange(InterfaceProxyMessage message)
		{
			return Proxy.AustauscKonsument(message.ProtobufSerialize()).ProtobufDeserialize<InterfaceProxyMessage>();
		}

		public object AppImplementationOfType(Type typeToAssignTo)
		{
			return Proxy?.AppImplementationOfType(typeToAssignTo);
		}

		public string ClientRequest(string request)
		{
			return Proxy?.ClientRequest(request);
		}
	}
}
