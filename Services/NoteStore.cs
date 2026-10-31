using System.Text.Json;
using System.Text.RegularExpressions;
using dikit.Infrastructure;
using dikit.Models;

namespace dikit.Services;

public sealed class StoreLoadResult
{
    public List<Note> Notes { get; } = new List<Note>();

    public List<string> Warnings { get; } = new List<string>();

    public AppSettings Settings { get; set; } = new AppSettings();
}

public sealed class NoteStore
{
    private static readonly Regex SafeNoteId = new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
    private readonly AppPaths _paths;
    private readonly AssetService _assets;
    private NoteManifest _manifest = new NoteManifest();

    public NoteStore(AppPaths paths, AssetService assets)
    {
        _paths = paths;
        _assets = assets;
    }

    public int BackupRetention { get; set; } = 5;

    public StoreLoadResult LoadAll()
    {
        _paths.EnsureDirectories();
        StoreLoadResult result = new StoreLoadResult();
        result.Settings = LoadSettings(result.Warnings);
        _manifest = LoadManifest(result.Warnings);

        foreach (string directory in Directory.EnumerateDirectories(_paths.NotesDirectory))
        {
            string id = Path.GetFileName(directory);
            if (!SafeNoteId.IsMatch(id))
            {
                result.Warnings.Add($"Skipped note folder with an unsafe name: {id}");
                continue;
            }

            string file = Path.Combine(directory, AppPaths.NoteFileName);
            bool hadFile = File.Exists(file);
            Note? note = hadFile ? TryReadNote(file, id, result.Warnings) : null;
            if (note is null)
            {
                note = RecoverFromBackups(id, result.Warnings);
                if (note is not null)
                {
                    if (hadFile)
                    {
                        Quarantine(file);
                    }

                    WriteNoteFile(note);
                    result.Warnings.Add($"Note '{id}' was recovered from a backup.");
                }
                else
                {
                    if (hadFile)
                    {
                        Quarantine(file);
                        result.Warnings.Add(
                            $"Note '{id}' could not be read; its data was moved to backups/quarantine.");
                    }
                    else
                    {
                        result.Warnings.Add($"Note '{id}' has no {AppPaths.NoteFileName}; it was skipped.");
                    }

                    continue;
                }
            }

            if (note.SchemaVersion != Note.CurrentSchemaVersion)
            {
                result.Warnings.Add(
                    $"Note '{id}' uses schema version {note.SchemaVersion}, which this build does not support; it was not overwritten.");
                continue;
            }

            if (note.Id != id)
            {
                note.Id = id;
                WriteNoteFile(note);
                result.Warnings.Add($"Note '{id}' had a mismatched id in its file; it was corrected.");
            }

            result.Notes.Add(note);
        }

        RebuildManifest(result.Notes);
        WriteManifest();
        return result;
    }

