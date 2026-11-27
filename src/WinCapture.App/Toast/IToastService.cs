using WinCapture.Core.Presentation;

namespace WinCapture.App.Toast;

/// <summary>Shows toast notifications. Safe to call from any thread (marshalled to the UI thread).</summary>
public interface IToastService
{
    /// <param name="onClick">Optional action run when the toast is clicked; the toast closes afterwards.</param>
    void Show(ToastKind kind, string title, string message, Action? onClick = null);
}
