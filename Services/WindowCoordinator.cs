using dikit.Models;
using dikit.ViewModels;
using dikit.Views;

namespace dikit.Services;

// Owns note, settings, and theme-studio window lifecycles so ViewModels hold
// state and intent only; window closes are reported back via NoteWindowClosed.
public sealed class WindowCoordinator
{
    private readonly Dictionary<NoteViewModel, NoteWindow> _windows =
        new Dictionary<NoteViewModel, NoteWindow>();
    private SettingsWindow? _settingsWindow;
    private int _cascadeOffset;

    public event Action<NoteViewModel>? NoteWindowClosed;

    public void ShowNote(NoteViewModel note)
    {
        if (_windows.TryGetValue(note, out NoteWindow? existing))
        {
            if (existing.WindowState == System.Windows.WindowState.Minimized)
            {
                existing.WindowState = System.Windows.WindowState.Normal;
            }

            existing.Activate();
            return;
        }

        NoteWindow window = new NoteWindow(note);
        _windows[note] = window;
        window.Closed += OnNoteWindowClosed;
        window.Show();
    }

    public void CloseNote(NoteViewModel note)
    {
        if (_windows.Remove(note, out NoteWindow? window))
        {
            window.Close();
        }
    }

    public void CloseAllNotes()
    {
        foreach (NoteWindow window in _windows.Values.ToList())
        {
            window.Close();
        }

        _windows.Clear();
    }

    public void PositionNewNote(NoteViewModel note, AppSettings settings)
    {
        System.Windows.Rect workArea = System.Windows.SystemParameters.WorkArea;
        int step = (_cascadeOffset % 8) * 28;
        note.Model.Window.X = workArea.Left + 140 + step;
        note.Model.Window.Y = workArea.Top + 120 + step;
        note.Model.Window.Width = settings.DefaultNoteWidth;
        note.Model.Window.Height = settings.DefaultNoteHeight;
        _cascadeOffset++;
    }

    public void ShowSettings(object dataContext)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow { DataContext = dataContext };
        if (System.Windows.Application.Current.MainWindow is { } owner && owner.IsVisible)
        {
            _settingsWindow.Owner = owner;
        }

        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }

    public void ShowThemeStudio(ThemeStudioViewModel studio, NoteViewModel? ownerNote)
    {
        ThemeStudioWindow window = new ThemeStudioWindow { DataContext = studio };
        if (ownerNote is not null && _windows.TryGetValue(ownerNote, out NoteWindow? host))
        {
            window.Owner = host;
        }
        else if (System.Windows.Application.Current.MainWindow is not null)
        {
            window.Owner = System.Windows.Application.Current.MainWindow;
        }

        window.Show();
    }

    private void OnNoteWindowClosed(object? sender, EventArgs e)
    {
        if (sender is NoteWindow { DataContext: NoteViewModel note })
        {
            _windows.Remove(note);
            NoteWindowClosed?.Invoke(note);
        }
    }
}
