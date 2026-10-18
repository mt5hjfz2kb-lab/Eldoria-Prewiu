using System;
using System.Globalization;
namespace Eldoria.UI.RoyalGold {
 public enum RoyalSection { Realm, Heroes, Army, Missions, Inventory, World, Bastion, Settings }
 public sealed class RoyalAction {
  public readonly string Id, Kind, Target, Label, Reason; public readonly long Revision; public readonly bool Enabled;
  public RoyalAction(string kind,string target,string label,string reason,long revision,bool enabled) { Id=Guid.NewGuid().ToString("N");Kind=kind??"";Target=target??"";Label=label??"";Reason=reason??"";Revision=revision;Enabled=enabled; }
 }
 public sealed class RoyalBuilding {
  public string Id="", Name="", Function="", State="", Requirements="", Costs="", Time="", Result="";
  public int Level; public float Progress; public bool OperationActive; public RoyalAction Action;
 }
 public sealed class RoyalSnapshot {
  public long Revision; public int Wood, Stone, Food, Power, BastionLevel, Available, Wounded, Prepared, HeroPower;
  public string Realm="Valoria", Chapter="", Objective="", ObjectiveProgress="", March="", Hero="", Profile="";
  public float MissionFraction; public bool IsTest, ShowFood; public RoyalBuilding Building;
 }
 public readonly struct RoyalResult {
  public readonly bool Ok; public readonly string Message;
  public RoyalResult(bool ok,string message){Ok=ok;Message=message??"";}
 }
 public interface IRoyalSession {
  RoyalSnapshot Read(string selectedBuilding, bool english);
  RoyalResult Submit(RoyalAction offer);
 }
 public interface IRoyalNavigation {
  bool CanOpen(RoyalSection section);
  void Open(RoyalSection section);
 }
 public static class RoyalFormatting {
  public static string Amount(int value,bool english=false) => value.ToString("N0",CultureInfo.GetCultureInfo(english?"en-US":"es-ES"));
  public static string Remaining(long end,long now,bool english=false) {
   if(end<=0)return ""; if(end<=now)return english?"Completing…":"Finalizando…";
   long seconds=(long)Math.Ceiling((end-now)/(double)TimeSpan.TicksPerSecond);
   return seconds>=3600?TimeSpan.FromSeconds(seconds).ToString(@"h\:mm\:ss"):TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");
  }
  public static string L(bool en,string es,string english)=>en?english:es;
 }
}
