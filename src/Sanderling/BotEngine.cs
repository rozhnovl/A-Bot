﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Sanderling
{
// "Newtonsoft.Json"

//  "WindowsInput"

//  "System.Drawing.Common"

//  "System.Drawing.Primitives"



	public class Request
	{
		public TaskOnIdentifiedWindowStructure TaskOnWindow;

		public object GetForegroundWindow;

		public WindowId GetWindowText;

		public class TaskOnIdentifiedWindowStructure
		{
			public WindowId windowId;

			public TaskOnWindowStructure task;
		}

		public class TaskOnWindowStructure
		{
			public object BringWindowToForeground;

			public Location2d MoveMouseToLocation;

			public MouseButton MouseButtonDown;

			public MouseButton MouseButtonUp;

			public KeyboardKey KeyboardKeyDown;

			public KeyboardKey KeyboardKeyUp;

			public object TakeScreenshot;
		}

		public class Location2d
		{
			public int x, y;
		}

		public class MouseButton
		{
			public object MouseButtonLeft;
			public object MouseButtonRight;
		}

		public class KeyboardKey
		{
			public int KeyboardKeyFromVirtualKeyCode;
		}
	}

	public class Response
	{
		public WindowId GetForegroundWindowResult;

		public object NoReturnValue;

		public string GetWindowTextResult;

		public TakeScreenshotResultStructure TakeScreenshotResult;

		public class TakeScreenshotResultStructure
		{
			public int[][] pixels;
		}
	}


	public class WindowId
	{
		public long WindowHandleFromInt;
	}
		/*

	byte[] SHA256FromByteArray(byte[] array)
	{
		using (var hasher = new SHA256Managed())
			return hasher.ComputeHash(buffer: array);
	}

	string ToStringBase16(byte[] array) => BitConverter.ToString(array).Replace("-", "");


	string serialRequest(string serializedRequest)
	{
		var requestStructure = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(serializedRequest);

		var response = request(requestStructure);

		return SerializeToJsonForBot(response);
	}

	public Response request(Request request)
	{
		SetProcessDPIAware();

		string GetWindowText(WindowId windowId)
		{
			var windowHandle = new IntPtr(windowId.WindowHandleFromInt);

			var windowTitle = new System.Text.StringBuilder(capacity: 256);

			WinApi.GetWindowText(windowHandle, windowTitle, windowTitle.Capacity);

			return windowTitle.ToString();
		}

		if (request.GetForegroundWindow != null)
		{
			return new Response
			{
				GetForegroundWindowResult = GetForegroundWindow()
			};
		}

		if (request.GetWindowText != null)
		{
			return new Response
			{
				GetWindowTextResult = GetWindowText(request.GetWindowText),
			};
		}

		if (request.TaskOnWindow != null)
		{
			return performTaskOnWindow(request.TaskOnWindow);
		}

		throw new Exception("Unexpected request value.");
	}

	public WindowId GetForegroundWindow() =>
		new WindowId { WindowHandleFromInt = WinApi.GetForegroundWindow().ToInt64() };

	Response performTaskOnWindow(Request.TaskOnIdentifiedWindowStructure taskOnIdentifiedWindow)
	{
		var windowHandle = new IntPtr(taskOnIdentifiedWindow.windowId.WindowHandleFromInt);

		var inputSimulator = new WindowsInput.InputSimulator();

		var task = taskOnIdentifiedWindow.task;

		if (task.BringWindowToForeground != null)
		{
			WinApi.SetForegroundWindow(windowHandle);
			WinApi.ShowWindow(windowHandle, WinApi.SW_RESTORE);
			return new Response { NoReturnValue = new object() };
		}

		if (task.MoveMouseToLocation != null)
		{
			var windowRect = new WinApi.Rect();
			if (WinApi.GetWindowRect(windowHandle, ref windowRect) == IntPtr.Zero)
				throw new Exception("GetWindowRect failed");

			WinApi.SetCursorPos(
				task.MoveMouseToLocation.x + windowRect.left,
				task.MoveMouseToLocation.y + windowRect.top);

			return new Response { NoReturnValue = new object() };
		}

		if (task.MouseButtonDown != null)
		{
			if (task.MouseButtonDown.MouseButtonLeft != null)
				inputSimulator.Mouse.LeftButtonDown();

			if (task.MouseButtonDown.MouseButtonRight != null)
				inputSimulator.Mouse.RightButtonDown();

			return new Response { NoReturnValue = new object() };
		}

		if (task.MouseButtonUp != null)
		{
			if (task.MouseButtonUp.MouseButtonLeft != null)
				inputSimulator.Mouse.LeftButtonUp();

			if (task.MouseButtonUp.MouseButtonRight != null)
				inputSimulator.Mouse.RightButtonUp();

			return new Response { NoReturnValue = new object() };
		}

		if (task.KeyboardKeyDown != null)
		{
			inputSimulator.Keyboard.KeyDown(
				(WindowsInput.Native.VirtualKeyCode)task.KeyboardKeyDown.KeyboardKeyFromVirtualKeyCode);
			return new Response { NoReturnValue = new object() };
		}

		if (task.KeyboardKeyUp != null)
		{
			inputSimulator.Keyboard.KeyUp(
				(WindowsInput.Native.VirtualKeyCode)task.KeyboardKeyUp.KeyboardKeyFromVirtualKeyCode);
			return new Response { NoReturnValue = new object() };
		}

		if (task.TakeScreenshot != null)
		{
			return new Response
			{
				TakeScreenshotResult = new Response.TakeScreenshotResultStructure
				{
					pixels = GetScreenshotOfWindowAsPixelsValues(windowHandle)
				}
			};
		}

		throw new Exception("Unexpected task in request: " + task);
	}

	void SetProcessDPIAware()
	{
		//  https://www.google.com/search?q=GetWindowRect+dpi
		//  https://github.com/dotnet/wpf/issues/859
		//  https://github.com/dotnet/winforms/issues/135
		WinApi.SetProcessDPIAware();
	}

	public byte[] GetScreenshotOfWindowAsImageFileBMP(IntPtr windowHandle)
	{
		var screenshotAsBitmap = GetScreenshotOfWindowAsBitmap(windowHandle);

		if (screenshotAsBitmap == null)
			return null;

		using (var stream = new System.IO.MemoryStream())
		{
			screenshotAsBitmap.Save(stream, format: System.Drawing.Imaging.ImageFormat.Bmp);
			return stream.ToArray();
		}
	}

	public int[][] GetScreenshotOfWindowAsPixelsValues(IntPtr windowHandle)
	{
		var screenshotAsBitmap = GetScreenshotOfWindowAsBitmap(windowHandle);

		if (screenshotAsBitmap == null)
			return null;

		var bitmapData = screenshotAsBitmap.LockBits(
			new System.Drawing.Rectangle(0, 0, screenshotAsBitmap.Width, screenshotAsBitmap.Height),
			System.Drawing.Imaging.ImageLockMode.ReadOnly,
			System.Drawing.Imaging.PixelFormat.Format24bppRgb);

		int byteCount = bitmapData.Stride * screenshotAsBitmap.Height;
		byte[] pixelsArray = new byte[byteCount];
		IntPtr ptrFirstPixel = bitmapData.Scan0;
		Marshal.Copy(ptrFirstPixel, pixelsArray, 0, pixelsArray.Length);

		screenshotAsBitmap.UnlockBits(bitmapData);

		var pixels = new int[screenshotAsBitmap.Height][];

		for (var rowIndex = 0; rowIndex < screenshotAsBitmap.Height; ++rowIndex)
		{
			var rowPixelValues = new int[screenshotAsBitmap.Width];

			for (var columnIndex = 0; columnIndex < screenshotAsBitmap.Width; ++columnIndex)
			{
				var pixelBeginInArray = bitmapData.Stride * rowIndex + columnIndex * 3;

				var red = pixelsArray[pixelBeginInArray + 2];
				var green = pixelsArray[pixelBeginInArray + 1];
				var blue = pixelsArray[pixelBeginInArray + 0];

				rowPixelValues[columnIndex] = (red << 16) | (green << 8) | blue;
			}

			pixels[rowIndex] = rowPixelValues;
		}

		return pixels;
	}

	public System.Drawing.Bitmap GetScreenshotOfWindowAsBitmap(IntPtr windowHandle)
	{
		SetProcessDPIAware();

		var windowRect = new WinApi.Rect();
		if (WinApi.GetWindowRect(windowHandle, ref windowRect) == IntPtr.Zero)
			return null;

		int width = windowRect.right - windowRect.left;
		int height = windowRect.bottom - windowRect.top;

		var asBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

		System.Drawing.Graphics.FromImage(asBitmap).CopyFromScreen(
			windowRect.left,
			windowRect.top,
			0,
			0,
			new System.Drawing.Size(width, height),
			System.Drawing.CopyPixelOperation.SourceCopy);

		return asBitmap;
	}


	static public class WinApi
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct Rect
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		public enum MouseButton
		{
			Left = 0,
			Middle = 1,
			Right = 2,
		}

		[DllImport("user32.dll")]
		static public extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static public extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

		[DllImport("user32.dll", SetLastError = true)]
		static public extern bool SetProcessDPIAware();

		[DllImport("user32.dll")]
		static public extern int SetForegroundWindow(IntPtr hWnd);

		public const int SW_RESTORE = 9;

		[DllImport("user32.dll")]
		static public extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		static public extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

		[DllImport("user32.dll", SetLastError = false)]
		static public extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static public extern bool SetCursorPos(int x, int y);
	}

	string SerializeToJsonForBot<T>(T value) =>
		Newtonsoft.Json.JsonConvert.SerializeObject(
			value,
			//  Use settings to get same derivation as at https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling.MemoryReading.Test/MemoryReadingDemo.cs#L91-L97
			new Newtonsoft.Json.JsonSerializerSettings
			{
				//  Bot code does not expect properties with null values, see https://github.com/Viir/bots/blob/880d745b0aa8408a4417575d54ecf1f513e7aef4/explore/2019-05-14.eve-online-bot-framework/src/Sanderling_Interface_20190514.elm
				NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,

				//	https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type/18223985#18223985
				ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
			});
		*/
}