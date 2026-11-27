namespace WinCapture.App;

/// <summary>
/// Single-instance guard. Ownership is a named Mutex; a named auto-reset event is used by a
/// second launch to tell the running instance to bring its settings window to the front.
/// </summary>
public sealed class SingleInstanceGuard : IDisposable
{
    private const string MutexName = @"Local\WinCapture.SingleInstance.Mutex";
    private const string ActivateEventName = @"Local\WinCapture.SingleInstance.Activate";

    private readonly Mutex _mutex;
    private readonly EventWaitHandle _activateEvent;
    private readonly ManualResetEvent _stopEvent = new(false);
    private Thread? _listener;
    private bool _disposed;

    private SingleInstanceGuard(Mutex mutex, EventWaitHandle activateEvent, bool isFirst)
    {
        _mutex = mutex;
        _activateEvent = activateEvent;
        IsFirstInstance = isFirst;
    }

    public bool IsFirstInstance { get; }

    /// <summary>Tries to become the single instance. When another instance exists, signals it.</summary>
    public static SingleInstanceGuard Acquire()
    {
        var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        var evt = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);
        if (!createdNew)
        {
            evt.Set();
        }

        return new SingleInstanceGuard(mutex, evt, createdNew);
    }

    /// <summary>Starts a background listener that invokes <paramref name="onActivate"/> on each signal.</summary>
    public void StartListening(Action onActivate)
    {
        if (!IsFirstInstance || _listener is not null)
        {
            return;
        }

        _listener = new Thread(() =>
        {
            WaitHandle[] handles = [_activateEvent, _stopEvent];
            while (WaitHandle.WaitAny(handles) == 0)
            {
                onActivate();
            }
        })
        {
            IsBackground = true,
            Name = "SingleInstanceListener",
        };
        _listener.Start();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _stopEvent.Set();
        _listener?.Join(1000);
        if (IsFirstInstance)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
        _activateEvent.Dispose();
        _stopEvent.Dispose();
    }
}
