using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
        }
        if(errors>0)return 1;
        Console.WriteLine("CONSTRUCTION_DELIGHT_SYNTAX_PASS files="+files.Length+
                          " compiler=RoslynSDK runtime_Unity_API_not_verified");
        return 0;
    }
}
