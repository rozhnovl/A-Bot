using System.Collections;
using System.Collections.Generic;
using Bib3;

namespace BotEngine.InvocationProxy;

public class Map<T1, T2>
{
	public class Indexer<T3, T4> : IEnumerable<KeyValuePair<T3, T4>>, IEnumerable
	{
		private Dictionary<T3, T4> _dictionary;

		public T4 this[T3 index]
		{
			get
			{
				return _dictionary[index];
			}
			set
			{
				_dictionary[index] = value;
			}
		}

		public Indexer(Dictionary<T3, T4> dictionary)
		{
			_dictionary = dictionary;
		}

		public IEnumerator<KeyValuePair<T3, T4>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}
	}

	private readonly object @lock = new object();

	private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();

	private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

	public Indexer<T1, T2> Forward { get; private set; }

	public Indexer<T2, T1> Reverse { get; private set; }

	public Map()
	{
		Forward = new Indexer<T1, T2>(_forward);
		Reverse = new Indexer<T2, T1>(_reverse);
	}

	public void Add(T1 t1, T2 t2)
	{
		lock (@lock)
		{
			_forward.Add(t1, t2);
			_reverse.Add(t2, t1);
		}
	}

	public void RemoveForward(T1 t1)
	{
		lock (@lock)
		{
			if (_forward.TryGetValue(t1, out var value))
			{
				_forward.Remove(t1);
				_reverse.Remove(value);
			}
		}
	}

	public T2 TryGetForwardOrDefault(T1 key)
	{
		return _forward.TryGetValueOrDefault(key);
	}

	public T1 TryGetReverseOrDefault(T2 key)
	{
		return _reverse.TryGetValueOrDefault(key);
	}
}
