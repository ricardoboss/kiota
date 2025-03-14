using Kiota.Builder.Filesystem;
using Kiota.Builder.PathSegmenters;

namespace Kiota.Builder.Writers.CSharp;
public class CSharpWriter : LanguageWriter
{
    public CSharpWriter(IFilesystem filesystem, string rootPath, string clientNamespaceName)
    {
        PathSegmenter = new CSharpPathSegmenter(filesystem, rootPath, clientNamespaceName);
        var conventionService = new CSharpConventionService();
        AddOrReplaceCodeElementWriter(new CodeClassDeclarationWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeBlockEndWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeEnumWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeIndexerWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeMethodWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodePropertyWriter(conventionService));
        AddOrReplaceCodeElementWriter(new CodeTypeWriter(conventionService));

    }
}
