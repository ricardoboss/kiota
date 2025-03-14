using Kiota.Builder.Filesystem;
using Kiota.Builder.PathSegmenters;

namespace Kiota.Builder.Writers.Swift;
public class SwiftWriter : LanguageWriter
{
    public SwiftWriter(IFilesystem filesystem, string rootPath, string clientNamespaceName)
    {
        PathSegmenter = new SwiftPathSegmenter(filesystem, rootPath, clientNamespaceName);
        var conventionService = new SwiftConventionService(clientNamespaceName);
        AddOrReplaceCodeElementWriter(new CodeClassDeclarationWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeBlockEndWriter());
        AddOrReplaceCodeElementWriter(new CodePropertyWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeNamespaceWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeEnumWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeMethodWriter(conventionService));
    }
}
