using WindowsInput.Native;

namespace Sanderling.ABot.Bot.Task
{
	public static class Hotkeys
	{
		public static HotkeyTask LaunchDrones =
			new HotkeyTask(VirtualKeyCode.VK_F, VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT);

		public static HotkeyTask ReturnDrones =
			new HotkeyTask(VirtualKeyCode.VK_R, VirtualKeyCode.SHIFT);
		public static HotkeyTask EngageDrones =
			new HotkeyTask(VirtualKeyCode.VK_F);
	}
}
