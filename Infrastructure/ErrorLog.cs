namespace dikit.Infrastructure;

// Appends timestamped lines to logs/dikit.log under the app data folder.
// Local only; failures to write are ignored so logging can never crash the app.
public static class ErrorLog
{
    private static readonly object Gate = new object();
    private static string? _file;

    public static void Initialize(AppPaths paths)
    {
        try
        {
            Directory.CreateDirectory(paths.LogsDirectory);
            _file = Path.Combine(paths.LogsDirectory, "dikit.log");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
            or ArgumentException)
        {
            _file = null;
        }
    }

    public static void Log(string message)
    {
        string? file = _file;
        if (file is null)
        {
            return;
        }

        try
        {
            lock (Gate)
            {
                File.AppendAllText(
                    file,
                    DateTimeOffset.Now.ToString("u") + " " + message + Environment.NewLine);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    public static void Log(Exception error)
    {
        Log(error.ToString());
    }
}
