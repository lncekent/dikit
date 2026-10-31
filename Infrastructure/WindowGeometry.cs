using System.Windows;
using dikit.Models;

namespace dikit.Infrastructure;

public static class WindowGeometry
{
    private const double MinimumVisibleWidth = 100;
    private const double MinimumVisibleHeight = 48;

    public static void ApplyTo(Window window, WindowPlacement placement)
    {
        window.Width = placement.Width >= window.MinWidth ? placement.Width : 340;
        window.Height = placement.Height >= window.MinHeight ? placement.Height : 400;

        if (placement.X is double x && placement.Y is double y)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = Clamp(x, LeftBound(window.Width), RightBound(window.Width));
            window.Top = Clamp(y, TopBound(), BottomBound(window.Height));
            EnsureOnAttachedMonitor(window);
        }
        else
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    public static void Capture(Window window, WindowPlacement placement)
    {
        placement.X = window.Left;
        placement.Y = window.Top;
        placement.Width = window.Width;
        placement.Height = window.Height;
    }

    private static double LeftBound(double width)
    {
        return SystemParameters.VirtualScreenLeft - width + MinimumVisibleWidth;
    }

    private static double RightBound(double width)
    {
        return SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - MinimumVisibleWidth;
    }

    private static double TopBound()
    {
        return SystemParameters.VirtualScreenTop;
    }

    private static double BottomBound(double height)
    {
        return SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight
            - Math.Min(height, MinimumVisibleHeight);
    }

    private static double Clamp(double value, double min, double max)
    {
        if (max < min)
        {
            return min;
        }

        return Math.Max(min, Math.Min(max, value));
    }

    // The virtual-screen clamp can still leave a note in the gap between
    // mismatched monitors; verify the title strip intersects an attached
    // monitor's work area and otherwise center it on the primary screen.
    private static void EnsureOnAttachedMonitor(Window window)
    {
        System.Windows.Forms.Screen? primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
        if (primaryScreen is null)
        {
            return;
        }

        System.Windows.Rect titleStrip = new System.Windows.Rect(
            window.Left, window.Top, window.Width, MinimumVisibleHeight);

        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
        {
            if (ToDips(screen).IntersectsWith(titleStrip))
            {
                return;
            }
        }

        System.Windows.Rect primary = ToDips(primaryScreen);
        window.Left = primary.Left + Math.Max(0, (primary.Width - window.Width) / 2);
        window.Top = primary.Top + Math.Max(0, (primary.Height - window.Height) / 2);
    }

    private static System.Windows.Rect ToDips(System.Windows.Forms.Screen screen)
    {
        (double scaleX, double scaleY) = DipsPerPixel(screen);
        System.Drawing.Rectangle area = screen.WorkingArea;
        return new System.Windows.Rect(
            area.Left / scaleX,
            area.Top / scaleY,
            area.Width / scaleX,
            area.Height / scaleY);
    }

    // Each monitor has its own scale on mixed-DPI setups; ask the OS for the
    // monitor's effective DPI and fall back to the primary display's ratio.
    private static (double X, double Y) DipsPerPixel(System.Windows.Forms.Screen screen)
    {
        try
        {
            POINT center = new POINT(
                screen.Bounds.Left + screen.Bounds.Width / 2,
                screen.Bounds.Top + screen.Bounds.Height / 2);
            IntPtr monitor = MonitorFromPoint(center, MonitorDefaultToNearest);
            if (GetDpiForMonitor(monitor, MonitorDpiEffective, out uint dpiX, out uint dpiY) == 0
                && dpiX > 0 && dpiY > 0)
            {
                return (dpiX / 96.0, dpiY / 96.0);
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException
            or System.Runtime.InteropServices.ExternalException)
        {
        }

        System.Windows.Forms.Screen primary = System.Windows.Forms.Screen.PrimaryScreen ?? screen;
        double fallbackX = SystemParameters.PrimaryScreenWidth > 0
            ? primary.Bounds.Width / SystemParameters.PrimaryScreenWidth
            : 1.0;
        double fallbackY = SystemParameters.PrimaryScreenHeight > 0
            ? primary.Bounds.Height / SystemParameters.PrimaryScreenHeight
            : 1.0;
        return (fallbackX, fallbackY);
    }

    private const uint MonitorDefaultToNearest = 2;
    private const int MonitorDpiEffective = 0;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [System.Runtime.InteropServices.DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(
        IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    private readonly struct POINT
    {
        public readonly int X;
        public readonly int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
