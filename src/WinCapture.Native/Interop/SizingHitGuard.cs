using System.Runtime.InteropServices;
using WinCapture.Core.Presentation;

namespace WinCapture.Native.Interop;

/// <summary>
/// Outermost window-procedure subclass (comctl32 SetWindowSubclass) that guarantees no sizing hit code and no
/// resize cursor ever reaches the user, whatever the WPF WindowChrome / WPF-UI TitleBar hooks answer.
/// It sees the FINAL WM_NCHITTEST result of the whole hook chain, so hook registration order does not matter:
///  - WM_NCHITTEST: the default chain runs first; a sizing result (HTLEFT..HTBOTTOMRIGHT, HTSIZE, HTMAXBUTTON) becomes HTCLIENT.
///  - WM_SETCURSOR: if the hit code is still a sizing code, the arrow cursor is set and the message is consumed.
/// </summary>
public sealed class SizingHitGuard : IDisposable
{
    private static readonly UIntPtr SubclassId = new(0x57435347); // "WCSG"

    private readonly IntPtr _hwnd;
    private readonly NativeMethods.SubclassProc _proc; // field keeps the delegate alive while the native pointer exists
    private readonly IntPtr _procPointer;
    private bool _installed;

    private SizingHitGuard(IntPtr hwnd)
    {
        _hwnd = hwnd;
        _proc = WndProc;
        _procPointer = Marshal.GetFunctionPointerForDelegate(_proc);
    }

    /// <summary>Installs the guard on a window; returns null when it could not be installed.</summary>
    public static SizingHitGuard? Install(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return null;
        }

        var guard = new SizingHitGuard(hwnd);
        guard._installed = NativeMethods.SetWindowSubclass(hwnd, guard._procPointer, SubclassId, UIntPtr.Zero);
        return guard._installed ? guard : null;
    }

    public void Dispose()
    {
        if (_installed)
        {
            _installed = false;
            NativeMethods.RemoveWindowSubclass(_hwnd, _procPointer, SubclassId);
        }
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, UIntPtr subclassId, UIntPtr refData)
    {
        switch (msg)
        {
            case NativeMethods.WM_NCHITTEST:
            {
                var result = NativeMethods.DefSubclassProc(hwnd, msg, wParam, lParam);
                return FixedSizeWindowPolicy.IsSizingHit((int)result.ToInt64())
                    ? (IntPtr)NativeMethods.HTCLIENT
                    : result;
            }

            case NativeMethods.WM_SETCURSOR:
            {
                var hit = (int)(short)(lParam.ToInt64() & 0xFFFF);
                if (FixedSizeWindowPolicy.IsSizingHit(hit))
                {
                    NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, NativeMethods.IDC_ARROW));
                    return (IntPtr)1; // TRUE: cursor handled, do not let DefWindowProc choose a sizing cursor
                }

                break;
            }

            case NativeMethods.WM_NCDESTROY:
                Dispose();
                break;
        }

        return NativeMethods.DefSubclassProc(hwnd, msg, wParam, lParam);
    }
}
