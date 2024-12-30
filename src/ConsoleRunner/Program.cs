// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using ABot;
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
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Sanderling.ABot;
using BotEngine.Client;
using Sanderling;
using Sanderling.ABot.Bot;
using SimpleInterfaceServerDispatcher = Sanderling.SimpleInterfaceServerDispatcher;
using System.Text;
using Eve64;
using IMemoryReader = Eve64.IMemoryReader;
using UITreeNode = PythonStructures.UITreeNode;

//using Sanderling.ABot.Bot;

Console.WriteLine("Hello, World!");
BotEngine.Interface.InterfaceAppDomainSetup.Setup();
var app = new App();
//app.InterfaceExchange();
var eve64bit = true;

int? processId = 38480;
IImmutableList<ulong>? possibleRootAddresses = null;
while (true)
{
	var sw = new Stopwatch(); sw.Start();
	if (eve64bit)
	{
		(IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndRootAddresses()
		{
			possibleRootAddresses ??= EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId.Value);

			return (new EveOnline64.MemoryReaderFromLiveProcess(processId.Value), possibleRootAddresses);
		}

		var (memoryReader, uiRootCandidatesAddresses) = GetMemoryReaderAndRootAddresses();

			Console.WriteLine("Got Root addresses: " + string.Join(',', uiRootCandidatesAddresses));


		IImmutableList<UITreeNode> ReadUITrees() =>
			uiRootCandidatesAddresses
				.Select(uiTreeRoot => Eve64.EveOnline64.ReadUITreeFromAddress(uiTreeRoot, memoryReader, 99))
				.Where(uiTree => uiTree != null)
				.ToImmutableList();

		var uiTrees = ReadUITrees();
		var uiTreesWithStats =
			uiTrees
				.Select(uiTree =>
					new
					{
						uiTree = uiTree,
						nodeCount = uiTree.EnumerateSelfAndDescendants().Count()
					})
				.OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
				.ToImmutableList();

		var uiTreesReport =
			uiTreesWithStats
				.Select(uiTreeWithStats =>
					$"\n0x{uiTreeWithStats.uiTree.pythonObjectAddress:X}: {uiTreeWithStats.nodeCount} nodes.")
				.ToImmutableList();

		Console.WriteLine($"Read {uiTrees.Count} UI trees:" + string.Join("", uiTreesReport));

		var largestUiTree =
			uiTreesWithStats
				.OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
				.FirstOrDefault().uiTree;


		app.MemoryMeasurementLast =
			new FromProcessMeasurement<IMemoryMeasurement>(
				Parser.ParseUserInterfaceFromUITree(Parser.ParseUITreeWithDisplayRegionFromUITree(largestUiTree)), 0,
				0);
	}
	else
	{
		app.MeasurementMemoryTake(processId.Value, app.MeasurementRequestTime() ?? 0);
	}

	sw.Stop();
	Console.WriteLine($"Successfully read memory ({sw.Elapsed}): " + app.MemoryMeasurementLast?.Value?.InfoPanelContainer?.LocationInfo.CurrentSolarSystemName);
	await Task.Delay(500);
}


namespace Sanderling
{
	internal class App
	{
		readonly object botLock = new object();

		readonly Sanderling.ABot.Bot.Bot bot = new Sanderling.ABot.Bot.Bot();

		const int FromMotionToMeasurementDelayMilli = 300;

		const int MemoryMeasurementDistanceMaxMilli = 3000;

		const string BotConfigFileName = "bot.config";

		PropertyGenTimespanInt64<Sanderling.ABot.Bot.MotionResult[]> BotStepLastMotionResult;

		PropertyGenTimespanInt64<KeyValuePair<Exception, StringAtPath>> BotConfigLoaded;

