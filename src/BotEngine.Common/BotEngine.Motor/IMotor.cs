using System.Collections.Generic;

namespace BotEngine.Motor;

public interface IMotor
{
	MotionResult ActSequenceMotion(IEnumerable<Motion> seqMotion);
}
