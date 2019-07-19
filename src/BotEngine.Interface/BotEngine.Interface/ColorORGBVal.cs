namespace BotEngine.Interface
{
	public struct ColorORGBVal
	{
		public int? OMilli;

		public int? RMilli;

		public int? GMilli;

		public int? BMilli;

		public bool AleUnglaicNul()
		{
			return OMilli.HasValue && RMilli.HasValue && GMilli.HasValue && BMilli.HasValue;
		}

		public ColorORGBVal(int? oMilli, int? rMilli, int? gMilli, int? bMilli)
		{
			OMilli = oMilli;
			RMilli = rMilli;
			GMilli = gMilli;
			BMilli = bMilli;
		}
	}
}
