namespace Optimat.EveOnline;

public abstract class SictProcessAuswertWurzelSuuce : SictProzesAuswertZuusctand
{
	public virtual void Berecne()
	{
	}

	public static SictProcessAuswertWurzelSuuce Berecne(SictProcessAuswertWurzelSuuce suuce)
	{
		if (suuce == null)
		{
			return null;
		}
		suuce.Berecne();
		return suuce;
	}
}
