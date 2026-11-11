using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var root=Path.GetFullPath(".");
var files=new[]{
  "Unity/Assets/Eldoria/Scripts/Domain/DailyTreasureRules.cs",
  "Unity/Assets/Eldoria/Scripts/Domain/GameState.cs",
  "Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureBastionCamera.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureChestGraphic.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRewardPresentation.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePrizeFlight.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRaysGraphic.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureMilestoneRules.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureMilestoneQueue.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureCeremonyTheme.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureFoley.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureStageDirection.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRevealTimeline.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureReliefGraphic.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasureRewardGlyph.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Editor/DailyTreasureNativeEvidence.cs",
  "Unity/Assets/Eldoria/DailyTreasures/Editor/DailyTreasureCityArtEvidence.cs",
  "Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3EarlyRetentionScreens.cs"
};
int errors=0;
var options=CSharpParseOptions.Default.WithPreprocessorSymbols("UNITY_EDITOR","UNITY_6000_3_OR_NEWER");
foreach(var name in files){
  var path=Path.Combine(root,name);
  if(!File.Exists(path)){Console.Error.WriteLine("MISSING source "+name);errors++;continue;}
  var tree=CSharpSyntaxTree.ParseText(File.ReadAllText(path),options,path:name);
  foreach(var diagnostic in tree.GetDiagnostics().Where(x=>x.Severity==DiagnosticSeverity.Error)){
    Console.Error.WriteLine(diagnostic);
    errors++;
  }
}
if(errors>0){Console.Error.WriteLine("DAILY_TREASURES_ROSLYN_SYNTAX_FAIL: "+errors);return 1;}
Console.WriteLine("PASS daily treasures Roslyn C# syntax: "+files.Length+" files. NOT Unity API compilation or visual approval");
return 0;
