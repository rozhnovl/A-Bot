namespace BotEngine.Interface.Process.Snapshot
{
	public class Snapshot
	{
		public readonly ProcessSnapshot ProcessSnapshot;

		public readonly WindowSnapshot MainWindowSnapshot;

		public Snapshot()
		{
		}

		public Snapshot(ProcessSnapshot processSnapshot, WindowSnapshot mainWindowSnapshot)
		{
			ProcessSnapshot = processSnapshot;
			MainWindowSnapshot = mainWindowSnapshot;
		}
	}
}
