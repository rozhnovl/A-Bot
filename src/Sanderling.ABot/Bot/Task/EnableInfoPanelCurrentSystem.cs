using System.Collections.Generic;
using Sanderling.Motor;
using BotEngine.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class EnableInfoPanelCurrentSystem : IBotTask
	{
		public IMemoryMeasurement MemoryMeasurement;

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionRecommendation> ClientActions
		{
			get
			{
				if (null != MemoryMeasurement?.InfoPanelContainer)
					yield break;

				yield return MemoryMeasurement?.InfoPanelButtonCurrentSystem?.MouseClick(MouseButtonIdEnum.Left).AsRecommendation();
			}
		}
	}
}
