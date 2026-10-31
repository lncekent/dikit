using System.Collections.ObjectModel;
using dikit.Infrastructure;
using dikit.Models;
using dikit.Services;
using dikit.Views;

namespace dikit.ViewModels;

public sealed class NotesManagerViewModel : ObservableObject
{
    private readonly NoteStore _store;
    private readonly AppSettings _settings;
    private readonly AssetService _assets;
    private readonly ThemeService _themes;
    private readonly Dictionary<NoteViewModel, NoteWindow> _windows =
        new Dictionary<NoteViewModel, NoteWindow>();
    private int _cascadeOffset;
    private bool _shuttingDown;

    public NotesManagerViewModel(
        NoteStore store,
        IEnumerable<Note> notes,
        IEnumerable<string> loadWarnings,
        AppSettings settings,
        AssetService assets,
        ThemeService themes)
    {
        _store = store;
        _settings = settings;
        _assets = assets;
        _themes = themes;
        _store.BackupRetention = Math.Max(1, settings.BackupRetention);
        _settings.PropertyChanged += OnSettingsPropertyChanged;

        foreach (string warning in loadWarnings)
        {
            LoadWarnings.Add(warning);
        }

        foreach (Note note in notes.OrderBy(n => n.CreatedUtc))
        {
            AddViewModel(note);
        }

        NewNoteCommand = new RelayCommand(NewNote);
        OpenNoteCommand = new RelayCommand(parameter =>
        {
            if (parameter is NoteViewModel vm)
            {
                OpenNote(vm);
            }
        });
        DuplicateNoteCommand = new RelayCommand(parameter =>
        {
            if (parameter is NoteViewModel vm)
            {
                DuplicateNote(vm);
            }
        });
        DeleteNoteCommand = new RelayCommand(parameter =>
        {
            if (parameter is NoteViewModel vm)
            {
                DeleteNote(vm);
            }
        });
        HideAllCommand = new RelayCommand(HideAll);
        RestoreAllCommand = new RelayCommand(RestoreAll);
        QuitCommand = new RelayCommand(Quit);
    }

    public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();

    public ObservableCollection<string> LoadWarnings { get; } = new ObservableCollection<string>();

    public AppSettings Settings => _settings;

    public ThemeService Themes => _themes;

    public AssetService Assets => _assets;

    public RelayCommand NewNoteCommand { get; }

    public RelayCommand OpenNoteCommand { get; }

    public RelayCommand DuplicateNoteCommand { get; }

    public RelayCommand DeleteNoteCommand { get; }

    public RelayCommand HideAllCommand { get; }

    public RelayCommand RestoreAllCommand { get; }

    public RelayCommand QuitCommand { get; }

    public void RestoreOpenNotes()
    {
        foreach (NoteViewModel vm in Notes.Where(vm => vm.Model.Window.IsOpen).ToList())
        {
            OpenNote(vm);
        }

        if (Notes.Count == 0)
        {
            NewNote();
        }
    }

    public void NewNote()
    {
        NoteViewModel vm = AddViewModel(_store.CreateNote());
        PositionNew(vm);
        OpenNote(vm);
    }

    public void OpenNote(NoteViewModel vm)
    {
        if (_windows.TryGetValue(vm, out NoteWindow? existing))
        {
            if (existing.WindowState == System.Windows.WindowState.Minimized)
            {
                existing.WindowState = System.Windows.WindowState.Normal;
            }

            existing.Activate();
            return;
        }

        vm.Model.Window.IsOpen = true;
        NoteWindow window = new NoteWindow(vm);
        _windows[vm] = window;
        window.Closed += OnWindowClosed;
        window.Show();
        vm.SaveNow();
    }

    public void HideNote(NoteViewModel vm)
    {
        vm.Model.Window.IsOpen = false;
        vm.SaveNow();
        if (_windows.Remove(vm, out NoteWindow? window))
        {
            window.Close();
        }
    }

    public void DuplicateNote(NoteViewModel vm)
    {
        Note copy = _store.DuplicateNote(vm.Model);
        NoteViewModel copyVm = AddViewModel(copy);
        OpenNote(copyVm);
    }

    public void DeleteNote(NoteViewModel vm)
    {
        if (_windows.Remove(vm, out NoteWindow? window))
        {
            window.Closed -= OnWindowClosed;
            window.Close();
        }

        _store.DeleteNote(vm.Id);
        Notes.Remove(vm);
    }

    public void HideAll()
    {
        foreach (NoteViewModel vm in Notes)
        {
            vm.Model.Window.IsOpen = false;
            vm.SaveNow();
        }

        foreach (NoteWindow window in _windows.Values.ToList())
        {
            window.Close();
        }

        _windows.Clear();
    }

    public void RestoreAll()
    {
        foreach (NoteViewModel vm in Notes)
        {
            OpenNote(vm);
        }
    }

    public void Quit()
    {
        Shutdown();
        System.Windows.Application.Current.Shutdown();
    }

    public void Shutdown()
    {
        if (_shuttingDown)
        {
            return;
        }

        _shuttingDown = true;
        foreach (NoteViewModel vm in Notes)
        {
            vm.SaveNow();
        }

        _store.SaveSettings(_settings);
    }

    public void ApplyGlobalTheme(string themeId)
    {
        _settings.GlobalThemeId = themeId;
        _store.SaveSettings(_settings);
    }

    public void OpenThemeStudio(NoteViewModel? note)
    {
        ThemeStudioViewModel studio = new ThemeStudioViewModel(note, this);
        ThemeStudioWindow window = new ThemeStudioWindow { DataContext = studio };
        if (note is not null && _windows.TryGetValue(note, out NoteWindow? host))
        {
            window.Owner = host;
        }
        else if (System.Windows.Application.Current.MainWindow is not null)
        {
            window.Owner = System.Windows.Application.Current.MainWindow;
        }

        window.Show();
    }

    private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppSettings.GlobalThemeId))
        {
            return;
        }

        foreach (NoteViewModel vm in Notes)
        {
            vm.RefreshTheme();
        }
    }

    private NoteViewModel AddViewModel(Note note)
    {
        NoteViewModel vm = new NoteViewModel(note, _store, _settings, _assets, _themes);
        vm.HideRequested += HideNote;
        vm.DeleteRequested += DeleteNote;
        vm.DuplicateRequested += DuplicateNote;
        vm.ThemeStudioRequested += OpenThemeStudio;
        Notes.Add(vm);
        return vm;
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (sender is NoteWindow { DataContext: NoteViewModel vm })
        {
            OnNoteWindowClosed(vm);
        }
    }

    private void OnNoteWindowClosed(NoteViewModel vm)
    {
        _windows.Remove(vm);
        if (!_shuttingDown && vm.Model.Window.IsOpen)
        {
            vm.Model.Window.IsOpen = false;
            vm.SaveNow();
        }
    }

    private void PositionNew(NoteViewModel vm)
    {
        System.Windows.Rect workArea = System.Windows.SystemParameters.WorkArea;
        int step = (_cascadeOffset % 8) * 28;
        vm.Model.Window.X = workArea.Left + 140 + step;
        vm.Model.Window.Y = workArea.Top + 120 + step;
        vm.Model.Window.Width = _settings.DefaultNoteWidth;
        vm.Model.Window.Height = _settings.DefaultNoteHeight;
        _cascadeOffset++;
    }
}
