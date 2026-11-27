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
 Color Ink=C("#120D1B"),Panel=C("#2C213A"),Gold=C("#C99C54"),Paper=C("#F1E9D4"),Muted=C("#C8B7CE"),Aqua=C("#63D9CF");
 static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
 float S=>width>height?0.82f:width/390f;
 float W=>width; float H=>height;
 float F(float n)=>n*S;
 RectTransform root;
 public static LocalProfile Fixture(int level=5) {
  var f=new ProgressFacts{Bastion=level,TotalPower=2452,KingdomPower=600,ArmyPower=648,VigilantStage=level>=2?5:0,ScoutReports=level>=3?3:0,DiscoveryMask=level>=3?3:0,RecordedHunts=level>=3?1:0,JourneyComplete=level>=3,RouteCleared=level>=3,EngendroDefeated=level>=2,LyraRecruited=level>=4,FissureClosed=level>=4,AshStalkerDefeated=level>=5};
  return new LocalProfile("Tester QA",f);
 }
 [Serializable] public class Palette {public string ink,surface,gold,paper,muted,action;}
 public void Bind(LocalProfile p,int w,int h,Design d,Font head,Font body){
 Profile=p;design=d;width=w;height=h;titleFont=head;bodyFont=body;root=GetComponent<RectTransform>();
 var path=System.IO.Path.Combine(Application.dataPath,"palette.json");
 if(System.IO.File.Exists(path)){var c=JsonUtility.FromJson<Palette>(System.IO.File.ReadAllText(path));Ink=C(c.ink);Panel=C(c.surface);Gold=C(c.gold);Paper=C(c.paper);Muted=C(c.muted);Aqua=C(c.action);}
 }
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
  var r=Box(p,name,x,y,w,h);var l=r.gameObject.AddComponent<Text>();l.font=serif?titleFont:bodyFont;l.text=text;l.fontSize=Mathf.Max(11,Mathf.RoundToInt(size));l.resizeTextForBestFit=true;l.resizeTextMinSize=11;l.resizeTextMaxSize=l.fontSize;l.color=color;l.alignment=align;l.supportRichText=false;l.horizontalOverflow=HorizontalWrapMode.Wrap;l.verticalOverflow=VerticalWrapMode.Truncate;l.raycastTarget=false;return l;
 }
 RectTransform Frame(Transform p,string name,float x,float y,float w,float h,Color color){
  var r=Box(p,name,x,y,w,h);Fill(r,Gold);var inner=Box(r,"Inner",F(1.5f),F(1.5f),w-F(3),h-F(3));Fill(inner,color);return r;
 }
 Button Click(RectTransform r,Action action){var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();b.onClick.AddListener(()=>action());return b;}
 void Clear(){for(int i=root.childCount-1;i>=0;i--){var g=root.GetChild(i).gameObject;g.SetActive(false);if(Application.isPlaying)Destroy(g);else DestroyImmediate(g);}}

 readonly Dictionary<string,Texture2D> gradients=new Dictionary<string,Texture2D>();
 RectTransform Solid(Transform p,string n,float x,float y,float w,float h,Color c){var r=Box(p,n,x,y,w,h);Fill(r,c).raycastTarget=false;return r;}
 RectTransform Shade(Transform p,string n,float x,float y,float w,float h,Color top,Color bottom){
  var key=top.ToString()+bottom.ToString();if(!gradients.TryGetValue(key,out var tex)){tex=new Texture2D(1,64,TextureFormat.RGBA32,false);for(int j=0;j<64;j++)tex.SetPixel(0,j,Color.Lerp(bottom,top,j/63f));tex.Apply();gradients[key]=tex;}
  var r=Box(p,n,x,y,w,h);var raw=r.gameObject.AddComponent<RawImage>();raw.texture=tex;raw.raycastTarget=false;return r;
 }
 void Crop(Transform p,string id,float x,float y,float w,float h,float pos=.5f){
  var t=Resources.Load<Texture2D>("PR198Art/"+id);if(!t)return;var r=Box(p,"Illustration_"+id,x,y,w,h);var a=r.gameObject.AddComponent<RawImage>();a.texture=t;a.raycastTarget=false;
  float target=w/h,aspect=t.width/(float)t.height;
  if(aspect>target){float u=target/aspect;a.uvRect=new Rect((1-u)*pos,0,u,1);}
  else{float v=aspect/target;a.uvRect=new Rect(0,(1-v)*.52f,1,v);}
 }
 RectTransform Plaque(Transform p,string name,float x,float y,float w,float h,bool featured=false){
  var r=Box(p,name,x,y,w,h);Fill(r,Ink);Solid(r,"MetalRim",0,0,w,h,new Color(Gold.r,Gold.g,Gold.b,featured ? .90f : .32f));
  Shade(r,"BeveledFace",F(1.5f),F(1.5f),w-F(3),h-F(3),Color.Lerp(Panel,Paper,.10f),Color.Lerp(Panel,Ink,.45f));
  Solid(r,"Glint",F(6),F(1.5f),w-F(12),F(1),new Color(Gold.r,Gold.g,Gold.b,.66f));
  Solid(r,"Inset",F(4),h-F(3),w-F(8),F(1),new Color(0,0,0,.85f));return r;
 }
 string State(ProfileEntry e)=>e.State==ProfileEntryState.Earned?"Conseguido":e.State==ProfileEntryState.Locked?"Bloqueado":e.State==ProfileEntryState.Unavailable?"No disponible":e.State==ProfileEntryState.Available?"Disponible":"";
 string Icon(ProfileEntry e,ProfileSection sec){
  if(e.Title=="Distinción por descubrir"||e.Title=="Hazaña por descubrir"||e.Title=="Hallazgo desconocido")return "unknown";
  var k=e.Key;
  var ids=new Dictionary<string,string>{{"name","medallion"},{"power","power"},{"kingdom","kingdom"},{"army","army"},{"prestige","prestige"},{"title","insignia-warden"},{"next-title","insignia-legend"},{"trophies","trophy"},{"discoveries","explorers"},{"offline","seal"},{"ranking:TotalPower","power"},{"ranking:Kingdom","kingdom"},{"ranking:Army","army"},{"ranking:Vigilants","vigilants"},{"ranking:Hunters","hunters"},{"ranking:Explorers","explorers"},{"week:discoveries","explorers"},{"week:gathered","kingdom"},{"bastion-2","medal-b2"},{"bastion-3","medal-b3"},{"bastion-4","medal-b4"},{"bastion-5","medal-b5"},{"vigilant-1","medal-v1"},{"vigilant-5","medal-v5"},{"monument-founder","statue-founder"},{"monument-guard","statue-guard"},{"monument-hunt","statue-hunt"},{"monument-fissure","statue-fissure"},{"monument-ash","statue-ash"},{"journey","frontier"},{"route","scout"},{"engendro","guard"},{"investigation","gem"},{"contact","chronicle"},{"closure","fissure"},{"alliance","banner"},{"patrol","guard"},{"travelers","scout"},{"supplies","kingdom"},{"breach","gem"}};
  if(ids.TryGetValue(k,out var a))return a;
  if(k.StartsWith("title:")){var n="insignia-"+k.Substring(6);return Resources.Load<Texture2D>("PR198Art/"+n)?n:"insignia-legend";}
  if(k.StartsWith("banner-"))return k=="banner-founder"||k=="banner-fissure"?"banner-purple":"banner-cream";
  if(k.StartsWith("bastion-"))return "medal-b5";
  if(k.StartsWith("vigilant-"))return "medal-v5";if(k.StartsWith("hunt-"))return "medal-hunt";if(k.StartsWith("scout-"))return "medal-scout";if(k.StartsWith("discovery-"))return "medal-discovery";
  return sec==ProfileSection.Discoveries?"folio":"trophy";
 }
 public void Show(ProfileSection sec,string detail=null){
  if(Profile==null||design==null||!ProfilePresentation.Sections(Profile).Contains(sec))throw new InvalidOperationException("Locked or missing view");
  Clear();ActiveView=detail!=null?"DiscoveryDetail":sec.ToString();var d=design.screens.Single(z=>z.section==ActiveView);
  Shade(root,"Background",0,0,W,H,Color.Lerp(Ink,Panel,.45f),Ink);
  float gutter=F(12),gap=F(8),top=F(48),nav=F(44);
  var sections=ProfilePresentation.Sections(Profile);int navCols=W>H?6:3;float navRows=Mathf.Ceil(sections.Count/(float)navCols);
  var mast=Solid(root,"Masthead",0,0,W,top,Ink);Art(mast,"medallion",gutter,F(3),F(39),F(39));Label(mast,"Brand","ELDORIA",F(55),0,W-F(156),top,F(24),Gold,true);
  Label(mast,"Fixture","PRUEBA QA",W-F(100),0,F(88),top,F(11),Muted,false,TextAnchor.MiddleRight);
  Solid(root,"Rule",0,top-F(1),W,F(1),Gold);
  float tabW=(W-2*gutter)/navCols;
  for(int i=0;i<sections.Count;i++){
   var s=sections[i];float x=gutter+i%navCols*tabW,y=top+i/navCols*nav;
   var tab=Box(root,"Tab_"+s,x,y,tabW,nav);Fill(tab,s==sec?Color.Lerp(Panel,Aqua,.16f):Color.Lerp(Ink,Panel,.37f));
   string icon=s==ProfileSection.Overview?"medallion":s==ProfileSection.Rankings?"power":s==ProfileSection.Trophies?"medal-b5":s==ProfileSection.Discoveries?"folio":s==ProfileSection.Heraldry?"insignia-warden":"statue-founder";
   Art(tab,icon,F(3),F(6),F(26),F(26));Label(tab,"TabLabel",ProfilePresentation.Label(s),F(32),0,tabW-F(36),nav-F(2),F(13),s==sec?Aqua:Paper);
   if(s==sec)Solid(tab,"Selected",0,nav-F(2),tabW,F(2),Aqua);Click(tab,()=>Show(s));
  }
  float bodyY=top+nav*navRows+F(4),bodyH=H-bodyY-F(20);
  var vp=Box(root,"Viewport",0,bodyY,W,bodyH);Fill(vp,new Color(0,0,0,.01f));vp.gameObject.AddComponent<RectMask2D>();
  var sr=vp.gameObject.AddComponent<ScrollRect>();sr.horizontal=false;sr.vertical=true;sr.viewport=vp;sr.movementType=ScrollRect.MovementType.Clamped;
  scrollContent=Box(vp,"Content",0,0,W,bodyH);sr.content=scrollContent;
  var rows=detail!=null?ProfilePresentation.DiscoveryEntries(Profile,detail):ProfilePresentation.Rows(Profile,sec,new DateTime(2026,11,24,13,0,0,DateTimeKind.Utc));
  float y0=0;float heroH=F(Mathf.Clamp(d.heroHeight,100,176));if(sec==ProfileSection.Overview)heroH=F(Mathf.Max(158,Mathf.Min(190,d.heroHeight)));if(detail!=null)heroH=F(100);
  string art=detail!=null?"hero-discoveries":d.hero;
  var hero=Box(scrollContent,"Hero",0,y0,W,heroH);Crop(hero,art,0,0,W,heroH,d.artSide=="right" ? .62f : .38f);
  Shade(hero,"Legibility",0,0,W,heroH,new Color(Ink.r,Ink.g,Ink.b,.04f),new Color(Ink.r,Ink.g,Ink.b,.97f));
  Solid(hero,"HeroBottom",0,heroH-F(2),W,F(2),Gold);
  int consumed=0;
  if(sec==ProfileSection.Overview&&rows.Count>0&&detail==null){
   Art(hero,"medallion",W-F(126),heroH-F(115),F(105),F(105));
   Label(hero,"Chapter","VALORIA · BASTIÓN "+Profile.Facts.Bastion,gutter,F(9),W-F(140),F(19),F(12),Gold);
   Label(hero,"Heading",ProfilePresentation.Label(sec),gutter,heroH-F(88),W-F(143),F(36),F(d.titleSize),Paper,true);
   Label(hero,"PlayerName",rows[0].Title,gutter,heroH-F(52),W-F(143),F(29),F(23),Paper,true);
   Label(hero,"Bastion",rows[0].Detail,gutter,heroH-F(23),W-F(140),F(21),F(13),Muted);consumed=1;
  }else{
   Label(hero,"Chapter","VALORIA · BASTIÓN "+Profile.Facts.Bastion,gutter,heroH-F(58),W-2*gutter,F(18),F(11),Gold);
   Label(hero,"Heading",ProfilePresentation.Label(sec),gutter,heroH-F(39),W-2*gutter,F(37),F(d.titleSize),Paper,true);
  }
  y0+=heroH+F(10);
  if(detail!=null){var back=Plaque(scrollContent,"Back",gutter,y0,W-2*gutter,F(42),true);Label(back,"BackText","Volver a hallazgos",F(11),0,W-2*gutter-F(22),F(42),F(15),Aqua);Click(back,()=>Show(sec));y0+=F(50);}
  for(int index=consumed;index<rows.Count;){
   var first=rows[index];bool overview=sec==ProfileSection.Overview;
   bool isGallery=sec==ProfileSection.Trophies||sec==ProfileSection.Monuments||(sec==ProfileSection.Heraldry&&first.Key.StartsWith("title:"));
   bool metric=overview&&(first.Key=="kingdom"||first.Key=="army"||first.Key=="trophies"||first.Key=="discoveries");
   bool dossier=sec==ProfileSection.Discoveries&&detail==null;
   bool prominent=first.Key=="power"||first.Key=="prestige"||first.Key=="offline";
   int count=(metric||isGallery)?Math.Min(2,rows.Count-index):1;
   if(overview&&count==2&&!new[]{"kingdom","army","trophies","discoveries"}.Contains(rows[index+1].Key))count=1;
   if(sec==ProfileSection.Heraldry&&count==2&&!rows[index+1].Key.StartsWith("title:"))count=1;
   if(sec==ProfileSection.Monuments&&d.columns==1)count=1;
   float cw=(W-2*gutter-gap*(count-1))/count;
   float rh=F(dossier?132:isGallery&&count==2?Mathf.Clamp(d.rowHeight,145,170):metric?76:prominent?first.Key=="offline"?104:84:Mathf.Clamp(d.rowHeight,78,112));
   for(int col=0;col<count;col++){
    var e=rows[index+col];float x=gutter+col*(cw+gap);bool locked=e.State==ProfileEntryState.Locked,earned=e.State==ProfileEntryState.Earned;string state=State(e);
    var card=Plaque(scrollContent,"Entry_"+e.Key,x,y0,cw,rh,prominent||earned);var tint=locked?new Color(.67f,.65f,.70f,1):Color.white;
    if(dossier){
     float iw=cw*.44f;if(e.Key=="frontier")Crop(card,"collection-"+e.Key,F(2),F(2),iw-F(3),rh-F(4));else Art(card,"collection-"+e.Key,F(5),F(5),iw-F(7),rh-F(10),true);Shade(card,"DossierFade",0,rh-F(31),iw,F(29),new Color(0,0,0,0),Ink);
     Label(card,"Title",e.Title,iw+F(9),F(7),cw-iw-F(16),F(44),F(19),Paper,true);
     Label(card,"Detail",e.Detail,iw+F(9),F(54),cw-iw-F(16),F(31),F(25),Gold,true);
     Label(card,"State",state,iw+F(9),rh-F(23),cw-iw-F(17),F(18),F(12),earned?Gold:Muted);var id=e.Key;Click(card,()=>Show(sec,id));
    }else if(isGallery&&count==2){
     float icon=Mathf.Min(F(Mathf.Max(77,d.iconSize)),rh-F(78));Art(card,Icon(e,sec),(cw-icon)/2,F(7),icon,icon,true,tint);
     Label(card,"Title",e.Title,F(8),rh-F(73),cw-F(16),F(33),F(17),Paper,true,TextAnchor.MiddleCenter);
     Label(card,"Detail",e.Detail,F(8),rh-F(40),cw-F(16),F(22),F(13),Muted,false,TextAnchor.MiddleCenter);
     Label(card,"State",state,F(7),rh-F(19),cw-F(14),F(17),F(11),earned?Gold:Muted,false,TextAnchor.MiddleCenter);
    }else{
     float sz=Mathf.Min(F(metric?38:Mathf.Clamp(d.iconSize,46,82)),rh-F(14),cw*.30f);
     Art(card,Icon(e,sec),F(9),(rh-sz)/2,sz,sz,true,tint);
     float tx=sz+F(17),tw=cw-tx-F(9);
     bool number=metric||e.Key=="power"||e.Key=="prestige"||e.Key.StartsWith("ranking:")||e.Key.StartsWith("week:");
     Label(card,"Title",e.Title,tx,F(5),tw,F(24),F(17),Paper,true);
     Label(card,"Detail",e.Detail,tx,F(30),tw,rh-F(state.Length>0?52:35),F(number?21:15),number?Gold:Muted,false,TextAnchor.MiddleLeft);
     Label(card,"State",state,tx,rh-F(19),tw,F(17),F(11),earned?Gold:Muted);
    }
   }
   index+=count;y0+=rh+gap;
  }
  scrollContent.sizeDelta=new Vector2(W,Mathf.Max(bodyH,y0+gutter));Label(root,"Footer","Prototipo nativo · proyección PR #198",gutter,H-F(20),W-2*gutter,F(20),F(10),Muted,false,TextAnchor.MiddleCenter);Canvas.ForceUpdateCanvases();
 }
}
