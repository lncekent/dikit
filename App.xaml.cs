using System.Windows;
using dikit.Infrastructure;
using dikit.Services;
using dikit.ViewModels;

namespace dikit;

public partial class App : Application
{
    private NotesManagerViewModel? _manager;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppPaths paths = new AppPaths();
        paths.EnsureDirectories();
        NoteStore store = new NoteStore(paths);
        AssetService assets = new AssetService(paths);
        ThemeService themes = new ThemeService(paths);
        StoreLoadResult loaded = store.LoadAll();
        _manager = new NotesManagerViewModel(
            store, loaded.Notes, loaded.Warnings, loaded.Settings, assets, themes);

        MainWindow launcher = new MainWindow { DataContext = _manager };
        MainWindow = launcher;
        launcher.Show();
        _manager.RestoreOpenNotes();
    }

    protected override void OnSessionEnding(SessionEndingEventArgs e)
    {
        _manager?.Shutdown();
        base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _manager?.Shutdown();
        base.OnExit(e);
    }
}
