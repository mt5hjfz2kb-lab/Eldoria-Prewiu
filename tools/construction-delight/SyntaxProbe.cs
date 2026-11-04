using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxProbe
{
    static int Main()
    {
        var files=new[]{
            "Unity/Assets/Eldoria/ConstructionDelight/ValoriaConstructionDelightV1.cs",
            "Unity/Assets/Eldoria/ConstructionDelight/Editor/ConstructionDelightNativeCapture.cs",
            "Unity/Assets/Eldoria/Scripts/Presentation/ConstructionDelightState.cs"
        };
        int errors=0;
        foreach(string path in files)
        {
            if(!File.Exists(path))
            {
                Console.Error.WriteLine("MISSING "+path);
                errors++;
                continue;
            }
            var text=File.ReadAllText(path);
            var tree=CSharpSyntaxTree.ParseText(text,
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest),
                path);
            var failures=tree.GetDiagnostics()
                .Where(x=>x.Severity==DiagnosticSeverity.Error).ToArray();
            foreach(var error in failures)
            {
                Console.Error.WriteLine("C_SHARP_SYNTAX_ERROR "+error.Id+
                    " "+error.Location.GetLineSpan()+" "+error.GetMessage());
                errors++;
            }
            Console.WriteLine("CONSTRUCTION_DELIGHT_SYNTAX_FILE "+
                              path+" syntax_errors="+failures.Length);
            if(path.EndsWith("/ValoriaConstructionDelightV1.cs",StringComparison.Ordinal))
            {
                // Prevent a real regression found in Unity's scene-site
                // fallback: VisibleBounds accidentally called itself forever.
                // A syntax-only green check cannot catch that recursion.
                var methods=tree.GetCompilationUnitRoot().DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m=>m.Identifier.ValueText=="VisibleBounds").ToArray();
                if(methods.Length!=1)
                {
                    Console.Error.WriteLine("CONSTRUCTION_DELIGHT_BOUNDS_GUARD_FAIL "+
                        "expected exactly one VisibleBounds method");
                    errors++;
                }
                else
                {
                    var method=methods[0];
                    bool callsSelf=method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Any(call=>call.Expression is IdentifierNameSyntax ident &&
                                   ident.Identifier.ValueText=="VisibleBounds");
                    string body=method.Body?.ToFullString()??"";
                    bool inspectsActualRenderers=body.Contains("GetComponentsInChildren<Renderer>",
                                                              StringComparison.Ordinal);
                    bool aggregatesActualBounds=body.Contains("bounds.Encapsulate",
                                                              StringComparison.Ordinal);
                    if(callsSelf || !inspectsActualRenderers || !aggregatesActualBounds)
                    {
                        Console.Error.WriteLine("CONSTRUCTION_DELIGHT_BOUNDS_GUARD_FAIL "+
                            "recursive or non-renderer VisibleBounds fallback");
                        errors++;
                    }
                    else Console.WriteLine("CONSTRUCTION_DELIGHT_BOUNDS_GUARD_PASS "+
                                           "scaffold bounds from visible renderers; no self recursion");
                }
            }
        }
        if(errors>0)return 1;
        Console.WriteLine("CONSTRUCTION_DELIGHT_SYNTAX_PASS files="+files.Length+
                          " compiler=RoslynSDK runtime_Unity_API_not_verified");
        return 0;
    }
}
