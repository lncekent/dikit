using System.Text.Json.Serialization;

namespace dikit.Models;

public sealed class ImageBlock : ContentBlock
{
    private string _assetId = string.Empty;
    private string? _alt;

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
}
