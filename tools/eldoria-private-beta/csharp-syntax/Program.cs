using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// PR179 read-only syntax proof, using the Roslyn compiler already shipped with
// .NET SDK 10. It checks both editor/test and WebGL preprocessor paths. This
// cannot replace real Unity compilation, runtime, physical iPhone or Android.
static Diagnostic[] SyntaxErrors(string source, string path, string[] defines)
{
    var parse=CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.Latest)
        .WithPreprocessorSymbols(defines);
    return CSharpSyntaxTree.ParseText(source,parse,path:path)
        .GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
}
if(args is ["--self-test"])
{
    foreach(var defines in new[]{new[]{"UNITY_EDITOR","UNITY_INCLUDE_TESTS"},
                                  new[]{"UNITY_WEBGL"}})
    {
        if(SyntaxErrors("namespace X { class Broken { void F( { } }",
                        "invalid.cs",defines).Length==0)
            throw new Exception("Roslyn did not reject malformed Unity source");
        if(SyntaxErrors("namespace X { public sealed class Valid {} }",
                        "valid.cs",defines).Length!=0)
            throw new Exception("Roslyn incorrectly rejected valid C#");
    }
    Console.WriteLine("PR179_ROSLYN_FAULT_INJECTION_PASS_SOURCE_ONLY");
    return 0;
}
if(args.Length!=1 || !Directory.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: csharp-syntax <repository-root> OR --self-test");
    return 2;
}
var root=Path.GetFullPath(args[0]);
var sources=new[]
{
    "Unity/Assets/Eldoria/World4XCanon/RegionOneBlenderLandscape.cs",
    "Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs",
    "Unity/Assets/Eldoria/World4XCanon/RegionOneResourcePlacement.cs",
    "Unity/Assets/Eldoria/World4XCanon/RegionOneTerrainRoutes.cs",
    "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs",
    "Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaIntegrationBuild.cs",
    "Unity/Assets/Eldoria/BetaMemoryProbe/Tests/PlayMode/LazyRegionStartupTests.cs"
}.ToList();
var ux=Path.Combine(root,"tools/eldoria-private-beta/native/ux");
using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(ux,"manifest.json")));
var frozen=doc.RootElement.GetProperty("files").EnumerateArray().ToArray();
if(frozen.Length!=19)throw new Exception("Expected exactly 19 frozen UX sources");
var overlays=new HashSet<string>(StringComparer.Ordinal);
foreach(var entry in frozen)
{
    var filename=entry.GetProperty("overlay").GetString()
        ?? throw new Exception("Missing frozen overlay");
    if(!filename.EndsWith(".cs.txt",StringComparison.Ordinal) || filename.Contains("..") ||
       Path.IsPathRooted(filename) || filename.Contains('\\') || filename.Contains('/'))
        throw new Exception("Unexpected frozen C# overlay path: "+filename);
    if(!overlays.Add(filename))throw new Exception("Duplicate frozen C# source: "+filename);
    sources.Add("tools/eldoria-private-beta/native/ux/"+filename);
}
var configs=new (string name,string[] defines)[]
{
    ("Editor PlayMode",new[]{"UNITY_EDITOR","UNITY_INCLUDE_TESTS","UNITY_6000_3_OR_NEWER"}),
    ("WebGL player",new[]{"UNITY_WEBGL","UNITY_6000_3_OR_NEWER"}),
    ("Windows player",new[]{"UNITY_STANDALONE_WIN","UNITY_6000_3_OR_NEWER"})
};
int checkedFiles=0;
foreach(var rel in sources)
{
    var file=Path.Combine(root,rel.Replace('/',Path.DirectorySeparatorChar));
    if(!File.Exists(file))throw new FileNotFoundException("Required source absent",file);
    var source=File.ReadAllText(file);
    foreach(var mode in configs)
    {
        var problems=SyntaxErrors(source,rel,mode.defines);
        if(problems.Length==0)continue;
        foreach(var problem in problems)Console.Error.WriteLine(mode.name+": "+problem);
        return 1;
    }
    checkedFiles++;
}
if(checkedFiles!=26)throw new Exception("Source membership changed: "+checkedFiles);
Console.WriteLine("PR179_ROSLYN_SYNTAX_PASS_EDITOR_WEBGL_WINDOWS: "+checkedFiles+
    " source files x 3 preprocessor modes = "+(checkedFiles*3)+" parsed versions.");
Console.WriteLine("SOURCE-ONLY: no Unity API type-check, PlayMode execution or device PASS");
return 0;
