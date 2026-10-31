using System.Windows;
using dikit.Infrastructure;
using dikit.Services;
using dikit.ViewModels;

namespace dikit;

public partial class App : System.Windows.Application
{
    private NotesManagerViewModel? _manager;
    private TrayIconService? _tray;
    private SingleInstanceGuard? _instanceGuard;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        _instanceGuard = new SingleInstanceGuard();
        if (!_instanceGuard.IsPrimary)
        {
            _instanceGuard.NotifyPrimaryToShow();
            Shutdown();
            return;
        }

        _instanceGuard.ShowRequested += (_, _) =>
            Dispatcher.BeginInvoke(ShowAllNotesWindow);

        AppPaths paths = new AppPaths();
        paths.EnsureDirectories();
        ErrorLog.Initialize(paths);

        NoteStore store = new NoteStore(paths);
        AssetService assets = new AssetService(paths);
        ThemeService themes = new ThemeService(paths);
        StartupService startup = new StartupService();
        StoreLoadResult loaded = store.LoadAll();
        _manager = new NotesManagerViewModel(
            store, loaded.Notes, loaded.Warnings, loaded.Settings, assets, themes,
            paths, startup);
        _manager.SyncStartupOption();

        MainWindow launcher = new MainWindow { DataContext = _manager };
        MainWindow = launcher;

        _tray = new TrayIconService(
            onNewNote: () => _manager.NewNote(),
            onAllNotes: ShowAllNotesWindow,
            onHideAll: () => _manager.HideAll(),
            onRestoreAll: () => _manager.RestoreAll(),
            onSettings: () => _manager.OpenSettings(),
            onQuit: () => _manager.Quit());
        _tray.Show();

        launcher.Show();
        _manager.RestoreOpenNotes();
    }

    private void ShowAllNotesWindow()
    {
        if (MainWindow is null)
        {
            return;
        }

        if (!MainWindow.IsVisible)
        {
            MainWindow.Show();
        }

        if (MainWindow.WindowState == WindowState.Minimized)
        {
            MainWindow.WindowState = WindowState.Normal;
        }

        MainWindow.Activate();
    }

    private void OnDispatcherUnhandledException(
        object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ErrorLog.Log(e.Exception);
        e.Handled = true;
    }

    protected override void OnSessionEnding(SessionEndingEventArgs e)
    {
        _manager?.Shutdown();
        base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _manager?.Shutdown();
        _tray?.Dispose();
        _instanceGuard?.Dispose();
        base.OnExit(e);
    }
}
