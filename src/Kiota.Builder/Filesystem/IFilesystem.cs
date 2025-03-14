using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kiota.Builder.Filesystem;

public interface IFilesystem
{
    void CreateDirectory(string path);
    void DeleteDirectory(string path, bool recursive = false);
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> EnumerateDirectories(string path);
    IEnumerable<string> EnumerateFiles(string path, bool recursive = false);
    void DeleteFile(string path);
    Stream OpenRead(string path);
    Stream OpenWrite(string path, bool create = true);
    DateTime GetLastWriteTime(string path);
    void CopyFile(string source, string target, bool overwrite);
    string GetTempPath();
    string GetCurrentDirectory();

    event EventHandler<FilesystemNodeEventArgs> NodeCreated;
    event EventHandler<FilesystemNodeEventArgs> NodeDeleted;
}

public class FilesystemNodeEventArgs : EventArgs
{
    public required string Path { get; set; }
    public required string Name { get; set; }
    public required bool IsDirectory { get; set; }
}

public static class FilesystemExtensions
{
    public static async Task<string> ReadAllTextAsync(this IFilesystem filesystem, string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filesystem);
        ArgumentException.ThrowIfNullOrEmpty(path);

        var stream = filesystem.OpenRead(path);
        var reader = new StreamReader(stream, leaveOpen: true);

        var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        reader.Dispose();
        await stream.DisposeAsync().ConfigureAwait(false);

        return content;
    }

    public static async Task WriteAllTextAsync(this IFilesystem filesystem, string path, string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filesystem);
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentException.ThrowIfNullOrEmpty(content);

        var stream = filesystem.OpenWrite(content);
        var writer = new StreamWriter(stream, leaveOpen: true);

        await writer.WriteAsync(content).ConfigureAwait(false);

        await writer.DisposeAsync().ConfigureAwait(false);
        await stream.DisposeAsync().ConfigureAwait(false);
    }
}
