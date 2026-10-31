using System.Text.Json.Serialization;
using dikit.Infrastructure;

namespace dikit.Models;

public sealed class AppSettings : ObservableObject
{
    private int _schemaVersion = 1;
    private string _globalThemeId = ThemeCatalog.DefaultThemeId;
    private bool _launchAtSignIn;
    private int _backupRetention = 5;
    private double _defaultNoteWidth = 340;
    private double _defaultNoteHeight = 400;

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion
    {
        get => _schemaVersion;
        set => SetProperty(ref _schemaVersion, value);
    }

    [JsonPropertyName("globalThemeId")]
    public string GlobalThemeId
    {
        get => _globalThemeId;
        set => SetProperty(ref _globalThemeId, value);
    }

    [JsonPropertyName("launchAtSignIn")]
    public bool LaunchAtSignIn
    {
        get => _launchAtSignIn;
        set => SetProperty(ref _launchAtSignIn, value);
    }

    [JsonPropertyName("backupRetention")]
    public int BackupRetention
    {
        get => _backupRetention;
        set => SetProperty(ref _backupRetention, value);
    }

    [JsonPropertyName("defaultNoteWidth")]
    public double DefaultNoteWidth
    {
        get => _defaultNoteWidth;
        set => SetProperty(ref _defaultNoteWidth, value);
    }

    [JsonPropertyName("defaultNoteHeight")]
    public double DefaultNoteHeight
    {
        get => _defaultNoteHeight;
        set => SetProperty(ref _defaultNoteHeight, value);
    }
}
