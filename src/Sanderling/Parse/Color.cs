using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	static public class ColorExtension
	{
		static public bool IsRed(this ColorORGB color) =>
			null != color &&
			color.BMilli < color.RMilli / 3 &&
			color.GMilli < color.RMilli / 3 &&
			300 < color.RMilli;	
		
		static public bool IsRed(this ColorComponents color) =>
			null != color &&
			color.BPercent < color.RPercent / 3 &&
			color.GPercent < color.RPercent / 3 &&
			80 < color.RPercent;
	}
}
