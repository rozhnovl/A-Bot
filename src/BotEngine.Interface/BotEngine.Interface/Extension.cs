using Bib3;
using BotEngine.Client;
using System;

namespace BotEngine.Interface
{
	public static class Extension
	{
		public static int? HueDistanceMili(this ColorHSV o0, ColorHSV o1)
		{
			if (o0 == null || o1 == null)
			{
				return null;
			}
			return (o0.HMilli + 500 - o1.HMilli) % 1000 - 500;
		}

		public static bool Equals(this ColorORGB o0, ColorORGB o1)
		{
			if (o0 == o1)
			{
				return true;
			}
			if (o0 == null || o1 == null)
			{
				return false;
			}
			return o0.OMilli == o1.OMilli && o0.RMilli == o1.RMilli && o0.GMilli == o1.GMilli && o0.BMilli == o1.BMilli;
		}

		public static int? HueDistanceMili(this ColorORGB o0, ColorORGB o1)
		{
			if (o0 == null || o1 == null)
			{
				return null;
			}
			ColorHSV o2 = o0.AsHSV();
			ColorHSV o3 = o1.AsHSV();
			return o2.HueDistanceMili(o3);
		}

		public static ColorHSV AsHSV(this ColorORGB color)
		{
			int? rMilli = color.RMilli;
			int? gMilli = color.GMilli;
			int? bMilli = color.BMilli;
			if (!rMilli.HasValue || !gMilli.HasValue || !bMilli.HasValue)
			{
				return new ColorHSV();
			}
			Bib3.Extension.RGBKonvertiirtNaacHueSatVal(rMilli.Value, gMilli.Value, bMilli.Value, 1000, 1000, out int hue, out int sat, out int val);
			return new ColorHSV(hue, sat, val);
		}

		public static ColorORGB AsColorORGB(this ColorORGBVal? val)
		{
			return val.HasValue ? new ColorORGB(val.Value.OMilli, val.Value.RMilli, val.Value.GMilli, val.Value.BMilli) : null;
		}

		public static ColorORGB AsColorORGBIfAnyHasValue(this ColorORGBVal? val)
		{
			return ((val?.OMilli).HasValue || (val?.RMilli).HasValue || (val?.GMilli).HasValue || (val?.BMilli).HasValue) ? new ColorORGB(val.Value.OMilli, val.Value.RMilli, val.Value.GMilli, val.Value.BMilli) : null;
		}

		public static FromProcessMeasurement<OutT> MapValue<InT, OutT>(this FromProcessMeasurement<InT> fromProcessMeasurement, Func<InT, OutT> view)
		{
			if (fromProcessMeasurement == null)
			{
				return null;
			}
			return new FromProcessMeasurement<OutT>(view(fromProcessMeasurement.Value), fromProcessMeasurement.Begin, fromProcessMeasurement.End, fromProcessMeasurement.ProcessId, fromProcessMeasurement.FirstIntegrationTime);
		}

		public static byte[] ListeOktetLeeseVonAdrese(this IMemoryReader reader, long adrese, long listeOktetZuLeeseAnzaal, bool gibZurükNullWennGeleeseneAnzahlKlainer = false)
		{
			byte[] array = new byte[listeOktetZuLeeseAnzaal];
			long num = reader?.ListeOktetLeeseVonAdrese(adrese, listeOktetZuLeeseAnzaal, array) ?? 0;
			if (num < listeOktetZuLeeseAnzaal)
			{
				if (gibZurükNullWennGeleeseneAnzahlKlainer)
				{
					return null;
				}
				int num2 = (int)num;
				if (num2 != num)
				{
					throw new NotImplementedException("GeleeseListeOktetAnzaalInt32 != GeleeseListeOktetAnzaal");
				}
				byte[] array2 = new byte[num2];
				Buffer.BlockCopy(array, 0, array2, 0, num2);
				return array2;
			}
			return array;
		}

		public static long ListeOktetLeeseVonAdrese(this IMemoryReader reader, long adrese, long listeOktetZuLeeseAnzaal, byte[] ziilArray)
		{
			int num = reader?.ReadBytes(adrese, (int)listeOktetZuLeeseAnzaal, ziilArray) ?? 0;
			return num;
		}

		public static PropertyGenTimespanInt64<HttpExchangeReport<AuthRequest, AuthResponse>> ExchangeAuthLastStillValid(this LicenseClient client)
		{
			return (client == null || !client.AuthCompleted) ? null : client?.ExchangeAuthLast;
		}
	}
}
