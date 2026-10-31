using System.Text.Json.Serialization;

namespace dikit.Models;

public sealed class TextBlock : ContentBlock
{
    private string _text = string.Empty;

    [JsonPropertyName("text")]
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
}
