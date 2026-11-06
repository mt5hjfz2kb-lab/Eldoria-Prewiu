using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run --project SyntaxAudit.csproj -- <Unity-assets-folder>");
    return 2;
}
var files = Directory.GetFiles(args[0],"*.cs",SearchOption.AllDirectories).OrderBy(s=>s).ToArray();
if (files.Length != 5)
{
    Console.Error.WriteLine("FORJA_CSHARP_SOURCE_COUNT_FAIL: "+files.Length);
    return 3;
}
var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("UNITY_EDITOR","UNITY_6000_3_OR_NEWER");
var count = 0;
foreach(var file in files)
{
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file),parseOptions,path:file);
    var errors = tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
    if(errors.Length != 0)
    {
        foreach(var error in errors) Console.Error.WriteLine(error.ToString());
        return 1;
    }
    count++;
}
Console.WriteLine("FORJA_ROSLYN_SYNTAX_PASS: "+count+" C# files under UNITY_EDITOR");
Console.WriteLine("NOT_A_UNITY_API_OR_RUNTIME_COMPILE");
return 0;
