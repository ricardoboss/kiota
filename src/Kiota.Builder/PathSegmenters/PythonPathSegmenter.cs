using Kiota.Builder.CodeDOM;
using Kiota.Builder.Extensions;
using Kiota.Builder.Filesystem;

namespace Kiota.Builder.PathSegmenters;
public class PythonPathSegmenter : CommonPathSegmenter
{
    public PythonPathSegmenter(IFilesystem filesystem, string rootPath, string clientNamespaceName) : base(filesystem, rootPath, clientNamespaceName) { }
    public override string FileSuffix => ".py";
    public override string NormalizeFileName(CodeElement currentElement)
    {
        return currentElement switch
        {
            CodeNamespace => "__init__",
            _ => GetDefaultFileName(currentElement)
        };
    }
    private static string GetDefaultFileName(CodeElement currentElement) => GetLastFileNameSegment(currentElement).ToSnakeCase();
    public override string NormalizeNamespaceSegment(string segmentName) => segmentName.ToSnakeCase();
}
