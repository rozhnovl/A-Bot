using AbyssalBot.Domain.Interfaces.Infrastructure;
using BotEngine.Motor;

namespace AbyssalBot.Infrastructure.Input;

/// <summary>
/// Implementation of IMotionExecutor using WindowMotor
/// </summary>
public class WindowMotorMotionExecutor : IMotionExecutor
{
    private readonly IMotor _motor;
    private int _motionDelayMilliseconds = 120;

    public int MotionDelayMilliseconds
    {
        get => _motionDelayMilliseconds;
        set => _motionDelayMilliseconds = value;
    }

    public WindowMotorMotionExecutor(IMotor motor)
    {
        _motor = motor ?? throw new ArgumentNullException(nameof(motor));
    }

    public async Task<MotionExecutionResult> ExecuteMotionsAsync(
        IEnumerable<MotionCommand> motions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var motionList = motions.ToList();
            var convertedMotions = ConvertToMotorMotions(motionList);

            // Execute using WindowMotor
            var result = _motor.ActSequenceMotion(convertedMotions);

            return new MotionExecutionResult(
                Success: result?.Success ?? false,
                MotionsExecuted: motionList.Count,
                ErrorMessage: result?.Exception?.Message
            );
        }
        catch (Exception ex)
        {
            return new MotionExecutionResult(
                Success: false,
                MotionsExecuted: 0,
                ErrorMessage: ex.Message,
                Exception: ex
            );
        }
    }

    public async Task<MotionExecutionResult> ExecuteMotionAsync(
        MotionCommand motion,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteMotionsAsync(new[] { motion }, cancellationToken);
    }

    public async Task EnsureWindowForegroundAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement window foreground logic using WindowMotor
        await Task.CompletedTask;
    }

    private IEnumerable<Motion> ConvertToMotorMotions(IEnumerable<MotionCommand> commands)
    {
        // TODO: Convert domain MotionCommands to BotEngine.Motor.Motion objects
        // This is a placeholder showing the pattern
        return Array.Empty<Motion>();
    }
}
