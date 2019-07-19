namespace Sanderling.Interface.MemoryStruct
{
	public class SquadronHealth : ISquadronHealth
	{
		public int? SquadronSizeMax
		{
			get;
			set;
		}

		public int? SquadronSizeCurrent
		{
			get;
			set;
		}
	}
}
