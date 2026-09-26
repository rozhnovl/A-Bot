using System;

namespace Sanderling.ABot.Bot
{
	public class BotStepResult
	{
		public Exception Exception;

		public MotionRecommendation[] ListMotion;

		public IBotTask[][] OutputListTaskPath;

		/// <summary>Structured summary consumed by every runner/dashboard.</summary>
		public StrategyStatus StrategyStatus;
	}
}
