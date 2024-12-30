using System;
using System.Collections.Generic;

namespace BotEngine;

public class Cache<TKey, TValue>
{
	private readonly IDictionary<TKey, TValue> Dict = new Dictionary<TKey, TValue>();

	private readonly Func<TKey, TValue> FuncMap;

	public Cache(Func<TKey, TValue> funcMap)
	{
		FuncMap = funcMap;
	}

	public TValue Retrieve(TKey key)
	{
		if (Dict.TryGetValue(key, out var value))
		{
			return value;
		}
		value = FuncMap(key);
		Dict[key] = value;
		return value;
	}
}
