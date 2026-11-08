using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

if(args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run --project SyntaxAudit.csproj -- <BreachWatch folder>");
    return 2;
}
var files = Directory.GetFiles(args[0],"*.cs",SearchOption.AllDirectories)
    .OrderBy(p=>p,StringComparer.Ordinal).ToArray();
if(files.Length < 5)
{
    Console.Error.WriteLine("BREACHWATCH_SOURCE_INCOMPLETE: only "+files.Length+" C# files");
    return 3;
}
var options=CSharpParseOptions.Default.WithPreprocessorSymbols("UNITY_EDITOR","UNITY_6000_3_OR_NEWER");
foreach(var file in files)
{
    var syntax=CSharpSyntaxTree.ParseText(File.ReadAllText(file),options,path:file);
    var problems=syntax.GetDiagnostics().Where(x=>x.Severity==DiagnosticSeverity.Error).ToArray();
    if(problems.Length == 0) continue;
    foreach(var diagnostic in problems) Console.Error.WriteLine(diagnostic);
    return 1;
}
Console.WriteLine("BREACHWATCH_ALL_SOURCE_ROSLYN_SYNTAX_PASS: "+files.Length+
    " independent runtime/editor C# sources, includes UNITY_EDITOR branch");
Console.WriteLine("NOT A UNITY API COMPILE OR ART APPROVAL");
return 0;
