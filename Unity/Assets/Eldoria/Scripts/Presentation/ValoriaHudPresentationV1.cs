using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.Presentation
{
 // Styles the existing live controls. No baked text, fictional resources or new gameplay entries.
 public sealed class ValoriaHudPresentationV1:MonoBehaviour
 {
  public static bool Enabled=true;
  static readonly Color Gold=new Color(.72f,.57f,.32f),Panel=new Color(.035f,.055f,.065f,.93f);
  public void Apply(int width,int height)
  {
   var safe=transform.Find("Safe area") as RectTransform;if(safe==null)return;
   bool portrait=height>width;
   bool compact=portrait||width<900;
   var scaler=GetComponent<CanvasScaler>();scaler.referenceResolution=new Vector2(390,844);scaler.matchWidthOrHeight=portrait?0f:.72f;

   foreach(var image in GetComponentsInChildren<Image>(true))
    image.raycastTarget=image.GetComponent<Button>()!=null||image.name.Contains("panel")||image.name.Contains("dock")||image.name.Contains("topbar")||image.name.Contains("navigation");
   foreach(var text in GetComponentsInChildren<Text>(true)){
    text.raycastTarget=false;text.fontSize=Mathf.Max(text.fontSize,compact?9:10);text.fontStyle=FontStyle.Normal;
    text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
   }

   var top=safe.Find("Reference topbar") as RectTransform;
   if(top!=null){
    top.sizeDelta=new Vector2(0,portrait?60f:54f);
    var im=top.GetComponent<Image>();if(im!=null)im.color=new Color(.025f,.045f,.058f,.91f);
    var l=top.GetComponent<HorizontalLayoutGroup>();if(l!=null){l.padding=new RectOffset(8,8,6,6);l.spacing=portrait?3:5;l.childForceExpandWidth=false;}
    SetWidth(top,"Realm crest",portrait?34:38);
    SetWidth(top,"Heading",portrait?62:72);
    SetWidth(top,"Wood resource",portrait?60:68);
    SetWidth(top,"Stone resource",portrait?60:68);
    SetWidth(top,"Power",portrait?66:76);
   }

   var quest=safe.Find("Quest panel") as RectTransform;
   if(quest!=null){
    quest.sizeDelta=new Vector2(portrait?330f:300f,portrait?64f:58f);
    quest.anchoredPosition=new Vector2(10f,portrait?-70f:-64f);
    var qi=quest.GetComponent<Image>();if(qi!=null)qi.color=new Color(.025f,.045f,.058f,.84f);
    var ql=quest.GetComponent<VerticalLayoutGroup>();if(ql!=null){ql.padding=new RectOffset(10,10,6,6);ql.spacing=0;ql.childControlWidth=true;ql.childForceExpandWidth=true;}
    var kick=quest.Find("Quest kicker")?.GetComponent<Text>();if(kick!=null){kick.fontSize=9;kick.color=Gold;}
    var obj=quest.Find("Objective")?.GetComponent<Text>();if(obj!=null){obj.fontSize=portrait?12:11;obj.fontStyle=FontStyle.Bold;}
   }

   var nav=safe.Find("Bottom navigation") as RectTransform;
   if(nav!=null){
    nav.sizeDelta=new Vector2(0,portrait?56f:50f);
    var ni=nav.GetComponent<Image>();if(ni!=null)ni.color=new Color(.022f,.038f,.048f,.94f);
    var nl=nav.GetComponent<HorizontalLayoutGroup>();if(nl!=null){nl.padding=new RectOffset(6,6,3,3);nl.spacing=2;}
    foreach(var b in nav.GetComponentsInChildren<Button>(true)){
     var le=b.GetComponent<LayoutElement>();if(le!=null){le.minHeight=portrait?48:42;le.preferredHeight=portrait?48:42;}
     var tx=b.GetComponentInChildren<Text>();if(tx!=null)tx.fontSize=portrait?9:8;
    }
   }

   var dock=safe.Find("World objective dock") as RectTransform;
   if(dock!=null){
    dock.sizeDelta=new Vector2(portrait?360f:320f,portrait?86f:80f);
    dock.anchoredPosition=new Vector2(0,portrait?64f:58f);
    var di=dock.GetComponent<Image>();if(di!=null)di.color=new Color(.025f,.045f,.058f,.90f);
    var layout=dock.GetComponent<VerticalLayoutGroup>();if(layout!=null){layout.padding=new RectOffset(8,8,4,4);layout.spacing=1;layout.childControlWidth=true;layout.childForceExpandWidth=true;}
    var desc=dock.Find("Story and world")?.GetComponent<Text>();if(desc!=null){desc.fontSize=portrait?9:8;var dle=desc.GetComponent<LayoutElement>();if(dle!=null)dle.preferredHeight=portrait?20:16;}
    var feedback=dock.Find("Feedback")?.GetComponent<Text>();if(feedback!=null){feedback.fontSize=8;var fle=feedback.GetComponent<LayoutElement>();if(fle!=null)fle.preferredHeight=10;}
    var row=dock.Find("Primary objective action");if(row!=null){
     var rle=row.GetComponent<LayoutElement>();if(rle!=null)rle.preferredHeight=portrait?40:38;
     var group=row.GetComponent<HorizontalLayoutGroup>();if(group!=null){group.childControlHeight=true;group.childForceExpandHeight=true;group.childControlWidth=true;group.childForceExpandWidth=true;}
     foreach(var b in row.GetComponentsInChildren<Button>()){
      var le=b.GetComponent<LayoutElement>();if(le!=null){le.minHeight=portrait?40:38;le.preferredHeight=portrait?40:38;le.flexibleWidth=1;}
      b.GetComponent<Image>().color=new Color(.73f,.58f,.31f,.98f);
      var tx=b.GetComponentInChildren<Text>();if(tx!=null){tx.fontSize=portrait?12:11;tx.fontStyle=FontStyle.Bold;tx.color=new Color(.055f,.065f,.065f);}
     }
    }
   }

   foreach(string n in new[]{"Reference topbar","Quest panel","World objective dock","Bottom navigation","Building interaction panel"}){
    var t=safe.Find(n);if(t==null)continue;
    if(t.Find("ReferenceArtFrame")==null){var g=new GameObject("ReferenceArtFrame",typeof(RectTransform),typeof(Image),typeof(LayoutElement));g.GetComponent<LayoutElement>().ignoreLayout=true;g.transform.SetParent(t,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=new Vector2(0,1);rt.anchorMax=Vector2.one;rt.pivot=new Vector2(.5f,1);rt.sizeDelta=new Vector2(0,1);g.GetComponent<Image>().color=Gold;g.GetComponent<Image>().raycastTarget=false;}
   }

   foreach(var b in GetComponentsInChildren<Button>(true)){
    if(b.transform.Find("ReferenceButtonFrame")==null){var f=new GameObject("ReferenceButtonFrame",typeof(RectTransform),typeof(Image),typeof(LayoutElement));f.GetComponent<LayoutElement>().ignoreLayout=true;f.transform.SetParent(b.transform,false);var fr=f.GetComponent<RectTransform>();fr.anchorMin=new Vector2(0,1);fr.anchorMax=Vector2.one;fr.pivot=new Vector2(.5f,1);fr.sizeDelta=new Vector2(0,1);f.GetComponent<Image>().color=Gold;f.GetComponent<Image>().raycastTarget=false;}
    if(b.GetComponent<Outline>()==null){var o=b.gameObject.AddComponent<Outline>();o.effectColor=new Color(.16f,.12f,.055f,.72f);o.effectDistance=new Vector2(1,-1);}
    var colors=b.colors;colors.highlightedColor=new Color(1.06f,1.05f,1.01f);colors.pressedColor=new Color(.75f,.78f,.82f);b.colors=colors;
   }
   Canvas.ForceUpdateCanvases();
  }

  static void SetWidth(RectTransform parent,string child,float width)
  {
   var t=parent.Find(child);if(t==null)return;
   var le=t.GetComponent<LayoutElement>();if(le!=null){le.preferredWidth=width;le.minWidth=width;}
  }
  void LateUpdate(){if(Screen.width!=lastWidth||Screen.height!=lastHeight){lastWidth=Screen.width;lastHeight=Screen.height;Apply(lastWidth,lastHeight);}}
  int lastWidth,lastHeight;
 }
}
