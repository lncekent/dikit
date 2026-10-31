using System.Text.Json.Serialization;
using dikit.Infrastructure;

namespace dikit.Models;

public sealed class NoteTheme : ObservableObject
{
    private string _id = string.Empty;
    private string _name = string.Empty;
    private string _background = "#FFF9DB";
    private string _foreground = "#3B3A30";
    private string _accent = "#E8C547";
    private string _fontFamily = "Inter";
    private double _cornerRadius = 14;
    private double _opacity = 1.0;
    private bool _isBuiltIn;

    [JsonPropertyName("id")]
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [JsonPropertyName("background")]
    public string Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }

    [JsonPropertyName("foreground")]
    public string Foreground
    {
        get => _foreground;
        set => SetProperty(ref _foreground, value);
    }

    [JsonPropertyName("accent")]
    public string Accent
    {
        get => _accent;
        set => SetProperty(ref _accent, value);
    }

    [JsonPropertyName("fontFamily")]
    public string FontFamily
    {
        get => _fontFamily;
        set => SetProperty(ref _fontFamily, value);
    }

    [JsonPropertyName("cornerRadius")]
    public double CornerRadius
    {
        get => _cornerRadius;
        set => SetProperty(ref _cornerRadius, value);
    }

    [JsonPropertyName("opacity")]
    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    [JsonPropertyName("isBuiltIn")]
    public bool IsBuiltIn
    {
        get => _isBuiltIn;
        set => SetProperty(ref _isBuiltIn, value);
    }

    public NoteTheme Clone()
    {
        return new NoteTheme
        {
            Id = Id,
            Name = Name,
            Background = Background,
            Foreground = Foreground,
            Accent = Accent,
            FontFamily = FontFamily,
            CornerRadius = CornerRadius,
            Opacity = Opacity,
            IsBuiltIn = IsBuiltIn,
        };
    }

    public void CopyStyleFrom(NoteTheme other)
    {
        Name = other.Name;
        Background = other.Background;
        Foreground = other.Foreground;
        Accent = other.Accent;
        FontFamily = other.FontFamily;
        CornerRadius = other.CornerRadius;
        Opacity = other.Opacity;
    }

    public bool StyleEquals(NoteTheme other)
    {
        return string.Equals(Background, other.Background, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Foreground, other.Foreground, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Accent, other.Accent, StringComparison.OrdinalIgnoreCase)
            && string.Equals(FontFamily, other.FontFamily, StringComparison.Ordinal)
            && CornerRadius == other.CornerRadius
            && Opacity == other.Opacity;
    }
}
