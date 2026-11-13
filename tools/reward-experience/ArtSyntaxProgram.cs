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
    "Unity/Assets/Eldoria/Issue186Art/Editor/Issue186BagCombinedNativeEvidence.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186NativeEventSystem.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186NativeRaycaster.cs",
    "Unity/Assets/Eldoria/Scripts/Domain/RewardExperienceRules.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/ResourceExperienceUiBridge.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186BagOverlay.cs",
    "Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186FirstUseCoach.cs",
    "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV41MailboxScreen.cs"
};
// Unity 6000.3.23f1 REAL attachment failure 43134015062: MonoBehaviours
// cannot be attached when their scripts live under a Unity Editor folder.
// Keep disposable UNITY_EDITOR-gated fixture helpers inside the owned runtime
// asmref and preserve their imported MonoImporter GUIDs across the move.
foreach (var fixture in new[]
{
    (Name: "Issue186NativeEventSystem", Guid: "e69a23728b864b85a194b9a6dfb2fb3b"),
    (Name: "Issue186NativeRaycaster", Guid: "17d6c579bf0e4986ac3f02b7ede2c3f3")
})
{
    var dir = "Unity/Assets/Eldoria/Issue186Art/";
    var source = dir + "Runtime/" + fixture.Name + ".cs";
    var forbidden = dir + "Editor/" + fixture.Name + ".cs";
    if (File.Exists(forbidden) || !File.Exists(source) ||
        !File.Exists(source + ".meta") ||
        !File.ReadAllText(source + ".meta").Contains("guid: " + fixture.Guid) ||
        !File.ReadAllText(source).Contains("#if UNITY_EDITOR"))
    {
        Console.Error.WriteLine("ISSUE186_UNATTACHABLE_EDITOR_FIXTURE_OR_GUID_MISMATCH " + fixture.Name);
        return 2;
    }
}
// Product regression: the introductory coach is installed into the ONE saved
// RoyalHUD. Source guards do not pretend to certify real Unity pointer hits.
string coachFile="Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186FirstUseCoach.cs";
string bagFile="Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186BagOverlay.cs";
string coachSource=File.ReadAllText(coachFile);
string bagSource=File.ReadAllText(bagFile);
string firstEntryFixture=File.ReadAllText("Unity/Assets/Eldoria/Issue186Art/Editor/Issue186BagCombinedNativeEvidence.cs");
string firstEntryGate=File.ReadAllText(".github/workflows/issue186-native.yml");
if(!File.Exists(coachFile+".meta") ||
   !File.ReadAllText(coachFile+".meta").Contains("MonoImporter:") ||
   !bagSource.Contains("coach.Initialize(owner,authority)") ||
   !bagSource.Contains("gameObject.AddComponent<Issue186FirstUseCoach>()") ||
   !coachSource.Contains("NarrativeCoherenceRules.Seen(state,\"start\")") ||
   !coachSource.Contains("SliceRules.CurrentObjectiveKey(state)") ||
   !coachSource.Contains("GuidedTutorialRules.ShouldPoint") ||
   !coachSource.Contains("Issue186ArtKit.BindFirstUsePointer") ||
   !coachSource.Contains("EventSystem.current") ||
   !coachSource.Contains("NoUiBlocker") ||
   !coachSource.Contains("no-real-physical-hit") ||
   !coachSource.Contains("state.PlayerName") ||
   !coachSource.Contains("ShowFirstNamePrompt()") ||
   !coachSource.Contains("nickname-input") ||
   !coachSource.Contains("nickname-confirm") ||
   !coachSource.Contains("narrative-continue") ||
   !firstEntryFixture.Contains("firstNicknameInputOnRealHit=true") ||
   !firstEntryFixture.Contains("firstNicknameConfirmOnRealHit=true") ||
   !firstEntryFixture.Contains("firstStoryContinueOnRealHit=true") ||
   !firstEntryFixture.Contains("Click(confirm,camera,target)") ||
   !firstEntryGate.Contains("@($r.images).Count -ne 24") ||
   !firstEntryGate.Contains("$r.realFirstHitClicks -ne 53") ||
   !File.ReadAllText("Unity/Assets/Eldoria/Issue186Art/Editor/Issue186BagCombinedNativeEvidence.cs")
       .Contains("firstSawmillHandOnRealHit") ||
   !File.ReadAllText("Unity/Assets/Eldoria/Issue186Art/Editor/Issue186BagCombinedNativeEvidence.cs")
       .Contains("Actual Aserradero tutorial hand did not find a frontmost hit") ||
   coachSource.Contains("new GameObject(\"Canvas\"") ||
   coachSource.Contains("gateway.Execute("))
{
    Console.Error.WriteLine("ISSUE186_FIRST_USE_REAL_UI_SOURCE_CONTRACT_FAILED");
    return 1;
}
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
foreach(var file in paths.Where(p=>p.EndsWith("Issue186NativeEventSystem.cs")||
                                   p.EndsWith("Issue186NativeRaycaster.cs")))
{
    var source=File.ReadAllText(file);
    var player=CSharpSyntaxTree.ParseText(source,CSharpParseOptions.Default
        .WithPreprocessorSymbols("UNITY_6000_0_OR_NEWER","UNITY_6000_3_OR_NEWER"));
    var editor=CSharpSyntaxTree.ParseText(source,opts);
    string name=Path.GetFileNameWithoutExtension(file);
    if(player.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
        .Any(n=>n.Identifier.ValueText==name) ||
       editor.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
        .Count(n=>n.Identifier.ValueText==name)!=1)
    {
        Console.Error.WriteLine("ISSUE186_NATIVE_HELPER_MUST_BE_EDITOR_ONLY "+file);
        return 1;
    }
}
Console.WriteLine("ISSUE186_ROSLYN_SYNTAX_PASS: " + paths.Length +
    " actual domain, Unity runtime and UNITY_EDITOR sources; explicit Application namespace guard; NOT an actual Unity compilation");
return 0;

