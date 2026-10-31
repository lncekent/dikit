using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using dikit.Infrastructure;
using dikit.Models;
using dikit.Services;

namespace dikit.ViewModels;

public sealed class NoteViewModel : ObservableObject
{
    private static readonly TimeSpan AutosaveDelay = TimeSpan.FromMilliseconds(400);

    private readonly Note _note;
    private readonly NoteStore _store;
    private readonly AppSettings _settings;
    private readonly Debouncer _saver = new Debouncer(AutosaveDelay);

    public NoteViewModel(Note note, NoteStore store, AppSettings settings)
    {
        _note = note;
        _store = store;
        _settings = settings;

        CloseCommand = new RelayCommand(() => HideRequested?.Invoke(this));
        TogglePinCommand = new RelayCommand(() => IsTopMost = !IsTopMost);
        DuplicateCommand = new RelayCommand(() => DuplicateRequested?.Invoke(this));
        DeleteCommand = new RelayCommand(() => DeleteRequested?.Invoke(this));

        _note.PropertyChanged += OnModelPropertyChanged;
        _note.Window.PropertyChanged += OnModelPropertyChanged;
        _note.Blocks.CollectionChanged += OnBlocksCollectionChanged;
        foreach (ContentBlock block in _note.Blocks)
        {
            block.PropertyChanged += OnModelPropertyChanged;
        }
    }

    public event Action<NoteViewModel>? HideRequested;

    public event Action<NoteViewModel>? DeleteRequested;

    public event Action<NoteViewModel>? DuplicateRequested;

    public Note Model => _note;

    public string Id => _note.Id;

    public string Title
    {
        get => _note.Title;
        set => _note.Title = value;
    }

    public ObservableCollection<ContentBlock> Blocks => _note.Blocks;

    public bool IsTopMost
    {
        get => _note.Window.AlwaysOnTop;
        set => _note.Window.AlwaysOnTop = value;
    }

    public bool IsOpen => _note.Window.IsOpen;

    public string StatusText => IsOpen ? "Open" : "Closed";

    public string UpdatedText => _note.UpdatedUtc.ToLocalTime().ToString("g");

    public NoteTheme Theme =>
        _note.ThemeOverride ?? ThemeCatalog.Resolve(_note.ThemeId ?? _settings.GlobalThemeId);

    public RelayCommand CloseCommand { get; }

    public RelayCommand TogglePinCommand { get; }

    public RelayCommand DuplicateCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public void SaveNow()
    {
        _saver.Cancel();
        _store.SaveNote(_note);
        OnPropertyChanged(nameof(UpdatedText));
    }

    private void Touch()
    {
        _note.UpdatedUtc = DateTimeOffset.UtcNow;
        _saver.Schedule(SaveNow);
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is WindowPlacement)
        {
            if (e.PropertyName == nameof(WindowPlacement.AlwaysOnTop))
            {
                OnPropertyChanged(nameof(IsTopMost));
            }
            else if (e.PropertyName == nameof(WindowPlacement.IsOpen))
            {
                OnPropertyChanged(nameof(IsOpen));
                OnPropertyChanged(nameof(StatusText));
            }
        }
        else if (sender is Note)
        {
            if (e.PropertyName == nameof(Note.UpdatedUtc))
            {
                return;
            }

            if (e.PropertyName == nameof(Note.Title))
            {
                OnPropertyChanged(nameof(Title));
            }
        }

        Touch();
    }

    private void OnBlocksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (ContentBlock block in e.OldItems)
            {
                block.PropertyChanged -= OnModelPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (ContentBlock block in e.NewItems)
            {
                block.PropertyChanged += OnModelPropertyChanged;
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset && sender is ObservableCollection<ContentBlock> blocks)
        {
            foreach (ContentBlock block in blocks)
            {
                block.PropertyChanged += OnModelPropertyChanged;
            }
        }

        Touch();
    }
}
