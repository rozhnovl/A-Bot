using Bib3;
using BotEngine.Interface;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Bot.Memory;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.ABot.Serialization;
using Sanderling.Parse;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	
	public class Bot
	{
		static public readonly Func<long> GetTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt;

		public BotStepInput StepLastInput { private set; get; }

		public PropertyGenTimespanInt64<BotStepResult> StepLastResult { private set; get; }

		private IStrategy strategy = /*new AnomalyHunting();//*/new AbyssalRunner();// new CorporationMissionTaker();

		private int motionId;

		public int stepIndex;
		/// <summary>
		/// Current measurements
		/// </summary>
		public FromProcessMeasurement<Sanderling.Parse.IMemoryMeasurement> MemoryMeasurementAtTime { private set; get; }

		readonly public OverviewMemory OverviewMemory = new OverviewMemory();

		private readonly IDictionary<long, int> MouseClickLastStepIndexFromUIElementId = new Dictionary<long, int>();

		/// <summary>
		/// Step number on which modules have been activated last time. Prevents duplicate clicks on modules during their activation
		/// </summary>
		private readonly IDictionary<ShipUIModuleButton, int> ToggleLastStepIndexFromModule = new Dictionary<ShipUIModuleButton, int>();

		public KeyValuePair<Deserialization, Config> ConfigSerialAndStruct { private set; get; }

		public long? MouseClickLastAgeStepCountFromUIElement(Interface.MemoryStruct.IUIElement uiElement)
		{
			if (null == uiElement)
				return null;

			var interactionLastStepIndex = MouseClickLastStepIndexFromUIElementId?.TryGetValueNullable(uiElement.Id);

			return stepIndex - interactionLastStepIndex;
		}

		public long? ToggleLastAgeStepCountFromModule(ShipUIModuleButton module) =>
			module == null ? null :
			stepIndex - ToggleLastStepIndexFromModule?.TryGetValueNullable(module);


		private void MemorizeStepInput(BotStepInput input)
		{
			MemoryMeasurementAtTime = input?.FromProcessMemoryMeasurement?.MapValue(measurement => measurement?.Parse());

			OverviewMemory.Aggregate(MemoryMeasurementAtTime);
		}

		private void MemorizeStepResult(BotStepResult stepResult)
		{
			var setMotionMouseWaypointUIElement =
				stepResult?.ListMotion
				?.Select(motion => motion?.MotionParam)
				?.Where(motionParam => 0 < motionParam?.MouseButton?.Count())
				?.Select(motionParam => motionParam?.MouseListWaypoint)
				?.ConcatNullable()?.Select(mouseWaypoint => mouseWaypoint?.UIElement)?.WhereNotDefault();

			foreach (var mouseWaypointUIElement in setMotionMouseWaypointUIElement.EmptyIfNull())
				MouseClickLastStepIndexFromUIElementId[mouseWaypointUIElement.Id] = stepIndex;
		}

		public BotStepResult Step(BotStepInput input)
		{
			var beginTimeMilli = GetTimeMilli();

			StepLastInput = input;

			Exception exception = null;

			var listMotion = new List<MotionRecommendation>();

			IBotTask[][] outputListTaskPath = null;

			try
			{
				MemorizeStepInput(input);

				outputListTaskPath = ((IBotTask)new BotTask(null) { Component = strategy.GetTasks(this) })
					?.EnumeratePathToNodeFromTreeDFirst(node => node?.Component)
					?.Where(taskPath => (taskPath?.LastOrDefault()).ShouldBeIncludedInStepOutput())
					?.TakeSubsequenceWhileUnwantedInferenceRuledOut()
					?.ToArray();

				foreach (var moduleToggle in outputListTaskPath.ConcatNullable().OfType<ModuleToggleTask>()
					.SelectMany(moduleToggleTask => moduleToggleTask?.modules).WhereNotDefault())
					ToggleLastStepIndexFromModule[moduleToggle] = stepIndex;

				foreach (var effect in outputListTaskPath.EmptyIfNull().SelectMany(taskPath =>
					(taskPath?.LastOrDefault()?.ApplicableEffects()).EmptyIfNull()))
				{
					listMotion.Add(effect);
				}
			}
			catch (Exception e)
			{
				exception = e;
			}

			var stepResult = new BotStepResult
			{
				Exception = exception,
				ListMotion = listMotion?.ToArrayIfNotEmpty(),
				OutputListTaskPath = outputListTaskPath,
			};

			MemorizeStepResult(stepResult);

			StepLastResult = new PropertyGenTimespanInt64<BotStepResult>(stepResult, beginTimeMilli, GetTimeMilli());

			++stepIndex;

			return stepResult;
		}
	}
}
