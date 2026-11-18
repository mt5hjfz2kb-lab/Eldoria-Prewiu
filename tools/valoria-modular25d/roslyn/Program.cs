using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

if (args.Length != 1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Expected Unity experiment directory argument");
    return 2;
}
string[] sources = Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories);
if (sources.Length < 6)
{
    Console.Error.WriteLine("Missing C# source files");
    return 2;
}
int issues=0, examined=0;
foreach (string path in sources.OrderBy(x=>x,StringComparer.Ordinal))
{
    string code=File.ReadAllText(path);
    foreach(bool editor in new[]{true,false})
    foreach(bool input in new[]{true,false})
    {
        var symbols=new List<string>{"UNITY_6000_0_OR_NEWER"};
        if(editor)symbols.Add("UNITY_EDITOR");
        if(input)symbols.Add("ENABLE_INPUT_SYSTEM");
        var options=new CSharpParseOptions(LanguageVersion.Latest,
            preprocessorSymbols:symbols);
        var tree=CSharpSyntaxTree.ParseText(code,options,path:path);
        var errors=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToList();
        foreach(var diagnostic in errors)
        {
            Console.Error.WriteLine(path+" editor="+editor+" input="+input+" "+diagnostic);
            issues++;
        }
        examined++;
    }
}
Console.WriteLine("Roslyn C# parse configurations="+examined+" files="+sources.Length+" syntaxErrors="+issues);
Console.WriteLine("Syntax only. No Unity assemblies, code generation, compile, PlayMode, or art approval.");
return issues==0 ? 0:1;