		public Int64? MeasurementRequestTime()
		{
			var memoryMeasurementLast = MemoryMeasurementLast;

			var botStepLastMotionResult = BotStepLastMotionResult;

			if (memoryMeasurementLast?.Begin < botStepLastMotionResult?.End &&
			    0 < botStepLastMotionResult?.Value?.Length)
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

				var stepResult = bot.Step(new Sanderling.ABot.Bot.BotStepInput
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
			IEnumerable<Sanderling.ABot.Bot.MotionRecommendation> sequenceMotion)
		{
			var processId = memoryMeasurement?.ProcessId;

			if (!processId.HasValue || null == sequenceMotion || sequenceMotion.IsNullOrEmpty())
				return;
			var process = System.Diagnostics.Process.GetProcessById(processId.Value);

			var listMotionResult = new List<Sanderling.ABot.Bot.MotionResult>();
			var startTime = GetTimeStopwatch();

			botLock.WhenLockIsAvailableEnter(300, () =>
			{
				throw new NotImplementedException();
				/*var motor = new WindowMotor(process.MainWindowHandle);

				foreach (var motion in sequenceMotion)
				{
					var motionResult =
						motor.ActSequenceMotion(motion.MotionParam.AsSequenceMotion(memoryMeasurement?.Value));

					listMotionResult.Add(new Bot.MotionResult
					{
						Id = motion.Id,
						Success = motionResult?.Success ?? false,
					});
				}*/
			}, "MotionExecution");
			BotStepLastMotionResult =
				new PropertyGenTimespanInt64<Sanderling.ABot.Bot.MotionResult[]>(listMotionResult.ToArray(), startTime,
					GetTimeStopwatch());

			Thread.Sleep(sequenceMotion.Max(sm => sm.DelayAfterMs ?? FromMotionToMeasurementDelayMilli));
		}

		void BotConfigLoad()
		{
			Exception exception = null;
			string configString = null;
			var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), BotConfigFileName);// AssemblyDirectoryPath.PathToFilesysChild(BotConfigFileName);

			try
			{
				using (var fileStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
					configString = new StreamReader(fileStream).ReadToEnd();
			}
			catch (Exception e)
			{
				exception = e;
			}

			BotConfigLoaded = new PropertyGenTimespanInt64<KeyValuePair<Exception, StringAtPath>>(
				new KeyValuePair<Exception, StringAtPath>(
					exception,
					new StringAtPath { Path = configFilePath, String = configString }), GetTimeStopwatch());
		}

		readonly Sensor sensor = new Sensor();

		public FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurementLast;

		readonly SimpleInterfaceServerDispatcher SensorServerDispatcher = new SimpleInterfaceServerDispatcher
		{
			InterfaceAppDomainSetupType = typeof(InterfaceAppDomainSetup),
			InterfaceAppDomainSetupTypeLoadFromMainModule = true,
			LicenseClientConfig = new LicenseClientConfig(),
		};

		public readonly Bib3.RateLimit.IRateLimitStateInt MemoryMeasurementRequestRateLimit =
			new Bib3.RateLimit.RateLimitStateIntSingle();

		public void InterfaceExchange()
		{
			int? eveOnlineClientProcessId = 12008;

			var measurementRequestTime = MeasurementRequestTime() ?? 0;

			if (eveOnlineClientProcessId.HasValue && measurementRequestTime <= GetTimeStopwatch())
				if (MemoryMeasurementRequestRateLimit.AttemptPass(GetTimeStopwatch(), 200))
					Task.Run(() => botLock.IfLockIsAvailableEnter(() =>
						MeasurementMemoryTake(eveOnlineClientProcessId.Value, measurementRequestTime)));
		}

		public void MeasurementMemoryTake(int processId, Int64 measurementBeginTimeMinMilli)
		{
			var measurement = sensor.MeasurementTake(processId, measurementBeginTimeMinMilli);

			if (null == measurement)
				return;

			MemoryMeasurementLast = measurement;
		}
		//static string AssemblyDirectoryPath => Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().EnsureEndsWith(@"\");

		static public Int64 GetTimeStopwatch() => Bib3.Glob.StopwatchZaitMiliSictInt();

		//todo
		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			SensorServerDispatcher.CyclicExchangeStart();

			//UI.Main.SimulateMeasurement += MainWindow_SimulateMeasurement;

			TimerConstruct();
		}

		private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var matchFullName =
				AppDomain.CurrentDomain.GetAssemblies()
					?.FirstOrDefault(candidate => string.Equals(candidate.GetName().FullName, args?.Name));

			if (null != matchFullName)
				return matchFullName;

