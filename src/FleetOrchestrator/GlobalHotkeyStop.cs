using System.Runtime.InteropServices;

namespace FleetOrchestrator;

/// <summary>
/// System-wide sticky emergency stop. GetAsyncKeyState does not depend on which window owns focus,
/// so the operator can stop the coordinator while any EVE client is active.
/// </summary>
internal static class GlobalHotkeyStop
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;
    private const int VkK = 0x4B;
    private const uint MouseLeftUp = 0x0004;
    private const uint MouseRightUp = 0x0010;
    private const uint MouseMiddleUp = 0x0040;

    public const string Combo = "Ctrl+Alt+K";
    public static volatile bool Triggered;

    public static void Start(Action onTriggered)
    {
        var thread = new Thread(() =>
        {
            static bool Down(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;
            var previous = false;
            while (!Triggered)
            {
                var current = Down(VkControl) && Down(VkMenu) && Down(VkK);
                if (current && !previous)
                {
                    Triggered = true;
                    // Do not leave a pressed mouse button behind if the stop lands during a gesture.
                    mouse_event(MouseLeftUp | MouseRightUp | MouseMiddleUp, 0, 0, 0, UIntPtr.Zero);
                    onTriggered();
                    break;
                }
                previous = current;
                Thread.Sleep(40);
            }
        })
        { IsBackground = true, Name = "fleet-emergency-hotkey" };
        thread.Start();
    }
}
