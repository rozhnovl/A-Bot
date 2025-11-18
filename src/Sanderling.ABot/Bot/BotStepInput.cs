namespace Sanderling.ABot.Bot;

public record BotStepInput
{
	public long TimeMilli { get; init; }

	public BotEngine.Interface.FromProcessMeasurement<Interface.MemoryStruct.IMemoryMeasurement>? FromProcessMemoryMeasurement { get; init; }

	public MotionResult[]? StepLastMotionResult { get; init; }
}
