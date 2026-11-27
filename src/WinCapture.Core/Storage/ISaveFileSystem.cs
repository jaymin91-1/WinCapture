namespace WinCapture.Core.Storage;

/// <summary>File system access needed to pick a save path (injected so Core stays pure).</summary>
public interface ISaveFileSystem
{
    bool FileExists(string path);

    /// <summary>Returns the file names (no directory part) in a folder; empty when it does not exist.</summary>
    IReadOnlyList<string> GetFileNames(string folder);
}
