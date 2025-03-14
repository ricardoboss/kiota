using System;
using System.Collections.Generic;
using System.IO;

namespace Kiota.Builder.Filesystem;

public class PhysicalFilesystem : IFilesystem
{
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);

        NodeCreated?.Invoke(this, new()
        {
            Path = path,
            Name = Path.GetFileName(path),
            IsDirectory = true,
        });
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        Directory.Delete(path, recursive);

        NodeDeleted?.Invoke(this, new()
        {
            Path = path,
            Name = Path.GetFileName(path),
            IsDirectory = true,
        });
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return Directory.EnumerateDirectories(path);
    }

    public IEnumerable<string> EnumerateFiles(string path, bool recursive = false)
    {
        return Directory.EnumerateFiles(path);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);

        NodeDeleted?.Invoke(this, new()
        {
            Path = path,
            Name = Path.GetFileName(path),
            IsDirectory = false,
        });
    }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }

    public DateTime GetLastWriteTime(string path)
    {
        return File.GetLastWriteTime(path);
    }

    public void CopyFile(string source, string target, bool overwrite)
    {
        var existed = File.Exists(target);

        File.Copy(source, target, overwrite);

        if (!existed)
        {
            NodeCreated?.Invoke(this, new()
            {
                Path = target,
                Name = Path.GetFileName(target),
                IsDirectory = false,
            });
        }
    }

    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    public event EventHandler<FilesystemNodeEventArgs>? NodeCreated;

    public event EventHandler<FilesystemNodeEventArgs>? NodeDeleted;

    public Stream OpenWrite(string path, bool create = true)
    {
        if (File.Exists(path))
        {
            NodeDeleted?.Invoke(this, new()
            {
                Path = path,
                Name = Path.GetFileName(path),
                IsDirectory = false,
            });
        }

        return File.OpenWrite(path);
    }
}
