namespace dikit.Infrastructure;

public sealed class Debouncer
{
    private readonly TimeSpan _delay;
    private readonly object _gate = new object();
    private CancellationTokenSource? _pending;

    public Debouncer(TimeSpan delay)
    {
        _delay = delay;
    }

    public void Schedule(Action action)
    {
        CancellationTokenSource cts;
        lock (_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            cts = new CancellationTokenSource();
            _pending = cts;
        }

        RunAfterDelay(action, cts.Token);
    }

    public void Cancel()
    {
        lock (_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            _pending = null;
        }
    }

    private async void RunAfterDelay(Action action, CancellationToken token)
    {
        try
        {
            await Task.Delay(_delay, token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!token.IsCancellationRequested)
        {
            action();
        }
    }
}
