using System;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.SpawnlingBastion2;
using Eldoria.UI.RoyalGold;
using UnityEngine;

namespace Eldoria.ReferenceClean {
 // Guidance is presentation only. It never advances timers, grants rewards or writes the save.
 public sealed class ValoriaJourneyGuidance:MonoBehaviour {
  ValoriaIntegratedWorld host;ValoriaReferenceCleanGameplay city;
  readonly Dictionary<string,BuildingLabel> labels=new Dictionary<string,BuildingLabel>();
  float next;
  public string Help=>city.Gateway.Snapshot().EngendroDefeated
   ?(host.English?"Bastion II secured. Next milestone: Bastion III, not available in this build.":"Bastión II asegurado. Próximo hito: Bastión III, aún no disponible.")
   :ContextHint();
  string Beat {
   get {
    var s=city.Gateway.Snapshot();
    if(s.EngendroDefeated)return "secured";
    if(s.BastionLevel>=2)return "barracks";
    if(s.ChapterProgress.GatheredWood>0)return "mark";
    if(s.SawmillLevel>0)return "world";
    return "start";
   }
  }
  static RoyalNarrativePresentation.Line L(string who,string es,string en)
   =>new RoyalNarrativePresentation.Line(who,es,en);
  RoyalNarrativePresentation.Line[] Lines(string beat){
   switch(beat){
    case "start":return new[]{
     L("NARRADOR","La Brecha apareció sobre Valoria antes del amanecer. Cuando cesaron los temblores, las murallas seguían en pie. El reino, no.",
        "The Breach appeared above Valoria before dawn. When the tremors stopped, the walls were still standing. The kingdom was not."),
     L("Sir Aldric","Valoria sobrevivió a la noche. No puedo decir lo mismo de nuestro reino.\n\nLa Brecha sigue ahí. Mientras permanezca abierta, esto volverá a ocurrir.\n\nEmpecemos por lo que aún podemos salvar.",
        "Valoria survived the night. I cannot say the same of our kingdom.\n\nThe Breach is still there. As long as it remains open, this will happen again.\n\nLet us begin with what we can still save."),
     L("NARRADOR","Valoria sobrevivió a la caída, pero no puede reconstruirse sola. Cada mejora del reino necesita recursos recuperados más allá de sus murallas. Empieza por el Aserradero.",
        "Valoria survived the fall, but cannot rebuild on its own. Each improvement needs resources recovered beyond its walls. Begin with the sawmill.")
    };
    case "world":return new[]{
     L("Sir Aldric","El aserradero vuelve a respirar. Ahora mira más allá de las murallas: recolectamos fuera para crecer dentro.",
       "The sawmill breathes again. Now look beyond the walls: we gather outside to grow within.")
    };
    case "mark":return new[]{
     L("NARRADOR","Bajo las raíces del bosque, los recolectores encuentran una marca negra grabada en la piedra. No parece quemada. Parece haber crecido desde dentro.",
       "Beneath the forest roots, the gatherers find a black mark carved into stone. It does not look burned. It seems to have grown from within."),
     L("Sir Aldric","No la toquéis. He visto esa forma antes... la noche en que cayó Valoria. Aún no sé qué significa. Y hasta saberlo, nadie se acerca solo a una marca como ésta.",
       "Do not touch it. I have seen that shape before... the night Valoria fell. I still do not know what it means. Until we do, nobody approaches a mark like this alone.")
    };
    case "barracks":return new[]{
     L("NARRADOR","Con el Bastión II, Valoria recupera una posición desde la que defenderse y espacio para un Cuartel. Las murallas exteriores aún muestran las cicatrices de la caída.",
       "With Bastion II, Valoria has a defensible position and room for a barracks. The outer walls still bear the scars of the fall."),
     L("Sir Aldric","Reconstruiremos primero. Las respuestas vendrán después. Un reino sin soldados no puede proteger lo que aún le queda.",
       "We rebuild first. Answers will come later. A kingdom without soldiers cannot protect what remains.")
    };
    case "secured":return new[]{
     L("NARRADOR","El Engendro ha caído y las tropas regresan a Valoria. El Bastión II está asegurado. La Brecha permanece abierta y aún quedan murallas por restaurar.",
       "The Spawn has fallen and the troops return to Valoria. Bastion II is secured. The Breach remains open, and walls still need repair."),
     L("Sir Aldric","Hemos protegido lo que teníamos. El siguiente hito será el Bastión III, pero aún no ha llegado el momento de construirlo.",
       "We have protected what we had. Bastion III is the next milestone, but its construction must wait.")
    };
    default:throw new ArgumentException("Unknown authored narrative beat "+beat);
   }
  }
  public string Story {
   get {
    var turns=Lines(Beat);var all=new System.Text.StringBuilder();
    foreach(var entry in turns){if(all.Length>0)all.Append("\n\n");all.Append(entry.Speaker+": "+(host.English?entry.English:entry.Spanish));}
    return all.ToString();
   }
  }
  public void PresentMilestone(){
   if(!host||!host.Ready||host.Hud.NarrativeOpen||host.Hud.Modal.gameObject.activeSelf)return;
   var s=city.Gateway.Snapshot();string beat=Beat;
   if(s.ChapterProgress!=null&&s.ChapterProgress.SeenNarrative!=null&&s.ChapterProgress.SeenNarrative.Contains(beat))return;
   // Honour the historical PlayerPrefs only for a loaded pre-migration save.
   // They must NEVER suppress a fresh game's first introduction.
   if(city.HadExistingSave&&PlayerPrefs.GetInt("valoria.journey.story."+beat,0)==1)return;
   host.Hud.ShowNarrativeSequence(Lines(beat),()=>{
    // Completing text, unlike merely opening a modal, commits the milestone.
    city.Gateway.AcknowledgeNarrative(beat);
    Refresh();
   });
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
   if(id!=null&&id.StartsWith("region1:wood:",StringComparison.Ordinal))return english?"Wood grove":"Arboleda de madera";
   if(id!=null&&id.StartsWith("region1:stone:",StringComparison.Ordinal))return english?"Stone deposit":"Yacimiento de piedra";
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
    if(pair.Key=="engendro-valoria"){
     visible&=s.BastionLevel>=2;
     // The enemy now remains legible at several strategic zooms. A label
     // pinned to the old 3m hotspot covered its actual violet head/torso.
     // Move the SAME clickable label above the silhouette: the rise is
     // proportional to orthographic zoom, so clearance remains roughly
     // constant in screen pixels in close, regional and panoramic views.
     // This changes no target, combat collider, unlock or narrative command.
     if(host.WorldCamera)pair.Value.WorldOffset=Vector3.up*
      (3f+Mathf.Clamp(host.WorldCamera.orthographicSize*.30f,4f,40f));
    }
    if(pair.Key=="corrupt-scout")visible&=!s.ScoutDefeated;
    // Selection already has a named contextual panel. Labels identify only useful targets.
    visible&=host.Selected!=pair.Key;
    if(!town){
     // The panorama shows true continental geography only. Hide ordinary
     // objective/city/encounter captions at the SAME threshold that suppresses
     // their resource models; keep all real hotspots, commands and objectives.
     // Unlike the old fixed 22m cutoff, this respects the current responsive
     // near/regional/global camera contract on both mobile orientations.
     float zoom=host.WorldCamera?host.WorldCamera.orthographicSize:0;
     float overview=host.WorldNavigation?host.WorldNavigation.globalZoom:0;
     bool panorama=overview>0&&zoom>=overview*.85f;
     visible&=!panorama&&(pair.Key==objective||pair.Key=="valoria-map-city"||zoom<=22f);
    }
    pair.Value.gameObject.SetActive(visible);
    if(visible&&pair.Key=="engendro-valoria"){
     // The canonical label previously crossed the larger animated creature
     // and concealed its head at 4X distance. Lift ONLY this real existing
     // label above the model instead of scaling a second HUD marker.
     // Near model original height=3.0m from the pinned finished V4
     // integration; use the same zoom-dependent visual-only presence.
     var actor=host.WorldRoot.GetComponentInChildren<SpawnlingEncounterVisual>(true);
     if(actor&&actor.ModelRoot&&actor.ModelRoot.activeInHierarchy){
      float height=3f*actor.PresenceAtZoom(host.WorldCamera.orthographicSize);
      pair.Value.WorldOffset=Vector3.up*(height+3f);
     }
    }
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
   if(key=="b2.complete")return en?"Bastion II secured. Next milestone: Bastion III (not yet available). Your progress is saved.":"Bastión II asegurado. Próximo hito: Bastión III (aún no disponible). Tu progreso está guardado.";
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
  public void ShowStory(){Refresh();if(!host.Hud.Modal.gameObject.activeSelf)host.Hud.ShowNarrativeSequence(Lines(Beat),null);}
 }
}
