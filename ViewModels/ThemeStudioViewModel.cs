using System.Collections.ObjectModel;
using dikit.Infrastructure;
using dikit.Models;
using dikit.Services;

namespace dikit.ViewModels;

public sealed class ThemeStudioViewModel : ObservableObject
{
    private readonly NoteViewModel? _note;
    private readonly NotesManagerViewModel _manager;
    private readonly ThemeService _themes;
    private static readonly System.Text.RegularExpressions.Regex HexColor =
        new System.Text.RegularExpressions.Regex(
            "^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    private string _editingColorTarget = "Background";

    public ThemeStudioViewModel(NoteViewModel? note, NotesManagerViewModel manager)
    {
        _note = note;
        _manager = manager;
        _themes = manager.Themes;

        Working = (note is not null
                ? (note.Model.ThemeOverride ?? manager.Themes.Resolve(note.Model.ThemeId ?? manager.Settings.GlobalThemeId))
                : manager.Themes.Resolve(manager.Settings.GlobalThemeId))
            .Clone();

        foreach (NoteTheme theme in _themes.All)
        {
            Presets.Add(theme);
        }

        SelectPresetCommand = new RelayCommand(parameter =>
        {
            if (parameter is NoteTheme theme)
            {
                Working.CopyStyleFrom(theme);
                Working.Id = theme.Id;
                OnPropertyChanged(nameof(EditingColorHex));
            }
        });
        PickColorCommand = new RelayCommand(parameter =>
        {
            if (parameter is string hex)
            {
                ApplyColor(hex);
            }
        });
        ApplyToNoteCommand = new RelayCommand(
            _ => _note?.ApplyTheme(Working),
            _ => _note is not null);
        ApplyToAllCommand = new RelayCommand(ApplyGlobally);
        SaveAsPresetCommand = new RelayCommand(SaveAsPreset);
        DeletePresetCommand = new RelayCommand(parameter =>
        {
            if (parameter is NoteTheme theme)
            {
                DeletePreset(theme);
            }
        }, parameter => parameter is NoteTheme { IsBuiltIn: false });
        ResetToInheritedCommand = new RelayCommand(
            _ => ResetToInherited(),
            _ => _note is not null);
    }

    public ObservableCollection<NoteTheme> Presets { get; } = new ObservableCollection<NoteTheme>();

    public NoteTheme Working { get; }

    public bool IsNoteScope => _note is not null;

    public string Title => IsNoteScope ? "Theme — this note" : "Theme — all notes";

    public string EditingColorTarget
    {
        get => _editingColorTarget;
        set
        {
            if (SetProperty(ref _editingColorTarget, value))
            {
                OnPropertyChanged(nameof(EditingColorHex));
            }
        }
    }

    public string EditingColorHex
    {
        get => EditingColorTarget switch
        {
            "Foreground" => Working.Foreground,
            "Accent" => Working.Accent,
            _ => Working.Background,
        };
        set => ApplyColor(value);
    }

    public IReadOnlyList<string> Palette { get; } = new List<string>
    {
        "#FFFFFFF2", "#F2F2F7", "#E5E5EA", "#E8F4FD", "#FDEFF3", "#F1F7E8",
        "#1C1C1EF2", "#2C2C2E", "#3A3A3C", "#FFFFFF",
        "#0A84FF", "#FF375F", "#BF5AF2", "#30D158", "#64D2FF", "#98989D",
    };

    public IReadOnlyList<string> FontNames { get; } = new List<string>
    {
        "Inter", "Segoe UI", "Segoe UI Variable Text", "Calibri", "Arial",
        "Georgia", "Cambria", "Consolas", "Courier New", "Times New Roman",
    };

    public RelayCommand SelectPresetCommand { get; }

    public RelayCommand PickColorCommand { get; }

    public RelayCommand ApplyToNoteCommand { get; }

    public RelayCommand ApplyToAllCommand { get; }

    public RelayCommand SaveAsPresetCommand { get; }

    public RelayCommand DeletePresetCommand { get; }

    public RelayCommand ResetToInheritedCommand { get; }

    private void ApplyColor(string hex)
    {
        if (!HexColor.IsMatch(hex.Trim()))
        {
            return;
        }

        switch (EditingColorTarget)
        {
            case "Foreground":
                Working.Foreground = hex;
                break;
            case "Accent":
                Working.Accent = hex;
                break;
            default:
                Working.Background = hex;
                break;
        }

        OnPropertyChanged(nameof(EditingColorHex));
    }

    private void ApplyGlobally()
    {
        NoteTheme? existing = _themes.Find(Working.Id);
        string themeId;
        if (existing is not null && existing.StyleEquals(Working))
        {
            themeId = existing.Id;
        }
        else
        {
            themeId = _themes.SaveUserTheme(Working).Id;
            RefreshPresets();
        }

        _manager.ApplyGlobalTheme(themeId);
    }

    private void SaveAsPreset()
    {
        _themes.SaveUserTheme(Working);
        RefreshPresets();
    }

    private void DeletePreset(NoteTheme theme)
    {
        if (!_manager.DeleteUserTheme(theme))
        {
            RefreshPresets();
            return;
        }

        RefreshPresets();
        if (Working.Id == theme.Id)
        {
            NoteTheme classic = _themes.Resolve(ThemeCatalog.DefaultThemeId);
            Working.CopyStyleFrom(classic);
            Working.Id = classic.Id;
            OnPropertyChanged(nameof(EditingColorHex));
        }
    }

    private void ResetToInherited()
    {
        _note?.ClearThemeOverride();
        Working.CopyStyleFrom(_themes.Resolve(_note?.Model.ThemeId ?? _manager.Settings.GlobalThemeId));
        OnPropertyChanged(nameof(EditingColorHex));
    }

    private void RefreshPresets()
    {
        Presets.Clear();
        foreach (NoteTheme theme in _themes.All)
        {
            Presets.Add(theme);
        }
    }
}
