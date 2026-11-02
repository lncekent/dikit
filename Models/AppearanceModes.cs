namespace dikit.Models;

public static class AppearanceModes
{
    public const string Light = "light";

    public const string Dark = "dark";

    public static string Normalize(string? mode)
    {
        return IsDark(mode) ? Dark : Light;
    }

    public static bool IsDark(string? mode)
    {
        return string.Equals(mode, Dark, StringComparison.OrdinalIgnoreCase);
    }
}
