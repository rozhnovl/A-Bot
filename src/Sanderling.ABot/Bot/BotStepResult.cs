namespace Sanderling.ABot.Bot;

public record BotStepResult
{
	public Exception? Exception { get; init; }

	public MotionRecommendation[]? ListMotion { get; init; }

	public IBotTask[][]? OutputListTaskPath { get; init; }
}
