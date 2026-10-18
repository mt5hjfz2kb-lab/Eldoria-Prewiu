using System;
using System.IO;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean {
 [DefaultExecutionOrder(-950)]
 public sealed class ValoriaReferenceCleanGameplay : MonoBehaviour, IPlayableSceneHost {
  public ValoriaReferenceCleanInteraction Controls;
  public ValoriaParcelPresentation Parcels;
  public GameObject BastionOne, BastionTwo;
  public SlicePresenter Presenter;
  public LocalGateway Gateway { get; private set; }
  public IClock Clock { get; private set; }
  public string SaveFile { get; private set; }
  public bool Ready { get; private set; }
  public string LoadError { get; private set; }
  public bool Probe => Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE")=="1";
  string priorProfile; GameObject expeditions;
  public Camera PlayerCamera => Controls.playerCamera;
  public sealed class TestClock:IClock { public long UtcTicks {get;private set;}=DateTime.UtcNow.Ticks; public void Step(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;} }
  void Awake(){
   priorProfile=SliceContentProfiles.ActiveRuntimeProfile;
   SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
   if(Controls==null)Controls=FindFirstObjectByType<ValoriaReferenceCleanInteraction>();
   Controls.gameplayOwnsInput=true;
   Clock=Probe?(IClock)new TestClock():new SystemClock();
   try {
    SaveFile=Probe?Path.Combine(Path.GetTempPath(),"eldoria-valoria-gameplay-"+Guid.NewGuid().ToString("N")+".json"):SliceBoot.SavePath;
    var store=new FileStateStore(SaveFile);
    // Existing owner save always wins. Preserve the legacy file byte-for-byte when
    // adopting it; no resource scaling, reset, or replacement of an existing save.
    if(!Probe&&!File.Exists(SaveFile)){
     var legacy=Path.Combine(UnityEngine.Application.persistentDataPath,"eldoria-unity-slice-v1.json");
     if(File.Exists(legacy)){var old=new FileStateStore(legacy).Load();if(old!=null)store.Save(old);}
    }
    Gateway=new LocalGateway(Clock,store);
    if(Presenter==null)Presenter=GetComponent<SlicePresenter>();
    Presenter.SceneHost=this;Presenter.UtcNowTicks=()=>Clock.UtcTicks;Presenter.Initialize(Gateway);Ready=true;
   }catch(Exception e){LoadError="No se pudo abrir el guardado. Se conserva intacto.";Debug.LogError("VALORIA_SAVE_PRESERVED "+e);}
  }
  void Start(){if(Ready)Presenter.OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
  void OnDestroy(){SliceContentProfiles.SetRuntimeProfileOverride(priorProfile);}
  void OnGUI(){if(!string.IsNullOrEmpty(LoadError))GUI.Box(new Rect(20,20,Mathf.Min(Screen.width-40,600),100),LoadError);}
  public void ReloadForProbe(){if(!Probe)throw new InvalidOperationException("Probe-only reload");Gateway=new LocalGateway(Clock,new FileStateStore(SaveFile));Presenter.Initialize(Gateway);Presenter.RefreshState();}
  public void Apply(PlayerState s){
   if(Parcels!=null)Parcels.Apply(s);
   if(BastionTwo!=null){BastionTwo.SetActive(s.BastionLevel>=2);if(BastionOne!=null)BastionOne.SetActive(s.BastionLevel<2);}
  }
  public void Home(){Controls.ResetView();}
  public void Pan(Vector2 delta){Controls.PanBy(delta);}
  public void Zoom(float delta){Controls.ZoomBy(delta);}
  public void Focus(GameObject target){
   var c=target.GetComponent<Collider>();var p=c!=null?c.bounds.center:target.transform.position;
   var ray=PlayerCamera.ViewportPointToRay(new Vector3(.5f,.5f,0));var plane=new Plane(Vector3.up,p);
   if(plane.Raycast(ray,out var d)){PlayerCamera.transform.position+=p-ray.GetPoint(d);Controls.FitToTerrain();}
  }
  public string TargetName(string id){return id=="bastion"?"Bastión I":id=="sawmill"?"Parcela 1":id=="barracks"?"Parcela 3":null;}
  public WorldHotspot Pick(Vector2 point){
   var s=Gateway.Snapshot();
   foreach(var hit in Physics.RaycastAll(PlayerCamera.ScreenPointToRay(point),500,1<<8).OrderBy(h=>h.distance)){
    var h=hit.collider.GetComponent<WorldHotspot>();if(h==null)continue;
    if(h.Id=="barracks"&&s.BastionLevel<2)continue;
    return h;
   }return null;
  }
  public void Navigate(string target){
   if(target!="Frontier"){if(expeditions!=null)expeditions.SetActive(false);Presenter.ClosePanel();Home();return;}
   OpenExpeditions();
  }
  public void OpenExpeditions(){
   if(expeditions==null){
    var safe=GameObject.Find("Eldoria HUD").transform.Find("Safe area");
    expeditions=new GameObject("Expediciones desde Valoria",typeof(RectTransform),typeof(Image));expeditions.transform.SetParent(safe,false);
    var r=expeditions.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(320,226);
    expeditions.GetComponent<Image>().color=new Color(.025f,.035f,.043f,.98f);
    var edge=expeditions.AddComponent<Outline>();edge.effectColor=new Color(.53f,.38f,.19f);edge.effectDistance=new Vector2(2,-2);
    AddText(expeditions.transform,"RUTAS DESDE VALORIA",new Vector2(0,94),new Vector2(300,24),14);
    AddButton("Bosque · madera",new Vector2(-76,44),()=>OpenNode("forest-valoria"));
    AddButton("Cantera · piedra",new Vector2(76,44),()=>OpenNode("quarry-valoria"));
    AddButton("Despejar la ruta",new Vector2(-76,-10),()=>OpenNode("corrupt-scout"));
    var boss=AddButton("Engendro",new Vector2(76,-10),()=>OpenNode("engendro-valoria"));boss.gameObject.name="ExpeditionEngendro";
    AddButton("Volver a Valoria",new Vector2(0,-72),()=>expeditions.SetActive(false));
   }
   Presenter.ClosePanel();expeditions.SetActive(true);
   var b=expeditions.GetComponentsInChildren<Button>(true).First(x=>x.name=="ExpeditionEngendro");b.gameObject.SetActive(Gateway.Snapshot().BastionLevel>=2);
  }
  void OpenNode(string id){expeditions.SetActive(false);Presenter.ShowExpedition(id);}
  Button AddButton(string label,Vector2 pos,UnityEngine.Events.UnityAction action){
   var go=new GameObject(label,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(expeditions.transform,false);
   var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=new Vector2(144,44);
   go.GetComponent<Image>().color=new Color(.22f,.18f,.11f);var b=go.GetComponent<Button>();b.onClick.AddListener(action);
   AddText(go.transform,label,Vector2.zero,new Vector2(140,42),11);return b;
  }
  static void AddText(Transform parent,string value,Vector2 pos,Vector2 size,int fontSize){
   var go=new GameObject(value,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();
   t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=fontSize;t.color=new Color(.98f,.85f,.58f);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;
   var r=t.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;
  }
 }
}
