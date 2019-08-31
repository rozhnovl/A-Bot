using Sanderling.Motor;

namespace Sanderling.ABot.Bot
{
	public class MotionRecommendation
	{
		private static int motionId;
		public readonly int Id;

		public readonly MotionParam MotionParam;

		public readonly int? DelayAfterMs;

		public MotionRecommendation(MotionParam motionParam, int? delayAfterMs)
		{
			Id = motionId++;
			MotionParam = motionParam;
			DelayAfterMs = delayAfterMs;
		}
	}
}