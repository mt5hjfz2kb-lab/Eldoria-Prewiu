using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var paths = new[]
{
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186ArtKit.cs",
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186ArtAudit.cs",
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186NativeEvidence.cs",
    "Unity/Assets/Eldoria/Scripts/Domain/RewardExperienceRules.cs"
};
var opts = CSharpParseOptions.Default.WithPreprocessorSymbols("UNITY_EDITOR","UNITY_6000_3_OR_NEWER");
foreach (var file in paths)
{
    if (!File.Exists(file))
    {
        Console.Error.WriteLine("ISSUE186_SOURCE_NOT_FOUND " + file);
        return 2;
    }
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), opts, path:file);
    var errors = tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0)
    {
        foreach(var e in errors) Console.Error.WriteLine(e);
        return 1;
    }
}
Console.WriteLine("ISSUE186_ROSLYN_SYNTAX_PASS: " + paths.Length +
    " actual domain, Unity runtime and UNITY_EDITOR sources; NOT an actual Unity compilation");
return 0;
