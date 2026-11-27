using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Eldoria.LegacyProgression;

// Scratch-only candidate: no global canvas, game save, network, or production bootstrap.
public class NativeProfile : MonoBehaviour {
 [Serializable] public class Design { public string concept; public float[] metal,enamel,gem; public int points; public float bevel; public ScreenDesign[] screens; }
 [Serializable] public class ScreenDesign {public string section,hero,artSide; public int heroHeight,rowHeight,columns,iconSize,titleSize;}
 public Design design; public Font titleFont,bodyFont; public int width=390,height=844;
 public LocalProfile Profile {get;private set;}
 public string ActiveView {get;private set;}
 public RectTransform scrollContent;
 Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
 readonly Color Ink=C("#120D1B"),Panel=C("#2C213A"),Gold=C("#C99C54"),Paper=C("#F1E9D4"),Muted=C("#C8B7CE"),Aqua=C("#63D9CF");
 static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
 float S=>Mathf.Min(width/390f,height/680f);
 float W=>width, H=>height;
 float F(float n)=>n*S;
 RectTransform root;
 public static LocalProfile Fixture(int level=5) {
  var f=new ProgressFacts{Bastion=level,TotalPower=2452,KingdomPower=600,ArmyPower=648,VigilantStage=level>=2?5:0,ScoutReports=level>=3?3:0,DiscoveryMask=level>=3?3:0,RecordedHunts=level>=3?1:0,JourneyComplete=level>=3,RouteCleared=level>=3,EngendroDefeated=level>=2,LyraRecruited=level>=4,FissureClosed=level>=4,AshStalkerDefeated=level>=5};
  return new LocalProfile("Tester QA",f);
 }
 public void Bind(LocalProfile p,int w,int h,Design d,Font head,Font body){Profile=p;design=d;width=w;height=h;titleFont=head;bodyFont=body;root=GetComponent<RectTransform>();}
 RectTransform Box(Transform parent,string name,float x,float y,float w,float h){
  var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
 }
 Image Fill(RectTransform r,Color color){var i=r.gameObject.AddComponent<Image>();i.color=color;return i;}
 Sprite SpriteFor(string id){
  if(sprites.TryGetValue(id,out var s))return s;
  var t=Resources.Load<Texture2D>("PR198Art/"+id);if(!t)return null;
  s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);sprites[id]=s;return s;
 }
 Image Art(Transform p,string id,float x,float y,float w,float h,bool fit=true,Color? tint=null){
  var r=Box(p,"Art_"+id,x,y,w,h);var i=Fill(r,tint??Color.white);i.sprite=SpriteFor(id);i.preserveAspect=fit;i.raycastTarget=false;if(!i.sprite)i.color=Color.clear;return i;
 }
 Text Label(Transform p,string name,string text,float x,float y,float w,float h,float size,Color color,bool serif=false,TextAnchor align=TextAnchor.MiddleLeft){
  var r=Box(p,name,x,y,w,h);var l=r.gameObject.AddComponent<Text>();l.font=serif?titleFont:bodyFont;l.text=text;l.fontSize=Mathf.Max(10,Mathf.RoundToInt(size));l.color=color;l.alignment=align;l.supportRichText=false;l.horizontalOverflow=HorizontalWrapMode.Wrap;l.verticalOverflow=VerticalWrapMode.Truncate;l.raycastTarget=false;return l;
 }
 RectTransform Frame(Transform p,string name,float x,float y,float w,float h,Color color){
  var r=Box(p,name,x,y,w,h);Fill(r,Gold);var inner=Box(r,"Inner",F(1.5f),F(1.5f),w-F(3),h-F(3));Fill(inner,color);return r;
 }
 Button Click(RectTransform r,Action action){var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.onClick.AddListener(()=>action());return b;}
 void Clear(){for(int i=root.childCount-1;i>=0;i--){var g=root.GetChild(i).gameObject;g.SetActive(false);if(Application.isPlaying)Destroy(g);else DestroyImmediate(g);}}
 string Icon(ProfileEntry e,ProfileSection s){
  if(e.Title=="Distinción por descubrir"||e.Title=="Hazaña por descubrir"||e.Title=="Hallazgo desconocido")return "unknown";
  var key=e.Key;
  var direct=new Dictionary<string,string>{{"name","medallion"},{"power","power"},{"kingdom","kingdom"},{"army","army"},{"prestige","prestige"},{"title","title"},{"next-title","gem"},{"trophies","trophy"},{"discoveries","explorers"},{"offline","seal"},{"ranking:TotalPower","power"},{"ranking:Kingdom","kingdom"},{"ranking:Army","army"},{"ranking:Vigilants","vigilants"},{"ranking:Hunters","hunters"},{"ranking:Explorers","explorers"},{"week:discoveries","explorers"},{"week:gathered","kingdom"},{"ashes","ashes"},{"fissure","fissure"},{"frontier","frontier"},{"monument-founder","founder"},{"monument-guard","fountain"},{"monument-hunt","hunting-monument"},{"monument-fissure","fissure-memorial"},{"monument-ash","ash-memorial"}};
  if(direct.TryGetValue(key,out var icon))return icon;
  if(key.StartsWith("title:"))return "medallion-"+(Math.Max(0,Array.FindIndex(LegacyProgressionRules.Titles.ToArray(),x=>"title:"+x.Id==key))%4);
  if(key.StartsWith("banner-"))return "banner";
  if(key.StartsWith("bastion-"))return "medallion-"+(int.TryParse(key.Substring(8),out var n)?n%4:0);
  if(key.StartsWith("vigilant-"))return "guard";if(key.StartsWith("hunt-"))return "hunt";if(key.StartsWith("scout-"))return "scout";if(key.StartsWith("discovery-"))return "explorers";
  return s==ProfileSection.Discoveries?"chronicle":"trophy";
 }
 string State(ProfileEntry e)=>e.State==ProfileEntryState.Earned?"Conseguido":e.State==ProfileEntryState.Locked?"Bloqueado":e.State==ProfileEntryState.Unavailable?"No disponible":e.State==ProfileEntryState.Available?"Disponible":"";
 public void Show(ProfileSection sec,string detail=null){
  if(Profile==null||design==null||!ProfilePresentation.Sections(Profile).Contains(sec))throw new InvalidOperationException("Locked or missing view");
  Clear();ActiveView=detail!=null?"DiscoveryDetail":sec.ToString();var d=design.screens.Single(x=>x.section==ActiveView);
  Art(root,"vault",0,0,W,H,false,new Color(.30f,.26f,.36f,1));Fill(Box(root,"Veil",0,0,W,H),new Color(.045f,.029f,.071f,.82f));
  var sections=ProfilePresentation.Sections(Profile);float gutter=F(12),top=F(54),nav=F(47),navRows=(W/H>1)?1:2;float navH=nav*navRows;
  var mast=Box(root,"Masthead",0,0,W,top);Fill(mast,Ink);Art(mast,"medallion",gutter,F(4),F(44),F(44));Label(mast,"Brand","ELDORIA",F(60),0,W-F(150),top,F(25),Gold,true);
  Label(mast,"Fixture","PRUEBA QA",W-F(108),0,F(96),top,F(11),Muted,false,TextAnchor.MiddleRight);
  Fill(Box(root,"MastheadRule",0,top-F(1),W,F(1)),Gold);
  int cols=(W/H>1)?6:3;float gap=F(4),tabW=(W-2*gutter-gap*(cols-1))/cols;
  for(int i=0;i<sections.Count;i++){
   var s=sections[i];int row=i/cols,col=i%cols;float x=gutter+col*(tabW+gap),y=top+row*nav;
   var tab=Frame(root,"Tab_"+s,x,y,tabW,nav-gap,s==sec?C("#25434B"):C("#2C213B"));
   Art(tab,s==ProfileSection.Overview?"medallion":s==ProfileSection.Rankings?"power":s==ProfileSection.Trophies?"trophy":s==ProfileSection.Discoveries?"chronicle":s==ProfileSection.Heraldry?"banner":"founder",F(4),F(8),F(24),F(24));
   Label(tab,"TabLabel",ProfilePresentation.Label(s),F(30),F(1),tabW-F(34),nav-gap-F(2),F(12),s==sec?Aqua:Paper);
   Click(tab,()=>Show(s));
  }
  float bodyY=top+navH+F(8),bodyH=H-bodyY-F(22);
  var viewport=Box(root,"Viewport",0,bodyY,W,bodyH);var vi=Fill(viewport,new Color(0,0,0,.01f));viewport.gameObject.AddComponent<RectMask2D>();
  var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.viewport=viewport;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;
  scrollContent=Box(viewport,"Content",0,0,W,bodyH);scroll.content=scrollContent;
  var heroH=F(d.heroHeight);float y0=0;var hero=Frame(scrollContent,"Hero",gutter,y0,W-2*gutter,heroH,Panel);Art(hero,d.hero,F(2),F(2),W-2*gutter-F(4),heroH-F(4),false,new Color(.76f,.71f,.79f,1));
  // A translucent title chamber preserves illustration and text as separate live layers.
  float zoneW=(W-2*gutter)*.62f,zoneX=d.artSide=="right"?F(2):W-2*gutter-zoneW-F(2);
  Fill(Box(hero,"InscriptionVeil",zoneX,F(2),zoneW,heroH-F(4)),new Color(.065f,.043f,.095f,.88f));
  Label(hero,"Chapter","VALORIA · BASTIÓN "+Profile.Facts.Bastion,zoneX+F(12),F(12),zoneW-F(20),F(20),F(11),Gold);
  Label(hero,"Heading",ProfilePresentation.Label(sec),zoneX+F(12),F(34),zoneW-F(22),F(48),F(d.titleSize),Paper,true);
  var rows=detail!=null?ProfilePresentation.DiscoveryEntries(Profile,detail):ProfilePresentation.Rows(Profile,sec,new DateTime(2026,11,24,13,0,0,DateTimeKind.Utc));
  int consumed=0;
  if(sec==ProfileSection.Overview&&rows.Count>0&&detail==null){
   var identity=rows[0];Label(hero,"PlayerName",identity.Title,zoneX+F(12),F(80),zoneW-F(22),F(30),F(24),Paper,true);Label(hero,"PlayerBastion",identity.Detail,zoneX+F(12),F(112),zoneW-F(22),F(24),F(15),Muted);
   float artX=d.artSide=="right"?W-2*gutter-F(126):F(9);Art(hero,"medallion",artX,Mathf.Max(F(12),(heroH-F(116))/2),F(116),F(116));consumed=1;
  }
  y0+=heroH+F(10);
  if(detail!=null){var back=Frame(scrollContent,"Back",gutter,y0,W-2*gutter,F(44),C("#25434B"));Label(back,"BackText","Volver a hallazgos",F(14),0,W-2*gutter-F(28),F(44),F(15),Aqua);Click(back,()=>Show(sec));y0+=F(52);}
  int grid=(W/H>1)?Math.Max(2,d.columns):d.columns;
  for(int index=consumed;index<rows.Count;){
   var e=rows[index];bool full=(e.Key=="offline"||e.Key=="power"||e.Key=="prestige"||sec==ProfileSection.Discoveries||detail!=null);int count=full?1:Math.Min(grid,rows.Count-index);
   if(count>1&&rows.Skip(index+1).Take(count-1).Any(a=>a.Key=="power"||a.Key=="prestige"||a.Key=="offline"))count=1;
   float cw=(W-2*gutter-gap*(count-1))/count;
   bool gallery=sec==ProfileSection.Trophies||sec==ProfileSection.Monuments;
   float rowH=F(e.Key=="offline"?Math.Max(100,d.rowHeight):gallery&&count>1?Math.Max(118,d.rowHeight):Math.Max(72,d.rowHeight));
   if(W/H>1)rowH=Mathf.Max(F(70),rowH);
   for(int col=0;col<count;col++){
    var entry=rows[index+col];float x=gutter+col*(cw+gap);var card=Frame(scrollContent,"Entry_"+entry.Key,x,y0,cw,rowH,entry.State==ProfileEntryState.Unavailable?C("#332739"):entry.State==ProfileEntryState.Locked?C("#231D2C"):Panel);
    Art(card,"panel_obsidian",F(2),F(2),cw-F(4),rowH-F(4),false,new Color(.55f,.47f,.62f,.80f));Fill(Box(card,"DarkInscription",F(3),F(3),cw-F(6),rowH-F(6)),new Color(.073f,.047f,.102f,.53f));
    var icon=Icon(entry,sec);var tint=entry.State==ProfileEntryState.Locked?new Color(.65f,.58f,.68f,1):Color.white;
    if(gallery&&count>1){
     float size=Mathf.Min(F(d.iconSize),rowH-F(56));Art(card,icon,(cw-size)/2,F(5),size,size,true,tint);
     Label(card,"Title",entry.Title,F(6),size+F(5),cw-F(12),F(34),F(14),Paper,true,TextAnchor.MiddleCenter);
     Label(card,"Detail",entry.Detail,F(6),size+F(38),cw-F(12),F(24),F(12),Muted,false,TextAnchor.MiddleCenter);
     Label(card,"State",State(entry),F(6),rowH-F(19),cw-F(12),F(17),F(11),entry.State==ProfileEntryState.Earned?Gold:Muted,false,TextAnchor.MiddleCenter);
    }else{
     float size=Mathf.Min(F(d.iconSize),rowH-F(18),cw*.30f);Art(card,icon,F(9),(rowH-size)/2,size,size,true,tint);
     float tx=size+F(18),tw=cw-tx-F(10);bool big=entry.Key=="power"||entry.Key=="prestige";
     Label(card,"Title",entry.Title,tx,F(5),tw,F(24),F(big?17:15),Paper,true);
     Label(card,"Detail",entry.Detail,tx,F(29),tw,rowH-F(48),F(big?23:14),big?Gold:Muted,false,TextAnchor.UpperLeft);
     Label(card,"State",State(entry),tx,rowH-F(19),tw,F(16),F(11),entry.State==ProfileEntryState.Earned?Gold:Muted);
    }
    if(sec==ProfileSection.Discoveries&&detail==null){string id=entry.Key;Click(card,()=>Show(sec,id));}
   }
   index+=count;y0+=rowH+F(8);
  }
  scrollContent.sizeDelta=new Vector2(W,Mathf.Max(bodyH,y0+gutter));
  Label(root,"Footer","Prototipo nativo · proyección PR #198",gutter,H-F(22),W-2*gutter,F(22),F(10),Muted,false,TextAnchor.MiddleCenter);
  Canvas.ForceUpdateCanvases();
 }
}
