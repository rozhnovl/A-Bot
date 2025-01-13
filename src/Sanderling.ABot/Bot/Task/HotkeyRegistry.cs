﻿using WindowsInput.Native;

namespace Sanderling.ABot.Bot.Task
{
	public static class HotkeyRegistry
	{
		public static HotkeyTask EngageDrones = new HotkeyTask(VirtualKeyCode.VK_F);
		public static HotkeyTask LaunchDrones = new HotkeyTask(VirtualKeyCode.VK_F, VirtualKeyCode.SHIFT);
		public static HotkeyTask ReturnDrones = new HotkeyTask(VirtualKeyCode.VK_R, VirtualKeyCode.SHIFT);
		public static VirtualKeyCode[] UnlockTargetModifier = new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT };
		public static VirtualKeyCode[] OrbitModifier = new[] { VirtualKeyCode.VK_W };
	}
}