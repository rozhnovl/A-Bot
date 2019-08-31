using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Sanderling.ABot.Bot.Task
{
	public class DiagnosticTask : ISerializableBotTask
	{
		public DiagnosticTask(string message)
		{
			MessageText = message;
		}

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionRecommendation> ClientActions => null;

		public string ToJson()
		{
			return $"{nameof(DiagnosticTask)}[{MessageText}]";
		}

		public string MessageText;
	}

	public class DynamicTask : ISerializableBotTask
	{
		private readonly List<ISerializableBotTask> component = new List<ISerializableBotTask>();

		public IEnumerable<IBotTask> Component => component;

		public IEnumerable<MotionRecommendation> ClientActions { get; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(component.Select(c => c.ToJson()).ToArray());
		}

		public DynamicTask With(ISerializableBotTask t)
		{
			component.Add(t);
			return this;
		}

		public DynamicTask With(string description)
		{
			component.Add(new DiagnosticTask(description));
			return this;
		}
	}
}