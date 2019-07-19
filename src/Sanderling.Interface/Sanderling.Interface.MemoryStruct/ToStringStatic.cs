namespace Sanderling.Interface.MemoryStruct
{
	public static class ToStringStatic
	{
		public static string SensorObjectToString(this object obj)
		{
			return (obj as string) ?? obj?.GetType()?.ToString();
		}
	}
}
