using System;

namespace BotEngine.Motor;

public class MotionResult
{
	public readonly bool Success;

	public readonly Exception Exception;

	private MotionResult()
	{
	}

	public MotionResult(bool success)
	{
		Success = success;
	}

	public MotionResult(Exception exception)
		: this(success: false)
	{
		Exception = exception;
	}
}
