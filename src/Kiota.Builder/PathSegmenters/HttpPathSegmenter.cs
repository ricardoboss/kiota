using Kiota.Builder.CodeDOM;
using Kiota.Builder.Extensions;
using Kiota.Builder.Filesystem;

namespace Kiota.Builder.PathSegmenters;
public class HttpPathSegmenter(IFilesystem filesystem, string rootPath, string clientNamespaceName) : CommonPathSegmenter(filesystem, rootPath, clientNamespaceName)
{
    public override string FileSuffix => ".http";
    public override string NormalizeNamespaceSegment(string segmentName) => segmentName.ToFirstCharacterUpperCase();
    public override string NormalizeFileName(CodeElement currentElement)
    {
        return GetLastFileNameSegment(currentElement).ToFirstCharacterUpperCase();
    }
}
