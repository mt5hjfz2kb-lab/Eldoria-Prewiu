using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Eldoria.UI.RoyalGold {
 [RequireComponent(typeof(CanvasGroup))]
 public sealed class RoyalHUD:MonoBehaviour {
  public RealmHeader Realm;public ResourceBar Resources;public MissionPanel Mission;public BottomNavigation Navigation;public BuildingDetailsPanel Building;public ModalWindow Modal;public RoyalText Feedback,TestMark;public RoyalButton Settings;
  IRoyalSession session;IRoyalNavigation host;RoyalSnapshot current;string selected="";bool english,pending,narrative,resetArmed;
  public Func<bool> ResetPlaytestProgress;
  RoyalNarrativePresentation cinematic;float next,submitAfter;RoyalSection shown;CanvasGroup group;int orientationKey=-1;
  RoyalHudLayout layout;Canvas canvas;readonly Vector3[] corners=new Vector3[4];
  public string SelectedBuildingId=>selected;
  public event Action<string> SelectionChanged;
  // Announce a real cinematic before displaying its modal so other destinations
  // can yield input synchronously; this does not change gameplay state.
  public event Action NarrativeOpening;
  void Awake(){group=GetComponent<CanvasGroup>();canvas=GetComponent<Canvas>();layout=GetComponentInChildren<RoyalHudLayout>(true);cinematic=Modal?Modal.GetComponent<RoyalNarrativePresentation>():null;SetBound(session!=null);}
  void SetBound(bool bound){if(!group)group=GetComponent<CanvasGroup>();group.alpha=bound?1:0;group.interactable=group.blocksRaycasts=bound;}
  public void Bind(IRoyalSession source,IRoyalNavigation navigation=null){session=source??throw new ArgumentNullException(nameof(source));host=navigation;SetBound(true);Refresh();}
  public void SelectBuilding(string id){
   selected=id??"";
   Mission.SetSelectionCompact(Screen.height>Screen.width&&!string.IsNullOrEmpty(selected));
   Refresh();Canvas.ForceUpdateCanvases();SelectionChanged?.Invoke(selected);
  }
  void OnEnable(){Realm.Info.onClick.AddListener(OpenRealm);Mission.Open.onClick.AddListener(OpenMissions);Settings.onClick.AddListener(OpenSettings);Building.Close.onClick.AddListener(CloseBuilding);Building.Primary.Button.onClick.AddListener(Submit);Modal.Extra.onClick.AddListener(Extra);if(Modal.Secondary)Modal.Secondary.onClick.AddListener(SettingsOrMarch);Navigation.Requested+=Open;}
  void OnDisable(){Realm.Info.onClick.RemoveListener(OpenRealm);Mission.Open.onClick.RemoveListener(OpenMissions);Settings.onClick.RemoveListener(OpenSettings);Building.Close.onClick.RemoveListener(CloseBuilding);Building.Primary.Button.onClick.RemoveListener(Submit);Modal.Extra.onClick.RemoveListener(Extra);if(Modal.Secondary)Modal.Secondary.onClick.RemoveListener(SettingsOrMarch);Navigation.Requested-=Open;}
  void Update(){if(session!=null&&Time.unscaledTime>=next){next=Time.unscaledTime+.25f;Refresh();}if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(NarrativeOpen)Extra();else if(Modal.gameObject.activeSelf)Modal.Hide();else CloseBuilding();}}
  public void SetEnglish(bool value){english=value;Refresh();if(NarrativeOpen){if(cinematic)cinematic.SetEnglish(value);}else if(Modal.gameObject.activeSelf)ShowModal(shown);}
  public void Refresh(){
   if(session==null)return;
   // Apply a different mobile panel layout only on orientation transitions,
   // not on every tick: a user's chapter expansion must remain respected.
   int orientation=Screen.height>Screen.width?1:0;
   if(orientation!=orientationKey){
    orientationKey=orientation;
    Mission.SetSelectionCompact(orientation==1&&!string.IsNullOrEmpty(selected));
   }
   try{current=session.Read(selected,english);Realm.Render(current,english);Resources.Render(current,english);Mission.Render(current,english);Navigation.Layout(english,host);Building.Render(current.Building,english,pending||Time.unscaledTime<submitAfter);if(layout)layout.Apply(((RectTransform)layout.transform).rect.width);TestMark.gameObject.SetActive(current.IsTest);TestMark.Put(L("PRUEBA AISLADA · SIN GUARDADO","ISOLATED TEST · NO SAVE"));if(Modal.gameObject.activeSelf&&shown!=RoyalSection.Settings&&!narrative)ShowModal(shown,false);}
   catch(Exception e){Feedback.Put(L("No se pudo actualizar la interfaz: ","Could not refresh UI: ")+e.Message);Building.Primary.Button.interactable=false;}
  }
  void Submit(){if(session==null||pending||Time.unscaledTime<submitAfter||Building.Offer==null)return;var offer=Building.Offer;pending=true;submitAfter=Time.unscaledTime+.35f;Building.Primary.Button.interactable=false;try{var r=session.Submit(offer);Feedback.Put(r.Message);}catch(Exception e){Feedback.Put(L("Acción no enviada: ","Action not sent: ")+e.Message);}finally{pending=false;Refresh();}}
  public Action ObjectiveRequested,StoryRequested;
  /// <summary>Visible scene rectangle in camera viewport coordinates (origin bottom-left), including offscreen native captures.</summary>
  public Rect GetVisibleSceneViewport(){
   if(!canvas)canvas=GetComponent<Canvas>();
   if(!layout)layout=GetComponentInChildren<RoyalHudLayout>(true);
   var camera=canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
   var full=camera?camera.pixelRect:new Rect(0,0,Screen.width,Screen.height);
   if(full.width<1||full.height<1)return new Rect(.1f,.2f,.8f,.6f);
   Canvas.ForceUpdateCanvases();
   var area=layout?ScreenBounds((RectTransform)layout.transform,camera):full;
   float gap=12*(canvas?canvas.scaleFactor:1);
   area.xMin+=gap;area.xMax-=gap;area.yMin+=gap;area.yMax-=gap;
   foreach(var header in new[]{(RectTransform)Realm.transform,(RectTransform)Resources.transform,(RectTransform)Settings.transform})
    if(header.gameObject.activeInHierarchy)area.yMax=Mathf.Min(area.yMax,ScreenBounds(header,camera).yMin-gap);
   if(Feedback&&Feedback.gameObject.activeInHierarchy&&!string.IsNullOrWhiteSpace(Feedback.text))area.yMax=Mathf.Min(area.yMax,ScreenBounds(Feedback.rectTransform,camera).yMin-gap);
   // The approved compact V3 action rail is VERTICAL, not a bottom dock.
   // Never reserve the entire lower screen for controls positioned on the
   // far left. This authoritative free-viewport check is also used by native
   // I-II gameplay and offscreen camera framing, not only by HUD decoration.
   var navRect=Navigation?(RectTransform)Navigation.transform:null;
   // A single currently unlocked action makes this rail SHORTER than
   // its width. Detect from its anchor, never from its current item count.
   // Otherwise one legitimate left-side Mundo button gets mistaken for a
   // bottom bar and eliminates nearly the entire available camera height.
   bool leftRail=navRect&&navRect.anchorMax.x<.05f&&navRect.anchorMin.y>.95f;
   foreach(var item in Navigation.Items)if(item.Button.gameObject.activeInHierarchy){
    var bounds=ScreenBounds((RectTransform)item.Button.transform,camera);
    if(leftRail)area.xMin=Mathf.Max(area.xMin,bounds.xMax+gap);
    else area.yMin=Mathf.Max(area.yMin,bounds.yMax+gap);
   }
   bool bottomSheet=Building.IsBottomSheet;
   if(Mission.gameObject.activeInHierarchy){var mission=ScreenBounds((RectTransform)Mission.transform,camera);if(bottomSheet)area.yMax=Mathf.Min(area.yMax,mission.yMin-gap);else area.xMin=Mathf.Max(area.xMin,mission.xMax+gap);}
   if(Building.gameObject.activeInHierarchy){var panel=ScreenBounds((RectTransform)Building.transform,camera);if(bottomSheet)area.yMin=Mathf.Max(area.yMin,panel.yMax+gap);else area.xMax=Mathf.Min(area.xMax,panel.xMin-gap);}
   return Rect.MinMaxRect(Mathf.Clamp01((area.xMin-full.xMin)/full.width),Mathf.Clamp01((area.yMin-full.yMin)/full.height),Mathf.Clamp01((area.xMax-full.xMin)/full.width),Mathf.Clamp01((area.yMax-full.yMin)/full.height));
  }
  Rect ScreenBounds(RectTransform r,Camera camera){r.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
  public bool NarrativeOpen=>narrative&&Modal.gameObject.activeSelf;
  public bool NarrativeIsRevealing=>NarrativeOpen&&cinematic&&cinematic.IsRevealing;
  public string NarrativeSpeaker=>NarrativeOpen&&cinematic?cinematic.CurrentSpeaker:"";
  public string NarrativeSourceText=>NarrativeOpen&&cinematic?cinematic.FullText:"";
  public int NarrativeTurnIndex=>NarrativeOpen&&cinematic?cinematic.TurnIndex:-1;
  public int NarrativeTurnCount=>NarrativeOpen&&cinematic?cinematic.TurnCount:0;
  public void ShowNarrative(string title,string body){
   ShowNarrativeSequence(new[]{new RoyalNarrativePresentation.Line(title,body,body)},null);
  }
  public void ShowNarrativeSequence(RoyalNarrativePresentation.Line[] turns,Action completed){
   if(turns==null||turns.Length==0||NarrativeOpen)return;
   if(!cinematic)cinematic=Modal.GetComponent<RoyalNarrativePresentation>();
   if(!cinematic)throw new InvalidOperationException("Royal Gold narrative presentation missing from its own ModalWindow prefab");
   NarrativeOpening?.Invoke();
   narrative=true;Modal.Show("","");
   cinematic.Begin(turns,english,()=>{
    narrative=false;
    // Acknowledging the final dialogue page writes a new save revision.
    // Refresh synchronous action offers before a player can tap Build/Recruit:
    // the previously displayed offer otherwise has an obsolete revision.
    completed?.Invoke();
    Refresh();
   });
  }
  void CloseBuilding(){SelectBuilding("");}void OpenRealm(){if(StoryRequested!=null)StoryRequested();else Open(RoyalSection.Realm);}void OpenMissions(){if(ObjectiveRequested!=null)ObjectiveRequested();else Open(RoyalSection.Missions);}void OpenSettings(){resetArmed=false;Open(RoyalSection.Settings);}void ShowMarch(){Modal.Hide();SelectBuilding("march-main");}
  void SettingsOrMarch(){
   if(shown!=RoyalSection.Settings){ShowMarch();return;}
   if(ResetPlaytestProgress==null)return;
   if(!resetArmed){resetArmed=true;ShowModal(RoyalSection.Settings,false);return;}
   if(ResetPlaytestProgress())return;
   resetArmed=false;ShowModal(RoyalSection.Settings,false);
   if(Feedback)Feedback.Put(L("No se pudo reiniciar la partida; comprueba el almacenamiento.","Reset failed; check device storage."));
  }
  void Open(RoyalSection section){if(current==null)return;if(section==RoyalSection.Inventory&&Navigation&&Navigation.ChronicleLayout){
     // The approved CONSTRUIR medallion is a genuine contextual shortcut,
     // not an extra economy system. All costs, timers, locks and spending
     // remain on the original selected building and its gateway offer.
     SelectBuilding(current.BastionLevel>=2?"barracks":"sawmill");
     return;
    }if(section==RoyalSection.Bastion){SelectBuilding("bastion");return;}if(section==RoyalSection.World||section==RoyalSection.Inventory){if(host!=null&&host.CanOpen(section)){Modal.Hide();CloseBuilding();host.Open(section);}return;}ShowModal(section);}
  void Extra(){if(narrative){if(cinematic)cinematic.Advance();return;}if(shown==RoyalSection.Heroes){Modal.Hide();host?.Open(RoyalSection.Inventory);return;}if(shown==RoyalSection.Settings){resetArmed=false;SetEnglish(!english);return;}if(shown==RoyalSection.Army){Modal.Hide();SelectBuilding("barracks");}}
  void ShowModal(RoyalSection section,bool takeFocus=true){
   if(current==null)return;narrative=false;Modal.Body.fontSize=26;shown=section;string body="";string title=BottomNavigation.Name(section,english);
   switch(section){
    case RoyalSection.Realm:body=L("Poder total: ","Total power: ")+RoyalFormatting.Amount(current.Power,english)+"\n\n"+L("Bastión · Nivel ","Bastion · Level ")+current.BastionLevel+"\n\n"+current.March;break;
    case RoyalSection.Heroes:body=current.Hero+"\n\n"+L("Poder de héroes: ","Hero power: ")+RoyalFormatting.Amount(current.HeroPower,english);break;
    case RoyalSection.Army:body=L("Tropas disponibles: ","Available troops: ")+current.Available+"\n\n"+L("Heridos: ","Wounded: ")+current.Wounded+"\n\n"+L("Preparadas: ","Prepared: ")+current.Prepared+"\n\n"+current.March;break;
    case RoyalSection.Missions:title=current.Chapter;body=current.Objective+"\n\n"+current.ObjectiveProgress;break;
    case RoyalSection.Settings:body=L("Idioma de la interfaz: Español\\n\\nReinicio de PRUEBA: archiva una copia de la partida y comienza desde el prólogo. No desinstala el juego.","Interface language: English\\n\\nTEST reset: archives the save locally and restarts from the prologue. It does not uninstall the app.")+(resetArmed?L("\\n\\n¿CONFIRMAS el reinicio de la partida actual? Pulsa otra vez para empezar de cero.","\\n\\nCONFIRM resetting this save? Tap again to start from zero."):"");break;
   }
   if(takeFocus)Modal.Show(title,body);else{Modal.Title.Put(title);Modal.Body.Put(body);}
   Modal.Extra.gameObject.SetActive(section==RoyalSection.Army||section==RoyalSection.Settings||(section==RoyalSection.Heroes&&host!=null&&host.CanOpen(RoyalSection.Inventory)));Modal.ExtraLabel.Put(section==RoyalSection.Heroes?L("Abrir Arcón","Open Chest"):section==RoyalSection.Settings?L("Cambiar a inglés","Switch to Spanish"):L("Abrir cuartel","Open barracks"));
   if(Modal.Secondary){Modal.Secondary.gameObject.SetActive(section==RoyalSection.Army||(section==RoyalSection.Heroes&&current.BastionLevel>=3)||(section==RoyalSection.Settings&&ResetPlaytestProgress!=null));Modal.SecondaryLabel.Put(section==RoyalSection.Settings?(resetArmed?L("CONFIRMAR REINICIO","CONFIRM RESET"):L("Reiniciar partida (pruebas)","Reset save (testing)")):L("Preparar marcha","Prepare march"));}
  }
  string L(string es,string en)=>english?en:es;
 }
}
