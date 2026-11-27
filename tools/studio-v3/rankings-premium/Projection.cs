using System.Text.Json;
using Eldoria.LegacyProgression;
var now=new DateTime(2026,11,24,13,0,0,DateTimeKind.Utc);
object Project(int level){
 var f=new ProgressFacts{Bastion=level,TotalPower=2452,KingdomPower=600,ArmyPower=648,VigilantStage=level>=2?5:0,ScoutReports=level>=3?3:0,DiscoveryMask=level>=3?3:0,RecordedHunts=level>=3?1:0,JourneyComplete=level>=3,RouteCleared=level>=3,EngendroDefeated=level>=2,LyraRecruited=level>=4,FissureClosed=level>=4,AshStalkerDefeated=level>=5};
 var p=new LocalProfile("Tester QA",f); // No multiplayer, no fake position or save integration.
 var sections=ProfilePresentation.Sections(p).Select(s=>new {id=s.ToString(),label=ProfilePresentation.Label(s),rows=ProfilePresentation.Rows(p,s,now).Select(e=>new {key=e.Key,title=e.Title,detail=e.Detail,state=e.State.ToString(),command=e.CommandKind,target=e.CommandTarget}).ToArray()}).ToArray();
 var details=p.Discoveries.ToDictionary(c=>c.Id,c=>ProfilePresentation.DiscoveryEntries(p,c.Id).Select(e=>new {key=e.Key,title=e.Title,detail=e.Detail,state=e.State.ToString(),command=e.CommandKind,target=e.CommandTarget}).ToArray());
 if(p.PublicRankingsAvailable||sections.SelectMany(s=>s.rows).Any(e=>e.command!=""))throw new Exception("Unexpected public ranking or command");
 if(level==1&&sections.Length!=1||level==5&&sections.Length!=6)throw new Exception("Navigation gate");
 return new {bastion=level,sections,details,publicRankings=p.PublicRankingsAvailable};
}
Console.WriteLine(JsonSerializer.Serialize(new {sourceSha="230bdf286c8b0b2cc0a1fe4aef0af797fcb9d84a",fixture=true,profiles=new[]{Project(1),Project(2),Project(3),Project(4),Project(5)}}));