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

    public static NoteTheme Resolve(string? themeId)
    {
        foreach (NoteTheme preset in Presets)
        {
            if (preset.Id == themeId)
            {
                return preset;
            }
        }

        return Presets[0];
    }
}
