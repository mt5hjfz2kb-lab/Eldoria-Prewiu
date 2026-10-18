using System;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Application;
namespace Eldoria.UI.RoyalGold {
 /// <summary>Presentation adapter only. Does not create a gateway, advance time, save, choose a profile or mutate a PlayerState.</summary>
 public sealed class ExistingSliceBinding : IRoyalSession {
  readonly ICommandGateway gateway; readonly IClock clock; readonly bool test;
  readonly HashSet<string> submitted=new HashSet<string>(); readonly Queue<string> recent=new Queue<string>();
  bool submitting;
  public ExistingSliceBinding(ICommandGateway gateway,IClock clock,bool test=false){this.gateway=gateway??throw new ArgumentNullException(nameof(gateway));this.clock=clock??throw new ArgumentNullException(nameof(clock));this.test=test;}
  public RoyalSnapshot Read(string selection,bool en){
   var s=gateway.Snapshot();if(s==null)throw new InvalidOperationException("PlayerState unavailable");
   var p=SliceRules.TotalPower(s);
   var d=new RoyalSnapshot{Revision=s.Revision,Wood=s.Resources.Wood,Stone=s.Resources.Stone,Food=s.Resources.Food,Power=p.Total,
    BastionLevel=s.BastionLevel,Available=s.Available.Total,Wounded=s.Wounded.Total,Prepared=s.PreparedTroops.Total,HeroPower=p.Heroes,
    Hero=s.PreparedHeroId=="aldric"?"Sir Aldric":s.PreparedHeroId,IsTest=test,Profile=SliceContentProfiles.ActiveRuntimeProfile,
    Chapter=L(en,s.BastionLevel<2?"CAPÍTULO I":"CAPÍTULO II",s.BastionLevel<2?"CHAPTER I":"CHAPTER II"),
    March=MarchText(s,en)};
   Mission(s,d,en); d.Building=Building(s,selection,en);return d;
  }
  public RoyalResult Submit(RoyalAction offer){
   if(offer==null||!offer.Enabled||string.IsNullOrEmpty(offer.Kind))return new RoyalResult(false,"Acción no disponible / Action unavailable");
   if(submitting||submitted.Contains(offer.Id))return new RoyalResult(false,"Solicitud ya enviada / Already submitted");
   var current=gateway.Snapshot();if(current.Revision!=offer.Revision)return new RoyalResult(false,"El estado cambió. Revisa los requisitos / State changed; review requirements");
   submitting=true;submitted.Add(offer.Id);recent.Enqueue(offer.Id);while(recent.Count>64)submitted.Remove(recent.Dequeue());
   try {var r=gateway.Execute(new GameCommand("royal-ui:"+offer.Id,current.PlayerId,offer.Kind,offer.Target,offer.Revision));return new RoyalResult(r.Ok,r.Message);}
   finally {submitting=false;}
  }
  static string L(bool en,string es,string eg)=>RoyalFormatting.L(en,es,eg);
  string MarchText(PlayerState s,bool en){
   string phase=s.March.Phase??"idle";string label;
   switch(phase){case "outbound":label=L(en,"En marcha","Outbound");break;case "gathering":label=L(en,"Recolectando","Gathering");break;case "returning":label=L(en,"Regresando","Returning");break;default:label=L(en,"Marcha disponible","March ready");break;}
   return label+(phase=="idle"?"":" · "+RoyalFormatting.Remaining(s.March.PhaseEndsUtcTicks,clock.UtcTicks,en));
  }
  static void Mission(PlayerState s,RoyalSnapshot d,bool en){
   var p=s.ChapterProgress;string key=SliceRules.CurrentObjectiveKey(s);int done=0,total=1;string title;
   switch(key){
    case "b1.build-sawmill":title=L(en,"Construye el aserradero","Build the sawmill");done=s.SawmillLevel;break;
    case "b1.gather-wood":title=L(en,"Reúne madera para Valoria","Gather wood for Valoria");done=p?.GatheredWood??0;total=SliceContentProfiles.Active.Chapter1GatherWood;break;
    case "b1.gather-stone":title=L(en,"Reúne piedra para el Bastión","Gather stone for the Bastion");done=p?.GatheredStone??0;total=SliceContentProfiles.Active.Chapter1GatherStone;break;
    case "b1.clear-route":title=L(en,"Despeja la ruta corrupta","Clear the corrupted route");break;
    case "b1.return":title=L(en,"Regresa a Valoria","Return to Valoria");break;
    case "b1.ascend":title=L(en,"Mejora el Bastión a nivel II","Upgrade the Bastion to level II");break;
    case "b2.build-barracks":title=L(en,"Construye el cuartel","Build the barracks");done=s.BarracksLevel;break;
    case "b2.train-archers":title=L(en,"Entrena arqueros","Train archers");done=p?.TrainedArchers??0;total=SliceContentProfiles.Active.Chapter2TrainArchers;break;
    case "b2.prepare-march":title=L(en,"Prepara y confirma la marcha","Prepare and confirm the march");break;
    case "b2.raise-expedition-power":title=L(en,"Refuerza la expedición","Strengthen the expedition");done=p?.ConfirmedExpeditionPower??0;total=SliceContentProfiles.Active.Chapter2ExpeditionPower;break;
    case "b2.defeat-engendro":title=L(en,"Derrota al Engendro","Defeat the Spawn");break;
    case "b2.complete":title=L(en,"Bastión II asegurado","Bastion II secured");done=1;break;
    default:title=L(en,"Sin objetivo disponible","No objective available");total=0;break;
   }
   d.Objective=title;d.MissionFraction=total>0?Math.Min(1f,Math.Max(0f,done/(float)total)):0;
   d.ObjectiveProgress=total>0?Math.Min(done,total)+" / "+total:"";
  }
  RoyalBuilding Building(PlayerState s,string id,bool en){
   if(string.IsNullOrEmpty(id))return null;
   var b=new RoyalBuilding{Id=id};int wood=0,stone=0,duration=0;string command="",target=id,action="",reason="";long deadline=0;
   bool constructionBusy=s.BuildingCompletesUtcTicks>0||s.BastionUpgradeCompletesUtcTicks>0;
   if(id=="sawmill"){
    b.Name=L(en,"Aserradero","Sawmill");b.Level=s.SawmillLevel;b.Function=L(en,"Producción de madera para el reino.","Wood production for the realm.");
    if(b.Level==0){wood=SliceRules.SawmillWoodCost;duration=SliceRules.SawmillBuildSeconds;command="Build";action=L(en,"Construir","Build");if(constructionBusy)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");}
    if((s.BuildingTaskId??"").StartsWith("sawmill:",StringComparison.Ordinal))deadline=s.BuildingCompletesUtcTicks;
   }else if(id=="barracks"){
    b.Name=L(en,"Cuartel","Barracks");b.Level=s.BarracksLevel;b.Function=L(en,"Recluta y prepara a tus arqueros.","Recruit and prepare your archers.");
    if(b.Level==0){wood=SliceRules.BarracksWoodCost;stone=SliceRules.BarracksStoneCost;duration=SliceRules.BarracksBuildSeconds;command="Build";action=L(en,"Construir","Build");if(s.BastionLevel<2)reason=L(en,"Requiere Bastión II.","Requires Bastion II.");else if(constructionBusy)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");}
    else{wood=SliceRules.RecruitWoodCost;stone=SliceRules.RecruitStoneCost;duration=SliceRules.RecruitSeconds;command="Recruit";target="archer:t1";action=L(en,"Entrenar ","Train ")+SliceRules.RecruitArchers+L(en," arqueros"," archers");deadline=s.RecruitmentCompletesUtcTicks;}
    if((s.BuildingTaskId??"").StartsWith("barracks:",StringComparison.Ordinal))deadline=s.BuildingCompletesUtcTicks;
   }else if(id=="bastion"){
    b.Name=L(en,"Bastión","Bastion");b.Level=s.BastionLevel;b.Function=L(en,"Centro de gobierno y progreso de Valoria.","Valoria's centre of government and progression.");
    if(b.Level==1){wood=SliceRules.Bastion2WoodCost;stone=SliceRules.Bastion2StoneCost;duration=SliceContentProfiles.WebContract.Bastion2UpgradeSeconds;command="AdvanceBastion";action=L(en,"Mejorar a Bastión II","Upgrade to Bastion II");if(!s.JourneyComplete)reason=L(en,"Completa los objetivos de Bastión I.","Complete the Bastion I objectives.");else if(constructionBusy)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");}
    deadline=s.BastionUpgradeCompletesUtcTicks;
   }else if(id=="march-main"){
    b.Name=L(en,"Marcha","March");b.Function=L(en,"Confirma las tropas disponibles para la expedición.","Confirm available troops for the expedition.");command="ConfigureMarch";action=L(en,"Confirmar marcha","Confirm march");
    if(s.BastionLevel<2||s.BarracksLevel<1)reason=L(en,"Requiere Bastión II y cuartel.","Requires Bastion II and barracks.");else if(s.March.Phase!="idle")reason=L(en,"Espera al regreso de la marcha.","Wait for the march to return.");else if(s.Available.Total<1)reason=L(en,"No hay tropas disponibles.","No troops available.");
   }else return null;
   if(s.Resources.Wood<wood||s.Resources.Stone<stone)reason+=(reason.Length>0?"\n":"")+L(en,"Faltan recursos para esta acción.","Insufficient resources for this action.");
   if(deadline>0){reason=L(en,"Operación en curso.","Operation in progress.");b.Time=RoyalFormatting.Remaining(deadline,clock.UtcTicks,en);b.State=reason;double remaining=Math.Max(0,(deadline-clock.UtcTicks)/(double)TimeSpan.TicksPerSecond);b.Progress=duration>0?(float)Math.Clamp(1-remaining/duration,0,1):0;}
   else{b.State=b.Level>0?L(en,"Operativo","Operational"):L(en,"Por construir","Not built");b.Time=duration>0?duration+" s":"";}
   b.Requirements=reason;b.Costs=command.Length==0?"":L(en,"Madera: ","Wood: ")+RoyalFormatting.Amount(wood,en)+"   ·   "+L(en,"Piedra: ","Stone: ")+RoyalFormatting.Amount(stone,en);
   if(command.Length>0)b.Action=new RoyalAction(command,target,action,reason,s.Revision,reason.Length==0&&deadline==0);
   return b;
  }
 }
}
