using Bib3.Geometrik;
using WindowsInput.Native;

namespace BotEngine.Motor;

public class Motion
{
	public readonly Vektor2DInt? MousePosition;

	public readonly MouseButtonIdEnum[] MouseButtonDown;

	public readonly MouseButtonIdEnum[] MouseButtonUp;

	public readonly VirtualKeyCode[] KeyDown;

	public readonly VirtualKeyCode[] KeyUp;

	public readonly string TextEntry;

	public readonly bool? WindowToForeground;

	public Motion()
	{
	}

	public Motion(Vektor2DInt? mousePosition, MouseButtonIdEnum[] mouseButtonDown = null, MouseButtonIdEnum[] mouseButtonUp = null, VirtualKeyCode[] keyDown = null, VirtualKeyCode[] keyUp = null, string textEntry = null, bool? windowToForeground = null)
	{
		MousePosition = mousePosition;
		MouseButtonDown = mouseButtonDown;
		MouseButtonUp = mouseButtonUp;
		KeyDown = keyDown;
		KeyUp = keyUp;
		TextEntry = textEntry;
		WindowToForeground = windowToForeground;
	}
}
