using System;

namespace BotEngine.Common;

public interface ITimesourceConfig
{
	long TimeContinuousMilli { get; }

	DateTime TimeCalDateTime { get; }
}
