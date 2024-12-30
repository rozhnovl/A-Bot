using System.Text;
using Newtonsoft.Json;

namespace BotEngine;

public static class NewtonsoftJson
{
	private static JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		DefaultValueHandling = (DefaultValueHandling)3
	};

	public static T DeserializeFromString<T>(this string @string)
	{
		if (@string == null)
		{
			return default(T);
		}
		return JsonConvert.DeserializeObject<T>(@string, Settings);
	}

	public static string SerializeToString(this object obj, Formatting formatting = 0)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (obj == null)
		{
			return null;
		}
		return JsonConvert.SerializeObject(obj, formatting, Settings);
	}

	public static T DeserializeFromUtf8<T>(this byte[] listeOktet)
	{
		if (listeOktet == null)
		{
			return default(T);
		}
		return Encoding.UTF8.GetString(listeOktet).DeserializeFromString<T>();
	}

	public static byte[] SerializeToUtf8(this object obj)
	{
		string text = obj.SerializeToString((Formatting)0);
		if (text == null)
		{
			return null;
		}
		return Encoding.UTF8.GetBytes(text);
	}
}
