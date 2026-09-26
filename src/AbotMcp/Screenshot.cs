using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AbotMcp;

/// <summary>
/// Screenshot the client window from the desktop (EVE renders through DirectX, so a window-DC copy
/// comes back black). The window must be visible on screen at capture time.
/// </summary>
internal static class Screenshot
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT r);

    public sealed record Result(string Path, byte[] Png, int Width, int Height, double Scale, int OriginX, int OriginY,
        int WindowWidth, int WindowHeight);

    public static Result Capture(IntPtr hwnd, int maxWidth, string dir)
    {
        if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out var r))
            throw new InvalidOperationException("window rectangle unavailable");
        var w = r.Right - r.Left;
        var h = r.Bottom - r.Top;
        if (w <= 0 || h <= 0) throw new InvalidOperationException("window has no size (minimized?)");

        using var full = new Bitmap(w, h);
        using (var g = Graphics.FromImage(full))
            g.CopyFromScreen(r.Left, r.Top, 0, 0, new Size(w, h));

        var scale = 1.0;
        Bitmap output = full;
        if (maxWidth > 0 && w > maxWidth)
        {
            scale = (double)maxWidth / w;
            output = new Bitmap(full, new Size(maxWidth, Math.Max(1, (int)Math.Round(h * scale))));
        }

        try
        {
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"shot-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
            using var ms = new MemoryStream();
            output.Save(ms, ImageFormat.Png);
            var bytes = ms.ToArray();
            File.WriteAllBytes(path, bytes);
            return new Result(path, bytes, output.Width, output.Height, scale, r.Left, r.Top, w, h);
        }
        finally
        {
            if (!ReferenceEquals(output, full)) output.Dispose();
        }
    }
}
