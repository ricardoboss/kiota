using System.Text;
using Kiota.Builder.Filesystem;

namespace Kiota.Web;

public class InMemoryFilesystem : IFilesystem
{
    public readonly FilesystemNode Root = FilesystemNode.Root;

    private static IEnumerable<string> GetSegments(string path)
    {
        if (path.StartsWith('.'))
        {
            yield return "work";
        }
        else if (!path.StartsWith('~'))
        {
            throw new ArgumentException("Path must start with '~' or '.'");
        }

        // cut off the leading '~' or '.'
        path = path[1..];

        foreach (var segment in path.Split(Path.DirectorySeparatorChar))
        {
            if (string.IsNullOrEmpty(segment))
                continue;

            yield return segment;
        }
    }

    public void CreateDirectory(string path) => CreateDirectoryInternal(path);

    private FilesystemNode CreateDirectoryInternal(string path)
    {
        var segments = GetSegments(path);
        var segmentQueue = new Queue<string>(segments);
        var currentNode = Root;
        while (segmentQueue.TryDequeue(out var segment))
        {
            var childNode =
                currentNode.Children.FirstOrDefault(x => x.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (childNode is null)
            {
                childNode = FilesystemNode.CreateDirectory(currentNode, segment);

                NodeCreated?.Invoke(this, new()
                {
                    Path = currentNode.Path,
                    Name = childNode.Name,
                    IsDirectory = childNode.IsDirectory,
                });
            }

            currentNode = childNode;
        }

        return currentNode;
    }

    public bool DirectoryExists(string path) => GetNode(path) is { IsDirectory: true };

    public bool FileExists(string path) => GetNode(path) is { IsDirectory: false };

    private FilesystemNode? GetNode(string path)
    {
        var segments = GetSegments(path);
        var currentNode = Root;
        foreach (var segment in segments)
        {
            var childNode =
                currentNode.Children.FirstOrDefault(x => x.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (childNode is null)
                return null;

            currentNode = childNode;
        }

        return currentNode;
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        var segments = GetSegments(path);
        var currentNode = Root;
        foreach (var segment in segments)
        {
            var childNode =
                currentNode.Children.FirstOrDefault(x => x.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));
            if (childNode is null)
                return [];

            currentNode = childNode;
        }

        return currentNode.Children.Where(x => x.IsDirectory).Select(x => x.Name);
    }

    public IEnumerable<string> EnumerateFiles(string path, bool recursive = false)
    {
        var currentNode = GetNode(path);
        if (currentNode is null)
            throw new DirectoryNotFoundException(path);

        foreach (var childNode in currentNode.Children)
        {
            if (childNode.IsDirectory)
            {
                if (!recursive)
                    continue;

                foreach (var childChildNode in EnumerateFiles(Path.Combine(path, childNode.Name), recursive))
                    yield return childChildNode;
            }
            else
            {
                yield return childNode.Path;
            }
        }
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        var node = GetNode(path);
        if (node is null)
            return;

        if (!node.IsDirectory)
            throw new InvalidOperationException("Cannot delete a file");

        DeleteNode(path, node);
    }

    public void DeleteFile(string path)
    {
        var node = GetNode(path);
        if (node is null)
            return;

        if (node.IsDirectory)
            throw new InvalidOperationException("Cannot delete a directory");

        DeleteNode(path, node);
    }

    private void DeleteNode(string path, FilesystemNode node)
    {
        var parent = node.Parent;
        if (parent is null)
            throw new InvalidOperationException("Cannot delete root node");

        parent.RemoveChild(node);

        NodeDeleted?.Invoke(this, new()
        {
            Path = parent.Path,
            Name = node.Name,
            IsDirectory = node.IsDirectory,
        });
    }

    public Stream OpenRead(string path)
    {
        var node = GetNode(path);
        if (node is null)
            throw new FileNotFoundException(path);

        return node.OpenRead();
    }

    public Stream OpenWrite(string path, bool create = true)
    {
        var file = GetNode(path);
        if (file is not null)
            return file.OpenWrite();

        var parentPath = Path.GetDirectoryName(path) ??
                         throw new ArgumentException("Path must contain a directory");
        var parent = CreateDirectoryInternal(parentPath);

        file = FilesystemNode.CreateFile(parent, Path.GetFileName(path));

        NodeCreated?.Invoke(this, new()
        {
            Path = parentPath,
            Name = file.Name,
            IsDirectory = file.IsDirectory,
        });

        return file.OpenWrite();
    }

    public DateTime GetLastWriteTime(string path)
    {
        var node = GetNode(path);
        if (node is null)
            throw new FileNotFoundException(path);

        return node.LastWriteTime;
    }

    public void CopyFile(string source, string target, bool overwrite)
    {
        var sourceNode = GetNode(source);
        if (sourceNode is null)
            throw new FileNotFoundException(source);

        var targetNode = GetNode(target);
        if (targetNode is not null && !overwrite)
            throw new IOException("File already exists");

        FilesystemNode? parent;
        if (targetNode is null)
        {
            parent = GetNode(Path.GetDirectoryName(target) ??
                             throw new ArgumentException("Path must contain a directory"));
            if (parent is null)
                throw new DirectoryNotFoundException(Path.GetDirectoryName(target));
        }
        else
        {
            parent = targetNode.Parent;
        }

        if (targetNode is null)
        {
            targetNode = FilesystemNode.CreateFile(parent!, Path.GetFileName(target));

            NodeCreated?.Invoke(this, new()
            {
                Path = parent!.Path,
                Name = targetNode.Name,
                IsDirectory = targetNode.IsDirectory,
            });
        }

        targetNode.Content = sourceNode.Content;
    }

    public string GetTempPath()
    {
        return Path.Combine("~", "tmp");
    }

    public string GetCurrentDirectory()
    {
        return Path.Combine("~", "work");
    }

    public event EventHandler<FilesystemNodeEventArgs>? NodeCreated;

    public event EventHandler<FilesystemNodeEventArgs>? NodeDeleted;
}

public sealed class FilesystemNode
{
    public string Name
    {
        get;
    }

