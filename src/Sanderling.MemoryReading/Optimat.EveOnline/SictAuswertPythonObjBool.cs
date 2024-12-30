using BotEngine.Interface;

namespace Optimat.EveOnline;

public class SictAuswertPythonObjBool : SictAuswertPythonObjInt
{
	public bool? Bool
	{
		get
		{
			int? @int = base.Int;
			if (@int.HasValue)
			{
				return @int != 0;
			}
			return null;
		}
	}

	public SictAuswertPythonObjBool(long HerkunftAdrese, byte[] ListeOktet = null, IMemoryReader DaatenKwele = null)
		: base(HerkunftAdrese, ListeOktet, DaatenKwele)
	{
	}
}