    public AppSettings LoadSettings(List<string>? warnings = null)
    {
        if (!File.Exists(_paths.SettingsFile))
        {
            return new AppSettings();
        }

        try
        {
            AppSettings? settings =
                JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_paths.SettingsFile), JsonOptions.Default);
            return settings ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            warnings?.Add($"App settings could not be read ({ex.Message}); defaults are in use.");
            QuarantineFile(_paths.SettingsFile);
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        AtomicFileWriter.WriteAllText(
            _paths.SettingsFile,
            JsonSerializer.Serialize(settings, JsonOptions.Default));
    }

    public Note CreateNote()
    {
        Note note = NewNote("New note");
        SaveNote(note);
        return note;
    }

    public Note CreateWelcomeNote()
    {
        Note note = NewNote("Welcome to Dikit");
        note.Blocks.Add(new TextBlock
        {
            Text = "Dikit keeps small notes on your desktop — text, images, links, and checklists, all saved automatically to this device.",
        });
        note.Blocks.Add(new TextBlock
        {
            Text = "Drag the title area to move a note, drag its edges to resize, and use + Text / + Checklist / + Link / + Image below to add blocks. Ctrl+V pastes a screenshot or a link.",
        });
        note.Blocks.Add(new ChecklistBlock { Text = "Try pinning this note or restyling it with the Theme button" });
        note.Blocks.Add(new TextBlock
        {
            Text = "Every note lives under All notes in the main window or the Dikit tray icon. Settings offers the launch-at-sign-in option, and Quit lives in the tray menu — closing a note never deletes it.",
        });
        SaveNote(note);
        return note;
    }

    private static Note NewNote(string title)
    {
        Note note = new Note
        {
            Id = NewNoteId(),
            Title = title,
            CreatedUtc = DateTimeOffset.UtcNow,
            UpdatedUtc = DateTimeOffset.UtcNow,
        };
        note.Blocks.Add(new TextBlock());
        return note;
    }

    public Note DuplicateNote(Note source)
    {
        string json = JsonSerializer.Serialize(source, JsonOptions.Default);
        Note copy = JsonSerializer.Deserialize<Note>(json, JsonOptions.Default) ?? new Note();
        copy.Id = NewNoteId();
        copy.Title = string.IsNullOrWhiteSpace(source.Title) ? "New note" : source.Title + " copy";
        copy.CreatedUtc = DateTimeOffset.UtcNow;
        copy.UpdatedUtc = DateTimeOffset.UtcNow;
        if (copy.Window.X is double x)
        {
            copy.Window.X = x + 28;
        }

        if (copy.Window.Y is double y)
        {
            copy.Window.Y = y + 28;
        }

        try
        {
            CopyAssets(source.Id, copy);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TryDeleteDirectory(_paths.NoteDirectory(copy.Id));
            throw;
        }

        SaveNote(copy);
        return copy;
    }

    private void CopyAssets(string sourceId, Note targetNote)
    {
        string source = _paths.NoteAssetsDirectory(sourceId);
        if (!Directory.Exists(source))
        {
            return;
        }

        string target = _paths.NoteAssetsDirectory(targetNote.Id);
        Directory.CreateDirectory(target);
        foreach (string fileName in targetNote.Blocks.OfType<ImageBlock>()
                     .Select(block => Path.GetFileName(block.AssetId))
                     .Where(name => !string.IsNullOrEmpty(name))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string file = Path.Combine(source, fileName!);
            if (File.Exists(file))
            {
                File.Copy(file, Path.Combine(target, fileName!), overwrite: false);
            }
        }
    }

    private static void TryDeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public void SaveNote(Note note)
    {
        note.SchemaVersion = Note.CurrentSchemaVersion;
        note.UpdatedUtc = DateTimeOffset.UtcNow;
        WriteNoteFile(note);
        if (TryCollectReferencedAssetIds(note, out HashSet<string> referencedAssetIds))
        {
            _assets.DeleteUnreferencedAssets(note.Id, referencedAssetIds);
        }

        UpsertManifestEntry(note);
        WriteManifest();
    }

    public void DeleteNote(string noteId)
    {
        string directory = _paths.NoteDirectory(noteId);
        if (Directory.Exists(directory))
        {
            Directory.CreateDirectory(_paths.DeletedDirectory);
            string destination = Path.Combine(
                _paths.DeletedDirectory,
                noteId + "-" + DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss"));
            Directory.Move(directory, destination);
        }

        _manifest.Notes.RemoveAll(entry => entry.Id == noteId);
        WriteManifest();
    }

    public IReadOnlyList<NoteManifestEntry> ManifestEntries => _manifest.Notes;

    private void WriteNoteFile(Note note)
    {
        note.SchemaVersion = Note.CurrentSchemaVersion;
        string file = _paths.NoteFile(note.Id);
        if (File.Exists(file))
        {
            BackupExisting(note.Id, file);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        AtomicFileWriter.WriteAllText(file, JsonSerializer.Serialize(note, JsonOptions.Default));
    }

    private bool TryCollectReferencedAssetIds(Note note, out HashSet<string> referenced)
    {
        referenced = ImageAssetIds(note).ToHashSet(StringComparer.OrdinalIgnoreCase);
        string backupDir = _paths.NoteBackupsDirectory(note.Id);
        try
        {
            if (!Directory.Exists(backupDir))
            {
                return true;
            }

            foreach (string backup in Directory.EnumerateFiles(backupDir, "*.json"))
            {
                try
                {
                    Note? backupNote = JsonSerializer.Deserialize<Note>(
                        File.ReadAllText(backup), JsonOptions.Default);
                    if (backupNote?.SchemaVersion == Note.CurrentSchemaVersion)
                    {
                        referenced.UnionWith(ImageAssetIds(backupNote));
                    }
                }
                catch (Exception ex) when (ex is IOException or JsonException
                    or NotSupportedException or UnauthorizedAccessException)
                {
                    return false;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<string> ImageAssetIds(Note note)
    {
        return note.Blocks.OfType<ImageBlock>()
            .Select(block => block.AssetId)
            .Where(assetId => !string.IsNullOrEmpty(assetId));
    }

    private void BackupExisting(string noteId, string file)
    {
        try
        {
            string backupDir = _paths.NoteBackupsDirectory(noteId);
            Directory.CreateDirectory(backupDir);
            string destination = Path.Combine(
                backupDir,
                DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff") + ".json");
            File.Copy(file, destination, overwrite: false);

            string[] backups = Directory.GetFiles(backupDir, "*.json")
                .OrderByDescending(name => name, StringComparer.Ordinal)
                .ToArray();
            foreach (string old in backups.Skip(Math.Max(1, BackupRetention)))
            {
                File.Delete(old);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private Note? TryReadNote(string file, string id, List<string>? warnings)
    {
        try
        {
            return JsonSerializer.Deserialize<Note>(File.ReadAllText(file), JsonOptions.Default);
        }
        catch (Exception ex) when (ex is IOException or JsonException
            or NotSupportedException or UnauthorizedAccessException)
        {
            // NotSupportedException: valid JSON whose block discriminator is
            // missing/misordered/unknown must not kill startup.
            warnings?.Add($"Note '{id}' failed to load: {ex.Message}");
            return null;
        }
    }

    private Note? RecoverFromBackups(string id, List<string> warnings)
    {
        string backupDir = _paths.NoteBackupsDirectory(id);
        if (!Directory.Exists(backupDir))
        {
            return null;
        }

        foreach (string backup in Directory.GetFiles(backupDir, "*.json")
                     .OrderByDescending(name => name, StringComparer.Ordinal))
        {
            try
            {
                Note? note = JsonSerializer.Deserialize<Note>(
                    File.ReadAllText(backup), JsonOptions.Default);
                if (note is not null)
                {
                    if (note.SchemaVersion != Note.CurrentSchemaVersion)
                    {
                        warnings.Add(
                            $"Backup '{Path.GetFileName(backup)}' for note '{id}' uses schema version "
                            + $"{note.SchemaVersion}, which this build does not support; it was not restored.");
                        continue;
                    }

                    note.Id = id;
                    return note;
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException
                or NotSupportedException or UnauthorizedAccessException)
            {
                warnings.Add($"Backup '{Path.GetFileName(backup)}' for note '{id}' is also unreadable.");
            }
        }

        return null;
    }

    private void Quarantine(string file)
    {
        QuarantineFile(file);
    }

    private void QuarantineFile(string file)
    {
        try
        {
            Directory.CreateDirectory(_paths.QuarantineDirectory);
            string destination = Path.Combine(
                _paths.QuarantineDirectory,
                Path.GetFileName(file) + "." + DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss"));
            File.Move(file, destination, overwrite: false);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private NoteManifest LoadManifest(List<string> warnings)
    {
        if (!File.Exists(_paths.ManifestFile))
        {
            return new NoteManifest();
        }

        try
        {
            return JsonSerializer.Deserialize<NoteManifest>(
                File.ReadAllText(_paths.ManifestFile), JsonOptions.Default) ?? new NoteManifest();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            warnings.Add($"Note manifest could not be read ({ex.Message}); it will be rebuilt.");
            return new NoteManifest();
        }
    }

    private void RebuildManifest(List<Note> notes)
    {
        _manifest.Notes.Clear();
        foreach (Note note in notes)
        {
            UpsertManifestEntry(note);
        }
    }

    private void UpsertManifestEntry(Note note)
    {
        NoteManifestEntry? entry = _manifest.Notes.FirstOrDefault(e => e.Id == note.Id);
        if (entry is null)
        {
            entry = new NoteManifestEntry { Id = note.Id };
            _manifest.Notes.Add(entry);
        }

        entry.Title = note.Title;
        entry.IsOpen = note.Window.IsOpen;
        entry.UpdatedUtc = note.UpdatedUtc;
    }

    private void WriteManifest()
    {
        AtomicFileWriter.WriteAllText(
            _paths.ManifestFile,
            JsonSerializer.Serialize(_manifest, JsonOptions.Default));
    }

    private static string NewNoteId()
    {
        return "note-" + Guid.NewGuid().ToString("N");
    }
}
