using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold {
 /// <summary>Original slice-inspired cinematic dialogue INSIDE the retained
 /// Royal Gold ModalWindow prefab. No second HUD, clock, state or gameplay.</summary>
 [DisallowMultipleComponent]
 public sealed class RoyalNarrativePresentation : MonoBehaviour {
  [Serializable] public struct Line {
   public string Speaker, Spanish, English;
   public Line(string speaker,string spanish,string english) {
    Speaker=speaker; Spanish=spanish; English=english;
   }
  }
  public Texture2D AldricPortrait;
  ModalWindow modal;
  RectTransform window, portraitRect;
  Vector2 oldWindowSize,oldTitlePos,oldTitleSize,oldBodyPos,oldBodySize,oldActionPos,oldActionSize;
  int oldBodyFontSize;TextAnchor oldBodyAlignment;
  HorizontalWrapMode oldHorizontal;VerticalWrapMode oldVertical;
  CanvasGroup entrance;
  Vector2 restingWindowPosition;
  RawImage face;
  RoyalText role, narratorGlyph;
  Graphic backdrop;
  Color oldBackdrop;
  readonly List<Line> turns=new List<Line>();
  readonly List<string> pages=new List<string>();
  Action onComplete;
  int lineIndex,pageIndex,shown;
  float characterBudget,hold,entryTime;
  bool english,active;
  public bool IsShowing=>active;
  public bool IsRevealing=>active&&shown<CurrentPage.Length;
  public string FullText=>active?string.Join("\n",turns.ConvertAll(l=>english?l.English:l.Spanish)):"";
  public string CurrentSpeaker=>active&&lineIndex<turns.Count?turns[lineIndex].Speaker:"";
  public int TurnIndex=>lineIndex;
  public int TurnCount=>turns.Count;
  string CurrentPage=>pageIndex<pages.Count?pages[pageIndex]:"";

  void Awake(){modal=GetComponent<ModalWindow>();}
  void EnsureParts(){
   if(!modal)modal=GetComponent<ModalWindow>();
   if(!modal||role)return;
   window=modal.Title.rectTransform.parent as RectTransform;
   if(!window)throw new InvalidOperationException("Royal Gold original modal window missing");
   restingWindowPosition=window.anchoredPosition;
   oldWindowSize=window.sizeDelta;
   oldTitlePos=modal.Title.rectTransform.anchoredPosition;
   oldTitleSize=modal.Title.rectTransform.sizeDelta;
   oldBodyPos=modal.Body.rectTransform.anchoredPosition;
   oldBodySize=modal.Body.rectTransform.sizeDelta;
   oldActionPos=((RectTransform)modal.Extra.transform).anchoredPosition;
   oldActionSize=((RectTransform)modal.Extra.transform).sizeDelta;
   oldBodyFontSize=modal.Body.fontSize;
   oldBodyAlignment=modal.Body.alignment;
   oldHorizontal=modal.Body.horizontalOverflow;
   oldVertical=modal.Body.verticalOverflow;
   entrance=window.GetComponent<CanvasGroup>();
   if(!entrance)entrance=window.gameObject.AddComponent<CanvasGroup>();
   entrance.blocksRaycasts=true;entrance.interactable=true;
   role=Instantiate(modal.Title,window);role.name="Narrative character role";
   role.color=new Color(.76f,.81f,.83f);role.fontSize=17;role.fontStyle=FontStyle.Normal;
   role.alignment=TextAnchor.MiddleLeft;role.raycastTarget=false;
   Position(role.rectTransform,115,-78,425,30);
   var p=new GameObject("Canonical Aldric portrait",typeof(RectTransform),typeof(RawImage),typeof(Outline));
   p.transform.SetParent(window,false);p.layer=gameObject.layer;
   portraitRect=p.GetComponent<RectTransform>();Position(portraitRect,28,-32,70,70);
   face=p.GetComponent<RawImage>();face.texture=AldricPortrait;
   face.raycastTarget=false;face.color=Color.white;
   var border=p.GetComponent<Outline>();border.effectColor=new Color(.77f,.61f,.32f,1);border.effectDistance=new Vector2(2,-2);
   narratorGlyph=Instantiate(modal.Title,window);narratorGlyph.name="Narrator sigil";
   narratorGlyph.color=new Color(.9f,.75f,.42f);narratorGlyph.fontSize=48;
   narratorGlyph.alignment=TextAnchor.MiddleCenter;narratorGlyph.raycastTarget=false;
   Position(narratorGlyph.rectTransform,28,-32,70,70);
   backdrop=modal.GetComponent<Graphic>();
   if(backdrop)oldBackdrop=backdrop.color;
  }
  static void Position(RectTransform r,float x,float y,float w,float h){
   r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
   r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);
  }
  public void Begin(Line[] entries,bool isEnglish,Action completed){
   if(entries==null||entries.Length==0)throw new ArgumentException("Narrative has no speaker turns");
   EnsureParts();if(!modal)throw new InvalidOperationException("Narrative must be hosted by the Royal Gold modal");
   turns.Clear();turns.AddRange(entries);onComplete=completed;
   if(role)role.gameObject.SetActive(true);
   english=isEnglish;lineIndex=0;pageIndex=0;shown=0;active=true;characterBudget=0;hold=0;entryTime=0;
   if(backdrop)backdrop.color=new Color(.005f,.014f,.03f,.60f);
   modal.Close.gameObject.SetActive(false);
   if(modal.Secondary)modal.Secondary.gameObject.SetActive(false);
   modal.Extra.gameObject.SetActive(true);
   modal.Body.fontSize=22;modal.Body.alignment=TextAnchor.UpperLeft;
   modal.Body.horizontalOverflow=HorizontalWrapMode.Wrap;
   modal.Body.verticalOverflow=VerticalWrapMode.Truncate;
   Position(modal.Title.rectTransform,115,-34,430,38);
   NextLine();
  }
  void NextLine(){
   pages.Clear();pageIndex=0;shown=0;characterBudget=0;hold=0;entryTime=0;
   var line=turns[lineIndex];
   bool narrator=line.Speaker=="NARRADOR";
   bool aldric=line.Speaker=="Sir Aldric"||line.Speaker=="Aldric";
   // An authored portrait identifies its actual speaker. Other speakers retain the
   // established royal narrative emblem until their own portrait is approved.
   bool showAldric=aldric&&AldricPortrait;
   if(face){face.texture=showAldric?AldricPortrait:null;face.gameObject.SetActive(showAldric);}
   if(narratorGlyph){narratorGlyph.gameObject.SetActive(!showAldric);narratorGlyph.Put(aldric?"♜":"✦");}
   modal.Title.Put(narrator?(english?"ASHES OF VALORIA":"LAS CENIZAS DE VALORIA"):line.Speaker.ToUpperInvariant());
   role.Put(narrator?(english?"Eldoria chronicle":"Crónica de Eldoria"):aldric?(english?"Guardian of Valoria":"Guardián de Valoria"):line.Speaker=="Lyra"?(english?"Valoria frontier":"Frontera de Valoria"):"VALORIA");
   string text=english?line.English:line.Spanish;
   foreach(var page in Paginate(text,218))pages.Add(page);
   LayoutNarrativePage();
   if(entrance){entrance.alpha=.28f;window.anchoredPosition=restingWindowPosition+new Vector2(0,-10f);}
  }
  void LayoutNarrativePage(){
   // The previous generic 590x580 window left an oversized empty slab over
   // the city. Fit each actual speaker/page to its readable text, while
   // retaining the original Royal Gold panel, border and button.
   Position(modal.Body.rectTransform,32,-144,526,228);
   modal.Body.Put(CurrentPage);
   Canvas.ForceUpdateCanvases();
   var textHeight=Mathf.Clamp(modal.Body.preferredHeight+18,128,228);
   Position(modal.Body.rectTransform,32,-144,526,textHeight);
   var buttonTop=144+textHeight+20;
   Position((RectTransform)modal.Extra.transform,30,-buttonTop,530,64);
   window.sizeDelta=new Vector2(590,buttonTop+84);
   modal.Body.Put("");
   modal.ExtraLabel.Put(english?"Reveal text":"Mostrar texto");
  }
  void RestoreModalLayout(){
   if(!modal||!window)return;
   if(face)face.gameObject.SetActive(false);
   if(narratorGlyph)narratorGlyph.gameObject.SetActive(false);
   if(role)role.gameObject.SetActive(false);
   window.anchoredPosition=restingWindowPosition;
   window.sizeDelta=oldWindowSize;
   modal.Title.rectTransform.anchoredPosition=oldTitlePos;
   modal.Title.rectTransform.sizeDelta=oldTitleSize;
   modal.Body.rectTransform.anchoredPosition=oldBodyPos;
   modal.Body.rectTransform.sizeDelta=oldBodySize;
   ((RectTransform)modal.Extra.transform).anchoredPosition=oldActionPos;
   ((RectTransform)modal.Extra.transform).sizeDelta=oldActionSize;
   modal.Body.fontSize=oldBodyFontSize;
   modal.Body.alignment=oldBodyAlignment;
   modal.Body.horizontalOverflow=oldHorizontal;
   modal.Body.verticalOverflow=oldVertical;
   if(entrance)entrance.alpha=1;
   if(backdrop)backdrop.color=oldBackdrop;
   if(modal.Close)modal.Close.gameObject.SetActive(true);
  }
  static IEnumerable<string> Paginate(string content,int limit){
   content=content??"";
   while(content.Length>limit){
    int at=content.LastIndexOf(' ',Math.Min(limit,content.Length-1));
    if(at<limit/2)at=limit;
    yield return content.Substring(0,at).Trim();
    content=content.Substring(at).TrimStart();
   }
   yield return content;
  }
  public void SetEnglish(bool value){
   if(english==value)return;
   english=value;if(!active)return;
   // Language change never acknowledges a milestone or discards a turn.
   NextLine();
  }
  void Update(){
   if(!active||!modal||!modal.gameObject.activeInHierarchy)return;
   // Match the original browser dialogueIn motion with a restrained
   // alpha/14px entrance. Unscaled time makes the animation independent of
   // a gameplay TestClock, pause, or the chosen economy profile.
   if(entryTime<.28f){
    entryTime=Mathf.Min(.28f,entryTime+Time.unscaledDeltaTime);
    float t=Mathf.Clamp01(entryTime/.28f);
    float eased=t*t*(3-2*t);
    if(entrance)entrance.alpha=Mathf.Lerp(.28f,1f,eased);
    if(window)window.anchoredPosition=Vector2.Lerp(
     restingWindowPosition+new Vector2(0,-10),restingWindowPosition,eased);
   }
   if(shown>=CurrentPage.Length)return;
   if(hold>0){hold-=Time.unscaledDeltaTime;return;}
   characterBudget+=Time.unscaledDeltaTime*46f;
   int count=Mathf.Min(CurrentPage.Length,Mathf.FloorToInt(characterBudget));
   if(count<=shown)return;
   char last=CurrentPage[count-1];
   shown=count;
   modal.Body.Put(CurrentPage.Substring(0,shown));
   if(last=='.'||last=='!'||last=='?')hold=.10f;
   if(shown==CurrentPage.Length)modal.ExtraLabel.Put(english?"Continue":"Continuar");
  }
  /// <summary>One click finishes the current typewriter page; the next click
  /// moves to the following page or speaker. No through-click or skipped turn.</summary>
  public bool Advance(){
   if(!active)return false;
   if(shown<CurrentPage.Length){
    shown=CurrentPage.Length;characterBudget=shown;hold=0;
    modal.Body.Put(CurrentPage);
    modal.ExtraLabel.Put(english?"Continue":"Continuar");
    return true;
   }
   if(pageIndex+1<pages.Count){
    pageIndex++;shown=0;characterBudget=0;hold=0;
    LayoutNarrativePage();
    return true;
   }
   if(lineIndex+1<turns.Count){lineIndex++;NextLine();return true;}
   var done=onComplete;onComplete=null;active=false;
   RestoreModalLayout();
   modal.Hide();done?.Invoke();return false;
  }
  void OnDisable(){
   // External interruption never commits read milestones.
   if(!active)return;
   RestoreModalLayout();
   active=false;onComplete=null;
  }
 }
}
