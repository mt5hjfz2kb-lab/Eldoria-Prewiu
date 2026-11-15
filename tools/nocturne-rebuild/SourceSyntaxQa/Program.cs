using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;

if (args.Length == 0)
    throw new ArgumentException("Pass all Nocturne C# source paths for Roslyn syntax parsing");
int parsed=0;
foreach (string path in args)
{
    if (!File.Exists(path)) throw new FileNotFoundException("C# source missing",path);
    string source=File.ReadAllText(path);
    var tree=CSharpSyntaxTree.ParseText(source,
        new CSharpParseOptions(LanguageVersion.CSharp12),path);
    var errors=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
    foreach(var error in errors)Console.Error.WriteLine(error.ToString());
    if(errors.Length>0)throw new InvalidDataException(path+" has syntax errors: "+errors.Length);
    if(!source.Contains("namespace Eldoria") && !path.EndsWith("SliceBoot.cs"))
        throw new InvalidDataException("Unexpected C# source scope: "+path);
    parsed++;
    Console.WriteLine("ROSLYN_SYNTAX_PASS "+path);
}
Console.WriteLine("NOCTURNE_SOURCE_SYNTAX_ONLY_NOT_UNITY_COMPILATION_PASS "+parsed);
