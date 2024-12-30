using Bib3;
using Bib3.RefNezDiferenz;
using BotEngine;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanderling.Interface
{
	public class FromInterfaceResponse
	{
		public string ErrorText;

		public bool MemoryMeasurementInProgress;

		public FromProcessMeasurement<MemoryMeasurementInitParam> MemoryMeasurementInit;

		public FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurement;

		public FromProcessMeasurement<WindowMeasurement> WindowMeasurement;

		private static SictMengeTypeBehandlungRictliinie SerialisPolicy = SerialisPolicyConstruct();

		public static readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer SerialisPolicyCache = new SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(SerialisPolicy);

		public static readonly SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer UITreeComponentTypeHandlePolicyCache = SerialisPolicyCache;

		public static SictMengeTypeBehandlungRictliinie SerialisPolicyConstruct()
		{
			return SictRefNezKopii.SctandardRictlinieMitScatescpaicer.Rictliinie;
		}

		public static string SerializeToString<T>(T snapshot)
		{
			return 
				JsonSerializer.Serialize(snapshot.WurzelSerialisiire(SerialisPolicyCache), new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } });
		}

		public static byte[] SerializeToUTF8<T>(T snapshot)
		{
			return Encoding.UTF8.GetBytes(SerializeToString(snapshot));
		}

		public static T DeserializeFromString<T>(string json)
		{
			return (T)(JsonSerializer.Deserialize<SictZuNezSictDiferenzScritAbbild>(json).ListeWurzelDeserialisiire(SerialisPolicyCache)?.FirstOrDefault());
		}

		public static T DeserializeFromUTF8<T>(byte[] utf8)
		{
			if (utf8 == null)
			{
				return default(T);
			}
			return DeserializeFromString<T>(Encoding.UTF8.GetString(utf8));
		}
	}
}
