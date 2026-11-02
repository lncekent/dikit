namespace dikit.Models;

public static class ThemeCatalog
{
    public const string DefaultThemeId = "classic";

    public static readonly IReadOnlyList<NoteTheme> Presets = new List<NoteTheme>
    {
        new NoteTheme
        {
            Id = "classic",
            Name = "Classic Light",
            Background = "#FFFFFFFF",
            Foreground = "#1C1C1E",
            Accent = "#0A84FF",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
        new NoteTheme
        {
            Id = "classic-dark",
            Name = "Classic Dark",
            Background = "#FF1C1C1E",
            Foreground = "#F2F2F7",
            Accent = "#0A84FF",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
    };

    private static readonly IReadOnlyList<NoteTheme> LegacyPresets = new List<NoteTheme>
    {
        new NoteTheme
        {
            Id = "classic-yellow",
            Name = "Classic Yellow",
            Background = "#FFF9DB",
            Foreground = "#3B3A30",
            Accent = "#E8C547",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
        new NoteTheme
        {
            Id = "lavender",
            Name = "Lavender",
            Background = "#F3EFFF",
            Foreground = "#3D3660",
            Accent = "#A78BDB",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
        new NoteTheme
        {
            Id = "matcha",
            Name = "Matcha",
            Background = "#F1F7E8",
            Foreground = "#2F4226",
            Accent = "#8BAE62",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
        new NoteTheme
        {
            Id = "sakura",
            Name = "Sakura",
            Background = "#FDEFF3",
            Foreground = "#4E3040",
            Accent = "#E08DAC",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
        new NoteTheme
        {
            Id = "midnight",
            Name = "Midnight",
            Background = "#26282E",
            Foreground = "#EAEAEA",
            Accent = "#6E8ACB",
            FontFamily = "Inter",
            CornerRadius = 14,
            Opacity = 1.0,
            IsBuiltIn = true,
        },
    };

    public static NoteTheme Resolve(string? themeId)
    {
        return Find(Presets, themeId) ?? Find(LegacyPresets, themeId) ?? Presets[0];
    }

    private static NoteTheme? Find(IReadOnlyList<NoteTheme> themes, string? themeId)
    {
        foreach (NoteTheme theme in themes)
        {
            if (theme.Id == themeId)
            {
                return theme;
            }
        }

        return null;
    }
}
