using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Eldoria.UI.RoyalGold {
 [RequireComponent(typeof(CanvasGroup))]
 public sealed class RoyalHUD:MonoBehaviour {
  public RealmHeader Realm;public ResourceBar Resources;public MissionPanel Mission;public BottomNavigation Navigation;public BuildingDetailsPanel Building;public ModalWindow Modal;public RoyalText Feedback,TestMark;public RoyalButton Settings;
  IRoyalSession session;IRoyalNavigation host;RoyalSnapshot current;string selected="";bool english,pending,narrative;float next,submitAfter;RoyalSection shown;CanvasGroup group;
  void Awake(){group=GetComponent<CanvasGroup>();SetBound(session!=null);}
  void SetBound(bool bound){if(!group)group=GetComponent<CanvasGroup>();group.alpha=bound?1:0;group.interactable=group.blocksRaycasts=bound;}
  public void Bind(IRoyalSession source,IRoyalNavigation navigation=null){session=source??throw new ArgumentNullException(nameof(source));host=navigation;SetBound(true);Refresh();}
  public void SelectBuilding(string id){selected=id??"";Refresh();}
  void OnEnable(){Realm.Info.onClick.AddListener(OpenRealm);Mission.Open.onClick.AddListener(OpenMissions);Settings.onClick.AddListener(OpenSettings);Building.Close.onClick.AddListener(CloseBuilding);Building.Primary.Button.onClick.AddListener(Submit);Modal.Extra.onClick.AddListener(Extra);if(Modal.Secondary)Modal.Secondary.onClick.AddListener(ShowMarch);Navigation.Requested+=Open;}
  void OnDisable(){Realm.Info.onClick.RemoveListener(OpenRealm);Mission.Open.onClick.RemoveListener(OpenMissions);Settings.onClick.RemoveListener(OpenSettings);Building.Close.onClick.RemoveListener(CloseBuilding);Building.Primary.Button.onClick.RemoveListener(Submit);Modal.Extra.onClick.RemoveListener(Extra);if(Modal.Secondary)Modal.Secondary.onClick.RemoveListener(ShowMarch);Navigation.Requested-=Open;}
  void Update(){if(session!=null&&Time.unscaledTime>=next){next=Time.unscaledTime+.25f;Refresh();}if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(Modal.gameObject.activeSelf)Modal.Hide();else CloseBuilding();}}
  public void SetEnglish(bool value){english=value;Refresh();if(Modal.gameObject.activeSelf)ShowModal(shown);}
  public void Refresh(){
   if(session==null)return;
   try{current=session.Read(selected,english);Realm.Render(current,english);Resources.Render(current,english);Mission.Render(current);Navigation.Layout(english,host);Building.Render(current.Building,english,pending||Time.unscaledTime<submitAfter);TestMark.gameObject.SetActive(current.IsTest);TestMark.Put(L("PRUEBA AISLADA · SIN GUARDADO","ISOLATED TEST · NO SAVE"));if(Modal.gameObject.activeSelf&&shown!=RoyalSection.Settings&&!narrative)ShowModal(shown,false);}
   catch(Exception e){Feedback.Put(L("No se pudo actualizar la interfaz: ","Could not refresh UI: ")+e.Message);Building.Primary.Button.interactable=false;}
  }
  void Submit(){if(session==null||pending||Time.unscaledTime<submitAfter||Building.Offer==null)return;var offer=Building.Offer;pending=true;submitAfter=Time.unscaledTime+.35f;Building.Primary.Button.interactable=false;try{var r=session.Submit(offer);Feedback.Put(r.Message);}catch(Exception e){Feedback.Put(L("Acción no enviada: ","Action not sent: ")+e.Message);}finally{pending=false;Refresh();}}
  public Action ObjectiveRequested,StoryRequested;
  public bool NarrativeOpen=>narrative&&Modal.gameObject.activeSelf;
  public void ShowNarrative(string title,string body){narrative=true;Modal.Show(title,body);Modal.Extra.gameObject.SetActive(true);Modal.ExtraLabel.Put("Continuar");if(Modal.Secondary)Modal.Secondary.gameObject.SetActive(false);}
  void CloseBuilding(){SelectBuilding("");}void OpenRealm(){if(StoryRequested!=null)StoryRequested();else Open(RoyalSection.Realm);}void OpenMissions(){if(ObjectiveRequested!=null)ObjectiveRequested();else Open(RoyalSection.Missions);}void OpenSettings(){Open(RoyalSection.Settings);}void ShowMarch(){Modal.Hide();SelectBuilding("march-main");}
  void Open(RoyalSection section){if(current==null)return;if(section==RoyalSection.Bastion){SelectBuilding("bastion");return;}if(section==RoyalSection.World||section==RoyalSection.Inventory){if(host!=null&&host.CanOpen(section)){Modal.Hide();CloseBuilding();host.Open(section);}return;}ShowModal(section);}
  void Extra(){if(narrative){Modal.Hide();return;}if(shown==RoyalSection.Settings){SetEnglish(!english);return;}if(shown==RoyalSection.Army){Modal.Hide();SelectBuilding("barracks");}}
  void ShowModal(RoyalSection section,bool takeFocus=true){
   if(current==null)return;narrative=false;shown=section;string body="";string title=BottomNavigation.Name(section,english);
   switch(section){
    case RoyalSection.Realm:body=L("Poder total: ","Total power: ")+RoyalFormatting.Amount(current.Power,english)+"\n\n"+L("Bastión · Nivel ","Bastion · Level ")+current.BastionLevel+"\n\n"+current.March;break;
    case RoyalSection.Heroes:body=current.Hero+"\n\n"+L("Poder de héroes: ","Hero power: ")+RoyalFormatting.Amount(current.HeroPower,english);break;
    case RoyalSection.Army:body=L("Tropas disponibles: ","Available troops: ")+current.Available+"\n\n"+L("Heridos: ","Wounded: ")+current.Wounded+"\n\n"+L("Preparadas: ","Prepared: ")+current.Prepared+"\n\n"+current.March;break;
    case RoyalSection.Missions:title=current.Chapter;body=current.Objective+"\n\n"+current.ObjectiveProgress;break;
    case RoyalSection.Settings:body=L("Idioma de la interfaz: Español\n\nLa ciudad y tu progreso no se modifican.\nLa interfaz respeta las zonas seguras de tu pantalla.","Interface language: English\n\nYour city and progression are unchanged.\nThe interface respects your screen's safe area.");break;
   }
   if(takeFocus)Modal.Show(title,body);else{Modal.Title.Put(title);Modal.Body.Put(body);}
   Modal.Extra.gameObject.SetActive(section==RoyalSection.Army||section==RoyalSection.Settings);Modal.ExtraLabel.Put(section==RoyalSection.Settings?L("Cambiar a inglés","Switch to Spanish"):L("Abrir cuartel","Open barracks"));
   if(Modal.Secondary){Modal.Secondary.gameObject.SetActive(section==RoyalSection.Army);Modal.SecondaryLabel.Put(L("Preparar marcha","Prepare march"));}
  }
  string L(string es,string en)=>english?en:es;
 }
}
