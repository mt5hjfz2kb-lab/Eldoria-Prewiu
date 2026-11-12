using System;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.Presentation;
using Eldoria.SpawnlingBastion2;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;

namespace Eldoria.ReferenceClean {
 // Guidance is presentation only. It never advances timers, grants rewards or writes the save.
 public sealed class ValoriaJourneyGuidance:MonoBehaviour {
  ValoriaIntegratedWorld host;ValoriaReferenceCleanGameplay city;
  readonly Dictionary<string,BuildingLabel> labels=new Dictionary<string,BuildingLabel>();
  float next;
  float nextNarrativeAt;
  public string Help=>ContextHint();
  string Beat {
   get {
    var s=city.Gateway.Snapshot();
    var pending=PendingBeat(s);
    return string.IsNullOrEmpty(pending)?NarrativeCoherenceRules.RecapBeat(s):pending;
   }
  }
  string PendingBeat(PlayerState s)=>NarrativeCoherenceRules.NextUnseenBeat(s);
  static RoyalNarrativePresentation.Line L(string who,string es,string en)
   =>new RoyalNarrativePresentation.Line(who,es,en);
  RoyalNarrativePresentation.Line[] Lines(string beat){
   switch(beat){
    case "extraction-oddity":return new[]{L("Sir Aldric","Detened los picos. Esa veta no refleja la luz como la piedra de Valoria. Guardad cualquier fragmento extraño. Si la tierra está cambiando cerca de la Brecha, necesitaremos saber por qué.","Stop the picks. That vein does not reflect light like Valoria's stone. Keep any unusual fragments. If the ground is changing near the Breach, we must learn why.")};
    case "consolidate-valoria":return new[]{
     L("Sir Aldric",string.IsNullOrEmpty(city.Gateway.Snapshot().ChapterThree.DevelopmentPriority)?
      "Valoria no puede reconstruirlo todo a la vez. Elige una prioridad: producción, defensa o refugio. Las mejoras de las otras áreas seguirán disponibles.":
      "Valoria ya tiene una prioridad de desarrollo. Seguiremos reconstruyendo sin renunciar a las mejoras de las otras áreas.",
      string.IsNullOrEmpty(city.Gateway.Snapshot().ChapterThree.DevelopmentPriority)?
      "Valoria cannot rebuild everything at once. Choose a priority: production, defence or shelter. Improvements in the other areas remain available.":
      "Valoria has chosen a development priority. We will keep rebuilding while preserving improvements in the other areas."),
     L("NARRADOR","La elección se reflejará en la ciudad. El siguiente paso será reunir los recursos para el Bastión V.","Your choice will be reflected in the city. The next step is gathering the resources for Bastion V.")};
    case "productive-valoria":return new[]{L("NARRADOR","Bastión V permite construir la Cantera de Valoria para generar piedra de forma continua. Cada edificio crece mediante sus propias mejoras.","Bastion V unlocks Valoria's Stoneworks for continuous stone production. Each building grows through its own upgrades.")};
    case "frontier-open":return new[]{
     L("Sir Aldric","La frontera se abre. El Granero nos dará alimento para explorarla. Hay una anomalía en la tierra, pero todavía no sabemos qué encontraremos allí.","The frontier opens. The Granary will feed our expeditions. There is an anomaly in the ground, but we do not yet know what awaits us.")};
    case "fissure-closed":return new[]{
     L("Lyra","Ese símbolo… pertenecía a mi hermano. No abandonaré su búsqueda. Lucharé junto a Valoria para seguir sus rutas.","That symbol… it belonged to my brother. I will not abandon the search. I will fight alongside Valoria to follow his trail."),
     L("Sir Aldric","Entonces Valoria también será tu hogar. Guardaremos el manuscrito; aún no sabemos qué significa.","Then Valoria will be your home too. We will keep the manuscript; we do not yet know what it means."),
     L("NARRADOR","Lyra se incorpora al Salón de Héroes: allí podrás preparar sus habilidades para futuras marchas. El manuscrito queda en el Arcón. Hemos cerrado la fisura local, no la Brecha principal.","Lyra joins the Heroes Hall: prepare her abilities there for future marches. The manuscript is kept in the Chest. We have closed the local fissure, not the main Breach.")};
    case "start":return new[]{
     L("NARRADOR","La Brecha se abrió en la tierra junto a Valoria antes del amanecer. Cuando cesaron los temblores, las murallas seguían en pie. El reino, no.",
        "The Breach opened in the ground beside Valoria before dawn. When the tremors stopped, the walls were still standing. The kingdom was not."),
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
     L("Sir Aldric","Hemos protegido lo que teníamos. Ya podemos preparar el ascenso a Bastión III y abastecer la nueva frontera.",
       "We have protected what we had. We can now prepare the ascent to Bastion III and provision the new frontier.")
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
   if(!host||!host.Ready||Time.unscaledTime<nextNarrativeAt)return;
   // The owner's first words must be the name prompt, not Aldric's narration.
   // Persisting the nickname is a precondition for the first story beat;
   // already named returning players keep their original tutorial progress.
   if(city?.Gateway==null||string.IsNullOrWhiteSpace(city.Gateway.Snapshot().PlayerName))return;
   var hud=host.Hud;
   if(!hud)return;
   // First-entry identity is an interactive chapter premise, not an
   // optional banner. Never let Aldric's first dialogue preempt the name.
   if(hud.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>()?.IsBlockingNarrative==true)return;
   if(!NarrativeCoherenceRules.CanPresent(
      hud.NarrativeOpen,
      hud.Modal&&hud.Modal.gameObject.activeSelf,
      hud.GetComponent<RoyalGoldV3ManagementScreens>()?.IsOpen??false,
      hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>()?.IsOpen??false,
      hud.GetComponent<RoyalGoldV41MailboxScreen>()?.IsOpen??false,
       !string.IsNullOrEmpty(hud.SelectedBuildingId)))return;
   var s=city.Gateway.Snapshot();string beat=PendingBeat(s);
   if(string.IsNullOrEmpty(beat))return;
   host.Hud.ShowNarrativeSequence(Lines(beat),()=>{
    // Completing text, unlike merely opening a modal, commits the milestone.
    city.Gateway.AcknowledgeNarrative(beat);
    if(beat=="fissure-closed"&&city.Gateway.Snapshot().ChapterThree.LyraRecruited)
     hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>()?.PresentRecruitment("lyra");
    // A cluster of completed tasks must never spam uninterrupted popups.
    nextNarrativeAt=Time.unscaledTime+1.5f;
    Refresh();
   });
  }
  public void Initialize(ValoriaIntegratedWorld h,ValoriaReferenceCleanGameplay c){
   host=h;city=c;host.Hud.ObjectiveRequested=FollowObjective;host.Hud.StoryRequested=ShowStory;
   var layer=new GameObject("World and construction labels",typeof(RectTransform)).GetComponent<RectTransform>();
   layer.SetParent(host.Hud.transform,false);layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;layer.SetAsFirstSibling();
   if(host.LabelPrefab){
    foreach(var id in new[]{"sawmill","barracks","granary","hunt-wolf","hunt-boar","meat-valoria","fissure-valoria","valoria-map-city","forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria","old-watch-ruin"}){
     var target=host.FindTarget(id);if(!target)continue;
     var label=Instantiate(host.LabelPrefab,layer).GetComponent<BuildingLabel>();label.name="Guidance "+id;
     foreach(var t in label.GetComponentsInChildren<RoyalText>(true))if(t!=label.Label)t.gameObject.SetActive(false);
     label.Bind(Name(id),target.transform,id=="sawmill"||id=="barracks"||id=="granary"?city.PlayerCamera:host.WorldCamera,layer);
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
   switch(id){case "stoneworks":return english?"Stoneworks":"Cantera productiva";case "ash-stalker-valoria":return english?"Ash Stalker":"Acechador de Ceniza";case "granary":return english?"Granary":"Granero";case "fissure-valoria":return english?"Fissure":"Fisura";case "meat-valoria":return english?"Meat reserve":"Reserva de carne";case "hunt-wolf":return english?"Ash wolf":"Lobo ceniciento";case "hunt-boar":return english?"Rock boar":"Jabalí de roca";case "sawmill":return english?"Sawmill":"Aserradero";case "barracks":return english?"Barracks":"Cuartel";case "bastion":return english?"Bastion":"Bastión";case "march-main":return english?"Expedition":"Expedición";case "valoria-map-city":return "Valoria";case "forest-valoria":return english?"Forest":"Bosque";case "quarry-valoria":return english?"Quarry":"Cantera";case "corrupt-scout":return english?"Corrupted route":"Ruta corrupta";case "engendro-valoria":return english?"Spawn":"Engendro";case "gate":return english?"World gate":"Puerta al mundo";default:return english?"Ruined watchtower":"Atalaya en ruinas";}
  }
  void LateUpdate(){if(!host||!host.Ready)return;if(Time.unscaledTime>=next){next=Time.unscaledTime+.2f;Refresh();PresentMilestone();}}
  public void Refresh(){
   if(!host||!host.Ready)return;
   var s=city.Gateway.Snapshot();string objective=ObjectiveTarget();host.Hud.Mission.SetContext(ContextHint());
   foreach(var pair in labels){
    bool town=pair.Key=="sawmill"||pair.Key=="barracks"||pair.Key=="granary";
    bool visible=town?!host.InWorld:host.InWorld;
    bool built=pair.Key=="sawmill"?s.SawmillLevel>0:pair.Key=="granary"?s.ChapterThree.GranaryLevel>0:s.BarracksLevel>0;
    if(pair.Key=="granary"||LocalGateway.IsChapterThreeTarget(pair.Key)){var spot=host.FindTarget(pair.Key);visible&=s.BastionLevel>=3&&spot&&spot.gameObject.activeInHierarchy;}
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
    // The selected unbuilt city site has NO visible Plot_/Available slab in
    // the integrated scene (deliberately hidden to avoid old floating tiles).
    // Do not also hide its sole existing world-anchored label: otherwise the
    // "Construir" action points at indistinguishable grass. The tiny native
    // marker is a spatial cue only, not a new gameplay offer or world model.
    bool selectedUnbuiltTown=town&&!built&&host.Selected==pair.Key&&visible;
    visible&=host.Selected!=pair.Key||selectedUnbuiltTown;
    if(!town){
     // The panorama shows true continental geography only. Hide ordinary
     // objective/city/encounter captions at the SAME threshold that suppresses
     // their resource models; keep all real hotspots, commands and objectives.
     // Unlike the old fixed 22m cutoff, this respects the current responsive
     // near/regional/global camera contract on both mobile orientations.
     float zoom=host.WorldCamera?host.WorldCamera.orthographicSize:0;
     float overview=host.WorldNavigation?host.WorldNavigation.globalZoom:0;
     bool panorama=overview>0&&zoom>=overview*.85f;
     visible&=!panorama&&(pair.Key==objective||pair.Key=="valoria-map-city"||LocalGateway.IsChapterThreeTarget(pair.Key)||zoom<=22f);
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
    bool ownBuild=((s.BuildingTaskId??"").StartsWith(pair.Key+":",StringComparison.Ordinal)||(s.BuildingTaskId??"").StartsWith("c3:"+pair.Key+":",StringComparison.Ordinal))&&s.BuildingCompletesUtcTicks>0;
    if(pair.Key=="fissure-valoria"&&s.ChapterThree.InvestigationEndsUtcTicks>city.Clock.UtcTicks)title+=" · "+RoyalFormatting.Remaining(s.ChapterThree.InvestigationEndsUtcTicks,city.Clock.UtcTicks,host.English);
    if(town&&!built)title=ownBuild?(host.English?"Building: ":"En obras: ")+title:(host.English?"Build here: ":"Construir aquí: ")+title;
    if(selectedUnbuiltTown){
     // Short, readable pointer at the actual authorized parcel. The bottom
     // parchment still owns the only paid build action and its exact cost.
     title=(ownBuild?(host.English?"BUILDING · ":"EN OBRAS · "):(host.English?"HERE · ":"AQUÍ · "))+Name(pair.Key,host.English);
     pair.Value.Label.fontSize=20;
    }else pair.Value.Label.fontSize=17;
    if(pair.Key=="valoria-map-city")title=host.English?"Enter Valoria":"Entrar en Valoria";
    pair.Value.Label.Put(title);
   }
  }
  string ContextHint(){
   var s=city.Gateway.Snapshot();bool en=host.English;
   if(s.March.Phase!="idle")return host.MarchStatus(s)+" · "+RoyalFormatting.Remaining(s.March.PhaseEndsUtcTicks,city.Clock.UtcTicks,en)+"\n"+(s.March.Phase=="returning"?host.Reward(s.March.PendingWood,s.March.PendingStone)+(en?" on arrival":" al llegar"):(en?"The route marker follows your expedition.":"El estandarte señala a tu expedición."));
   if(s.ChapterThree.InvestigationEndsUtcTicks>city.Clock.UtcTicks)return (en?"Aldric investigates · ":"Aldric investiga · ")+RoyalFormatting.Remaining(s.ChapterThree.InvestigationEndsUtcTicks,city.Clock.UtcTicks,en)+(en?"\nMeanwhile, upgrade the Sawmill, Barracks and Granary to II.":"\nMientras, mejora Aserradero, Cuartel y Granero a II.");
   if(s.BuildingCompletesUtcTicks>0)return (en?"Construction paid · ":"Obra pagada · ")+RoyalFormatting.Remaining(s.BuildingCompletesUtcTicks,city.Clock.UtcTicks,en);
   if(s.RecruitmentCompletesUtcTicks>0)return (en?"Training ":"Entrenando ")+s.PendingRecruitArchers+(en?" archers · ":" arqueros · ")+RoyalFormatting.Remaining(s.RecruitmentCompletesUtcTicks,city.Clock.UtcTicks,en)+(en?"\nImprove your city or visit Vigilants with available troops.":"\nMejora la ciudad o visita Vigilantes con las tropas disponibles.");
   if(s.BastionUpgradeCompletesUtcTicks>0)return (en?"Bastion upgrade paid · ":"Mejora del Bastión pagada · ")+RoyalFormatting.Remaining(s.BastionUpgradeCompletesUtcTicks,city.Clock.UtcTicks,en);
   // Teach the real first delivery in the existing contextual hint.
   if(UnityContextTutorial.NeedsMailboxOpening(s))
    return UnityContextTutorial.MailboxFirstUse(en);
   string key=SliceRules.CurrentObjectiveKey(s),target=ObjectiveTarget();
   // One useful instruction for the real objective; it is never a modal.
   string teaching=UnityContextTutorial.ForObjective(key,en);
   if(!string.IsNullOrEmpty(teaching))return teaching;
   if(key=="b2.complete")return en?"Bastion II secured. Prepare 650 wood and 500 stone to open the frontier.":"Bastión II asegurado. Prepara 650 madera y 500 piedra para abrir la frontera.";
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
   if(s.BuildingCompletesUtcTicks>0&&s.BuildingTaskId.StartsWith("c3:",StringComparison.Ordinal))return s.BuildingTaskId.Split(':')[1]=="barracks"?"upgrade-barracks":s.BuildingTaskId.Split(':')[1];
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
    case "b2.complete":target="bastion";wood=650;stone=500;break;
    case "b3.granary":target="granary";wood=240;stone=180;break;
    case "b3.food":return "meat-valoria";
    case "b3.hunt":return s.ChapterThree.WolfRespawnUtcTicks>city.Clock.UtcTicks?"hunt-boar":"hunt-wolf";
    case "b3.investigate":case "b3.lyra":case "b3.fissure":return "fissure-valoria";
    case "b3.complete":case "b4.priority":case "b4.ascend":return "bastion";
    case "b5.stoneworks":return "stoneworks";
    case "b5.stone":return "quarry-valoria";
    case "b5.army":return "barracks";
    case "b5.rare":return "ash-stalker-valoria";
    case "b5.infrastructure":return s.SawmillLevel<5?"sawmill":s.BarracksLevel<5?"upgrade-barracks":s.ChapterThree.GranaryLevel<5?"granary":"stoneworks";
    case "b3.infrastructure":return s.SawmillLevel<2?"sawmill":s.BarracksLevel<2?"upgrade-barracks":"granary";
    default:return "";
   }
   return s.Resources.Wood<wood?"forest-valoria":s.Resources.Stone<stone?"quarry-valoria":target;
  }
  public void FollowObjective(){
   string id=ObjectiveTarget();if(id==""){ShowStory();return;}
   if(id=="valoria-map-city"){host.SetWorld(false);city.Home();return;}
   if(!host.NavigateToObjective(id))return;
   Refresh();
  }
  public void ShowStory(){
   if(host?.Hud?.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>()?.IsBlockingNarrative==true)return;
   Refresh();if(host.Hud.Modal.gameObject.activeSelf)return;
   string beat=Beat;
   host.Hud.ShowNarrativeSequence(Lines(beat),()=>{
    city.Gateway.AcknowledgeNarrative(beat);
    nextNarrativeAt=Time.unscaledTime+1.5f;
    Refresh();
   });
  }
 }
}


