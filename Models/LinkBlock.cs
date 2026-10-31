using System.Text.Json.Serialization;

namespace dikit.Models;

public sealed class LinkBlock : ContentBlock
{
    private string _text = string.Empty;
    private string _url = string.Empty;

    [JsonPropertyName("text")]
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    [JsonPropertyName("url")]
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }
}
