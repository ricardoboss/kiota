using Kiota.Builder.Filesystem;
using Kiota.Builder.PathSegmenters;

namespace Kiota.Builder.Writers.Java;
public class JavaWriter : LanguageWriter
{
    public JavaWriter(IFilesystem filesystem, string rootPath, string clientNamespaceName)
    {
        PathSegmenter = new JavaPathSegmenter(filesystem, rootPath, clientNamespaceName);
        var conventionService = new JavaConventionService();
        AddOrReplaceCodeElementWriter(new CodeClassDeclarationWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeBlockEndWriter());
        AddOrReplaceCodeElementWriter(new CodeEnumWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeMethodWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodePropertyWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeTypeWriter(conventionService));
    }
}
