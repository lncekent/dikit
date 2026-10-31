using System.Text.Json.Serialization;
using dikit.Infrastructure;

namespace dikit.Models;

public sealed class WindowPlacement : ObservableObject
{
    private double? _x;
    private double? _y;
    private double _width = 340;
    private double _height = 400;
    private bool _isOpen;
    private bool _alwaysOnTop;

    [JsonPropertyName("x")]
    public double? X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }

    [JsonPropertyName("y")]
    public double? Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }

    [JsonPropertyName("width")]
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    [JsonPropertyName("height")]
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    [JsonPropertyName("isOpen")]
    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }

    [JsonPropertyName("alwaysOnTop")]
    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set => SetProperty(ref _alwaysOnTop, value);
    }
}
