using System.Diagnostics;
using BotEngine.WinApi;
using BotEngine.Windows;

namespace AbotEngine;

/// <summary>
/// Resolves the interactive top-level window for an EVE process.
///
/// EVE can report MainWindowHandle == 0 even while its UI is fully readable from
/// memory (for example during a client transition or when the render window was
/// recreated). Input must use a current HWND, so do not cache Process.MainWindowHandle
/// as the only source of truth.
/// </summary>
internal static class WindowHandleResolver
{
    private const uint WsVisible = 0x10000000;

    public static IntPtr Resolve(Process process, Action<string>? trace = null)
    {
        try
        {
            process.Refresh();
            var main = process.MainWindowHandle;
            if (IsUsable(main))
                return main;

            var handles = User32.ListeWindowTopLevelHandle().ToList();

            // Some EVE launchers expose the actual client window only through the
            // client threads, while Process.MainWindowHandle remains zero. Include
            // those handles as a fallback; Candidate.For still verifies ownership.
            try
            {
                handles.AddRange(process.Threads.Cast<ProcessThread>()
                    .SelectMany(thread => User32.ListeThreadWindowHandle((uint)thread.Id)));
            }
            catch (Exception e)
            {
                trace?.Invoke($"pid {process.Id}: thread-window lookup failed: {e.Message}");
            }

            var candidates = handles
                .Distinct()
                .Select(handle => Candidate.For(handle, process.Id))
                .Where(candidate => candidate is not null)
                .Select(candidate => candidate!)
                .OrderByDescending(candidate => candidate.Visible)
                .ThenByDescending(candidate => candidate.Area)
                .ToArray();

            var resolved = candidates.FirstOrDefault()?.Handle ?? IntPtr.Zero;
            if (resolved != IntPtr.Zero)
            {
                trace?.Invoke(
                    $"pid {process.Id}: resolved input HWND 0x{resolved.ToInt64():X} " +
                    $"(fallback; {candidates.Length} process window(s))");
            }
            else
            {
                trace?.Invoke($"pid {process.Id}: no usable top-level input window found");
            }

            return resolved;
        }
        catch (Exception e)
        {
            trace?.Invoke($"pid {process.Id}: input window lookup failed: {e.Message}");
            return IntPtr.Zero;
        }
    }

    private static bool IsUsable(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return false;

        var info = new WINDOWINFO(true);
        if (!User32.GetWindowInfo(handle, ref info))
            return false;

        return info.rcWindow.Width > 0 && info.rcWindow.Height > 0;
    }

    private sealed record Candidate(IntPtr Handle, bool Visible, long Area)
    {
        public static Candidate? For(IntPtr handle, int processId)
        {
            if (!IsUsable(handle))
                return null;

            User32.GetWindowThreadProcessId(handle, out var ownerPid);
            if (ownerPid != processId)
                return null;

            var info = new WINDOWINFO(true);
            if (!User32.GetWindowInfo(handle, ref info))
                return null;

            var width = Math.Max(0, info.rcWindow.Width);
            var height = Math.Max(0, info.rcWindow.Height);
            return new Candidate(handle, (info.dwStyle & WsVisible) != 0, (long)width * height);
        }
    }
}
