namespace dikit.Infrastructure;

// The guard fails open (IsPrimary = true) when the named objects cannot be
// created; a duplicate instance is a smaller problem than refusing to start.
public sealed class SingleInstanceGuard : IDisposable
{
    private const string MutexName = @"Local\Dikit.SingleInstance";
    private const string ShowEventName = @"Local\Dikit.ShowRequest";

    private readonly Mutex? _mutex;
    private readonly bool _ownsMutex;
    private readonly EventWaitHandle? _showEvent;
    private readonly Thread? _listener;
    private volatile bool _disposed;

    public SingleInstanceGuard()
    {
        bool created = false;
        try
        {
            _mutex = new Mutex(initiallyOwned: true, MutexName, out created);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException
            or WaitHandleCannotBeOpenedException)
        {
            _mutex = null;
        }

        // Fail open: a mutex creation error leaves created=false, which would
        // make this instance exit as a supposed secondary — treat it as primary.
        IsPrimary = created || _mutex is null;
        _ownsMutex = created;
        if (!IsPrimary)
        {
            return;
        }

        try
        {
            _showEvent = new EventWaitHandle(
                false, EventResetMode.AutoReset, ShowEventName);
            _listener = new Thread(Listen) { IsBackground = true, Name = "Dikit single-instance" };
            _listener.Start();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            _showEvent = null;
        }
    }

    public event EventHandler? ShowRequested;

    public bool IsPrimary { get; }

    // The primary creates the event right after acquiring the mutex, so a fast
    // second instance can arrive first; retry briefly instead of exiting
    // without activating the running instance.
    public void NotifyPrimaryToShow()
    {
        if (_showEvent is not null)
        {
            try
            {
                _showEvent.Set();
                return;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException
                or ObjectDisposedException)
            {
            }
        }

        for (int attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using EventWaitHandle handle = EventWaitHandle.OpenExisting(ShowEventName);
                handle.Set();
                return;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Thread.Sleep(100);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                return;
            }
        }
    }

    private void Listen()
    {
        while (!_disposed)
        {
            bool signaled;
            try
            {
                signaled = _showEvent!.WaitOne(500);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (signaled && !_disposed)
            {
                ShowRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        try
        {
            _showEvent?.Set();
            _showEvent?.Dispose();
            if (_ownsMutex)
            {
                _mutex?.ReleaseMutex();
            }

            _mutex?.Dispose();
        }
        catch (ApplicationException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
