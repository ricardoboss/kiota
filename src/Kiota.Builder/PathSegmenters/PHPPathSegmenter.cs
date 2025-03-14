using System;
using Kiota.Builder.CodeDOM;
using Kiota.Builder.Extensions;
using Kiota.Builder.Filesystem;

namespace Kiota.Builder.PathSegmenters;
public class PhpPathSegmenter : CommonPathSegmenter
{
    public PhpPathSegmenter(IFilesystem filesystem, string rootPath, string clientNamespaceName) : base(filesystem, rootPath, clientNamespaceName) { }
    public override string FileSuffix => ".php";
    private static readonly char[] pathSeparators = ['.', '\\'];
    public override string NormalizeNamespaceSegment(string segmentName) => segmentName.ToFirstCharacterUpperCase();
    protected static new string GetLastFileNameSegment(CodeElement currentElement) => currentElement?.Name.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries)[^1] ?? string.Empty;
    public override string NormalizeFileName(CodeElement currentElement) => GetLastFileNameSegment(currentElement).ToFirstCharacterUpperCase();
}
