﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using WindowsInput.Native;

namespace Sanderling.Parse
{
	static public class Culture
	{
		static public CultureInfo ParseCulture => CultureInfo.InvariantCulture;

		static public void InvokeInParseCulture(this Action method)
		{
			var OriginalCulture = Thread.CurrentThread.CurrentCulture;

			Thread.CurrentThread.CurrentCulture = ParseCulture;

			try
			{
				method?.Invoke();
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = OriginalCulture;
			}
		}
	}

	static public class CultureAggregated
	{
		static VirtualKeyCode CtrlKeyCode => Extension.CtrlKeyCode;
		static VirtualKeyCode AltKeyCode => Extension.AltKeyCode;

		static readonly KeyValuePair<string, VirtualKeyCode>[] SetKeyFCodeFromUIText =
			Enumerable.Range(0, 12).Select(index => new KeyValuePair<string, VirtualKeyCode>("f" + (index + 1), VirtualKeyCode.F1 + index)).ToArray();

		static public readonly KeyValuePair<string, VirtualKeyCode>[] SetKeyCodeFromUIText = new[]
		{
			new KeyValuePair<string, VirtualKeyCode>("ctrl", CtrlKeyCode),
			new KeyValuePair<string, VirtualKeyCode>("alt", AltKeyCode),
			new KeyValuePair<string, VirtualKeyCode>("shift", VirtualKeyCode.SHIFT),
			new KeyValuePair<string, VirtualKeyCode>("enter", VirtualKeyCode.RETURN),
			new KeyValuePair<string, VirtualKeyCode>("space", VirtualKeyCode.SPACE),

			//	"de"
			new KeyValuePair<string, VirtualKeyCode>("strg", CtrlKeyCode),
			new KeyValuePair<string, VirtualKeyCode>("umschalt", VirtualKeyCode.SHIFT),
			new KeyValuePair<string, VirtualKeyCode>("eingabe", VirtualKeyCode.RETURN),
			new KeyValuePair<string, VirtualKeyCode>("leer", VirtualKeyCode.SPACE),

			//	TODO: Insert missing mappings here.

		}.Concat(SetKeyFCodeFromUIText).ToArray();

		static public IEnumerable<string> KeyCodeFromUITextSetCollidingKey() =>
			SetKeyCodeFromUIText?.GroupBy(keyCodeFromUIText => keyCodeFromUIText.Key?.ToLower())?.Where(group => 1 < group.Count())?.Select(group => group.Key);
	}
}
