using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictProcessMitIdAuswertWurzelSuuce : MemoryAuswertWurzelSuuce
{
	public readonly int ProcessId;

	public SictProcessMitIdAuswertWurzelSuuce(int processId)
		: base((IMemoryReader)new ProcessMemoryReader(processId))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		ProcessId = processId;
	}
}
