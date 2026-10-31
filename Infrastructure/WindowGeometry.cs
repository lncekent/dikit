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
}
