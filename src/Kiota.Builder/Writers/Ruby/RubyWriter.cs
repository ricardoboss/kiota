using Kiota.Builder.Filesystem;
using Kiota.Builder.PathSegmenters;

namespace Kiota.Builder.Writers.Ruby;
public class RubyWriter : LanguageWriter
{
    public RubyWriter(IFilesystem filesystem, string rootPath, string clientNamespaceName)
    {
        PathSegmenter = new RubyPathSegmenter(filesystem, rootPath, clientNamespaceName);
        var conventionService = new RubyConventionService();
        var pathSegmenter = new RubyPathSegmenter(filesystem, rootPath, clientNamespaceName);
        AddOrReplaceCodeElementWriter(new CodeClassDeclarationWriter(conventionService, clientNamespaceName, pathSegmenter));
        AddOrReplaceCodeElementWriter(new CodeBlockEndWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeNamespaceWriter(conventionService, pathSegmenter));
        AddOrReplaceCodeElementWriter(new CodeEnumWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeMethodWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodePropertyWriter(conventionService));
    }
}