			var matchName =
				AppDomain.CurrentDomain.GetAssemblies()
					?.FirstOrDefault(candidate => string.Equals(candidate.GetName().Name, args?.Name));

			return matchName;
		}

		void TimerConstruct()
		{
			var timer = new Timer(Timer_Tick, new object(), TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / 3));

			//timer.Start();
		}

		void Timer_Tick(object e)
		{
			//Window?.ProcessInput();

			InterfaceExchange();

			//UIPresent();

			var motionEnable = /*MainControl?.IsBotMotionEnabled ?? */false;

			Task.Run(() => BotProgress(motionEnable));
		}

		//private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		//{
		//	try
		//	{
		//		var filePath = AssemblyDirectoryPath.PathToFilesysChild(DateTime.Now.SictwaiseKalenderString(".", 0) + " Exception");

		//		filePath.WriteToFileAndCreateDirectoryIfNotExisting(Encoding.UTF8.GetBytes(e.Exception.SictString()));

		//		var message = "exception written to file: " + filePath;

		//		MessageBox.Show(message, message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
		//	}
		//	catch (Exception PersistException)
		//	{
		//		Bib3.FCL.GBS.Extension.MessageBoxException(PersistException);
		//	}

		//	Bib3.FCL.GBS.Extension.MessageBoxException(e.Exception);

		//	e.Handled = true;
		//}

		private void MainWindow_SimulateMeasurement(Interface.MemoryStruct.IMemoryMeasurement measurement)
		{
			var time = GetTimeStopwatch();

			MemoryMeasurementLast =
				new BotEngine.Interface.FromProcessMeasurement<Interface.MemoryStruct.IMemoryMeasurement>(measurement,
					time, time);
		}
	}

	public static class LockExtension
	{
		public static bool IsLocked(this object lockRef)
		{
			bool lockTaken = false;
			try
			{
				Monitor.TryEnter(lockRef, ref lockTaken);
				return !lockTaken;
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockRef);
				}
			}
		}

		public static T BranchOnTryEnter<T>(this object lockRef, int waitForLockTimeoutMilli, Func<T> enteredBranch,
			Func<T> enteredNotBranch)
		{
			bool lockTaken = false;
			try
			{
				Monitor.TryEnter(lockRef, waitForLockTimeoutMilli, ref lockTaken);
				if (lockTaken)
				{
					return enteredBranch();
				}

				return enteredNotBranch();
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockRef);
				}
			}
		}

		public static T BranchOnTryEnter<T>(this object lockRef, Func<T> enteredBranch, Func<T> enteredNotBranch)
		{
			return BranchOnTryEnter(lockRef, 0, enteredBranch, enteredNotBranch);
		}

		public static void IfLockIsAvailableEnter(this object lockRef, Action action, string lockName = null)
		{
			WhenLockIsAvailableEnter(lockRef, 0, action, lockName);
		}

		private static ConcurrentDictionary<string, Mutex> ResolvedMutexes = new ConcurrentDictionary<string, Mutex>();

		public static void WhenLockIsAvailableEnter(this object lockRef, int waitForLockTimeoutMilli, Action action,
			string lockName = null)
		{
			bool lockTaken = false;
			if (lockName != null)
			{
				Mutex mutex = ResolvedMutexes.GetOrAdd(lockName, (ln) =>
				{
					try
					{
						return Mutex.OpenExisting(lockName);
					}
					catch
					{
						return new Mutex(false, lockName);
					}
				});

				try
				{
					lockTaken = mutex.WaitOne(waitForLockTimeoutMilli);
					if (lockTaken)
						action();
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception caught in WhenLockIsAvailableEnter: " + e.ToString());
				}
				finally
				{
					if (lockTaken)
					{
						mutex.ReleaseMutex();
					}
				}

			}
			else
			{

				try
				{
					Monitor.TryEnter(lockRef, waitForLockTimeoutMilli, ref lockTaken);
					if (lockTaken)
					{
						action();
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine("Exception caught in WhenLockIsAvailableEnter: " + e.ToString());
				}
				finally
				{
					if (lockTaken)
					{
						Monitor.Exit(lockRef);
					}
				}
			}
		}

	}
}