using Serilog;
using WinCapture.Core.Geometry;
using WinCapture.Native.Interop;

namespace WinCapture.App.Overlay;

/// <summary>Opens the selection overlay and hands its result to <see cref="RegionCoordinator"/>. UI thread only.</summary>
public sealed class SelectionController
{
    private readonly RegionCoordinator _coordinator;
    private SelectionOverlayWindow? _window;

    public SelectionController(RegionCoordinator coordinator) => _coordinator = coordinator;

    /// <summary>Starts a selection; ignored while one is already open.</summary>
    public void BeginSelection()
    {
        if (_window is not null)
        {
            return;
        }

        var window = new SelectionOverlayWindow(WindowInterop.GetVirtualScreen());
        _window = window;
        window.Completed += (_, result) =>
        {
            _window = null;
            try
            {
                _coordinator.CommitSelection(result); // null/too small keeps the previous region
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Applying selection failed");
            }

            window.Close();
        };
        window.Show();
    }
}
