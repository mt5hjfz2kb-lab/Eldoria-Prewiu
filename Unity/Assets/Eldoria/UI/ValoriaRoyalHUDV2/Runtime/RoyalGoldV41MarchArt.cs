using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Decorative, non-interactive layers for the canonical march preparation.
    /// Uses the existing approved Eldoria frame atlas and native uGUI geometry.
    /// Never owns army state, statistics, controls, unlocks or save data.
    /// </summary>
    internal static class RoyalGoldV41MarchArt
    {
        static readonly Color Edge = new Color(.84f,.66f,.37f,.88f);
        static readonly Color Glow = new Color(.98f,.85f,.53f,.35f);
        static readonly Color Recess = new Color(.028f,.046f,.069f,.75f);

        static Image Layer(string name,RectTransform parent,Color color)
        {
            var r=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),
                typeof(Image)).GetComponent<RectTransform>();
            r.SetParent(parent,false);
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;
            r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
            var image=r.GetComponent<Image>();
            image.color=color;image.raycastTarget=false;
            return image;
        }

        static Image Stroke(string name,RectTransform parent,
            Vector2 anchorMin,Vector2 anchorMax,Vector2 size,Color color)
        {
            var image=Layer(name,parent,color);
            var r=image.rectTransform;
            r.anchorMin=anchorMin;r.anchorMax=anchorMax;
            r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=Vector2.zero;
            r.sizeDelta=size;
            return image;
        }

        internal static void DressHeroCard(RectTransform card,
            RectTransform portraitViewport,RoyalText name)
        {
            // Separate scenic recess sits behind the source portrait. The
            // portrait itself remains the owner's exact canonical source.
            var recess=Layer("March art / painterly portrait recess",portraitViewport,Recess);
            recess.transform.SetAsFirstSibling();
            // Original approved gold edge texture is truly nine-sliced, not a
            // screenshot or an opaque rectangle across the whole hero card.
            var rim=Layer("March art / independent hero nine-slice",card,Edge);
            rim.sprite=RoyalGoldV41SourceArt.Get("HeroCardNineSliceGold",48);
            if(rim.sprite){rim.type=Image.Type.Sliced;rim.fillCenter=false;}
            var rect=rim.rectTransform;
            rect.offsetMin=new Vector2(3f,3f);
            rect.offsetMax=new Vector2(-3f,-3f);
            rim.transform.SetAsLastSibling();

            Stroke("March art / left heraldic engraving",card,
                new Vector2(.04f,.50f),new Vector2(.04f,.50f),
                new Vector2(2f,0f),Glow).rectTransform.sizeDelta=
                new Vector2(2f,Mathf.Max(12f,card.rect.height*.33f));
            // Edge contrast around the original hero name, with absolutely no
            // baked hero data or edit to the existing click target.
            var outline=name.GetComponent<Outline>();
            if(!outline)outline=name.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.012f,.019f,.029f,.94f);
            outline.effectDistance=new Vector2(1.4f,-1.4f);
            outline.useGraphicAlpha=true;
        }

        internal static void DressTroopSurface(RectTransform troop)
        {
            var rim=Layer("March art / separate engraved troop border",troop,
                new Color(.88f,.70f,.41f,.85f));
            rim.sprite=RoyalGoldV41SourceArt.Get("HeroCardNineSliceGold",48);
            if(rim.sprite){rim.type=Image.Type.Sliced;rim.fillCenter=false;}
            rim.transform.SetAsLastSibling();
            rim.rectTransform.offsetMin=new Vector2(3f,3f);
            rim.rectTransform.offsetMax=new Vector2(-3f,-3f);
            Stroke("March art / brass left chapter rule",troop,
                new Vector2(.015f,.5f),new Vector2(.015f,.5f),
                new Vector2(3f,24f),Glow);
        }

        internal static void DressSummary(RectTransform summary)
        {
            var rim=Layer("March art / independent summary border",summary,Edge);
            rim.sprite=RoyalGoldV41SourceArt.Get("HeroCardNineSliceGold",44);
            if(rim.sprite){rim.type=Image.Type.Sliced;rim.fillCenter=false;}
            rim.rectTransform.offsetMin=new Vector2(3,3);
            rim.rectTransform.offsetMax=new Vector2(-3,-3);
            rim.transform.SetAsLastSibling();
        }
    }
}
