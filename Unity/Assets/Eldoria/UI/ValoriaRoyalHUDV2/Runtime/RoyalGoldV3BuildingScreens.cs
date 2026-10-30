using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Eldoria.UI.RoyalGold.V2 {
 /// <summary>Independent city management destinations, projected from the shared session.</summary>
 [DisallowMultipleComponent,DefaultExecutionOrder(1500)] public sealed class RoyalGoldV3BuildingScreens:MonoBehaviour {
  RoyalHUD hud;ICommandGateway gateway;Func<string,RoyalBuilding> read;Action refreshed;
  RectTransform root,header,art,detail,footer,choices,operationProgress,operationFill;RawImage portrait;
  RoyalText title,kicker,description,costs,status,notice,artCaption,actionLabel;
  RoyalButton action,ranking,training,back,speedChoice;int speedMinutes=1;RoyalText speedLabel;RoyalGoldV3RankingScreen rankings;readonly RoyalButton[] priority=new RoyalButton[3];readonly RoyalButton[] tabs=new RoyalButton[5];
  string selected="",message="";float refresh,lastW,lastH;Rect lastSafe;bool suspended;
  public bool IsOpen=>root&&root.gameObject.activeSelf;
  public RectTransform VisibleRoot=>IsOpen?root:null;
  public RoyalButton VisibleAction=>IsOpen&&action.gameObject.activeInHierarchy?action:null;
  public RoyalButton VisibleClose=>IsOpen?back:null;
  public string VisibleOperationText=>IsOpen?costs.text:"";
  public RectTransform VisibleProgress=>IsOpen&&operationProgress.gameObject.activeSelf?operationFill:null;
  public string BuildingId=>selected;
  public Texture ReferenceTexture=>portrait?portrait.texture:null;
  void LateUpdate(){if(IsOpen&&!(rankings&&rankings.IsOpen)){root.SetAsLastSibling();if(back&&back.targetGraphic.depth<0)RebindGraphics(root);}}
  static readonly Color Gold=new Color(.89f,.73f,.46f),Ivory=new Color(.95f,.925f,.865f),Muted=new Color(.72f,.79f,.82f);
  static readonly string[] Ids={"bastion","sawmill","upgrade-barracks","granary","stoneworks"};
  static readonly string[] Names={"BASTIÓN","ASERRADERO","CUARTEL","GRANERO","CANTERA"};
  public void Initialize(RoyalHUD h,ICommandGateway g,Func<string,RoyalBuilding> building,Action changed){
   if(hud!=h){if(hud){hud.SelectionChanged-=Select;hud.NarrativeOpening-=Suspend;}hud=h;if(hud){hud.SelectionChanged+=Select;hud.NarrativeOpening+=Suspend;}}
   gateway=g;read=building;refreshed=changed;
   rankings=h.GetComponent<RoyalGoldV3RankingScreen>();if(!rankings)rankings=h.gameObject.AddComponent<RoyalGoldV3RankingScreen>();rankings.Initialize(h,g);
  }
  void OnDestroy(){if(hud){hud.SelectionChanged-=Select;hud.NarrativeOpening-=Suspend;}}
  // No building opens a standalone destination. The original management art
  // remains in the repository, but construction/upgrades use the existing
  // anchored V3 action and timer. Only actual III–V priority/speedup commands
  // need a small, safe-area contextual dock.
  bool NeedsDecisionDock(){
   if(!hud||gateway==null||read==null||string.IsNullOrEmpty(selected)||
      hud.NarrativeOpen||(hud.Modal&&hud.Modal.gameObject.activeInHierarchy))return false;
   var state=gateway.Snapshot();var building=read(selected);
   return building!=null&&
    (selected=="bastion"&&state.BastionLevel>=4&&
       string.IsNullOrEmpty(state.ChapterThree.DevelopmentPriority)||
     building.OperationActive&&(state.SpeedupOneMinute>0||state.SpeedupFiveMinutes>0));
  }
  void RefreshVisibility(){
   bool show=NeedsDecisionDock();
   if(show&&root==null)Ensure();
   if(root&&root.gameObject.activeSelf!=show){
    root.gameObject.SetActive(show);
    lastW=-1f;
   }
   if(show){SynchronizeLayout();Refresh();}
  }
  void Suspend(){if(root)root.gameObject.SetActive(false);suspended=true;}
  void Select(string id){
   selected=Array.IndexOf(Ids,id)>=0?id:"";
   message="";suspended=false;RefreshVisibility();
  }
  public void Close(){suspended=false;if(root)root.gameObject.SetActive(false);if(hud)hud.SelectBuilding("");}
  void Update(){
   if(!hud||gateway==null)return;
   if(hud.NarrativeOpen||(hud.Modal&&hud.Modal.gameObject.activeInHierarchy)){Suspend();return;}
   if(Time.unscaledTime>=refresh){
    refresh=Time.unscaledTime+.2f;
    RefreshVisibility();
   }
   if(!IsOpen)return;
   if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){Close();return;}
   if(Mathf.Abs(root.rect.width-lastW)>.5f||
      Mathf.Abs(root.rect.height-lastH)>.5f||lastSafe!=Screen.safeArea)
    SynchronizeLayout();
  }
  // Same native resize repair used by the established V3 context HUD. Dynamic
  // graphics can retain depth -1, making the old navigation win raycasts even
  // when the destination is the last sibling. Keep the single existing Canvas.
  internal static void RebindGraphics(RectTransform view){
   foreach(var graphic in view.GetComponentsInChildren<Graphic>(true)){
    if(!graphic||!graphic.enabled)continue;
    graphic.enabled=false;graphic.enabled=true;graphic.SetAllDirty();
   }
   Canvas.ForceUpdateCanvases();
  }
  internal static RectTransform Rect(string name,Transform parent){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);g.layer=parent.gameObject.layer;return (RectTransform)g.transform;}
  internal static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
  internal static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
  internal static RoyalText Text(string name,Transform parent,int font,Color color){var r=Rect(name,parent);r.gameObject.AddComponent<CanvasRenderer>();var t=r.gameObject.AddComponent<RoyalText>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=font;t.color=color;t.alignment=TextAnchor.UpperLeft;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
  internal static RectTransform Panel(string name,Transform parent){var r=Rect(name,parent);r.gameObject.AddComponent<CanvasRenderer>();r.gameObject.AddComponent<Image>().color=new Color(.065f,.105f,.14f,.96f);var edge=r.gameObject.AddComponent<Outline>();edge.effectColor=new Color(.55f,.43f,.25f,.65f);edge.effectDistance=new Vector2(1,-1);return r;}
  internal static RoyalButton Button(string name,Transform parent,string label,Action click,out RoyalText text){var r=Panel(name,parent);var b=r.gameObject.AddComponent<RoyalButton>();b.targetGraphic=r.GetComponent<Image>();b.onClick.AddListener(()=>click());text=Text(name+" label",r,16,Ivory);text.Put(label);text.alignment=TextAnchor.MiddleCenter;Stretch((RectTransform)text.transform);((RectTransform)text.transform).offsetMin=new Vector2(8,5);((RectTransform)text.transform).offsetMax=new Vector2(-8,-5);return b;}
  void Ensure(){if(root)return;
   var safe=hud.GetComponentInChildren<RoyalHudLayout>(true);
   root=Panel("V3 / Contextual building decisions",safe?safe.transform:hud.transform);
   root.GetComponent<Image>().color=new Color(.045f,.075f,.105f,.97f);
   var backdrop=Rect("Royal hall",root);Stretch(backdrop);backdrop.gameObject.AddComponent<CanvasRenderer>();var bg=backdrop.gameObject.AddComponent<RoyalGoldV3ManagementBackdrop>();bg.color=Color.white;bg.raycastTarget=false;
   header=Rect("Header",root);art=Panel("Approved architecture",root);detail=Panel("Building management",root);footer=Rect("Footer",root);choices=Rect("Development council",detail);
   training=Button("Open training",header,"ENTRENAMIENTO",()=>hud.SelectBuilding("barracks"),out _);
   ranking=Button("Rankings",header,"CLASIFICACIONES",()=>rankings.Open(),out _);
   title=Text("Building title",header,30,Ivory);kicker=Text("Chapter and level",header,14,Gold);
   for(int i=0;i<Ids.Length;i++){int n=i;tabs[i]=Button("Open "+Ids[i],header,Names[i],()=>hud.SelectBuilding(Ids[n]),out _);}
   var image=Rect("Approved level V reference",art);image.gameObject.AddComponent<CanvasRenderer>();portrait=image.gameObject.AddComponent<RawImage>();portrait.raycastTarget=false;portrait.uvRect=new UnityEngine.Rect(0,0,1,1);
   artCaption=Text("Reference caption",art,13,Muted);
   description=Text("Function",detail,23,Ivory);costs=Text("Exact cost",detail,19,Gold);status=Text("Requirements and progress",detail,17,Muted);
   operationProgress=Panel("Authoritative work progress",detail);operationProgress.GetComponent<Image>().raycastTarget=false;operationFill=Panel("Gateway fill",operationProgress);operationFill.GetComponent<Image>().color=Gold;operationFill.GetComponent<Image>().raycastTarget=false;Stretch(operationFill);
   string[] choiceNames={"PRODUCCIÓN\n+260 madera · +180 piedra","DEFENSA\n+220 piedra · +100 poder","VIDA CIVIL\n+180 madera · +280 comida"};
   string[] values={"production","defense","civil"};for(int i=0;i<3;i++){int n=i;priority[i]=Button("Choose "+values[i],choices,choiceNames[i],()=>Choose(values[n]),out _);}
   notice=Text("Command feedback",footer,13,Gold);
   action=Button("Building action",footer,"MEJORAR",Submit,out actionLabel);
   speedChoice=Button("Choose speedup duration",footer,"1 MIN",()=>{speedMinutes=speedMinutes==1?5:1;Refresh();},out speedLabel);
   back=Button("Close contextual decisions",footer,"CERRAR",Close,out _);back.name="Back";
   // Never draw a full-screen royal hall over the actual playable city.
   backdrop.gameObject.SetActive(false);header.gameObject.SetActive(false);art.gameObject.SetActive(false);
   root.gameObject.SetActive(false);
  }
  void Choose(string value){var s=gateway.Snapshot();var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,"ChooseDevelopment",value,s.Revision));message=result.Ok?"Prioridad guardada. Valoria recibe los recursos elegidos.":result.Message;refreshed?.Invoke();RefreshVisibility();}
  int SpeedMinutes(PlayerState s){if(speedMinutes==5&&s.SpeedupFiveMinutes==0)speedMinutes=1;if(speedMinutes==1&&s.SpeedupOneMinute==0)speedMinutes=5;return speedMinutes;}
  void Submit(){
   var b=read(selected);if(b==null)return;
   var s=gateway.Snapshot();string kind,target;
   if(b.OperationActive&&(s.SpeedupFiveMinutes>0||s.SpeedupOneMinute>0)){
    kind="UseSpeedup";target=(selected=="bastion"?"bastion":"building")+":"+(SpeedMinutes(s)==5?"5m":"1m");
   }else{
    if(b.Action==null||!b.Action.Enabled)return;kind=b.Action.Kind;target=b.Action.Target;
   }
   var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision));
   message=result.Message;refreshed?.Invoke();RefreshVisibility();
  }
  void Refresh(){if(!IsOpen)return;var s=gateway.Snapshot();var b=read(selected);if(b==null)return;
   ranking.gameObject.SetActive(s.ChapterThree.RankUnlocked);training.gameObject.SetActive(selected=="upgrade-barracks"&&s.BarracksLevel>0);
   int index=Array.IndexOf(Ids,selected);title.Put(Names[index]);kicker.Put("VALORIA  /  GESTIÓN DE LA CIUDAD  /  NIVEL "+b.Level);
   string resource=index==0?"bastion-v":index==1?"aserradero-v":index==2?"cuartel-v":index==3?"granero-v":"cantera-v";
   portrait.texture=Resources.Load<Texture2D>("BastionIVV/Portraits/"+resource);
   artCaption.Put(b.Level>=5?"ARQUITECTURA V · DISEÑO DE REFERENCIA":"PRÓXIMA ARQUITECTURA · NIVEL V\nCada mejora desarrolla este edificio de forma independiente.");
   bool choosing=selected=="bastion"&&s.BastionLevel>=4&&string.IsNullOrEmpty(s.ChapterThree.DevelopmentPriority);
   description.Put(choosing?"EL CONSEJO DE VALORIA\nElige una prioridad de desarrollo.":b.Function);
   costs.Put(choosing?"Una elección permanente. Ninguna opción bloquea edificios.":b.Level>=5&&!b.OperationActive?"NIVEL V CONSOLIDADO":b.OperationActive?"OBRA EN CURSO · "+b.Time:b.Costs);
   status.Put(choosing?"Producción refuerza los talleres; defensa, las reservas militares; vida civil, el abastecimiento.":b.OperationActive?"Los recursos ya están pagados. La obra continúa al cerrar esta pantalla.":string.IsNullOrEmpty(b.Requirements)?b.Level>=5?"Nivel V alcanzado. Esta arquitectura será la base de futuras mejoras.":"Tiempo de obra: "+b.Time+"\nLa mejora visual se aplica al finalizar.":b.Requirements);
   operationProgress.gameObject.SetActive(b.OperationActive&&!choosing);operationFill.anchorMax=new Vector2(Mathf.Clamp01(b.Progress),1);
   choices.gameObject.SetActive(choosing);action.gameObject.SetActive(!choosing);action.interactable=b.OperationActive?(s.SpeedupFiveMinutes>0||s.SpeedupOneMinute>0):
    b.Action!=null&&b.Action.Enabled;
   int minutes=SpeedMinutes(s),charges=minutes==5?s.SpeedupFiveMinutes:s.SpeedupOneMinute;
   speedChoice.gameObject.SetActive(b.OperationActive&&s.SpeedupOneMinute>0&&s.SpeedupFiveMinutes>0);
   speedLabel.Put(minutes+" MIN · CAMBIAR");
   string boost=charges>0?minutes+" MIN · x"+charges:"";
   actionLabel.Put(b.OperationActive&&boost.Length>0?"ACELERAR · "+boost:
    b.Level>=5&&!b.OperationActive?"NIVEL V COMPLETADO":b.Action?.Label??(b.OperationActive?"EN CURSO":"EDIFICIO OPERATIVO"));notice.Put(message);
   for(int i=0;i<tabs.Length;i++)tabs[i].interactable=Ids[i]!=selected&&(i<3||s.BastionLevel>=(i==3?3:5));
  }
  public void SynchronizeLayout(){
   if(!root||!IsOpen)return;
   var state=gateway.Snapshot();
   bool council=selected=="bastion"&&state.BastionLevel>=4&&
    string.IsNullOrEmpty(state.ChapterThree.DevelopmentPriority);
   var safe=root.parent as RectTransform;
   float availableWidth=safe?safe.rect.width:Screen.width;
   float availableHeight=safe?safe.rect.height:Screen.height;
   float navHeight=hud.Navigation?((RectTransform)hud.Navigation.transform).rect.height:82f;
   float width=Mathf.Min(Mathf.Max(260f,availableWidth-24f),620f);
   float height=Mathf.Min(council?374f:174f,
      Mathf.Max(158f,availableHeight-navHeight-18f));
   root.anchorMin=root.anchorMax=new Vector2(.5f,0f);
   root.pivot=new Vector2(.5f,0f);
   root.sizeDelta=new Vector2(width,height);
   root.anchoredPosition=new Vector2(0f,navHeight+12f);
   root.SetAsLastSibling();
   Canvas.ForceUpdateCanvases();
   float pad=12f;
   float dw=width-2*pad;
   float detailHeight=height-70f;
   Place(detail,pad,pad,dw,detailHeight);
   Place((RectTransform)description.transform,12f,10f,dw-24f,council?51f:36f);
   Place((RectTransform)costs.transform,12f,council?64f:48f,dw-24f,council?35f:25f);
   Place((RectTransform)status.transform,12f,council?101f:77f,dw-24f,council?34f:30f);
   Place(operationProgress,12f,detailHeight-16f,dw-24f,6f);
   Place(choices,12f,140f,dw-24f,152f);
   for(int i=0;i<3;i++)Place((RectTransform)priority[i].transform,0f,i*51f,dw-24f,47f);
   Place(footer,pad,height-57f,dw,48f);
   Place((RectTransform)back.transform,0f,3f,dw*.22f,43f);
   Place((RectTransform)speedChoice.transform,dw*.23f,3f,dw*.26f,43f);
   Place((RectTransform)action.transform,dw*.50f,3f,dw*.50f,43f);
   Place((RectTransform)notice.transform,dw*.24f,0f,dw*.75f,40f);
   lastW=root.rect.width;lastH=root.rect.height;lastSafe=Screen.safeArea;
   Refresh();RebindGraphics(root);
  }
 }
}
