using BotEngine.Windows;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BotEngine.WinApi
{
	public static class User32
	{
		public enum MouseEventFlagEnum
		{
			MOUSEEVENTF_ABSOLUTE = 0x8000,
			MOUSEEVENTF_LEFTDOWN = 2,
			MOUSEEVENTF_LEFTUP = 4,
			MOUSEEVENTF_MIDDLEDOWN = 0x20,
			MOUSEEVENTF_MIDDLEUP = 0x40,
			MOUSEEVENTF_MOVE = 1,
			MOUSEEVENTF_RIGHTDOWN = 8,
			MOUSEEVENTF_RIGHTUP = 0x10,
			MOUSEEVENTF_WHEEL = 0x800,
			MOUSEEVENTF_XDOWN = 0x80,
			MOUSEEVENTF_XUP = 0x100
		}

		public delegate bool EnumWindowsProc(IntPtr hWnd, ref ListeIntPtr listeWindowHandle);

		public class ListeIntPtr
		{
			public readonly List<IntPtr> Liste = new List<IntPtr>();
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumWindows(EnumWindowsProc callback, ref ListeIntPtr listeWindowHandle);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.Dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr parentHandle, EnumWindowsProc callback, ref ListeIntPtr listeWindowHandle);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, [Out] [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT pos);

		[DllImport("user32.dll")]
		public static extern bool GetCursorInfo(ref CURSORINFO pci);

		[DllImport("user32.dll")]
		public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint nFlags);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static IntPtr MAKELPARAM(ushort wLow, ushort wHigh)
		{
			int value = wLow | (wHigh << 16);
			return (IntPtr)value;
		}

		public static string GetWindowText(IntPtr hWnd)
		{
			StringBuilder stringBuilder = new StringBuilder(4444);
			GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}

		public static IEnumerable<IntPtr> ListeWindowTopLevelHandle()
		{
			ListeIntPtr listeWindowHandle = new ListeIntPtr();
			bool flag = EnumWindows(User32.EnumWindowsCallback, ref listeWindowHandle);
			return listeWindowHandle.Liste;
		}

		public static IEnumerable<IntPtr> ListeChildWindowHandle(IntPtr parentWindowHandle)
		{
			ListeIntPtr listeWindowHandle = new ListeIntPtr();
			bool flag = EnumChildWindows(parentWindowHandle, User32.EnumWindowsCallback, ref listeWindowHandle);
			return listeWindowHandle.Liste;
		}

		private static bool EnumWindowsCallback(IntPtr hWnd, ref ListeIntPtr listeWindowHandle)
		{
			listeWindowHandle.Liste.Add(hWnd);
			return true;
		}

		[DllImport("user32.dll")]
		public static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
	}
}
