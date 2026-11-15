using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;

if(args.Length!=1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run -- PROJECT_ROOT");
    return 2;
}
var root=Path.GetFullPath(args[0]);
var experiment=Path.Combine(root,"Unity","Assets","Eldoria","ExperimentalAshenCitadel");
var runtime=Path.Combine(experiment,"Runtime");
var editor=Path.Combine(experiment,"Editor");
var sources=Directory.GetFiles(runtime,"*.cs",SearchOption.TopDirectoryOnly)
    .Concat(Directory.GetFiles(editor,"*.cs",SearchOption.TopDirectoryOnly)).ToList();
if(sources.Count<4)throw new Exception("Experimental Unity code incomplete");
var opts=CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest)
    .WithPreprocessorSymbols("UNITY_EDITOR","UNITY_6000_0_OR_NEWER","UNITY_6000_3_OR_NEWER");
int errors=0;
// Prevent ambiguous namespace resolution; genuine Unity showed CS0234 twice
// while the syntax-only parser still returned success.
foreach(var path in sources)
{
    var text = File.ReadAllText(path);
    if(Regex.IsMatch(text, @"(?<!UnityEngine\\.)\\bApplication\\.(dataPath|persistentDataPath)"))
        throw new Exception("ASHEN_UNQUALIFIED_UNITY_APPLICATION: " + path);
}
foreach(var path in sources)
{
    var tree=CSharpSyntaxTree.ParseText(File.ReadAllText(path),opts,path:path);
    var diags=tree.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToList();
    foreach(var diag in diags){Console.Error.WriteLine(diag);errors++;}
    Console.WriteLine((diags.Count==0?"ASHEN_CSHARP_SYNTAX_PASS ":"ASHEN_CSHARP_SYNTAX_FAIL ")+
        Path.GetRelativePath(root,path));
}
if(errors>0)return 1;
var boot=File.ReadAllText(Path.Combine(root,"Unity","Assets","Eldoria",
    "Scripts","Presentation","SliceBoot.cs"));
if(!boot.Contains("SceneManager.GetActiveScene().name == \"AshenCitadel\""))
    throw new Exception("Legacy composition root may overwrite experimental scene");
var ui=File.ReadAllText(Path.Combine(runtime,"AshenCitadelExperience.cs"));
var screens=File.ReadAllText(Path.Combine(runtime,"AshenCitadelExtendedUI.cs"));
var editorSrc=File.ReadAllText(Path.Combine(editor,"AshenCitadelSceneBuilder.cs"));
var gateway=File.ReadAllText(Path.Combine(root,"Unity","Assets","Eldoria","Scripts","Application","LocalGateway.cs"));
var states=File.ReadAllText(Path.Combine(root,"Unity","Assets","Eldoria","Scripts","Domain","GameState.cs"));
foreach(var contract in new[]{"AdvanceBastion","UpgradeBuilding","ConfigureMarch","Recruit","Gather",
    "Fight","ClaimDailyTreasure","MailRead","MailDelete","ChooseDevelopment","InvestigateFissure",
    "ContactLyra","VigilantBattle","ScoutDispatch","ScoutClaim"})
{
    if(!ui.Contains(contract)&&!screens.Contains(contract))
        throw new Exception("Missing UI command "+contract);
    if(!gateway.Contains(contract) && !File.ReadAllText(Path.Combine(root,"Unity","Assets","Eldoria",
        "Scripts","Domain","EarlyRetentionRules.cs")).Contains(contract))
        throw new Exception("No authoritative command contract "+contract);
}
foreach(var marker in new[]{"stages[i].SetActive(!worldMode && i==visibleStage-1)",
    "s.SawmillLevel>0","s.BarracksLevel>0","s.ChapterThree.GranaryLevel>0",
    "s.ChapterThree.StoneworksLevel>0","SetRuntimeProfileOverride","RefreshClockLabel"})
    if(!ui.Contains(marker))throw new Exception("State-controlled art marker missing: "+marker);
foreach(var marker in new[]{"ScenePath","NewScene","SaveScene","BindClick","AshenBastion_","AddRift"})
    if(!editorSrc.Contains(marker))throw new Exception("Incomplete isolated scene authoring: "+marker);
if(!ui.Contains("eldoria-ashen-experiment-v1.json"))
    throw new Exception("Experiment could read the production save");
if(ui.Contains("state.BastionLevel=")||screens.Contains("state.BastionLevel="))
    throw new Exception("No fabricated level changes allowed");
if(states.Contains("BastionLevel = 1") && !gateway.Contains("BastionProgressionCatalog.RequirementsMet"))
    throw new Exception("Gateway progression contract broken");
Console.WriteLine("ASHEN_GATEWAY_BINDINGS_STATIC_PASS_NOT_UNITY_PLAYMODE");
return 0;
