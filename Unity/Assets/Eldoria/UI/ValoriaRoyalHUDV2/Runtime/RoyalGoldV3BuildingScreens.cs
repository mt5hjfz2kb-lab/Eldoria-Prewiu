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
  RoyalButton action,ranking,training,back;RoyalGoldV3RankingScreen rankings;readonly RoyalButton[] priority=new RoyalButton[3];readonly RoyalButton[] tabs=new RoyalButton[5];
  string selected="",message="";float refresh,lastW,lastH;Rect lastSafe;bool suspended;
  public bool IsOpen=>root&&root.gameObject.activeSelf;
  public RectTransform VisibleRoot=>IsOpen?root:null;
  public RoyalButton VisibleAction=>IsOpen&&action.gameObject.activeInHierarchy?action:null;
  public RoyalButton VisibleClose=>IsOpen?back:null;
  public string VisibleOperationText=>IsOpen?costs.text:"";
  public RectTransform VisibleProgress=>IsOpen&&operationProgress.gameObject.activeSelf?operationFill:null;
  public string BuildingId=>selected;
  public Texture ReferenceTexture=>portrait?portrait.texture:null;
  void LateUpdate(){if(IsOpen&&!(rankings&&rankings.IsOpen))root.SetAsLastSibling();}
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
  void Select(string id){
   if(Array.IndexOf(Ids,id)<0){if(root)root.gameObject.SetActive(false);suspended=false;selected="";return;}
   selected=id;message="";Ensure();
   if(hud.NarrativeOpen||(hud.Modal&&hud.Modal.gameObject.activeSelf)){root.gameObject.SetActive(false);suspended=true;return;}
   suspended=false;root.gameObject.SetActive(true);root.SetAsLastSibling();lastW=-1;SynchronizeLayout();
  }
  public void Close(){suspended=false;if(root)root.gameObject.SetActive(false);hud.SelectBuilding("");}
  void Update(){
   if(!hud||gateway==null)return;
   if(hud.NarrativeOpen||(hud.Modal&&hud.Modal.gameObject.activeSelf)){Suspend();return;}
   if(suspended&&!string.IsNullOrEmpty(selected)){suspended=false;Select(selected);}
   if(!IsOpen||rankings&&rankings.IsOpen)return;
   if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){Close();return;}
   if(Time.unscaledTime>=refresh){refresh=Time.unscaledTime+.2f;Refresh();}
   if(Mathf.Abs(root.rect.width-lastW)>.5f||Mathf.Abs(root.rect.height-lastH)>.5f||lastSafe!=Screen.safeArea)SynchronizeLayout();
  }
  internal static RectTransform Rect(string name,Transform parent){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);g.layer=parent.gameObject.layer;return (RectTransform)g.transform;}
  internal static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
  internal static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
  internal static RoyalText Text(string name,Transform parent,int font,Color color){var r=Rect(name,parent);r.gameObject.AddComponent<CanvasRenderer>();var t=r.gameObject.AddComponent<RoyalText>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=font;t.color=color;t.alignment=TextAnchor.UpperLeft;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
  internal static RectTransform Panel(string name,Transform parent){var r=Rect(name,parent);r.gameObject.AddComponent<CanvasRenderer>();r.gameObject.AddComponent<Image>().color=new Color(.065f,.105f,.14f,.96f);var edge=r.gameObject.AddComponent<Outline>();edge.effectColor=new Color(.55f,.43f,.25f,.65f);edge.effectDistance=new Vector2(1,-1);return r;}
  internal static RoyalButton Button(string name,Transform parent,string label,Action click,out RoyalText text){var r=Panel(name,parent);var b=r.gameObject.AddComponent<RoyalButton>();b.targetGraphic=r.GetComponent<Image>();b.onClick.AddListener(()=>click());text=Text(name+" label",r,16,Ivory);text.Put(label);text.alignment=TextAnchor.MiddleCenter;Stretch((RectTransform)text.transform);((RectTransform)text.transform).offsetMin=new Vector2(8,5);((RectTransform)text.transform).offsetMax=new Vector2(-8,-5);return b;}
  void Ensure(){if(root)return;
   root=Panel("V3 / City management destination",hud.transform);Stretch(root);root.GetComponent<Image>().color=new Color(.025f,.045f,.07f,1);
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
   back=Button("Back to city",footer,"VOLVER A VALORIA",Close,out _);back.name="Back";root.gameObject.SetActive(false);
  }
  void Choose(string value){var s=gateway.Snapshot();var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,"ChooseDevelopment",value,s.Revision));message=result.Ok?"Prioridad guardada. Valoria recibe los recursos elegidos.":result.Message;refreshed?.Invoke();SynchronizeLayout();}
  void Submit(){var b=read(selected);if(b?.Action==null||!b.Action.Enabled)return;var s=gateway.Snapshot();var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,b.Action.Kind,b.Action.Target,s.Revision));message=result.Message;refreshed?.Invoke();Refresh();}
  void Refresh(){if(!IsOpen)return;var s=gateway.Snapshot();var b=read(selected);if(b==null)return;
   ranking.gameObject.SetActive(s.ChapterThree.RankUnlocked);training.gameObject.SetActive(selected=="upgrade-barracks"&&s.BarracksLevel>0);
   int index=Array.IndexOf(Ids,selected);title.Put(Names[index]);kicker.Put("VALORIA  /  GESTIÓN DE LA CIUDAD  /  NIVEL "+b.Level);
   string resource=index==0?"bastion-v":index==1?"aserradero-v":index==2?"cuartel-v":index==3?"granero-v":"cantera-v";
   portrait.texture=Resources.Load<Texture2D>("BastionIVV/Portraits/"+resource);
   artCaption.Put(b.Level>=5?"ARQUITECTURA V · DISEÑO DE REFERENCIA":"PRÓXIMA ARQUITECTURA · NIVEL V\nCada mejora desarrolla este edificio de forma independiente.");
   bool choosing=selected=="bastion"&&s.BastionLevel>=4&&string.IsNullOrEmpty(s.ChapterThree.DevelopmentPriority);
   description.Put(choosing?"EL CONSEJO DE VALORIA\nElige una prioridad de desarrollo.":b.Function);
   costs.Put(choosing?"Una elección permanente. Ninguna opción bloquea edificios.":b.OperationActive?"OBRA EN CURSO · "+b.Time:b.Costs);
   status.Put(choosing?"Producción refuerza los talleres; defensa, las reservas militares; vida civil, el abastecimiento.":b.OperationActive?"Los recursos ya están pagados. La obra continúa al cerrar esta pantalla.":string.IsNullOrEmpty(b.Requirements)?b.Level>=5?"Nivel V alcanzado. Esta arquitectura será la base de futuras mejoras.":"Tiempo de obra: "+b.Time+"\nLa mejora visual se aplica al finalizar.":b.Requirements);
   operationProgress.gameObject.SetActive(b.OperationActive&&!choosing);operationFill.anchorMax=new Vector2(Mathf.Clamp01(b.Progress),1);
   choices.gameObject.SetActive(choosing);action.gameObject.SetActive(!choosing);action.interactable=b.Action!=null&&b.Action.Enabled&&!b.OperationActive;actionLabel.Put(b.Action?.Label??(b.OperationActive?"EN CURSO":b.Level>=5?"NIVEL V COMPLETADO":"EDIFICIO OPERATIVO"));notice.Put(message);
   for(int i=0;i<tabs.Length;i++)tabs[i].interactable=Ids[i]!=selected&&(i<3||s.BastionLevel>=(i==3?3:5));
  }
  public void SynchronizeLayout(){if(!root)return;root.SetAsLastSibling();Canvas.ForceUpdateCanvases();float w=root.rect.width,h=root.rect.height;lastW=w;lastH=h;lastSafe=Screen.safeArea;bool wide=w>h;float scale=Mathf.Max(.01f,hud.GetComponent<Canvas>().scaleFactor);float m=Mathf.Max(Mathf.Clamp(w*.025f,12,30),Mathf.Max(lastSafe.xMin,Screen.width-lastSafe.xMax)/scale+12),gap=16,head=wide?130:180,foot=100;
   float topInset=Mathf.Max(m,(Screen.height-lastSafe.yMax)/scale+12),bottomInset=Mathf.Max(m,lastSafe.yMin/scale+12);
   Place(header,m,topInset,w-2*m,head);Place((RectTransform)ranking.transform,wide?w-2*m-200:(w-2*m)*.51f,wide?2:76,wide?200:(w-2*m)*.49f,36);Place((RectTransform)training.transform,wide?w-2*m-390:0,wide?2:76,wide?180:(w-2*m)*.49f,36);Place((RectTransform)title.transform,0,0,w-2*m,40);Place((RectTransform)kicker.transform,0,44,w-2*m,28);
   for(int i=0;i<tabs.Length;i++)Place((RectTransform)tabs[i].transform,i*(w-2*m)/5,wide?80:124,(w-2*m)/5-5,wide?35:48);
   float top=topInset+head,body=h-top-foot-bottomInset;float aw=wide?(w-2*m-gap)*.43f:w-2*m;float ah=wide?body:body*.30f;
   Place(art,m,top,aw,ah);Place(detail,wide?m+aw+gap:m,wide?top:top+ah+gap,wide?w-2*m-aw-gap:w-2*m,wide?body:body-ah-gap);
   float side=Mathf.Min(aw-24,ah-65);Place((RectTransform)portrait.transform,(aw-side)*.5f,8,side,side);Place((RectTransform)artCaption.transform,14,ah-52,aw-28,48);
   float dw=detail.rect.width,dh=detail.rect.height,pad=18;
   bool compactCouncil=!wide&&selected=="bastion"&&gateway.Snapshot().BastionLevel>=4&&string.IsNullOrEmpty(gateway.Snapshot().ChapterThree.DevelopmentPriority);
   Place((RectTransform)description.transform,pad,pad,dw-2*pad,compactCouncil?65:80);Place((RectTransform)costs.transform,pad,compactCouncil?90:112,dw-2*pad,compactCouncil?48:52);Place((RectTransform)status.transform,pad,compactCouncil?150:180,dw-2*pad,compactCouncil?65:95);
   Place(operationProgress,pad,dh-32,dw-2*pad,7);
   Place(choices,pad,dh-166,dw-2*pad,156);for(int i=0;i<3;i++)Place((RectTransform)priority[i].transform,0,i*51,dw-2*pad,46);
   Place(footer,m,h-foot-bottomInset,w-2*m,foot);Place((RectTransform)notice.transform,0,0,w-2*m,30);Place((RectTransform)action.transform,(w-2*m)*.48f,35,(w-2*m)*.52f,45);Place((RectTransform)footer.Find("Back"),0,35,(w-2*m)*.45f,45);Refresh();
  }
 }
}
