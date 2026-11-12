using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var paths = new[]
{
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186ArtKit.cs",
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186ArtAudit.cs",
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186NativeEvidence.cs",
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186NativeEventSystem.cs",
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186NativeRaycaster.cs",
    "Unity/Assets/Eldoria/Scripts/Domain/RewardExperienceRules.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/ResourceExperienceUiBridge.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186BagOverlay.cs",
    "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV41MailboxScreen.cs"
};
var opts = CSharpParseOptions.Default.WithPreprocessorSymbols("UNITY_EDITOR","UNITY_6000_0_OR_NEWER","UNITY_6000_3_OR_NEWER");
foreach (var file in paths)
{
    if (!File.Exists(file))
    {
        Console.Error.WriteLine("ISSUE186_SOURCE_NOT_FOUND " + file);
        return 2;
    }
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), opts, path:file);
    // Known real Unity compiler regression: Eldoria.Application is a namespace.
    // Syntax alone accepted Application.dataPath/isBatchMode but Unity resolved
    // that name to the game namespace and failed with CS0234. Inspect real
    // syntax nodes (not comments/strings), without pretending to typecheck Unity.
    var ambiguous=tree.GetRoot().DescendantNodes().OfType<MemberAccessExpressionSyntax>()
        .Where(n=>n.Expression is IdentifierNameSyntax id &&
                  id.Identifier.ValueText=="Application").ToArray();
    if(ambiguous.Length>0)
    {
        foreach(var node in ambiguous)
            Console.Error.WriteLine("ISSUE186_UNQUALIFIED_APPLICATION_API "+file+":"+
                (node.GetLocation().GetLineSpan().StartLinePosition.Line+1)+
                " — use explicit UnityEngine.Application inside Eldoria");
        return 1;
    }
    var errors = tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0)
    {
        foreach(var e in errors) Console.Error.WriteLine(e);
        return 1;
    }
}
Console.WriteLine("ISSUE186_ROSLYN_SYNTAX_PASS: " + paths.Length +
    " actual domain, Unity runtime and UNITY_EDITOR sources; explicit Application namespace guard; NOT an actual Unity compilation");
return 0;

