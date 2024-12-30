using System;
using Bib3;

namespace BotEngine.Common;

public class TimesourceConfig : ITimesourceConfig
{
	public static ITimesourceConfig StaticConfig = new TimesourceConfig
	{
		TimeContinuousMilliFunc = Glob.StopwatchZaitMiliSictInt,
		TimeCalDateTimeFunc = () => DateTime.Now
	};

	public Func<long> TimeContinuousMilliFunc;

	public Func<DateTime> TimeCalDateTimeFunc;

	public DateTime TimeCalDateTime => TimeCalDateTimeFunc?.Invoke() ?? default(DateTime);

	public long TimeContinuousMilli => TimeContinuousMilliFunc?.Invoke() ?? 0;
}
