using Bib3;
using BotEngine.Interface;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Bot.Memory;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.ABot.Bot.Configuration;
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

		/// <summary>The run profile (ship fit + abyss tier) this bot is configured for.</summary>
		public RunProfile Profile { get; }

		private readonly IStrategy strategy;

		public Bot() : this(ProfilesRegistry.Default) { }

		public Bot(RunProfile profile, string role = "", int pid = 0)
			: this(profile, new AbyssalRunner(profile), role, pid) { }

		internal Bot(RunProfile profile, IStrategy strategy, string role = "", int pid = 0)
		{
			Profile = profile;
			this.strategy = strategy;
			Role = role ?? "";
			Pid = pid;
		}

		/// <summary>Fleet role from fleet.config.json (tank / wing-1 / …). Empty for a solo runner.</summary>
		public string Role { get; }

		/// <summary>Client process id; used as the fire-board claimant identity. 0 = unspecified (solo).</summary>
		public int Pid { get; }

		/// <summary>A bot running the simple on-grid belt-combat behavior instead of the abyss strategy.</summary>
		public static Bot BeltTest(RunProfile profile) => new Bot(profile, new BeltTestStrategy(profile));
		public static Bot Anomaly(RunProfile profile) => new Bot(profile, new AnomalyStrategy(profile));
		/// <summary>Read-only perception mode used to rehearse a coordinated fleet doctrine.</summary>
		public static Bot Monitor(RunProfile profile) => new Bot(profile, new MonitorStrategy());

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
				StrategyStatus = BuildStrategyStatus(outputListTaskPath, exception),
			};

			MemorizeStepResult(stepResult);

			StepLastResult = new PropertyGenTimespanInt64<BotStepResult>(stepResult, beginTimeMilli, GetTimeMilli());

			++stepIndex;

			return stepResult;
		}

		/// <summary>
		/// Convert the existing diagnostics/action tree into one common status contract. This makes the
		/// admin view work for old and new strategies without forcing every strategy to know about HTTP/UI.
		/// </summary>
		private StrategyStatus BuildStrategyStatus(IBotTask[][] paths, Exception exception)
		{
			var leaves = paths.EmptyIfNull()
				.Select(path => path?.LastOrDefault())
				.WhereNotDefault()
				.ToArray();
			var details = leaves.OfType<DiagnosticTask>()
				.Select(d => NormalizeStatusText(d.MessageText))
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Distinct()
				.TakeLast(8)
				.ToArray();
			var actionable = leaves.FirstOrDefault(leaf => leaf.ContainsEffect());
			var action = actionable switch
			{
				ISerializableBotTask serializable => NormalizeStatusText(serializable.ToJson()),
				not null => actionable.GetType().Name,
				_ => "",
			};

			var stateLine = details.FirstOrDefault(d =>
				d.StartsWith("Current state is ", StringComparison.OrdinalIgnoreCase));
			var stage = stateLine is null
				? ""
				: stateLine.Substring("Current state is ".Length).Trim();
			var summary = details.LastOrDefault(d => d != stateLine)
			              ?? (!string.IsNullOrWhiteSpace(action) ? action : stateLine)
			              ?? "No status reported";

			var state = ClassifyStrategyState(exception, action, summary);
			return new StrategyStatus
			{
				Strategy = strategy.GetType().Name,
				Stage = stage,
				State = state,
				Summary = exception is null ? summary : $"{exception.GetType().Name}: {exception.Message}",
				Action = action,
				Details = details,
			};
		}

		private static StrategyRunState ClassifyStrategyState(Exception exception, string action, string summary)
		{
			if (exception is not null)
				return StrategyRunState.Error;

			var text = summary ?? "";
			if (new[] { "NOT READY", "INVALID", "BROKEN", "cannot", "failed", "danger" }
			    .Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase)))
				return StrategyRunState.Warning;
			if (!string.IsNullOrWhiteSpace(action))
				return StrategyRunState.Acting;
			if (new[] { "wait", "hold", "loading", "docked", "no ship", "in progress" }
			    .Any(marker => text.Contains(marker, StringComparison.OrdinalIgnoreCase)))
				return StrategyRunState.Waiting;
			return StrategyRunState.Idle;
		}

		private static string NormalizeStatusText(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return "";
			var oneLine = string.Join(" · ", text
				.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(part => part.Trim()));
			return oneLine.Length <= 500 ? oneLine : oneLine.Substring(0, 497) + "...";
		}
	}
}
