using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanderling.ABot.Bot;
using Sanderling.Motor;

namespace Sanderling.ABot
{
	public static class MotionExtension
	{
		public static MotionRecommendation AsRecommendation(this MotionParam mp, int? delayAfterMs = null)
		{
			if (mp == null)
				return null;
			return new MotionRecommendation(mp, delayAfterMs);
		}
	}
}
