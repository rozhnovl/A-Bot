namespace BotEngine.Interface
{
	public class ColorORGB
	{
		public int? OMilli;

		public int? RMilli;

		public int? GMilli;

		public int? BMilli;

		public bool AllNotNull()
		{
			return OMilli.HasValue && RMilli.HasValue && GMilli.HasValue && BMilli.HasValue;
		}

		public ColorORGB()
		{
		}

		public ColorORGB(ColorORGBVal? farbeVal)
			: this(farbeVal.HasValue ? farbeVal.Value.OMilli : null, farbeVal.HasValue ? farbeVal.Value.RMilli : null, farbeVal.HasValue ? farbeVal.Value.GMilli : null, farbeVal.HasValue ? farbeVal.Value.BMilli : null)
		{
		}

		public static ColorORGB VonVal(ColorORGBVal? farbeValNulbar)
		{
			if (!farbeValNulbar.HasValue)
			{
				return null;
			}
			return new ColorORGB(farbeValNulbar);
		}

		public ColorORGB(int? oMilli, int? rMilli, int? gMilli, int? bMilli)
		{
			OMilli = oMilli;
			RMilli = rMilli;
			GMilli = gMilli;
			BMilli = bMilli;
		}
	}
}
