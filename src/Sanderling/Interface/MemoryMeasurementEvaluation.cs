using BotEngine.Interface;
using Sanderling.Parse;
using System;

namespace Sanderling.Interface
{
	public class MemoryMeasurementEvaluation
	{
		public MemoryStruct.IMemoryMeasurement MemoryMeasurement { private set; get; }

		public Parse.IMemoryMeasurement MemoryMeasurementParsed { private set; get; }

		public Exception MemoryMeasurementParseException { private set; get; }

		public Accumulation.IMemoryMeasurement MemoryMeasurementAccumulation { private set; get; }

		public Exception MemoryMeasurementAccuException { private set; get; }

		public MemoryMeasurementEvaluation()
		{
		}

		public MemoryMeasurementEvaluation(
			FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> memoryMeasurement)
		{
			this.MemoryMeasurement = memoryMeasurement?.Value;

			try
			{
				MemoryMeasurementParsed = memoryMeasurement?.Value?.Parse();
			}
			catch (Exception Exception)
			{
				MemoryMeasurementParseException = Exception;
			}

			if (null == memoryMeasurement)
			{
				return;
			}
		}
	}

}
