using Bib3;
using BotEngine.Client;
using System;

namespace BotEngine.Interface
{
	public static class Extension
	{

		public static FromProcessMeasurement<OutT> MapValue<InT, OutT>(this FromProcessMeasurement<InT> fromProcessMeasurement, Func<InT, OutT> view)
		{
			if (fromProcessMeasurement == null)
			{
				return null;
			}
			return new FromProcessMeasurement<OutT>(view(fromProcessMeasurement.Value), fromProcessMeasurement.Begin, fromProcessMeasurement.End, fromProcessMeasurement.ProcessId, fromProcessMeasurement.FirstIntegrationTime);
		}

	}
}
