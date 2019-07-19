using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BotEngine.Interface
{
	public class SictOrdnetNaacSclüselInt<TElement> : ILookup<long, TElement>, IEnumerable<IGrouping<long, TElement>>, IEnumerable
	{
		private readonly ILookup<long, TElement> Liste;

		public IEnumerable<TElement> this[long key]
		{
			get
			{
				ILookup<long, TElement> liste = Liste;
				return (liste != null) ? liste[key] : null;
			}
		}

		public int Count => Liste?.Count() ?? 0;

		public SictOrdnetNaacSclüselInt(IEnumerable<TElement> menge, Func<TElement, long> sictSclüsel)
		{
			if (menge != null)
			{
				if (sictSclüsel == null)
				{
					sictSclüsel = ((TElement t) => 0L);
				}
				Liste = menge.OrderBy(sictSclüsel).ToLookup(sictSclüsel);
			}
		}

		public bool Contains(long key)
		{
			return Liste?.Contains(key) ?? false;
		}

		public IEnumerator<IGrouping<long, TElement>> GetEnumerator()
		{
			return Liste?.GetEnumerator();
		}

		public IEnumerator<TElement> GetEnumeratorFlat()
		{
			if (Liste != null)
			{
				foreach (IGrouping<long, TElement> grupe in Liste)
				{
					foreach (TElement item in grupe)
					{
						yield return item;
					}
				}
			}
		}

		public IEnumerable<TElement> Flat()
		{
			return Liste?.Aggregate(new TElement[0], (IEnumerable<TElement> a, IGrouping<long, TElement> b) => a.Concat(b));
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IGrouping<long, TElement> GrupeFürSclüsel(long sclüsel)
		{
			KeyValuePair<long, IGrouping<long, TElement>> keyValuePair = IndexUndGrupeFürKlainerGlaicSclüsel(sclüsel);
			if (keyValuePair.Key == sclüsel)
			{
				return keyValuePair.Value;
			}
			return null;
		}

		public long IndexKlainerGlaicSclüsel(long sclüsel)
		{
			return IndexUndGrupeFürKlainerGlaicSclüsel(sclüsel).Key;
		}

		public KeyValuePair<long, IGrouping<long, TElement>> IndexUndGrupeFürKlainerGlaicSclüsel(long sclüsel)
		{
			if (Liste != null)
			{
				if (sclüsel < 0)
				{
					throw new NotImplementedException("Sclüsel < 0");
				}
				int num = Liste.Count / 2;
				int num2 = num;
				while (0 < num)
				{
					IGrouping<long, TElement> grouping = Liste.ElementAt(num2);
					if (grouping.Key != sclüsel)
					{
						if (grouping.Key >= sclüsel)
						{
							goto IL_00b6;
						}
						if (num2 < Liste.Count - 1)
						{
							IGrouping<long, TElement> grouping2 = Liste.ElementAt(num2 + 1);
							if (grouping2.Key < sclüsel)
							{
								num2 += num;
								goto IL_00b6;
							}
						}
					}
					return new KeyValuePair<long, IGrouping<long, TElement>>(num2, grouping);
					IL_00b6:
					if (sclüsel < grouping.Key)
					{
						num2 -= num;
					}
					num = (num + 1) / 2;
					num2 = Math.Max(0, Math.Min(Liste.Count - 1, num2));
				}
			}
			return new KeyValuePair<long, IGrouping<long, TElement>>(-1L, null);
		}
	}
}
