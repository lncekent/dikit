using dikit.Infrastructure;

namespace dikit.Services;

// Uses the Windows Forms NotifyIcon on the WPF UI thread; the shared
// Windows Desktop framework provides it, so no third-party package is needed.
public sealed class TrayIconService : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly Action _onAllNotes;
    private System.Drawing.Icon? _icon;
    private bool _disposed;

    public TrayIconService(
        Action onNewNote,
        Action onAllNotes,
        Action onHideAll,
        Action onRestoreAll,
        Action onSettings,
        Action onQuit)
    {
        _onAllNotes = onAllNotes;

        System.Windows.Forms.ContextMenuStrip menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("New note", null, (_, _) => onNewNote());
        menu.Items.Add("All notes", null, (_, _) => onAllNotes());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Hide all", null, (_, _) => onHideAll());
        menu.Items.Add("Restore all", null, (_, _) => onRestoreAll());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Settings", null, (_, _) => onSettings());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => onQuit());

        _icon = LoadIcon();
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "Dikit",
            Icon = _icon,
            ContextMenuStrip = menu,
            Visible = false,
        };
        _notifyIcon.MouseClick += OnMouseClick;
    }

    public void Show()
    {
        _notifyIcon.Visible = true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon?.Dispose();
        _icon = null;
    }

    private void OnMouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (e.Button == System.Windows.Forms.MouseButtons.Left)
        {
            _onAllNotes();
        }
    }

    private static System.Drawing.Icon? LoadIcon()
    {
        try
        {
            System.IO.Stream? stream = System.Windows.Application.GetResourceStream(
                new Uri("pack://application:,,,/Resources/dikit.ico"))?.Stream;
            return stream is not null ? new System.Drawing.Icon(stream) : null;
        }
        catch (Exception ex) when (ex is IOException or UriFormatException or ArgumentException)
        {
            ErrorLog.Log($"Tray icon could not be loaded: {ex.Message}");
            return null;
        }
    }
}
