using System.Runtime.InteropServices;

namespace SingleRunner;

/// <summary>
/// Global operator hotkeys for live runs, polled system-wide via <c>GetAsyncKeyState</c> on a background
/// thread (works even while EVE has focus, no message loop needed):
///   • Ctrl+Alt+K — emergency STOP: flips the sticky <see cref="Triggered"/>; the loop halts and releases the mouse.
///   • Ctrl+Alt+S — SNAPSHOT: rising-edge latched via <see cref="ConsumeSnapshotRequest"/>; the loop dumps a
///                  screenshot + the raw/parsed UI state to disk so the operator can later mark up, together
///                  with Claude, what the bot should have read (a missing window, an unparsed field, etc.).
/// </summary>
internal static class HotkeyStop
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VK_CONTROL = 0x11, VK_MENU = 0x12, VK_K = 0x4B, VK_S = 0x53;

    public const string Combo = "Ctrl+Alt+K";
    public const string SnapshotCombo = "Ctrl+Alt+S";

    public static volatile bool Triggered;
    private static int snapshotRequests;

    /// <summary>Returns true once per Ctrl+Alt+S press (rising edge), then clears the latch.</summary>
    public static bool ConsumeSnapshotRequest() => Interlocked.Exchange(ref snapshotRequests, 0) > 0;

    public static void Start()
    {
        var t = new Thread(() =>
        {
            var down = (int k) => (GetAsyncKeyState(k) & 0x8000) != 0;
            var sPrev = false;
            while (true)
            {
                var ctrlAlt = down(VK_CONTROL) && down(VK_MENU);
                if (ctrlAlt && down(VK_K))
                    Triggered = true;

                var sNow = ctrlAlt && down(VK_S);
                if (sNow && !sPrev)                       // rising edge: one request per press, not per poll
                    Interlocked.Increment(ref snapshotRequests);
                sPrev = sNow;

                Thread.Sleep(40);
            }
        })
        { IsBackground = true, Name = "hotkeys" };
        t.Start();
    }
}
