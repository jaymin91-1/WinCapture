using System.IO;
using WinCapture.Core.Storage;

namespace WinCapture.App.Capture;

/// <summary>Real file system access for <see cref="CaptureFileNamer"/>.</summary>
public sealed class SaveFileSystem : ISaveFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public IReadOnlyList<string> GetFileNames(string folder) =>
        Directory.Exists(folder)
            ? Directory.EnumerateFiles(folder).Select(f => Path.GetFileName(f)).ToList()
            : [];
}
