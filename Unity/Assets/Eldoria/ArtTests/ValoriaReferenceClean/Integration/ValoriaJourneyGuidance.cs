using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean {
 // Presentation and navigation only. The original gateway still owns every rule/save.
 public sealed class ValoriaJourneyGuidance:MonoBehaviour {
  ValoriaIntegratedWorld host;ValoriaReferenceCleanGameplay city;
  readonly Dictionary<string,BuildingLabel> labels=new Dictionary<string,BuildingLabel>();
  RoyalText story;float next;
  public string Story=>city.Gateway.Snapshot().EngendroDefeated?"Aldric: «Valoria vuelve a respirar.»\nEl Engendro ha caído. El ascenso a Bastión III llegará en el siguiente capítulo.":host.InWorld?city.Presenter.FrontierNarrativeText:city.Presenter.NarrativeText;
  public void Initialize(ValoriaIntegratedWorld h,ValoriaReferenceCleanGameplay c){
   host=h;city=c;host.Hud.ObjectiveRequested=FollowObjective;host.Hud.StoryRequested=ShowStory;
   var mission=host.Hud.Mission;
   ((RectTransform)mission.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,342);
   mission.Details.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,265);
   story=Instantiate(mission.Objective,mission.Details);story.name="Recovered slice narrative";
   story.fontSize=18;story.color=new Color(.77f,.81f,.82f);story.lineSpacing=1.12f;
   story.rectTransform.anchoredPosition=new Vector2(20,-130);story.rectTransform.sizeDelta=new Vector2(276,124);
   mission.Objective.fontSize=24;mission.Chapter.fontSize=19;
   mission.ProgressText.fontSize=19;
   var layer=new GameObject("World and construction labels",typeof(RectTransform)).GetComponent<RectTransform>();
   layer.SetParent(host.Hud.transform,false);layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;layer.SetAsFirstSibling();
   if(host.LabelPrefab){
    foreach(var id in new[]{"sawmill","barracks","valoria-map-city","forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria","old-watch-ruin"}){
     var target=Find(id);if(!target)continue;
     var label=Instantiate(host.LabelPrefab,layer).GetComponent<BuildingLabel>();label.name="Guidance "+id;
     foreach(var t in label.GetComponentsInChildren<RoyalText>(true))if(t!=label.Label)t.gameObject.SetActive(false);
     label.Bind(Name(id),target.transform,id=="sawmill"||id=="barracks"?city.PlayerCamera:host.WorldCamera,layer);
     label.WorldOffset=Vector3.up*3;label.Label.fontSize=19;label.Label.alignment=TextAnchor.MiddleCenter;
     label.Select.onClick.AddListener(()=>{host.Select(id);});
     labels.Add(id,label);
    }
   }
   Refresh();
  }
  WorldHotspot Find(string id)=>UnityEngine.Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x=>x.Id==id&&(id=="sawmill"||id=="barracks"||id=="bastion"?!x.transform.IsChildOf(host.WorldRoot.transform):x.transform.IsChildOf(host.WorldRoot.transform)));
  static string Name(string id){switch(id){case "sawmill":return "Aserradero";case "barracks":return "Cuartel";case "valoria-map-city":return "Valoria · Entrar";case "forest-valoria":return "Bosque · Madera";case "quarry-valoria":return "Cantera · Piedra";case "corrupt-scout":return "Ruta corrupta";case "engendro-valoria":return "Engendro";default:return "Atalaya en ruinas";}}
  void LateUpdate(){if(!host||!host.Ready)return;if(Time.unscaledTime>=next){next=Time.unscaledTime+.2f;Refresh();}}
  public void Refresh(){
   var s=city.Gateway.Snapshot();var key=SliceRules.CurrentObjectiveKey(s);
   var text=city.Presenter.NarrativeText;
   if(s.EngendroDefeated)text="Aldric: «Valoria vuelve a respirar.»\nEl Engendro ha caído. La ciudad está a salvo; el ascenso a Bastión III llegará en el siguiente capítulo.";
   else if(host.InWorld)text=city.Presenter.FrontierNarrativeText;
   if(story)story.Put(text.Length>230?text.Substring(0,227)+"…":text);
   foreach(var pair in labels){
    bool town=pair.Key=="sawmill"||pair.Key=="barracks";
    bool visible=town?!host.InWorld:host.InWorld;
    if(pair.Key=="barracks")visible&=s.BastionLevel>=2;
    if(pair.Key=="engendro-valoria")visible&=s.BastionLevel>=2;
    if(pair.Key=="corrupt-scout")visible&=!s.ScoutDefeated;
    pair.Value.gameObject.SetActive(visible);
    string title=Name(pair.Key);
    if(pair.Key=="sawmill"&&s.SawmillLevel==0)title=s.BuildingCompletesUtcTicks>0?"Aserradero · En obras":"Construir aserradero";
    if(pair.Key=="barracks"&&s.BarracksLevel==0)title=s.BuildingCompletesUtcTicks>0?"Cuartel · En obras":"Construir cuartel";
    if(pair.Key==ObjectiveTarget())title="› "+title;
    pair.Value.Label.Put(title);
   }
  }
  public string ObjectiveTarget(){
   var s=city.Gateway.Snapshot();string key=SliceRules.CurrentObjectiveKey(s);
   int wood=0,stone=0;string target="";
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
   bool town=id=="sawmill"||id=="barracks"||id=="bastion"||id=="march-main";
   host.SetWorld(!town);
   var target=Find(id=="march-main"?"barracks":id);
   if(target){if(!town)host.WorldNavigation.SetZoomLevel(2,true);host.Focus(target.gameObject);}
   host.Select(id);Refresh();
  }
  public void ShowStory(){
   Refresh();host.Hud.ShowNarrative("Crónica de Valoria",Story);
   host.Hud.Modal.Extra.gameObject.SetActive(false);if(host.Hud.Modal.Secondary)host.Hud.Modal.Secondary.gameObject.SetActive(false);
  }
 }
}
