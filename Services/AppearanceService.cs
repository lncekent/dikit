using System.Windows;
using System.Windows.Media;
using dikit.Models;

namespace dikit.Services;

public sealed class AppearanceService
{
    public void Apply(string? mode)
    {
        bool dark = AppearanceModes.IsDark(mode);
        ResourceDictionary resources = System.Windows.Application.Current.Resources;
        resources["AppBackgroundBrush"] = Brush(dark ? "#17181C" : "#FFFDF5");
        resources["AppSurfaceBrush"] = Brush(dark ? "#24262C" : "#FFFFFF");
        resources["AppSurfaceAltBrush"] = Brush(dark ? "#2E3038" : "#F4F2EA");
        resources["AppTextBrush"] = Brush(dark ? "#F2F2F7" : "#3B3A30");
        resources["AppMutedTextBrush"] = Brush(dark ? "#A7ACB8" : "#8A8668");
        resources["AppBorderBrush"] = Brush(dark ? "#33FFFFFF" : "#1F000000");
        resources["AppHoverBrush"] = Brush(dark ? "#22FFFFFF" : "#24000000");
        resources["AppCheckedBrush"] = Brush(dark ? "#33FFFFFF" : "#3A000000");
        resources["AppDangerBrush"] = Brush(dark ? "#FF6B61" : "#A33B3B");
        resources["RecoveryBackgroundBrush"] = Brush(dark ? "#3B321B" : "#FFF3CD");
        resources["RecoveryBorderBrush"] = Brush(dark ? "#8A6D1F" : "#E8C547");
        resources["RecoveryTextBrush"] = Brush(dark ? "#F2D786" : "#6B5B1E");
    }

    private static SolidColorBrush Brush(string hex)
    {
        return new SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex));
    }
}
