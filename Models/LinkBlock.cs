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
        set
        {
            if (SetProperty(ref _url, value))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    [JsonIgnore]
    public bool IsValid
    {
        get
        {
            return Uri.TryCreate(_url, UriKind.Absolute, out Uri? uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
