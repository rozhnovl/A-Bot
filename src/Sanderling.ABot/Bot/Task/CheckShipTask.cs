using System.Collections.Generic;
using System.Linq;
using BotEngine.Motor;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class CheckShipTask : IBotTask
	{
		private readonly IMemoryMeasurement MemoryMeasurement;
		private readonly string desiredShipName;

		public CheckShipTask(IMemoryMeasurement memoryMeasurement, string desiredShipName)
		{
			MemoryMeasurement = memoryMeasurement;
			this.desiredShipName = desiredShipName;
		}

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> ClientActions
		{
			get
			{
				if (MemoryMeasurement?.WindowShipFitting?.FirstOrDefault() == null)
					yield return MemoryMeasurement.Neocom.FittingButton.MouseClick(MouseButtonIdEnum.Left);

				var fittingWindow = MemoryMeasurement?.WindowShipFitting?.FirstOrDefault();
				if (fittingWindow != null)
					ShipNameFound = fittingWindow.LabelText.Any(lt => lt.Text == desiredShipName);

			}
		}
		public bool? ShipNameFound { get; private set; }
	}
}