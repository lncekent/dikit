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
        "#FFF9DB", "#FDEFF3", "#F3EFFF", "#F1F7E8", "#E8F4FD", "#FFF4E3",
        "#26282E", "#3B3A30", "#6B6B66", "#FFFFFF",
        "#E8C547", "#E08DAC", "#A78BDB", "#8BAE62", "#6E8ACB", "#D9825F",
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
