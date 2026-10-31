using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using dikit.Infrastructure;
using dikit.Models;
using dikit.Services;

namespace dikit.ViewModels;

public sealed class NotesManagerViewModel : ObservableObject
{
    private readonly NoteStore _store;
    private readonly AppSettings _settings;
    private readonly AssetService _assets;
    private readonly ThemeService _themes;
    private readonly AppPaths _paths;
    private readonly StartupService _startup;
    private readonly WindowCoordinator _windows;
    private bool _shuttingDown;
    private string _searchText = string.Empty;

    public NotesManagerViewModel(
        NoteStore store,
        IEnumerable<Note> notes,
        IEnumerable<string> loadWarnings,
        AppSettings settings,
        AssetService assets,
        ThemeService themes,
        AppPaths paths,
        StartupService startup,
        WindowCoordinator windows)
    {
        _store = store;
        _settings = settings;
        _assets = assets;
        _themes = themes;
        _paths = paths;
        _startup = startup;
        _windows = windows;
        _windows.NoteWindowClosed += OnNoteWindowClosed;
        _store.BackupRetention = Math.Max(1, settings.BackupRetention);
        _settings.PropertyChanged += OnSettingsPropertyChanged;

        NotesView = CollectionViewSource.GetDefaultView(Notes);
        NotesView.Filter = item =>
            item is NoteViewModel vm && vm.MatchesSearch(_searchText);

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
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        QuitCommand = new RelayCommand(Quit);
    }

    public ObservableCollection<NoteViewModel> Notes { get; } = new ObservableCollection<NoteViewModel>();

    public ICollectionView NotesView { get; }

    public ObservableCollection<string> LoadWarnings { get; } = new ObservableCollection<string>();

    public AppSettings Settings => _settings;

    public string DataFolderPath => _paths.Root;

    public bool IsShuttingDown => _shuttingDown;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                NotesView.Refresh();
            }
        }
    }

    public bool LaunchAtSignIn
    {
        get => _settings.LaunchAtSignIn;
        set
        {
            if (_settings.LaunchAtSignIn == value)
            {
                return;
            }

            _settings.LaunchAtSignIn = value;
            _startup.SetEnabled(value);
            _store.SaveSettings(_settings);
            OnPropertyChanged();
        }
    }

    public int BackupRetention
    {
        get => _settings.BackupRetention;
        set
        {
            int clamped = Math.Clamp(value, 1, 30);
            if (_settings.BackupRetention == clamped)
            {
                return;
            }

            _settings.BackupRetention = clamped;
            _store.BackupRetention = clamped;
            _store.SaveSettings(_settings);
            OnPropertyChanged();
        }
    }

    public ThemeService Themes => _themes;

    public AssetService Assets => _assets;

    public RelayCommand NewNoteCommand { get; }

    public RelayCommand OpenNoteCommand { get; }

    public RelayCommand DuplicateNoteCommand { get; }

    public RelayCommand DeleteNoteCommand { get; }

    public RelayCommand HideAllCommand { get; }

    public RelayCommand RestoreAllCommand { get; }

    public RelayCommand OpenSettingsCommand { get; }

    public RelayCommand QuitCommand { get; }

    public void RestoreOpenNotes()
    {
        foreach (NoteViewModel vm in Notes.Where(vm => vm.Model.Window.IsOpen).ToList())
        {
            OpenNote(vm);
        }

        if (Notes.Count == 0)
        {
            CreateAndOpen(_store.CreateWelcomeNote());
        }
    }

    public void NewNote()
    {
        CreateAndOpen(_store.CreateNote());
    }

    public void OpenNote(NoteViewModel vm)
    {
        vm.Model.Window.IsOpen = true;
        _windows.ShowNote(vm);
        vm.SaveNow();
    }

    public void HideNote(NoteViewModel vm)
    {
        vm.Model.Window.IsOpen = false;
        vm.SaveNow();
        _windows.CloseNote(vm);
    }

    public void DuplicateNote(NoteViewModel vm)
    {
        Note copy;
        try
        {
            copy = _store.DuplicateNote(vm.Model);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorLog.Log($"Note '{vm.Id}' could not be duplicated: {ex.Message}");
            LoadWarnings.Add($"'{vm.Title}' could not be duplicated; its images could not be copied.");
            return;
        }

        CreateAndOpen(copy);
    }

    public void DeleteNote(NoteViewModel vm)
    {
        vm.CancelPendingSave();
        _windows.CloseNote(vm);
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

        _windows.CloseAllNotes();
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

        try
        {
            _store.SaveSettings(_settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorLog.Log($"Settings could not be saved on shutdown: {ex.Message}");
        }
    }

    public void ApplyGlobalTheme(string themeId)
    {
        _settings.GlobalThemeId = themeId;
        _store.SaveSettings(_settings);
    }

    public void SyncStartupOption()
    {
        _startup.SetEnabled(_settings.LaunchAtSignIn);
    }

    public void OpenSettings()
    {
        _windows.ShowSettings(this);
    }

    public void OpenThemeStudio(NoteViewModel? note)
    {
        _windows.ShowThemeStudio(new ThemeStudioViewModel(note, this), note);
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

    private void CreateAndOpen(Note note)
    {
        NoteViewModel vm = AddViewModel(note);
        _windows.PositionNewNote(vm, _settings);
        OpenNote(vm);
    }

    private NoteViewModel AddViewModel(Note note)
    {
        NoteViewModel vm = new NoteViewModel(note, _store, _settings, _assets, _themes);
        vm.HideRequested += HideNote;
        vm.DeleteRequested += DeleteNote;
        vm.DuplicateRequested += DuplicateNote;
        vm.ThemeStudioRequested += OpenThemeStudio;
        vm.SearchableContentChanged += OnSearchableContentChanged;
        Notes.Add(vm);
        return vm;
    }

    private void OnSearchableContentChanged(NoteViewModel vm)
    {
        if (_searchText.Length > 0)
        {
            NotesView.Refresh();
        }
    }

    private void OnNoteWindowClosed(NoteViewModel vm)
    {
        if (!_shuttingDown && vm.Model.Window.IsOpen)
        {
            vm.Model.Window.IsOpen = false;
            vm.SaveNow();
        }
    }
}
