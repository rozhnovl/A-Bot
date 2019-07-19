using Bib3;
using Bib3.RefNezDiferenz;
using System;
using System.Linq;
using System.Reflection;

namespace BotEngine.Interface
{
	public class InterfaceAppDomainSetup
	{
		private static object Lock;

		private static bool SetupComplete;

		private static bool ProtobufSetupComplete;

		static InterfaceAppDomainSetup()
		{
			Lock = new object();
			SetupComplete = false;
			ProtobufSetupComplete = false;
			Setup();
		}

		public static void Setup()
		{
			lock (Lock)
			{
				if (!SetupComplete)
				{
					SetupComplete = true;
					SyncedSetup();
				}
			}
		}

		private static void SyncedSetup()
		{
			SetupProtobuf();
		}

		public static void SetupProtobuf()
		{
			lock (Lock)
			{
				if (!ProtobufSetupComplete)
				{
					SyncedSetupProtobuf();
				}
			}
		}

		private static void SyncedSetupProtobuf()
		{
			Protobuf.ProtobufSetup();
			Type[] source = new Type[1]
			{
				typeof(SictZuNezSictDiferenzScritAbbild)
			};
			Type[] array = new Type[0];
			(from type in source
			select type.Assembly)?.ForEach(delegate(Assembly assembly)
			{
				ProtobufRuntimeTypeModelSetup.SetupForAssembly(assembly);
			});
			array?.ForEach(delegate(Type type)
			{
				ProtobufRuntimeTypeModelSetup.SetupForType(type);
			});
		}
	}
}
