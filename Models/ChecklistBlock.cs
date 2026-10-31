using System.Text.Json.Serialization;

namespace dikit.Models;

public sealed class ChecklistBlock : ContentBlock
{
    private string _text = string.Empty;
    private bool _isChecked;

    [JsonPropertyName("text")]
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    [JsonPropertyName("checked")]
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}
