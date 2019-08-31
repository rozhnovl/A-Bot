using Bib3;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ABot;

namespace Sanderling.ABot.Exe
{
	partial class App
	{
		readonly object botLock = new object();

		readonly Bot.Bot bot = new Bot.Bot();

		const int FromMotionToMeasurementDelayMilli = 300;

		const int MemoryMeasurementDistanceMaxMilli = 3000;

		const string BotConfigFileName = "bot.config";

		PropertyGenTimespanInt64<Bot.MotionResult[]> BotStepLastMotionResult;

		PropertyGenTimespanInt64<KeyValuePair<Exception, StringAtPath>> BotConfigLoaded;

		Int64? MeasurementRequestTime()
		{
			var memoryMeasurementLast = MemoryMeasurementLast;

			var botStepLastMotionResult = BotStepLastMotionResult;

			if (memoryMeasurementLast?.Begin < botStepLastMotionResult?.End && 0 < botStepLastMotionResult?.Value?.Length)
				return botStepLastMotionResult?.End + FromMotionToMeasurementDelayMilli;

			return memoryMeasurementLast?.Begin + MemoryMeasurementDistanceMaxMilli;
		}

		void BotProgress(bool motionEnable)
		{
			botLock.IfLockIsAvailableEnter(() =>
				{
					Debug.WriteLine($"Bot at thread {Thread.CurrentThread.ManagedThreadId} called BotProgress");
					var memoryMeasurementLast = this.MemoryMeasurementLast;

					var time = memoryMeasurementLast?.End;

					if (!time.HasValue)
						return;

					if (time <= bot?.StepLastInput?.TimeMilli)
						return;

					BotConfigLoad();

					var stepResult = bot.Step(new Bot.BotStepInput
					{
						TimeMilli = time.Value,
						FromProcessMemoryMeasurement = memoryMeasurementLast,
						StepLastMotionResult = BotStepLastMotionResult?.Value,
						ConfigSerial = BotConfigLoaded?.Value.Value,
					});

					if (motionEnable)
						BotMotion(memoryMeasurementLast, stepResult?.ListMotion);
					Debug.WriteLine($"Bot at thread {Thread.CurrentThread.ManagedThreadId} finished BotProgress");
				}, nameof(BotProgress) + Process.GetCurrentProcess().Id);
		}

		void BotMotion(
			FromProcessMeasurement<IMemoryMeasurement> memoryMeasurement,
			IEnumerable<Bot.MotionRecommendation> sequenceMotion)
		{
			var processId = memoryMeasurement?.ProcessId;

			if (!processId.HasValue || null == sequenceMotion ||sequenceMotion.IsNullOrEmpty())
				return;
			var process = System.Diagnostics.Process.GetProcessById(processId.Value);

			var listMotionResult = new List<Bot.MotionResult>();
			var startTime = GetTimeStopwatch();

			botLock.WhenLockIsAvailableEnter(300, () =>
			{
				var motor = new WindowMotor(process.MainWindowHandle);
				
				foreach (var motion in sequenceMotion)
				{
					var motionResult =
						motor.ActSequenceMotion(motion.MotionParam.AsSequenceMotion(memoryMeasurement?.Value));

					listMotionResult.Add(new Bot.MotionResult
					{
						Id = motion.Id,
						Success = motionResult?.Success ?? false,
					});
				}
			},"MotionExecution");
			BotStepLastMotionResult = new PropertyGenTimespanInt64<Bot.MotionResult[]>(listMotionResult.ToArray(), startTime, GetTimeStopwatch());

			Thread.Sleep(sequenceMotion.Max(sm => sm.DelayAfterMs ?? FromMotionToMeasurementDelayMilli));
		}

		void BotConfigLoad()
		{
			Exception exception = null;
			string configString = null;
			var configFilePath = AssemblyDirectoryPath.PathToFilesysChild(BotConfigFileName);

			try
			{
				using (var fileStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
					configString = new StreamReader(fileStream).ReadToEnd();
			}
			catch (Exception e)
			{
				exception = e;
			}

			BotConfigLoaded = new PropertyGenTimespanInt64<KeyValuePair<Exception, StringAtPath>>(new KeyValuePair<Exception, StringAtPath>(
				exception,
				new StringAtPath { Path = configFilePath, String = configString }), GetTimeStopwatch());
		}
	}
}
