using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using dikit.Infrastructure;
using dikit.Models;

namespace dikit.Services;

public sealed class ThemeService
{
    private static readonly Regex SafeThemeId =
        new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);

    private readonly AppPaths _paths;
    private readonly List<NoteTheme> _userThemes = new List<NoteTheme>();

    public ThemeService(AppPaths paths)
    {
        _paths = paths;
        LoadUserThemes();
    }

    public IReadOnlyList<NoteTheme> All
    {
        get
        {
            List<NoteTheme> all = new List<NoteTheme>();
            all.AddRange(ThemeCatalog.Presets);
            all.AddRange(_userThemes);
            return all;
        }
    }

    public NoteTheme Resolve(string? themeId)
    {
        foreach (NoteTheme theme in _userThemes)
        {
            if (theme.Id == themeId)
            {
                return theme;
            }
        }

        return ThemeCatalog.Resolve(themeId);
    }

    public NoteTheme? Find(string? themeId)
    {
        foreach (NoteTheme theme in All)
        {
            if (theme.Id == themeId)
            {
                return theme;
            }
        }

        return null;
    }

    public NoteTheme SaveUserTheme(NoteTheme theme)
    {
        NoteTheme copy = theme.Clone();
        copy.Id = NewUserThemeId();
        copy.IsBuiltIn = false;
        if (string.IsNullOrWhiteSpace(copy.Name))
        {
            copy.Name = "Custom theme";
        }

        Directory.CreateDirectory(_paths.ThemesDirectory);
        string file = Path.Combine(_paths.ThemesDirectory, copy.Id + ".json");
        AtomicFileWriter.WriteAllText(file, JsonSerializer.Serialize(copy, JsonOptions.Default));
        _userThemes.Add(copy);
        return copy;
    }

    private void LoadUserThemes()
    {
        if (!Directory.Exists(_paths.ThemesDirectory))
        {
            return;
        }

        foreach (string file in Directory.EnumerateFiles(_paths.ThemesDirectory, "*.json"))
        {
            string id = Path.GetFileNameWithoutExtension(file);
            if (!SafeThemeId.IsMatch(id))
            {
                continue;
            }

            try
            {
                NoteTheme? theme = JsonSerializer.Deserialize<NoteTheme>(
                    File.ReadAllText(file), JsonOptions.Default);
                if (theme is not null)
                {
                    theme.Id = id;
                    theme.IsBuiltIn = false;
                    _userThemes.Add(theme);
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
            }
        }
    }

    private static string NewUserThemeId()
    {
        return "user-" + Guid.NewGuid().ToString("N")[..8];
    }
}
