using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

if(args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run -- <the exact Unity Editor-only pilot .cs>");
    return 2;
}
var file = args[0];
var options = CSharpParseOptions.Default.WithPreprocessorSymbols("UNITY_EDITOR", "UNITY_6000_3_OR_NEWER");
var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(file), options, path:file);
var diagnostics = syntax.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
foreach (var d in diagnostics) Console.Error.WriteLine(d);
if(diagnostics.Length > 0) return 1;
Console.WriteLine("BASTION_CARD_ROSLYN_PARSE_PASS_NOT_UNITY_COMPILE");
return 0;
