namespace dikit.Infrastructure;

public sealed class AppPaths
{
    public const string NoteFileName = "note.json";

    public AppPaths()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Dikit"))
    {
    }

    public AppPaths(string root)
    {
        Root = root;
    }

    public string Root { get; }

    public string NotesDirectory => Path.Combine(Root, "notes");

    public string ThemesDirectory => Path.Combine(Root, "themes");

    public string BackupsDirectory => Path.Combine(Root, "backups");

    public string LogsDirectory => Path.Combine(Root, "logs");

    public string SettingsFile => Path.Combine(Root, "app-settings.json");

    public string ManifestFile => Path.Combine(Root, "manifest.json");

    public string DeletedDirectory => Path.Combine(BackupsDirectory, "deleted");

    public string QuarantineDirectory => Path.Combine(BackupsDirectory, "quarantine");

    public string NoteDirectory(string noteId) => Path.Combine(NotesDirectory, noteId);

    public string NoteFile(string noteId) => Path.Combine(NoteDirectory(noteId), NoteFileName);

    public string NoteAssetsDirectory(string noteId) => Path.Combine(NoteDirectory(noteId), "assets");

    public string NoteBackupsDirectory(string noteId) => Path.Combine(BackupsDirectory, noteId);

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(NotesDirectory);
        Directory.CreateDirectory(ThemesDirectory);
        Directory.CreateDirectory(BackupsDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }
}
