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
   var scaler=GetComponent<CanvasScaler>();scaler.referenceResolution=new Vector2(390,844);scaler.matchWidthOrHeight=height>width?0:1;
   foreach(var image in GetComponentsInChildren<Image>(true))image.raycastTarget=image.GetComponent<Button>()!=null||image.name.Contains("panel")||image.name.Contains("dock")||image.name.Contains("topbar")||image.name.Contains("navigation");
   foreach(var text in GetComponentsInChildren<Text>(true)){
    text.raycastTarget=false;text.fontSize=Mathf.Max(text.fontSize,11);text.fontStyle=FontStyle.Normal;
    text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
   }
   foreach(string n in new[]{"Reference topbar","Quest panel","World objective dock","Bottom navigation","Building interaction panel"}){
    var t=safe.Find(n);if(t==null)continue;var im=t.GetComponent<Image>();if(im!=null)im.color=Panel;
    if(t.Find("ReferenceArtFrame")==null){var g=new GameObject("ReferenceArtFrame",typeof(RectTransform),typeof(Image),typeof(LayoutElement));g.GetComponent<LayoutElement>().ignoreLayout=true;g.transform.SetParent(t,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=new Vector2(0,1);rt.anchorMax=Vector2.one;rt.pivot=new Vector2(.5f,1);rt.sizeDelta=new Vector2(0,1);g.GetComponent<Image>().color=Gold;g.GetComponent<Image>().raycastTarget=false;}
   }
   var top=safe.Find("Reference topbar");if(top!=null){var l=top.GetComponent<HorizontalLayoutGroup>();l.padding=new RectOffset(10,10,8,8);l.spacing=5;}
   var quest=safe.Find("Quest panel") as RectTransform;if(quest!=null){var ql=quest.GetComponent<VerticalLayoutGroup>();ql.childControlWidth=true;ql.childForceExpandWidth=true;quest.sizeDelta=new Vector2(310,66);var kick=quest.Find("Quest kicker")?.GetComponent<Text>();if(kick!=null){kick.fontSize=10;kick.color=Gold;}var obj=quest.Find("Objective")?.GetComponent<Text>();if(obj!=null){obj.fontSize=13;obj.fontStyle=FontStyle.Bold;}}
   var dock=safe.Find("World objective dock") as RectTransform;if(dock!=null){dock.sizeDelta=new Vector2(360,94);var layout=dock.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(10,10,5,5);layout.spacing=2;layout.childControlWidth=true;layout.childForceExpandWidth=true;
    var desc=dock.Find("Story and world")?.GetComponent<Text>();if(desc!=null){desc.fontSize=11;desc.GetComponent<LayoutElement>().preferredHeight=26;}
    var feedback=dock.Find("Feedback")?.GetComponent<Text>();if(feedback!=null){feedback.fontSize=10;feedback.GetComponent<LayoutElement>().preferredHeight=12;}
    var row=dock.Find("Primary objective action");if(row!=null){var group=row.GetComponent<HorizontalLayoutGroup>();group.childControlHeight=true;group.childForceExpandHeight=true;group.childControlWidth=true;group.childForceExpandWidth=true;row.GetComponent<LayoutElement>().preferredHeight=44;foreach(var b in row.GetComponentsInChildren<Button>()){var le=b.GetComponent<LayoutElement>();le.minHeight=44;le.preferredHeight=44;le.flexibleWidth=1;b.GetComponent<Image>().color=Gold;var tx=b.GetComponentInChildren<Text>();if(tx!=null){tx.fontSize=13;tx.fontStyle=FontStyle.Bold;tx.color=new Color(.07f,.08f,.08f);}}}
   }
   foreach(var b in GetComponentsInChildren<Button>(true)){
    if(b.transform.Find("ReferenceButtonFrame")==null){var f=new GameObject("ReferenceButtonFrame",typeof(RectTransform),typeof(Image),typeof(LayoutElement));f.GetComponent<LayoutElement>().ignoreLayout=true;f.transform.SetParent(b.transform,false);var fr=f.GetComponent<RectTransform>();fr.anchorMin=new Vector2(0,1);fr.anchorMax=Vector2.one;fr.pivot=new Vector2(.5f,1);fr.sizeDelta=new Vector2(0,1);f.GetComponent<Image>().color=Gold;f.GetComponent<Image>().raycastTarget=false;}
    if(b.GetComponent<Outline>()==null){var o=b.gameObject.AddComponent<Outline>();o.effectColor=new Color(.2f,.15f,.07f);o.effectDistance=new Vector2(1,-1);}
    var colors=b.colors;colors.highlightedColor=new Color(1.1f,1.08f,1.03f);colors.pressedColor=new Color(.75f,.78f,.82f);b.colors=colors;
   }
   Canvas.ForceUpdateCanvases();
  }
  void LateUpdate(){if(Screen.width!=lastWidth||Screen.height!=lastHeight){lastWidth=Screen.width;lastHeight=Screen.height;Apply(lastWidth,lastHeight);}}
  int lastWidth,lastHeight;
 }
}
