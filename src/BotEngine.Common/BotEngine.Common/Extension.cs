using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bib3;
using Bib3.RefNezDiferenz;
using ProtoBuf;

namespace BotEngine.Common;

public static class Extension
{
	public static bool EqualsIgnoreCase(this string string0, string string1)
	{
		return string.Equals(string0, string1, StringComparison.OrdinalIgnoreCase);
	}

	public static string ToBase64(this byte[] listeOktet)
	{
		if (listeOktet == null)
		{
			return null;
		}
		return Convert.ToBase64String(listeOktet);
	}

	public static byte[] FromBase64(this string base64)
	{
		if (base64 == null)
		{
			return null;
		}
		return Convert.FromBase64String(base64);
	}

	public static byte[] ProtobufSerialize(this object obj, int blokListeOktetAnzaal = 65536)
	{
		if (obj == null)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream();
		Serializer.Serialize<object>((Stream)memoryStream, obj);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return memoryStream.LeeseGesamt(blokListeOktetAnzaal);
	}

	public static object ProtobufDeserialize(this byte[] listeOktet, Type type)
	{
		if (listeOktet == null)
		{
			return null;
		}
		return Serializer.NonGeneric.Deserialize(type, (Stream)new MemoryStream(listeOktet));
	}

	public static T ProtobufDeserialize<T>(this byte[] listeOktet)
	{
		if (listeOktet == null)
		{
			return default(T);
		}
		return Serializer.Deserialize<T>((Stream)new MemoryStream(listeOktet));
	}

	public static byte[] SerializeSingleBib3RefNezDifProtobuf(this object obj, SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null, int blokListeOktetAnzaal = 65536)
	{
		return obj.WurzelSerialisiire(rictliinieMitScatescpaicer).ProtobufSerialize(blokListeOktetAnzaal);
	}

	public static object[] DeSerializeProtobufBib3RefNezDif(this byte[] seriel, SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer rictliinieMitScatescpaicer = null)
	{
		return seriel.ProtobufDeserialize<SictZuNezSictDiferenzScritAbbild>().ListeWurzelDeserialisiire(rictliinieMitScatescpaicer);
	}

	public static bool IdEquals(this IObjectIdInt64 obj0, long id)
	{
		if (obj0 == null)
		{
			return false;
		}
		return id == obj0.Id;
	}

	public static bool Identical(this IObjectIdInt64 obj0, IObjectIdInt64 obj1)
	{
		if (obj0 == null && obj1 == null)
		{
			return true;
		}
		if (obj0 == null)
		{
			return false;
		}
		if (obj1 == null)
		{
			return false;
		}
		return obj0.Id == obj1.Id;
	}

	public static bool? Konjunkt(this IEnumerable<bool?> setBoolNulable)
	{
		if (setBoolNulable == null)
		{
			return null;
		}
		bool? result = null;
		foreach (bool? item in setBoolNulable)
		{
			if (item.HasValue)
			{
				result = ((!result.HasValue) ? item : new bool?(result.Value && item.Value));
			}
		}
		return result;
	}

	public static KeyValuePair<T, string>[] ListeEnumWertUndSictStringAbbildBerecne<T>(Func<T, string> sictString) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("!typeof(T).IsEnum");
		}
		List<KeyValuePair<T, string>> list = new List<KeyValuePair<T, string>>();
		string[] names = Enum.GetNames(typeof(T));
		foreach (string value in names)
		{
			T val = (T)Enum.Parse(typeof(T), value);
			string value2 = null;
			if (sictString != null)
			{
				value2 = sictString(val);
			}
			list.Add(new KeyValuePair<T, string>(val, value2));
		}
		return list.ToArray();
	}

	public static IEnumerable<T> TrimHeadToKeepEnumerator<T>(this Queue<T> toTrimQueue, int toKeepCount)
	{
		return toTrimQueue?.DequeueEnumerator(() => toKeepCount < toTrimQueue.Count);
	}

	public static IEnumerable<T> TrimHeadToKeep<T>(this Queue<T> toTrimQueue, int toKeepCount)
	{
		return toTrimQueue.TrimHeadToKeepEnumerator(toKeepCount).ToArray();
	}

	public static string StringJoin(this IEnumerable<object> seq, string separator)
	{
		if (seq == null)
		{
			return null;
		}
		return string.Join(separator, seq);
	}

	public static T GetInstanceWithIdFromCLRGraph<T>(this long id, object graphRoot, SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer policyCache) where T : class, IObjectIdInt64
	{
		IEnumerable<object> enumerable = graphRoot.EnumMengeRefAusNezAusWurzel(policyCache);
		object result;
		if (enumerable == null)
		{
			result = null;
		}
		else
		{
			IEnumerable<T> enumerable2 = enumerable.OfType<T>();
			result = ((enumerable2 != null) ? enumerable2.FirstOrDefault((T graphNode) => graphNode.Id == id) : null);
		}
		return (T)result;
	}

	public static T GetInstanceWithIdFromCLRGraph<T>(this T objectIdInMemory, object graphRoot, SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer policyCache) where T : class, IObjectIdInt64
	{
		return (objectIdInMemory == null) ? null : objectIdInMemory.Id.GetInstanceWithIdFromCLRGraph<T>(graphRoot, policyCache);
	}
}
