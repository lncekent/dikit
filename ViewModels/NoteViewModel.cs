using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media.Imaging;
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
    private readonly AssetService _assets;
    private readonly ThemeService _themes;
    private readonly Debouncer _saver = new Debouncer(AutosaveDelay);

    public NoteViewModel(
        Note note,
        NoteStore store,
        AppSettings settings,
        AssetService assets,
        ThemeService themes)
    {
        _note = note;
        _store = store;
        _settings = settings;
        _assets = assets;
        _themes = themes;

        CloseCommand = new RelayCommand(() => HideRequested?.Invoke(this));
        TogglePinCommand = new RelayCommand(() => IsTopMost = !IsTopMost);
        DuplicateCommand = new RelayCommand(() => DuplicateRequested?.Invoke(this));
        DeleteCommand = new RelayCommand(() => DeleteRequested?.Invoke(this));
        ThemeStudioCommand = new RelayCommand(() => ThemeStudioRequested?.Invoke(this));
        AddTextBlockCommand = new RelayCommand(() => _note.Blocks.Add(new TextBlock()));
        AddChecklistBlockCommand = new RelayCommand(() => _note.Blocks.Add(new ChecklistBlock()));
        AddLinkBlockCommand = new RelayCommand(() => _note.Blocks.Add(new LinkBlock()));
        RemoveBlockCommand = new RelayCommand(parameter =>
        {
            if (parameter is ContentBlock block)
            {
                RemoveBlock(block);
            }
        });
        OpenLinkCommand = new RelayCommand(parameter =>
        {
            if (parameter is LinkBlock link)
            {
                LinkLauncher.TryOpen(link.Url);
            }
        });

        _note.PropertyChanged += OnModelPropertyChanged;
        _note.Window.PropertyChanged += OnModelPropertyChanged;
        _note.Blocks.CollectionChanged += OnBlocksCollectionChanged;
        AttachThemeOverride(_note.ThemeOverride);
        foreach (ContentBlock block in _note.Blocks)
        {
            AttachBlock(block);
        }
    }

    public event Action<NoteViewModel>? SearchableContentChanged;

    public event Action<NoteViewModel>? HideRequested;

    public event Action<NoteViewModel>? DeleteRequested;

    public event Action<NoteViewModel>? DuplicateRequested;

    public event Action<NoteViewModel>? ThemeStudioRequested;

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
        _note.ThemeOverride ?? _themes.Resolve(_note.ThemeId ?? _settings.GlobalThemeId);

    public RelayCommand CloseCommand { get; }

    public RelayCommand TogglePinCommand { get; }

    public RelayCommand DuplicateCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public RelayCommand ThemeStudioCommand { get; }

    public RelayCommand AddTextBlockCommand { get; }

    public RelayCommand AddChecklistBlockCommand { get; }

    public RelayCommand AddLinkBlockCommand { get; }

    public RelayCommand RemoveBlockCommand { get; }

    public RelayCommand OpenLinkCommand { get; }

    public void AddLinkFromUrl(string url)
    {
        _note.Blocks.Add(new LinkBlock { Text = url, Url = url });
    }

    public bool AddImageFromBitmap(BitmapSource bitmap)
    {
        string? assetId = _assets.SaveBitmap(_note.Id, bitmap);
        if (assetId is null)
        {
            return false;
        }

        _note.Blocks.Add(new ImageBlock { AssetId = assetId });
        return true;
    }

    public bool AddImageFromFile(string sourcePath)
    {
        string? assetId = _assets.ImportFile(_note.Id, sourcePath);
        if (assetId is null)
        {
            return false;
        }

        _note.Blocks.Add(new ImageBlock { AssetId = assetId });
        return true;
    }

    public void RemoveBlock(ContentBlock block)
    {
        // Assets are kept: retained note backups may still reference them.
        _note.Blocks.Remove(block);
    }

    public void ApplyTheme(NoteTheme theme)
    {
        _note.ThemeOverride = theme.Clone();
    }

    public void ClearThemeOverride()
    {
        _note.ThemeOverride = null;
    }

    public void RefreshTheme()
    {
        OnPropertyChanged(nameof(Theme));
    }

    public void CancelPendingSave()
    {
        _saver.Cancel();
    }

    public void SaveNow()
    {
        _saver.Cancel();
        try
        {
            _store.SaveNote(_note);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ErrorLog.Log($"Note '{_note.Id}' could not be saved: {ex.Message}");
        }

        OnPropertyChanged(nameof(UpdatedText));
    }

    public bool MatchesSearch(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        string needle = query.Trim();
        if (_note.Title.Contains(needle, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (ContentBlock block in _note.Blocks)
        {
            switch (block)
            {
                case TextBlock text when Contains(text.Text, needle):
                case ChecklistBlock checklist when Contains(checklist.Text, needle):
                case ImageBlock image when Contains(image.Alt, needle):
                    return true;
                case LinkBlock link when Contains(link.Text, needle) || Contains(link.Url, needle):
                    return true;
            }
        }

        return false;

        static bool Contains(string? haystack, string needle)
        {
            return haystack?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }

    private void Touch()
    {
        _note.UpdatedUtc = DateTimeOffset.UtcNow;
        _saver.Schedule(SaveNow);
    }

    private void AttachBlock(ContentBlock block)
    {
        block.PropertyChanged += OnModelPropertyChanged;
        if (block is ImageBlock image)
        {
            image.ResolvedPath = _assets.ResolvePath(_note.Id, image.AssetId);
        }
    }

    private void AttachThemeOverride(NoteTheme? theme)
    {
        if (theme is not null)
        {
            theme.PropertyChanged += OnModelPropertyChanged;
        }
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ContentBlock)
        {
            SearchableContentChanged?.Invoke(this);
        }
        else if (sender is WindowPlacement)
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
                SearchableContentChanged?.Invoke(this);
            }
            else if (e.PropertyName == nameof(Note.ThemeId)
                || e.PropertyName == nameof(Note.ThemeOverride))
            {
                if (e.PropertyName == nameof(Note.ThemeOverride))
                {
                    AttachThemeOverride(_note.ThemeOverride);
                }

                OnPropertyChanged(nameof(Theme));
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
                AttachBlock(block);
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset && sender is ObservableCollection<ContentBlock> blocks)
        {
            foreach (ContentBlock block in blocks)
            {
                AttachBlock(block);
            }
        }

        SearchableContentChanged?.Invoke(this);
        Touch();
    }
}
