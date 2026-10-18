using System;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.UI.RoyalGold;
using UnityEngine;

namespace Eldoria.ReferenceClean {
 // Guidance is presentation only. It never advances timers, grants rewards or writes the save.
 public sealed class ValoriaJourneyGuidance:MonoBehaviour {
  ValoriaIntegratedWorld host;ValoriaReferenceCleanGameplay city;
  readonly Dictionary<string,BuildingLabel> labels=new Dictionary<string,BuildingLabel>();
  readonly HashSet<string> seen=new HashSet<string>();
  float next;
  public string Help=>city.Gateway.Snapshot().EngendroDefeated?"Aldric: «Valoria vuelve a respirar.»\nEl Engendro ha caído. La ciudad está a salvo.":ContextHint();
  string Beat {get{var s=city.Gateway.Snapshot();return s.BastionLevel>=2?"barracks":s.ChapterProgress.GatheredWood>0?"mark":s.SawmillLevel>0?"world":"start";}}
  string Speaker=>Beat=="start"||Beat=="mark"?"Narrador":"Sir Aldric";
  string Dialogue {get{switch(Beat){
   case "start":return "Valoria sobrevivió a la caída, pero no puede reconstruirse sola. Cada mejora del reino necesita recursos recuperados más allá de sus murallas. Empieza por el Aserradero.";
   case "world":return "El aserradero vuelve a respirar. Ahora mira más allá de las murallas: recolectamos fuera para crecer dentro.";
   case "mark":return "Bajo las raíces del bosque, los recolectores encuentran una marca negra grabada en la piedra. No parece quemada. Parece haber crecido desde dentro.\n\nSir Aldric: No la toquéis. He visto esa forma antes... la noche en que cayó Valoria. Aún no sé qué significa. Y hasta saberlo, nadie se acerca solo a una marca como ésta.";
   default:return "El Bastión vuelve a sostenerse, aunque aún quedan murallas por restaurar. Un reino sin soldados, no. El Cuartel transforma recursos en tropas y aumenta lo que podemos afrontar fuera.";
  }}}
  public string Story=>Speaker+": "+Dialogue+"\n\n"+Help;
  public void PresentMilestone(){
   if(!host||!host.Ready||host.Hud.Modal.gameObject.activeSelf)return;
   string beat=Beat,key="valoria.journey.story."+beat;
   if(seen.Contains(beat)||(!UnityEngine.Application.isEditor&&PlayerPrefs.GetInt(key,0)==1))return;
   seen.Add(beat);if(!UnityEngine.Application.isEditor){PlayerPrefs.SetInt(key,1);PlayerPrefs.Save();}
   host.Hud.ShowNarrative(Speaker,Dialogue);
  }
  public void Initialize(ValoriaIntegratedWorld h,ValoriaReferenceCleanGameplay c){
   host=h;city=c;host.Hud.ObjectiveRequested=FollowObjective;host.Hud.StoryRequested=ShowStory;
   var layer=new GameObject("World and construction labels",typeof(RectTransform)).GetComponent<RectTransform>();
   layer.SetParent(host.Hud.transform,false);layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;layer.SetAsFirstSibling();
   if(host.LabelPrefab){
    foreach(var id in new[]{"sawmill","barracks","valoria-map-city","forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria","old-watch-ruin"}){
     var target=host.FindTarget(id);if(!target)continue;
     var label=Instantiate(host.LabelPrefab,layer).GetComponent<BuildingLabel>();label.name="Guidance "+id;
     foreach(var t in label.GetComponentsInChildren<RoyalText>(true))if(t!=label.Label)t.gameObject.SetActive(false);
     label.Bind(Name(id),target.transform,id=="sawmill"||id=="barracks"?city.PlayerCamera:host.WorldCamera,layer);
     var collider=target.GetComponent<Collider>();label.WorldOffset=Vector3.up*(collider?Mathf.Max(3,collider.bounds.extents.y+1.8f):3);
     label.Label.fontSize=17;label.Label.alignment=TextAnchor.MiddleCenter;
     label.Select.onClick.AddListener(()=>host.Select(id));labels.Add(id,label);
    }
   }
   Refresh();
  }
  public static string Name(string id,bool english=false){
   switch(id){case "sawmill":return english?"Sawmill":"Aserradero";case "barracks":return english?"Barracks":"Cuartel";case "bastion":return english?"Bastion":"Bastión";case "march-main":return english?"Expedition":"Expedición";case "valoria-map-city":return "Valoria";case "forest-valoria":return english?"Forest":"Bosque";case "quarry-valoria":return english?"Quarry":"Cantera";case "corrupt-scout":return english?"Corrupted route":"Ruta corrupta";case "engendro-valoria":return english?"Spawn":"Engendro";case "gate":return english?"World gate":"Puerta al mundo";default:return english?"Ruined watchtower":"Atalaya en ruinas";}
  }
  void LateUpdate(){if(!host||!host.Ready)return;if(Time.unscaledTime>=next){next=Time.unscaledTime+.2f;Refresh();PresentMilestone();}}
  public void Refresh(){
   if(!host||!host.Ready)return;
   var s=city.Gateway.Snapshot();string objective=ObjectiveTarget();host.Hud.Mission.SetContext(ContextHint());
   foreach(var pair in labels){
    bool town=pair.Key=="sawmill"||pair.Key=="barracks";
    bool visible=town?!host.InWorld:host.InWorld;
    bool built=pair.Key=="sawmill"?s.SawmillLevel>0:s.BarracksLevel>0;
    if(town&&built)visible&=pair.Key==objective;
    if(pair.Key=="barracks")visible&=s.BastionLevel>=2;
    if(pair.Key=="engendro-valoria")visible&=s.BastionLevel>=2;
    if(pair.Key=="corrupt-scout")visible&=!s.ScoutDefeated;
    // Selection already has a named contextual panel. Labels identify only useful targets.
    visible&=host.Selected!=pair.Key;
    if(!town)visible&=pair.Key==objective||pair.Key=="valoria-map-city"||host.WorldCamera.orthographicSize<=22;
    pair.Value.gameObject.SetActive(visible);
    string title=Name(pair.Key,host.English);
    bool ownBuild=(s.BuildingTaskId??"").StartsWith(pair.Key+":",StringComparison.Ordinal)&&s.BuildingCompletesUtcTicks>0;
    if(town&&!built)title=ownBuild?(host.English?"Building: ":"En obras: ")+title:(host.English?"Build here: ":"Construir aquí: ")+title;
    else if(pair.Key=="valoria-map-city")title=host.English?"Enter Valoria":"Entrar en Valoria";
    pair.Value.Label.Put(title);
   }
  }
  string ContextHint(){
   var s=city.Gateway.Snapshot();bool en=host.English;
   if(s.March.Phase!="idle")return host.MarchStatus(s)+" · "+RoyalFormatting.Remaining(s.March.PhaseEndsUtcTicks,city.Clock.UtcTicks,en)+"\n"+(s.March.Phase=="returning"?host.Reward(s.March.PendingWood,s.March.PendingStone)+(en?" on arrival":" al llegar"):(en?"The route marker follows your expedition.":"El estandarte señala a tu expedición."));
   if(s.BuildingCompletesUtcTicks>0)return (en?"Construction paid · ":"Obra pagada · ")+RoyalFormatting.Remaining(s.BuildingCompletesUtcTicks,city.Clock.UtcTicks,en);
   if(s.RecruitmentCompletesUtcTicks>0)return (en?"Training ":"Entrenando ")+SliceRules.RecruitArchers+(en?" archers · ":" arqueros · ")+RoyalFormatting.Remaining(s.RecruitmentCompletesUtcTicks,city.Clock.UtcTicks,en);
   if(s.BastionUpgradeCompletesUtcTicks>0)return (en?"Bastion upgrade paid · ":"Mejora del Bastión pagada · ")+RoyalFormatting.Remaining(s.BastionUpgradeCompletesUtcTicks,city.Clock.UtcTicks,en);
   string key=SliceRules.CurrentObjectiveKey(s),target=ObjectiveTarget();
   if(key=="b2.complete")return en?"Chapter II complete. Your progress is saved.":"Capítulo II completado. Tu progreso está guardado.";
   if(target=="forest-valoria")return en?"Select the forest, then Gather wood. Resources arrive with the returning troops.":"Selecciona el bosque y pulsa Recoger madera. Los recursos llegan con las tropas.";
   if(target=="quarry-valoria")return en?"Select the quarry, then Gather stone. Resources arrive with the returning troops.":"Selecciona la cantera y pulsa Recoger piedra. Los recursos llegan con las tropas.";
   if(target=="sawmill"||key=="b2.build-barracks")return en?"Go to objective highlights the building plot. Review its cost and choose Build.":"Ir al objetivo señala dónde construir. Revisa el coste y pulsa Construir.";
   if(target=="bastion")return en?"Return to the city and upgrade the Bastion from its panel.":"Vuelve a la ciudad y mejora el Bastión desde su panel.";
   if(target=="barracks")return en?"Open the barracks to train your archers.":"Abre el cuartel para entrenar a tus arqueros.";
   if(target=="march-main")return en?"Confirm Aldric and the available troops for the expedition.":"Confirma a Aldric y las tropas disponibles para la expedición.";
   return en?"Go to objective centres the destination and opens its action.":"Ir al objetivo centra el destino y abre su acción.";
  }
  public string ObjectiveTarget(){
   var s=city.Gateway.Snapshot();
   // Paid actions keep their actual target; their debited cost must not redirect to gathering.
   if(s.BuildingCompletesUtcTicks>0)return (s.BuildingTaskId??"").StartsWith("barracks:",StringComparison.Ordinal)?"barracks":"sawmill";
   if(s.RecruitmentCompletesUtcTicks>0)return "barracks";
   if(s.BastionUpgradeCompletesUtcTicks>0)return "bastion";
   if(s.March.Phase!="idle")return s.March.TargetId;
   string key=SliceRules.CurrentObjectiveKey(s);int wood=0,stone=0;string target="";
   switch(key){
    case "b1.build-sawmill":target="sawmill";wood=SliceRules.SawmillWoodCost;break;
    case "b1.gather-wood":return "forest-valoria";
    case "b1.gather-stone":return "quarry-valoria";
    case "b1.clear-route":return "corrupt-scout";
    case "b1.return":return "valoria-map-city";
    case "b1.ascend":target="bastion";wood=SliceRules.Bastion2WoodCost;stone=SliceRules.Bastion2StoneCost;break;
    case "b2.build-barracks":target="barracks";wood=SliceRules.BarracksWoodCost;stone=SliceRules.BarracksStoneCost;break;
    case "b2.train-archers":target="barracks";wood=SliceRules.RecruitWoodCost;stone=SliceRules.RecruitStoneCost;break;
    case "b2.prepare-march":case "b2.raise-expedition-power":return "march-main";
    case "b2.defeat-engendro":return "engendro-valoria";
    default:return "";
   }
   return s.Resources.Wood<wood?"forest-valoria":s.Resources.Stone<stone?"quarry-valoria":target;
  }
  public void FollowObjective(){
   string id=ObjectiveTarget();if(id==""){ShowStory();return;}
   if(id=="valoria-map-city"){host.SetWorld(false);city.Home();return;}
   bool town=!ValoriaIntegratedWorld.IsWorldTarget(id);
   if(!host.TrySetWorld(!town))return;
   host.Select(id);Refresh();
  }
  public void ShowStory(){Refresh();host.Hud.ShowNarrative("Crónica de Valoria",Speaker+": "+Dialogue);}
 }
}
