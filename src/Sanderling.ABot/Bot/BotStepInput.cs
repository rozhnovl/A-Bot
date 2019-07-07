using System;
using System.Collections.Generic;

namespace Sanderling.ABot.Bot
{
	public class BotStepInput
	{
		public Int64 TimeMilli;

		public BotEngine.Interface.FromProcessMeasurement<Interface.MemoryStruct.IMemoryMeasurement> FromProcessMemoryMeasurement;

		public StringAtPath ConfigSerial;

		public MotionResult[] StepLastMotionResult;
	}
}
