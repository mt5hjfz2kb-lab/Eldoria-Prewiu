using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Eldoria.UI.RoyalGold.V2 {
 /// <summary>Compact city-local council/speedup utilities on the existing HUD; never a building destination.</summary>
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
  void Suspend(){if(IsOpen){root.gameObject.SetActive(false);suspended=true;}}
  bool NeedsInlineControl(string id){
   if((Array.IndexOf(Ids,id)<0&&id!="barracks")||gateway==null)return false;
   var state=gateway.Snapshot();
   if(id=="bastion"&&state.BastionLevel>=4&&
      string.IsNullOrEmpty(state.ChapterThree.DevelopmentPriority))return true;
   var building=read!=null?read(id):null;
   return building!=null&&building.OperationActive&&
      (state.SpeedupOneMinute>0||state.SpeedupFiveMinutes>0);
  }
  void Select(string id){
   if(!NeedsInlineControl(id)){
    if(root)root.gameObject.SetActive(false);
    selected="";suspended=false;return;
   }
   selected=id;message="";Ensure();
   if(hud.NarrativeOpen||(hud.Modal&&hud.Modal.gameObject.activeSelf)){
    root.gameObject.SetActive(false);suspended=true;return;
   }
   suspended=false;root.gameObject.SetActive(true);root.SetAsLastSibling();
   lastW=-1;SynchronizeLayout();
  }
  public void Close(){
   suspended=false;if(root)root.gameObject.SetActive(false);
   if(hud)hud.SelectBuilding("");
  }
  void Update(){
   if(!hud||gateway==null)return;
   if(hud.NarrativeOpen||(hud.Modal&&hud.Modal.gameObject.activeSelf)){Suspend();return;}
   if(suspended&&!string.IsNullOrEmpty(selected)){suspended=false;Select(selected);}
   if(!IsOpen||rankings&&rankings.IsOpen)return;
   if(!NeedsInlineControl(selected)){root.gameObject.SetActive(false);selected="";return;}
   if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){Close();return;}
   if(Time.unscaledTime>=refresh){refresh=Time.unscaledTime+.2f;Refresh();}
   if(Mathf.Abs(root.rect.width-lastW)>.5f||Mathf.Abs(root.rect.height-lastH)>.5f||lastSafe!=Screen.safeArea)SynchronizeLayout();
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
   var safeLayout=hud.GetComponentInChildren<RoyalHudLayout>(true);
   root=Panel("V3 / inline building utility",safeLayout?safeLayout.transform:hud.transform);
   root.anchorMin=root.anchorMax=new Vector2(.5f,0f);root.pivot=new Vector2(.5f,0f);
   root.GetComponent<Image>().color=new Color(.025f,.045f,.07f,.96f);
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
   back=Button("Back to city",footer,"VOLVER A VALORIA",Close,out _);back.name="Back";root.gameObject.SetActive(false);
  }
  void Choose(string value){var s=gateway.Snapshot();var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,"ChooseDevelopment",value,s.Revision));message=result.Ok?"Prioridad guardada. Valoria recibe los recursos elegidos.":result.Message;refreshed?.Invoke();SynchronizeLayout();}
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
   message=result.Message;refreshed?.Invoke();Refresh();
  }
  void Refresh(){
   if(!IsOpen)return;
   var s=gateway.Snapshot();var b=read(selected);
   if(b==null){root.gameObject.SetActive(false);return;}
   bool choosing=selected=="bastion"&&s.BastionLevel>=4&&
      string.IsNullOrEmpty(s.ChapterThree.DevelopmentPriority);
   bool speedup=b.OperationActive&&(s.SpeedupOneMinute>0||s.SpeedupFiveMinutes>0);
   if(!choosing&&!speedup){root.gameObject.SetActive(false);return;}
   // Build/Upgrade is exclusively the existing city/context action.
   // Neither ordinary selection nor a completed building opens a new screen.
   if(ranking)ranking.gameObject.SetActive(false);
   training.gameObject.SetActive(false);
   foreach(var tab in tabs)tab.gameObject.SetActive(false);
   art.gameObject.SetActive(false);
   title.Put(choosing?"CONSEJO DE VALORIA":b.Name.ToUpperInvariant());
   kicker.Put(choosing?"PRIORIDAD DEL BASTIÓN IV":"OBRA EN CURSO · "+b.Time);
   description.Put(choosing?"Elige una prioridad de desarrollo:":"Acelera la obra sin salir de la ciudad.");
   costs.gameObject.SetActive(false);status.gameObject.SetActive(false);
   operationProgress.gameObject.SetActive(false);
   choices.gameObject.SetActive(choosing);action.gameObject.SetActive(!choosing);
   int minutes=SpeedMinutes(s),charges=minutes==5?s.SpeedupFiveMinutes:s.SpeedupOneMinute;
   speedChoice.gameObject.SetActive(!choosing&&s.SpeedupOneMinute>0&&s.SpeedupFiveMinutes>0);
   speedLabel.Put(minutes+" MIN · CAMBIAR");
   action.interactable=!choosing&&speedup;
   actionLabel.Put("ACELERAR "+minutes+" MIN · x"+charges);
   notice.Put(message);
  }
  public void SynchronizeLayout(){
   if(!root)return;
   var safe=root.parent as RectTransform;
   Canvas.ForceUpdateCanvases();
   float availableW=safe?safe.rect.width:Screen.width;
   float availableH=safe?safe.rect.height:Screen.height;
   var snapshot=gateway.Snapshot();
   bool choosing=selected=="bastion"&&snapshot.BastionLevel>=4&&
      string.IsNullOrEmpty(snapshot.ChapterThree.DevelopmentPriority);
   float width=Mathf.Min(440f,Mathf.Max(260f,availableW-24f));
   float height=choosing?315f:196f;
   var dock=hud.Navigation?hud.Navigation.transform as RectTransform:null;
   float dockHeight=dock?dock.rect.height:104f;
   float bottom=Mathf.Min(dockHeight+12f,Mathf.Max(8f,availableH-height-8f));
   // Single existing safe-area canvas; touch blocks only the small card.
   root.anchorMin=root.anchorMax=new Vector2(.5f,0f);
   root.pivot=new Vector2(.5f,0f);
   root.sizeDelta=new Vector2(width,height);
   root.anchoredPosition=new Vector2(0,bottom);
   Place(header,14,12,width-28,52);
   Place((RectTransform)title.transform,0,0,width-28,29);
   Place((RectTransform)kicker.transform,0,29,width-28,22);
   Place(detail,14,68,width-28,choosing?198:52);
   var bg=detail.GetComponent<Image>();
   if(bg)bg.color=new Color(.055f,.08f,.12f,.76f);
   Place((RectTransform)description.transform,9,7,width-46,choosing?35:36);
   if(choosing){
    Place(choices,9,52,width-46,141);
    for(int i=0;i<3;i++){
     Place((RectTransform)priority[i].transform,0,i*47,width-46,44);
     var label=priority[i].GetComponentInChildren<RoyalText>();
     if(label)label.fontSize=14;
    }
   }
   Place(footer,14,choosing?269:124,width-28,choosing?44:66);
   Place((RectTransform)footer.Find("Back"),0,choosing?0:24,(width-28)*.34f,40);
   Place((RectTransform)notice.transform,(width-28)*.37f,0,(width-28)*.62f,20);
   Place((RectTransform)speedChoice.transform,(width-28)*.39f,0,(width-28)*.61f,23);
   Place((RectTransform)action.transform,(width-28)*.39f,choosing?0:24,(width-28)*.61f,40);
   lastW=root.rect.width;lastH=root.rect.height;lastSafe=Screen.safeArea;
   Refresh();
   RebindGraphics(root);
  }
 }
}