    public bool IsDirectory => _children is not null;

    private string? _content;

    public DateTime LastWriteTime
    {
        get;
        private set;
    } = DateTime.MinValue;

    public string? Content
    {
        get => _content;
        set
        {
            _content = value;
            LastWriteTime = DateTime.UtcNow;
        }
    }

    private readonly List<FilesystemNode>? _children;

    public IReadOnlyList<FilesystemNode> Children
    {
        get
        {
            if (_children is null)
                throw new InvalidOperationException("Cannot access children of a non-directory");

            return _children;
        }
    }

    public void RemoveChild(FilesystemNode child)
    {
        if (_children is null)
            throw new InvalidOperationException("Cannot remove children from a non-directory");

        _children.Remove(child);
    }

    public FilesystemNode? Parent
    {
        get;
    }

    private FilesystemNode(FilesystemNode? parent, string name,
        IEnumerable<FilesystemNode> children)
    {
        Name = name;
        Parent = parent;
        _children = [..children];
    }

    private FilesystemNode(FilesystemNode? parent, string name, string? content)
    {
        Name = name;
        Parent = parent;
        Content = content;
    }

    public static FilesystemNode Root => new(null, "~", []);

    public static FilesystemNode
        CreateDirectory(FilesystemNode parent, string name, params FilesystemNode[] children)
    {
        if (parent is not { IsDirectory: true })
            throw new InvalidOperationException("Parent must be a directory");

        var node = new FilesystemNode(parent, name, children);

        parent._children!.Add(node);

        return node;
    }

    public static FilesystemNode CreateFile(FilesystemNode parent, string name, string? content = null)
    {
        if (parent is not { IsDirectory: true })
            throw new InvalidOperationException("Parent must be a directory");

        var node = new FilesystemNode(parent, name, content);

        parent._children!.Add(node);

        return node;
    }

    public Stream OpenWrite()
    {
        if (IsDirectory)
            throw new InvalidOperationException("Cannot write to a directory");

        return new FilesystemNodeStream(this);
    }

    public Stream OpenRead()
    {
        if (IsDirectory)
            throw new InvalidOperationException("Cannot read from a directory");

        return new MemoryStream(Encoding.UTF8.GetBytes(Content ?? ""));
    }

    public string Path => Parent is null ? Name : $"{Parent}/{Name}";

    public override string ToString() => Path;
}

sealed file class FilesystemNodeStream : Stream
{
    private readonly MemoryStream _stream = new();
    private readonly FilesystemNode _node;

    public FilesystemNodeStream(FilesystemNode node)
    {
        _node = node;

        var bytes = Encoding.UTF8.GetBytes(node.Content ?? "");
        _stream.Write(bytes, 0, bytes.Length);
        _stream.Position = 0;
    }

    public override void Flush()
    {
        _node.Content = Encoding.UTF8.GetString(_stream.ToArray());

        _stream.SetLength(0);
        _stream.Position = 0;
    }

    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }
}
