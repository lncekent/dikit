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

        double scaleX = DipsPerPixelX(primaryScreen);
        double scaleY = DipsPerPixelY(primaryScreen);
        System.Windows.Rect titleStrip = new System.Windows.Rect(
            window.Left, window.Top, window.Width, MinimumVisibleHeight);

        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
        {
            System.Windows.Rect workArea = ToDips(screen.WorkingArea, scaleX, scaleY);
            if (workArea.IntersectsWith(titleStrip))
            {
                return;
            }
        }

        System.Windows.Rect primary = ToDips(primaryScreen.WorkingArea, scaleX, scaleY);
        window.Left = primary.Left + Math.Max(0, (primary.Width - window.Width) / 2);
        window.Top = primary.Top + Math.Max(0, (primary.Height - window.Height) / 2);
    }

    private static System.Windows.Rect ToDips(System.Drawing.Rectangle area, double scaleX, double scaleY)
    {
        return new System.Windows.Rect(
            area.Left / scaleX,
            area.Top / scaleY,
            area.Width / scaleX,
            area.Height / scaleY);
    }

    private static double DipsPerPixelX(System.Windows.Forms.Screen screen)
    {
        double pixels = screen.Bounds.Width;
        double dips = SystemParameters.PrimaryScreenWidth;
        return pixels > 0 && dips > 0 ? pixels / dips : 1.0;
    }

    private static double DipsPerPixelY(System.Windows.Forms.Screen screen)
    {
        double pixels = screen.Bounds.Height;
        double dips = SystemParameters.PrimaryScreenHeight;
        return pixels > 0 && dips > 0 ? pixels / dips : 1.0;
    }
}
