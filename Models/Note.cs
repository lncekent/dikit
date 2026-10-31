using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using dikit.Infrastructure;

namespace dikit.Models;

public sealed class Note : ObservableObject
{
    public const int CurrentSchemaVersion = 1;

    private int _schemaVersion = CurrentSchemaVersion;
    private string _id = string.Empty;
    private string _title = string.Empty;
    private string? _themeId;
    private NoteTheme? _themeOverride;
    private ObservableCollection<ContentBlock> _blocks = new ObservableCollection<ContentBlock>();
    private WindowPlacement _window = new WindowPlacement();
    private DateTimeOffset _createdUtc = DateTimeOffset.UtcNow;
    private DateTimeOffset _updatedUtc = DateTimeOffset.UtcNow;

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion
    {
        get => _schemaVersion;
        set => SetProperty(ref _schemaVersion, value);
    }

    [JsonPropertyName("id")]
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value ?? string.Empty);
    }

    [JsonPropertyName("title")]
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value ?? string.Empty);
    }

    [JsonPropertyName("themeId")]
    public string? ThemeId
    {
        get => _themeId;
        set => SetProperty(ref _themeId, value);
    }

    [JsonPropertyName("themeOverride")]
    public NoteTheme? ThemeOverride
    {
        get => _themeOverride;
        set => SetProperty(ref _themeOverride, value);
    }

    [JsonPropertyName("blocks")]
    public ObservableCollection<ContentBlock> Blocks
    {
        get => _blocks;
        set => SetProperty(ref _blocks, value ?? new ObservableCollection<ContentBlock>());
    }

    [JsonPropertyName("window")]
    public WindowPlacement Window
    {
        get => _window;
        set => SetProperty(ref _window, value ?? new WindowPlacement());
    }

    [JsonPropertyName("createdUtc")]
    public DateTimeOffset CreatedUtc
    {
        get => _createdUtc;
        set => SetProperty(ref _createdUtc, value);
    }

    [JsonPropertyName("updatedUtc")]
    public DateTimeOffset UpdatedUtc
    {
        get => _updatedUtc;
        set => SetProperty(ref _updatedUtc, value);
    }
}
