namespace BotEngine.Interface
{
	public class ColorHSV
	{
		public int? HMilli;

		public int? SMilli;

		public int? VMilli;

		public ColorHSV()
		{
		}

		public ColorHSV(int? hMilli, int? sMilli, int? vMilli)
		{
			HMilli = hMilli;
			SMilli = sMilli;
			VMilli = vMilli;
		}
	}
}
