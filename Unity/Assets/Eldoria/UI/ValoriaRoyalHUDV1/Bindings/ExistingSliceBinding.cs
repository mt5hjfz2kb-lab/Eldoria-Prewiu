using System;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Application;

namespace Eldoria.UI.RoyalGold {
 /// <summary>Reads the existing state and requests gateway commands. No persistence, clock advancement or profile selection.</summary>
 public sealed class ExistingSliceBinding:IRoyalSession {
  readonly ICommandGateway gateway; readonly IClock clock; readonly bool test;
  readonly HashSet<string> submitted=new HashSet<string>(); readonly Queue<string> recent=new Queue<string>(); bool submitting;
  string observedRecruitment="";int observedRecruitCount,completedRecruitCount;long completionShownUntil;
  public ExistingSliceBinding(ICommandGateway gateway,IClock clock,bool test=false){this.gateway=gateway??throw new ArgumentNullException(nameof(gateway));this.clock=clock??throw new ArgumentNullException(nameof(clock));this.test=test;}
  public RoyalSnapshot Read(string selection,bool en){
   var s=gateway.Snapshot(); if(s==null)throw new InvalidOperationException("PlayerState unavailable"); var p=SliceRules.TotalPower(s);
   var d=new RoyalSnapshot{Revision=s.Revision,Wood=s.Resources.Wood,Stone=s.Resources.Stone,Food=s.Resources.Food,Power=p.Total,
    BastionLevel=s.BastionLevel,Available=s.Available.Total,Wounded=s.Wounded.Total,Prepared=s.PreparedTroops.Total,HeroPower=p.Heroes,
    Hero=s.PreparedHeroId=="aldric"?"Sir Aldric":s.PreparedHeroId,IsTest=test,Profile=SliceContentProfiles.ActiveRuntimeProfile,
    Chapter=L(en,s.BastionLevel<2?"CAPÍTULO I":"CAPÍTULO II",s.BastionLevel<2?"CHAPTER I":"CHAPTER II"),March=MarchText(s,en)};
   if(s.ChapterThree.LyraRecruited)d.Hero="Sir Aldric · Lyra ("+s.ChapterThree.LyraXp+" XP)";
   d.ShowFood=s.BastionLevel>=3;if(s.BastionLevel>=3)d.Chapter=L(en,"CAPÍTULO III","CHAPTER III");
   ObserveRecruitment(s);Mission(s,d,en); d.Building=Building(s,selection,en); return d;
  }
  public RoyalResult Submit(RoyalAction offer){
   if(offer==null||!offer.Enabled||string.IsNullOrEmpty(offer.Kind))return new RoyalResult(false,"Acción no disponible / Action unavailable");
   if(submitting||submitted.Contains(offer.Id))return new RoyalResult(false,"Solicitud ya enviada / Already submitted");
   var current=gateway.Snapshot(); if(current.Revision!=offer.Revision)return new RoyalResult(false,"El estado cambió. Revisa los requisitos / State changed; review requirements");
   submitting=true; submitted.Add(offer.Id); recent.Enqueue(offer.Id); while(recent.Count>64)submitted.Remove(recent.Dequeue());
   try{var r=gateway.Execute(new GameCommand("royal-ui:"+offer.Id,current.PlayerId,offer.Kind,offer.Target,offer.Revision));return new RoyalResult(r.Ok,r.Message);}finally{submitting=false;}
  }
  static string L(bool en,string es,string eg)=>RoyalFormatting.L(en,es,eg);
  void ObserveRecruitment(PlayerState s){
   // A completion message follows an observed, completed gateway task. Reloading never grants troops or invents a result.
   if(s.RecruitmentCompletesUtcTicks>0){observedRecruitment=s.RecruitmentTaskId??"";observedRecruitCount=s.PendingRecruitArchers;completionShownUntil=0;}
   else if(observedRecruitment.Length>0){
    if(s.CompletedTaskIds.Contains(observedRecruitment)){completedRecruitCount=observedRecruitCount;completionShownUntil=clock.UtcTicks+TimeSpan.FromSeconds(12).Ticks;}
    observedRecruitment="";
   }
  }
  string MarchText(PlayerState s,bool en){
   string phase=s.March.Phase??"idle",label;
   switch(phase){case "outbound":label=L(en,"En marcha","Outbound");break;case "gathering":label=L(en,"Recolectando","Gathering");break;case "returning":label=L(en,"Regresando","Returning");break;default:label=L(en,"Marcha disponible","March ready");break;}
   return label+(phase=="idle"?"":" · "+TargetName(s.March.TargetId,en)+" · "+RoyalFormatting.Remaining(s.March.PhaseEndsUtcTicks,clock.UtcTicks,en));
  }
  static string TargetName(string id,bool en){switch(id){case "forest-valoria":return L(en,"Bosque","Forest");case "quarry-valoria":return L(en,"Cantera","Quarry");case "corrupt-scout":return L(en,"Ruta corrupta","Corrupted route");case "engendro-valoria":return L(en,"Engendro","Spawn");default:return L(en,"Valoria","Valoria");}}
  static void Mission(PlayerState s,RoyalSnapshot d,bool en){
   var p=s.ChapterProgress; string key=SliceRules.CurrentObjectiveKey(s); int done=0,total=1; string title;
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
    case "b2.complete":title=L(en,"Eleva el Bastión a nivel III","Upgrade the Bastion to level III");break;
    case "b3.granary":title=L(en,"Construye el Granero","Build the Granary");break;
    case "b3.food":title=L(en,"Reúne 500 comida","Gather 500 food");done=s.Resources.Food;total=500;break;
    case "b3.hunt":title=L(en,"Completa dos cacerías","Complete two hunts");done=s.ChapterThree.Hunts;total=2;break;
    case "b3.investigate":title=L(en,"Investiga la fisura","Investigate the fissure");break;
    case "b3.lyra":title=L(en,"Conoce a Lyra","Meet Lyra");break;
    case "b3.fissure":title=L(en,"Asalta la fisura junto a Lyra","Assault the fissure with Lyra");break;
    case "b3.infrastructure":title=L(en,"Aserradero, Cuartel y Granero a nivel 2","Sawmill, Barracks and Granary to level 2");break;
    case "b3.complete":title=L(en,"Bastión III preparado · siguiente etapa pendiente","Bastion III ready · next stage pending");done=1;break;
    default:title=L(en,"Sin objetivo disponible","No objective available");total=0;break;
   }
   d.Objective=title;d.MissionFraction=total>0?Math.Min(1f,Math.Max(0f,done/(float)total)):0;d.ObjectiveProgress=total>0?Math.Min(done,total)+" / "+total:"";
   // Chapter checklist is a pure projection of the current AUTHORITATIVE
   // PLAYER STATE. It never grants resources or creates tutorial-only quests.
   // Chapter-I gathering uses cumulative recovered totals, NOT wallet balance.
   // Expedition power is independent from the realm's total power.
   if(s.BastionLevel<2){
    d.MissionSteps=new[]{
     new RoyalMissionStep(L(en,"Aserradero construido","Sawmill built"),s.SawmillLevel>0),
     new RoyalMissionStep(L(en,"Madera recolectada","Wood recovered"),p!=null&&p.GatheredWood>=SliceContentProfiles.Active.Chapter1GatherWood),
     new RoyalMissionStep(L(en,"Piedra recolectada","Stone recovered"),p!=null&&p.GatheredStone>=SliceContentProfiles.Active.Chapter1GatherStone),
     new RoyalMissionStep(L(en,"Ruta corrupta despejada","Corrupted route cleared"),s.ScoutDefeated||p!=null&&p.RouteCleared),
     new RoyalMissionStep(L(en,"Regreso a Valoria","Return to Valoria"),s.JourneyComplete),
     new RoyalMissionStep(L(en,"Bastión II alcanzado","Bastion II reached"),s.BastionLevel>=2)
    };
   }else if(s.BastionLevel>=3){
    d.MissionSteps=new[]{new RoyalMissionStep(L(en,"Granero construido","Granary built"),s.ChapterThree.GranaryLevel>0),
     new RoyalMissionStep(L(en,"500 comida reunida","500 food gathered"),p.GrantedRewards.Contains("c3-food")),
     new RoyalMissionStep(L(en,"Dos cacerías","Two hunts"),s.ChapterThree.Hunts>=2),
     new RoyalMissionStep(L(en,"Fisura cerrada · Lyra reclutada","Fissure closed · Lyra recruited"),s.ChapterThree.FissureClosed),
     new RoyalMissionStep(L(en,"Infraestructura a nivel 2","Infrastructure at level 2"),s.SawmillLevel>=2&&s.BarracksLevel>=2&&s.ChapterThree.GranaryLevel>=2)};
   }else{
    d.MissionSteps=new[]{
     new RoyalMissionStep(L(en,"Cuartel construido","Barracks built"),s.BarracksLevel>0),
     new RoyalMissionStep(L(en,"Arqueros entrenados","Archers trained"),p!=null&&p.TrainedArchers>=SliceContentProfiles.Active.Chapter2TrainArchers),
     new RoyalMissionStep(L(en,"Marcha confirmada","March confirmed"),s.MarchConfigured&&p!=null&&p.MarchConfirmed),
     new RoyalMissionStep(L(en,"Poder de expedición","Expedition power"),p!=null&&p.ConfirmedExpeditionPower>=SliceContentProfiles.Active.Chapter2ExpeditionPower),
     new RoyalMissionStep(L(en,"Engendro derrotado","Spawn defeated"),s.EngendroDefeated)
    };
   }
  }
  RoyalBuilding ChapterThreeBuilding(PlayerState s,string id,bool en){
   var c=s.ChapterThree;
   bool ascend=id=="bastion"&&s.BastionLevel==2;
   bool building=s.BastionLevel>=3&&(id=="granary"||id=="sawmill"||id=="upgrade-barracks");
   bool world=s.BastionLevel>=3&&LocalGateway.IsChapterThreeTarget(id);
   if(!ascend&&!building&&!world)return null;
   var b=new RoyalBuilding{Id=id};string kind="",target=id,label="",reason="";int wood=0,stone=0,seconds=0;long deadline=0;
   if(ascend){
    b.Name=L(en,"Bastión","Bastion");b.Level=2;wood=650;stone=500;seconds=14;kind="AdvanceBastion";label=L(en,"Elevar a Bastión III","Upgrade to Bastion III");
    b.Function=L(en,"Granero, comida, caza y una frontera más amplia.","Granary, food, hunting and an expanded frontier.");
    reason=BastionProgressionCatalog.MissingSummary(s,BastionProgressionCatalog.ForLevel(3));deadline=s.BastionUpgradeCompletesUtcTicks;
   }else if(building){
    target=id=="upgrade-barracks"?"barracks":id;
    b.Name=target=="granary"?L(en,"Granero","Granary"):target=="sawmill"?L(en,"Aserradero","Sawmill"):L(en,"Cuartel","Barracks");
    b.Level=target=="granary"?c.GranaryLevel:target=="sawmill"?s.SawmillLevel:s.BarracksLevel;
    b.Function=target=="barracks"?L(en,"Infraestructura militar. Recluta desde el Cuartel.","Military infrastructure. Recruit from the Barracks."):
     b.Level+L(en,target=="granary"?" comida/s":" madera/s",target=="granary"?" food/s":" wood/s");
    wood=b.Level==0?240:(target=="barracks"?140:120)*b.Level;stone=b.Level==0?180:(target=="barracks"?90:78)*b.Level;
    seconds=b.Level==0?9:8+b.Level*2;kind=b.Level==0?"Build":"UpgradeBuilding";label=L(en,b.Level==0?"Construir":"Mejorar",b.Level==0?"Build":"Upgrade");
    if(b.Level>=s.BastionLevel)reason=L(en,"Nivel máximo para el Bastión actual.","Maximum level for the current Bastion.");
    if(b.Level>=2&&target!="barracks"&&!c.Inventory.Contains(target=="sawmill"?"iron-brace":"treated-hide"))reason=L(en,"Falta material especial del mundo.","Requires a special world material.");
    if(s.BuildingTaskId.StartsWith("c3:"+target+":",StringComparison.Ordinal))deadline=s.BuildingCompletesUtcTicks;
    else if(s.BuildingCompletesUtcTicks>0||s.BastionUpgradeCompletesUtcTicks>0)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");
   }else{
    b.Name=id=="fissure-valoria"?L(en,"Fisura","Fissure"):id=="hunt-wolf"?L(en,"Lobo ceniciento","Ash wolf"):id=="hunt-boar"?L(en,"Jabalí de roca","Rock boar"):L(en,"Reserva de carne","Meat reserve");
    kind=id=="meat-valoria"?"Gather":"Fight";label=L(en,"Enviar expedición","Send expedition");
    if(id=="fissure-valoria"){
     if(c.FissureClosed){b.State=L(en,"Fisura cerrada","Fissure closed");return b;}
     kind=!c.FissureExplored?"InvestigateFissure":!c.LyraContacted?"ContactLyra":"Fight";
     label=L(en,kind=="InvestigateFissure"?"Investigar · 20 min":kind=="ContactLyra"?"Hablar con Lyra":"Asaltar con Lyra",kind=="InvestigateFissure"?"Investigate · 20 min":kind=="ContactLyra"?"Talk to Lyra":"Assault with Lyra");
     b.Function=L(en,"Una fisura local. Lyra busca a su hermano; aún nadie conoce el origen de la Brecha.","A local fissure. Lyra seeks her brother; the Breach's origin remains unknown.");
     deadline=c.InvestigationEndsUtcTicks;seconds=1200;
    }else if(id=="meat-valoria")b.Function=c.MeatRemaining+L(en," comida disponible · carga 300"," food remaining · load 300");
    else b.Function=L(en,"Caza: alimento y materiales. Sin recompensa directa de Poder.","Hunting: food and materials. No direct Power reward.");
    if(c.InvestigationEndsUtcTicks>0&&id!="fissure-valoria")reason=L(en,"Aldric está investigando la fisura.","Aldric is investigating the fissure.");
    if(s.March.Phase!="idle"){reason=L(en,"Espera al regreso de la marcha.","Wait for the march to return.");if(s.March.TargetId==id){deadline=s.March.PhaseEndsUtcTicks;seconds=SliceRules.TravelSeconds;}}
    if(id=="meat-valoria"&&c.MeatRemaining<=0)reason=L(en,"Reserva agotada.","Reserve depleted.");
    if(id=="hunt-wolf"&&c.WolfRespawnUtcTicks>clock.UtcTicks||id=="hunt-boar"&&c.BoarRespawnUtcTicks>clock.UtcTicks)reason=L(en,"La caza aún no ha regresado.","The hunt has not returned yet.");
   }
   if(deadline>0){b.OperationActive=true;b.State=L(en,"En curso","In progress");b.Time=RoyalFormatting.Remaining(deadline,clock.UtcTicks,en);b.Progress=seconds>0?Math.Max(0,Math.Min(1,1-(float)((deadline-clock.UtcTicks)/(double)TimeSpan.TicksPerSecond)/seconds)):0;return b;}
   b.Costs=wood>0?ResourcesText(wood,stone,en):"";b.Time=seconds>0?seconds+" s":"";
   if(s.Resources.Wood<wood||s.Resources.Stone<stone)reason=L(en,"Faltan recursos.","Insufficient resources.");
   b.Requirements=reason;b.Action=new RoyalAction(kind,target,label,reason,s.Revision,string.IsNullOrEmpty(reason));return b;
  }
  RoyalBuilding Building(PlayerState s,string id,bool en){
   if(string.IsNullOrEmpty(id))return null;
   var three=ChapterThreeBuilding(s,id,en);if(three!=null)return three;
   var b=new RoyalBuilding{Id=id}; int wood=0,stone=0,duration=0; string command="",target=id,action="",reason=""; long deadline=0;
   bool constructionBusy=s.BuildingCompletesUtcTicks>0||BastionTiming.Deadline(s)>0;
   if(id=="sawmill"){
    b.Name=L(en,"Aserradero","Sawmill");b.Level=s.SawmillLevel;b.Function=L(en,"Instalación de madera del reino.","The realm's timber facility.");
    if(b.Level==0){wood=SliceRules.SawmillWoodCost;duration=SliceRules.SawmillBuildSeconds;command="Build";action=L(en,"Construir","Build");if(constructionBusy)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");}
    if((s.BuildingTaskId??"").StartsWith("sawmill:",StringComparison.Ordinal))deadline=s.BuildingCompletesUtcTicks;
   }else if(id=="barracks"){
    b.Name=L(en,"Cuartel","Barracks");b.Level=s.BarracksLevel;b.Function=L(en,"Recluta y prepara a tus arqueros.","Recruit and prepare your archers.");
    if(b.Level==0){wood=SliceRules.BarracksWoodCost;stone=SliceRules.BarracksStoneCost;duration=SliceRules.BarracksBuildSeconds;command="Build";action=L(en,"Construir","Build");if(s.BastionLevel<2)reason=L(en,"Requiere Bastión II.","Requires Bastion II.");else if(constructionBusy)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");}
    else{int count=s.RecruitmentCompletesUtcTicks>0&&s.PendingRecruitArchers>0?s.PendingRecruitArchers:SliceRules.RecruitArchers;wood=SliceRules.RecruitWoodCostFor(count);stone=SliceRules.RecruitStoneCostFor(count);duration=SliceRules.RecruitSecondsFor(count);command="Recruit";target="archer:t1";action=L(en,"Entrenar ","Train ")+SliceRules.RecruitArchers+L(en," arqueros"," archers");deadline=s.RecruitmentCompletesUtcTicks;}
    if((s.BuildingTaskId??"").StartsWith("barracks:",StringComparison.Ordinal))deadline=s.BuildingCompletesUtcTicks;
   }else if(id=="bastion"){
    b.Name=L(en,"Bastión","Bastion");b.Level=s.BastionLevel;b.Function=L(en,"Centro de gobierno y progreso de Valoria.","Valoria's centre of government and progression.");
    if(b.Level==1){wood=SliceRules.Bastion2WoodCost;stone=SliceRules.Bastion2StoneCost;duration=BastionTiming.DurationSeconds;command="AdvanceBastion";action=L(en,"Mejorar a Bastión II","Upgrade to Bastion II");if(!s.JourneyComplete)reason=L(en,"Completa los objetivos de Bastión I.","Complete the Bastion I objectives.");else if(constructionBusy)reason=L(en,"Otra obra está en curso.","Another construction is in progress.");}
    deadline=BastionTiming.Deadline(s);
   }else if(id=="march-main"){
    b.Name=L(en,"Marcha","March");b.Function=L(en,"Confirma las tropas disponibles para la expedición.","Confirm available troops for the expedition.");command="ConfigureMarch";action=L(en,"Confirmar marcha","Confirm march");
    if(s.BastionLevel<2||s.BarracksLevel<1)reason=L(en,"Requiere Bastión II y cuartel.","Requires Bastion II and barracks.");else if(s.March.Phase!="idle")reason=L(en,"Espera al regreso de la marcha.","Wait for the march to return.");else if(s.Available.Total<1)reason=L(en,"No hay tropas disponibles.","No troops available.");
   }else return null;
   if(deadline>0){
    b.OperationActive=true;
    bool recruitment=command=="Recruit";
    b.State=recruitment?L(en,"Entrenando ","Training ")+s.PendingRecruitArchers+L(en," arqueros"," archers"):
     command=="AdvanceBastion"?L(en,"Mejorando a Bastión II","Upgrading to Bastion II"):L(en,"Construyendo ","Building ")+b.Name;
    b.Time=L(en,"Tiempo restante · ","Time remaining · ")+RoyalFormatting.Remaining(deadline,clock.UtcTicks,en);
    b.Costs=L(en,"Coste pagado\n","Cost paid\n")+ResourcesText(wood,stone,en);
    b.Result=recruitment?L(en,"Al terminar: +","On completion: +")+s.PendingRecruitArchers+L(en," arqueros disponibles."," available archers."):
     command=="AdvanceBastion"?L(en,"Al terminar: Bastión II y acceso al cuartel.","On completion: Bastion II and barracks unlocked."):
     L(en,"Al terminar: ","On completion: ")+b.Name+L(en," operativo."," operational.");
    double remaining=Math.Max(0,(deadline-clock.UtcTicks)/(double)TimeSpan.TicksPerSecond);
    b.Progress=duration>0?(float)Math.Min(1,Math.Max(0,1-remaining/duration)):0;
    // Paid operations have no second purchase offer and no new affordability warning.
    return b;
   }
   b.State=id=="march-main"?MarchText(s,en):b.Level>0?L(en,"Operativo","Operational"):L(en,"Por construir","Not built");
   b.Time=duration>0?L(en,"Duración · ","Duration · ")+TimeSpan.FromSeconds(duration).ToString(@"m\:ss"):"";
   if(id=="barracks"&&b.Level>0){
    bool completed=clock.UtcTicks<completionShownUntil;
    b.State=completed?L(en,"Entrenamiento completado","Training complete"):b.State;
    b.Result=(completed?"+"+completedRecruitCount+L(en," arqueros incorporados.\n"," archers added.\n"):"")+
     L(en,"Arqueros disponibles: ","Available archers: ")+s.Available.Total+
     L(en," · Entrenados: "," · Trained: ")+(s.ChapterProgress?.TrainedArchers??0);
   }
   if(id=="march-main"){
    b.Result=L(en,"Tropas disponibles: ","Available troops: ")+s.Available.Total+
     (s.MarchConfigured?L(en," · Preparadas: "," · Prepared: ")+s.PreparedTroops.Total:"");
    if(s.March.Phase!="idle"){
     b.OperationActive=true;b.Requirements="";b.Time="";
     b.Result=L(en,"Tropas en marcha: ","Troops marching: ")+s.March.Troops.Total;
     if(s.March.PendingWood>0||s.March.PendingStone>0)b.Result+="\n"+L(en,"Llegará a Valoria: ","Returning to Valoria: ")+ResourcesText(s.March.PendingWood,s.March.PendingStone,en);
     return b;
    }
   }
   var missing=new List<string>();
   if(s.Resources.Wood<wood)missing.Add(RoyalFormatting.Amount(wood-s.Resources.Wood,en)+L(en," madera"," wood"));
   if(s.Resources.Stone<stone)missing.Add(RoyalFormatting.Amount(stone-s.Resources.Stone,en)+L(en," piedra"," stone"));
   if(missing.Count>0)reason+=(reason.Length>0?"\n":"")+L(en,"Faltan ","Missing ")+string.Join(" · ",missing)+".";
   b.Requirements=reason;
   b.Costs=command.Length==0||command=="ConfigureMarch"?"":L(en,"Coste de la acción\n","Action cost\n")+ResourcesText(wood,stone,en);
   if(command.Length>0)b.Action=new RoyalAction(command,target,action,reason,s.Revision,reason.Length==0);
   return b;
  }
  static string ResourcesText(int wood,int stone,bool en){
   var parts=new List<string>();
   if(wood>0)parts.Add(L(en,"Madera ","Wood ")+RoyalFormatting.Amount(wood,en));
   if(stone>0)parts.Add(L(en,"Piedra ","Stone ")+RoyalFormatting.Amount(stone,en));
   return string.Join(" · ",parts);
  }
 }
}
