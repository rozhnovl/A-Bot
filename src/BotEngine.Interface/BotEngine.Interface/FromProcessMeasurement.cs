using Bib3;

namespace BotEngine.Interface
{
	public class FromProcessMeasurement<T> : PropertyGenTimespanInt64<T>
	{
		public int ProcessId;

		public long? FirstIntegrationTime;

		public FromProcessMeasurement()
		{
		}

		public FromProcessMeasurement(T measurement, long beginTime, long endTime, int processId = 0, long? firstIntegrationTime = default(long?))
			: base(measurement, beginTime, endTime)
		{
			ProcessId = processId;
			FirstIntegrationTime = firstIntegrationTime;
		}
	}
}
