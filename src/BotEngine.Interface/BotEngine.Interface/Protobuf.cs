using Bib3;
using BotEngine.Client;
using System;
using System.Collections.Generic;

namespace BotEngine.Interface
{
	public static class Protobuf
	{
		private static IEnumerable<Type> MengeType()
		{
			return new Type[7]
			{
				typeof(FromClientToServerMessage),
				typeof(FromServerToClientMessage),
				typeof(FromInterfaceAppManagerToServerMessage),
				typeof(FromServerToInterfaceAppManagerMessage),
				typeof(FromInterfaceProxyToConsumerMessage),
				typeof(FromConsumerToInterfaceProxyMessage),
				typeof(InterfaceProxyMessage)
			};
		}

		public static void ProtobufSetup()
		{
			foreach (Type item in MengeType().EmptyIfNull())
			{
				ProtobufRuntimeTypeModelSetup.SetupForType(item);
			}
		}
	}
}
