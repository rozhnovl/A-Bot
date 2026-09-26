using Bib3.Geometrik;
using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using WindowsInput.Native;

namespace AbotMcp;

/// <summary>Translate operator-friendly strings (keys, buttons, coordinates) into engine motions.</summary>
internal static class Input
{
    private static readonly Dictionary<string, VirtualKeyCode> Named = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ctrl"] = VirtualKeyCode.CONTROL, ["control"] = VirtualKeyCode.CONTROL,
        ["alt"] = VirtualKeyCode.MENU, ["shift"] = VirtualKeyCode.SHIFT,
        ["esc"] = VirtualKeyCode.ESCAPE, ["escape"] = VirtualKeyCode.ESCAPE,
        ["enter"] = VirtualKeyCode.RETURN, ["return"] = VirtualKeyCode.RETURN,
        ["space"] = VirtualKeyCode.SPACE, ["tab"] = VirtualKeyCode.TAB,
        ["backspace"] = VirtualKeyCode.BACK, ["del"] = VirtualKeyCode.DELETE, ["delete"] = VirtualKeyCode.DELETE,
        ["up"] = VirtualKeyCode.UP, ["down"] = VirtualKeyCode.DOWN, ["left"] = VirtualKeyCode.LEFT, ["right"] = VirtualKeyCode.RIGHT,
        ["home"] = VirtualKeyCode.HOME, ["end"] = VirtualKeyCode.END,
        ["pageup"] = VirtualKeyCode.PRIOR, ["pagedown"] = VirtualKeyCode.NEXT,
        ["minus"] = VirtualKeyCode.OEM_MINUS, ["-"] = VirtualKeyCode.OEM_MINUS,
        ["plus"] = VirtualKeyCode.OEM_PLUS, ["="] = VirtualKeyCode.OEM_PLUS,
        ["win"] = VirtualKeyCode.LWIN,
    };

    public static VirtualKeyCode ParseKey(string k)
    {
        k = k.Trim();
        if (k.Length == 0) throw new ArgumentException("empty key");
        if (Named.TryGetValue(k, out var named)) return named;
        if (k.Length == 1)
        {
            var c = char.ToUpperInvariant(k[0]);
            if (c is >= 'A' and <= 'Z' or >= '0' and <= '9')
                return Enum.Parse<VirtualKeyCode>("VK_" + c);
        }
        if (Enum.TryParse<VirtualKeyCode>(k.ToUpperInvariant(), out var direct)) return direct;
        if (Enum.TryParse<VirtualKeyCode>("VK_" + k.ToUpperInvariant(), out var prefixed)) return prefixed;
        throw new ArgumentException($"unknown key '{k}' (examples: f1, ctrl+f1, alt+p, esc, space, a, 3)");
    }

    /// <summary>"ctrl+f1 f2 esc" -> three chords; each chord is pressed together.</summary>
    public static VirtualKeyCode[][] ParseChords(string keys) =>
        keys.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(chord => chord.Split('+', StringSplitOptions.RemoveEmptyEntries).Select(ParseKey).ToArray())
            .Where(c => c.Length > 0)
            .ToArray();

    public static VirtualKeyCode[] ParseModifiers(string? modifiers) =>
        string.IsNullOrWhiteSpace(modifiers)
            ? Array.Empty<VirtualKeyCode>()
            : modifiers.Split(new[] { '+', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ParseKey).ToArray();

    public static MouseButtonIdEnum ParseButton(string? button) => (button ?? "left").Trim().ToLowerInvariant() switch
    {
        "left" or "l" or "" => MouseButtonIdEnum.Left,
        "right" or "r" => MouseButtonIdEnum.Right,
        "middle" or "m" => MouseButtonIdEnum.Middle,
        "none" or "hover" or "move" => MouseButtonIdEnum.None,
        var other => throw new ArgumentException($"unknown button '{other}' (left|right|middle|hover)"),
    };

    public static IEnumerable<MotionParam> Click(IUIElement element, MouseButtonIdEnum button,
        VirtualKeyCode[] modifiers, bool doubleClick)
    {
        foreach (var m in modifiers) yield return m.KeyDown();
        yield return doubleClick ? element.MouseDoubleClick(button) : element.MouseClick(button);
        foreach (var m in modifiers.Reverse()) yield return m.KeyUp();
    }

    /// <summary>A synthetic element at absolute screen coordinates; bypasses the occlusion model.</summary>
    public static IUIElement PointElement(int x, int y, int half = 2) => new UIElement(new ObjectIdInMemory(-1))
    {
        Region = new RectInt(x - half, y - half, x + half, y + half),
        InTreeIndex = int.MaxValue,
        ChildLastInTreeIndex = int.MaxValue,
    };

    /// <summary>A synthetic element over a raw-tree node's visible region; bypasses the occlusion model.</summary>
    public static IUIElement RegionElement(long id, RectInt region) => new UIElement(new ObjectIdInMemory(id))
    {
        Region = region,
        InTreeIndex = int.MaxValue,
        ChildLastInTreeIndex = int.MaxValue,
    };

    public static string Describe(IUIElement el)
    {
        var r = el.RegionInteraction?.Region ?? el.Region;
        return r is null ? $"#{el.Id} (no region)" : $"#{el.Id} @({r.Value.Center().A},{r.Value.Center().B})";
    }
}
