using System.Text.Json.Serialization;

namespace dikit.Models;

public sealed class ImageBlock : ContentBlock
{
    private string _assetId = string.Empty;
    private string? _alt;
    private string? _resolvedPath;

    [JsonPropertyName("assetId")]
    public string AssetId
    {
        get => _assetId;
        set => SetProperty(ref _assetId, value);
    }

    [JsonPropertyName("alt")]
    public string? Alt
    {
        get => _alt;
        set => SetProperty(ref _alt, value);
    }

    [JsonIgnore]
    public string? ResolvedPath
    {
        get => _resolvedPath;
        set => SetProperty(ref _resolvedPath, value);
    }
}
