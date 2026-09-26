using System.Runtime.InteropServices;

namespace AbotMcp;

/// <summary>Global Ctrl+Alt+K emergency stop polled via GetAsyncKeyState (works while EVE has focus).</summary>
internal static class HotkeyStop
{
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int virtualKey);
    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    private const int VkControl = 0x11, VkMenu = 0x12, VkK = 0x4B;
    private const uint MouseLeftUp = 0x0004, MouseRightUp = 0x0010, MouseMiddleUp = 0x0040;

    public const string Combo = "Ctrl+Alt+K";

    public static void Start(Action onTriggered)
    {
        var thread = new Thread(() =>
        {
            static bool Down(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;
            var previous = false;
            while (true)
            {
                var current = Down(VkControl) && Down(VkMenu) && Down(VkK);
                if (current && !previous)
                {
                    // Never leave a pressed mouse button behind if the stop lands mid-gesture.
                    mouse_event(MouseLeftUp | MouseRightUp | MouseMiddleUp, 0, 0, 0, UIntPtr.Zero);
                    onTriggered();
                }
                previous = current;
                Thread.Sleep(40);
            }
        })
        { IsBackground = true, Name = "mcp-emergency-hotkey" };
        thread.Start();
    }
}
