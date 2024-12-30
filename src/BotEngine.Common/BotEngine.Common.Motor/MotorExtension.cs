using Bib3.Geometrik;
using BotEngine.Motor;

namespace BotEngine.Common.Motor;

public static class MotorExtension
{
	public static MotionResult MouseMotion(this IMotor motor, Vektor2DInt location, MouseButtonIdEnum[] setMouseButtonId = null)
	{
		return motor.ActSequenceMotion(new Motion[1]
		{
			new Motion(location, setMouseButtonId, setMouseButtonId)
		});
	}

	public static MotionResult MouseClick(this IMotor motor, Vektor2DInt location, MouseButtonIdEnum mouseButtonId)
	{
		return motor.MouseMotion(location, new MouseButtonIdEnum[1] { mouseButtonId });
	}

	public static MotionResult MouseMove(this IMotor motor, Vektor2DInt location)
	{
		return motor.MouseMotion(location);
	}

	public static MotionResult MouseClickLeft(this IMotor motor, Vektor2DInt location)
	{
		return motor.MouseClick(location, MouseButtonIdEnum.Left);
	}

	public static MotionResult MouseClickRight(this IMotor motor, Vektor2DInt location)
	{
		return motor.MouseClick(location, MouseButtonIdEnum.Right);
	}
}
