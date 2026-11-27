using System.Windows.Interop;
using Serilog;
using WinCapture.Core.Hotkeys;
using WinCapture.Core.Settings;
using WinCapture.Native.Interop;

namespace WinCapture.App;

/// <summary>
/// Registers the global hotkeys with RegisterHotKey (no keyboard hook) on a message-only window.
/// UI thread only. <see cref="Reapply"/> unregisters everything and registers again from settings.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private readonly ISettingsStore _settings;
    private readonly Action<HotkeyAction> _onHotkey;
    private readonly Action<string, string> _notifyUser;
    private readonly HwndSource _source;
    private readonly Dictionary<HotkeyAction, int> _registeredIds = new();
    private Dictionary<HotkeyAction, HotkeyStatus> _statuses = new();

    public HotkeyService(ISettingsStore settings, Action<HotkeyAction> onHotkey, Action<string, string> notifyUser)
    {
        _settings = settings;
        _onHotkey = onHotkey;
        _notifyUser = notifyUser;

        // Message-only window: receives WM_HOTKEY, never visible.
        _source = new HwndSource(new HwndSourceParameters("WinCaptureHotkeys")
        {
            ParentWindow = NativeMethods.HWND_MESSAGE,
            WindowStyle = 0,
        });
        _source.AddHook(WndProc);
    }

    /// <summary>Latest status per action (for the settings hotkey tab in session 6).</summary>
    public IReadOnlyDictionary<HotkeyAction, HotkeyStatus> Statuses => _statuses;

    public event EventHandler? StatusesChanged;

    /// <summary>Unregisters all hotkeys, re-reads settings and registers again.</summary>
    public void Reapply()
    {
        UnregisterAll();

        var plan = HotkeyPlan.Build(_settings.Load().Hotkeys);
        var statuses = new Dictionary<HotkeyAction, HotkeyStatus>();
        foreach (var (action, status) in plan.Rejected)
        {
            statuses[action] = status;
            Log.Warning("Hotkey {Action} not registered: {Status}", action, status);
        }

        foreach (var (action, combo) in plan.ToRegister)
        {
            var id = 1 + (int)action;
            var error = WindowInterop.RegisterHotkey(_source.Handle, id, (uint)combo.Modifiers, (uint)combo.VirtualKey);
            if (error == 0)
            {
                _registeredIds[action] = id;
                statuses[action] = HotkeyStatus.Registered;
            }
            else
            {
                statuses[action] = HotkeyStatus.RegisterFailed;
                Log.Warning("RegisterHotKey failed for {Action} ({Combo}), Win32 error {Error}", action, combo, error);
            }
        }

        _statuses = statuses;
        StatusesChanged?.Invoke(this, EventArgs.Empty);

        var failed = statuses.Where(p => p.Value != HotkeyStatus.Registered).Select(p => p.Key).OrderBy(a => a).ToList();
        if (failed.Count > 0)
        {
            _notifyUser("단축키 등록 실패",
                $"{string.Join(", ", failed)} 단축키를 등록하지 못했습니다. 설정의 단축키 탭에서 변경해 주세요.");
        }
    }

    public void UnregisterAll()
    {
        foreach (var id in _registeredIds.Values)
        {
            WindowInterop.UnregisterHotkey(_source.Handle, id);
        }

        _registeredIds.Clear();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            var action = _registeredIds.FirstOrDefault(p => p.Value == id, default);
            if (action.Value == id)
            {
                handled = true;
                try
                {
                    _onHotkey(action.Key);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Hotkey handler failed for {Action}", action.Key);
                }
            }
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterAll();
        _source.RemoveHook(WndProc);
        _source.Dispose();
    }
}
