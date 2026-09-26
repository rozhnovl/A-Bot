using System.Runtime.InteropServices;

namespace SingleRunner;

/// <summary>
/// Screenshot the EVE client window to a .bmp with no external dependencies. EVE renders through
/// DirectX, so a plain PrintWindow/BitBlt of the window DC usually comes back black — instead we BitBlt
/// the window's on-screen rectangle straight out of the desktop DC, which captures whatever is actually
/// visible there regardless of the render path. Trade-off: the window must be visible (not minimized or
/// fully covered) at the moment of capture — fine for a "snapshot what I'm looking at" debugging hotkey.
/// We write an uncompressed 32-bpp BMP by hand (no System.Drawing / no PNG encoder needed).
/// </summary>
internal static class WindowCapture
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT r);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int w, int h);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, int rop);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr h);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint start, uint lines, byte[] bits, ref BITMAPINFO bi, uint usage);

    private const int SRCCOPY = 0x00CC0020;

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth, biHeight;
        public ushort biPlanes, biBitCount;
        public uint biCompression, biSizeImage;
        public int biXPelsPerMeter, biYPelsPerMeter;
        public uint biClrUsed, biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER h;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] public uint[] cols;
    }

    public static bool Capture(IntPtr hwnd, string path, out string error)
    {
        error = "";
        if (hwnd == IntPtr.Zero) { error = "no window handle"; return false; }
        if (!GetWindowRect(hwnd, out var r)) { error = "GetWindowRect failed"; return false; }

        int w = r.Right - r.Left, h = r.Bottom - r.Top;
        if (w <= 0 || h <= 0) { error = $"bad window size {w}x{h} (minimized?)"; return false; }

        IntPtr screen = GetDC(IntPtr.Zero);
        IntPtr mem = CreateCompatibleDC(screen);
        IntPtr bmp = CreateCompatibleBitmap(screen, w, h);
        try
        {
            var old = SelectObject(mem, bmp);
            if (!BitBlt(mem, 0, 0, w, h, screen, r.Left, r.Top, SRCCOPY)) { error = "BitBlt failed"; SelectObject(mem, old); return false; }
            SelectObject(mem, old);

            // Positive biHeight => bottom-up DIB, which is exactly BMP's on-disk row order.
            var bi = new BITMAPINFO
            {
                h = new BITMAPINFOHEADER
                {
                    biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                    biWidth = w, biHeight = h, biPlanes = 1, biBitCount = 32, biCompression = 0,
                },
            };
            int stride = w * 4;
            var bits = new byte[stride * h];
            if (GetDIBits(mem, bmp, 0, (uint)h, bits, ref bi, 0) == 0) { error = "GetDIBits failed"; return false; }

            WriteBmp(path, w, h, bits, stride);
            return true;
        }
        catch (Exception e) { error = e.Message; return false; }
        finally
        {
            DeleteObject(bmp);
            DeleteDC(mem);
            ReleaseDC(IntPtr.Zero, screen);
        }
    }

    private static void WriteBmp(string path, int w, int h, byte[] bits, int stride)
    {
        int imgSize = stride * h;
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        // BITMAPFILEHEADER (14 bytes)
        bw.Write((byte)'B'); bw.Write((byte)'M');
        bw.Write(54 + imgSize);   // file size
        bw.Write(0);              // reserved
        bw.Write(54);             // pixel data offset
        // BITMAPINFOHEADER (40 bytes)
        bw.Write(40);
        bw.Write(w);
        bw.Write(h);
        bw.Write((short)1);       // planes
        bw.Write((short)32);      // bpp
        bw.Write(0);              // BI_RGB
        bw.Write(imgSize);
        bw.Write(0); bw.Write(0); // ppm x/y
        bw.Write(0); bw.Write(0); // clr used / important
        bw.Write(bits, 0, imgSize);
    }
}
