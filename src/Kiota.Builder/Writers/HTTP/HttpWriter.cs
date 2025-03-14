using Kiota.Builder.Filesystem;
using Kiota.Builder.PathSegmenters;

namespace Kiota.Builder.Writers.Http;

public class HttpWriter : LanguageWriter
{
    public HttpWriter(IFilesystem filesystem, string rootPath, string clientNamespaceName)
    {
        PathSegmenter = new HttpPathSegmenter(filesystem, rootPath, clientNamespaceName);
        var conventionService = new HttpConventionService();
        AddOrReplaceCodeElementWriter(new CodeClassDeclarationWriter(conventionService));
        AddOrReplaceCodeElementWriter(new GenericCodePropertyWriter(conventionService));
        AddOrReplaceCodeElementWriter(new GenericCodeMethodWriter(conventionService));
        AddOrReplaceCodeElementWriter(new GenericCodeElementWriter(conventionService));
    }
}
