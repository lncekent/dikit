using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using dikit.Infrastructure;

namespace dikit.Services;

public sealed class AssetService
{
    private const int MaxBitmapEdge = 2048;
    private const long MaxAssetBytes = 15L * 1024 * 1024;

    private static readonly Regex SafeAssetId =
        new Regex("^assets/[a-zA-Z0-9_.-]+$", RegexOptions.Compiled);

    private static readonly HashSet<string> ImportableExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".bmp",
        };

    private readonly AppPaths _paths;

    public AssetService(AppPaths paths)
    {
        _paths = paths;
    }

    public string? SaveBitmap(string noteId, BitmapSource bitmap)
    {
        try
        {
            BitmapSource bounded = Downscale(bitmap);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bounded));

            using MemoryStream memory = new MemoryStream();
            encoder.Save(memory);
            if (memory.Length > MaxAssetBytes)
            {
                return null;
            }

            string fileName = "img-" + Guid.NewGuid().ToString("N") + ".png";
            string directory = _paths.NoteAssetsDirectory(noteId);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory, fileName), memory.ToArray());
            return "assets/" + fileName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }

    public string? ImportFile(string noteId, string sourcePath)
    {
        try
        {
            string? extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(extension)
                || !ImportableExtensions.Contains(extension)
                || !File.Exists(sourcePath))
            {
                return null;
            }

            if (new FileInfo(sourcePath).Length > MaxAssetBytes)
            {
                return null;
            }

            string fileName = "img-" + Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            string directory = _paths.NoteAssetsDirectory(noteId);
            Directory.CreateDirectory(directory);
            File.Copy(sourcePath, Path.Combine(directory, fileName), overwrite: false);
            return "assets/" + fileName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }

    public string? ResolvePath(string noteId, string? assetId)
    {
        if (string.IsNullOrEmpty(assetId) || !SafeAssetId.IsMatch(assetId))
        {
            return null;
        }

        string noteDirectory = Path.GetFullPath(_paths.NoteDirectory(noteId));
        string fullPath = Path.GetFullPath(Path.Combine(noteDirectory, assetId!));
        if (!fullPath.StartsWith(noteDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return File.Exists(fullPath) ? fullPath : null;
    }

    public void DeleteUnreferencedAssets(string noteId, IReadOnlySet<string> referencedAssetIds)
    {
        try
        {
            DirectoryInfo noteDirectory = new DirectoryInfo(_paths.NoteDirectory(noteId));
            DirectoryInfo assetsDirectory = new DirectoryInfo(_paths.NoteAssetsDirectory(noteId));
            if (!IsPlainDirectory(noteDirectory) || !IsPlainDirectory(assetsDirectory))
            {
                return;
            }

            foreach (FileInfo file in assetsDirectory.EnumerateFiles())
            {
                if (IsReparsePoint(file)
                    || referencedAssetIds.Contains("assets/" + file.Name))
                {
                    continue;
                }

                file.Delete();
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static bool IsPlainDirectory(DirectoryInfo directory)
    {
        return directory.Exists && directory.LinkTarget is null
            && !directory.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    private static bool IsReparsePoint(FileInfo file)
    {
        return file.LinkTarget is not null
            || file.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    private static BitmapSource Downscale(BitmapSource bitmap)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int longest = Math.Max(width, height);
        if (longest <= MaxBitmapEdge || longest <= 0)
        {
            return bitmap;
        }

        double scale = MaxBitmapEdge / (double)longest;
        TransformedBitmap scaled = new TransformedBitmap();
        scaled.BeginInit();
        scaled.Source = bitmap;
        scaled.Transform = new System.Windows.Media.ScaleTransform(scale, scale);
        scaled.EndInit();
        scaled.Freeze();
        return scaled;
    }
}
