using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    // One feathered patch of courtyard shadow, projected screen-right
    // from the boots to match the painted light on the upper-left wall.
    internal sealed class TrainingGroundContact : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect r=GetPixelAdjustedRect();
            const int segments=24;
            Vector2 origin=r.center-new Vector2(r.width*.16f,0f);
            mesh.AddVert(origin,new Color32(43,32,28,101),Vector2.zero);
            for(int ring=1;ring<=2;ring++)
            {
                float radius=ring==1?.48f:1f;
                Color32 shade=new Color32(43,32,28,(byte)(ring==1?54:0));
                for(int n=0;n<segments;n++)
                {
                    float angle=2f*Mathf.PI*n/segments;
                    Vector2 edge=r.center+new Vector2(r.width*.5f*Mathf.Cos(angle),r.height*.5f*Mathf.Sin(angle));
                    mesh.AddVert(Vector2.Lerp(origin,edge,radius),shade,Vector2.zero);
                }
            }
            for(int n=0;n<segments;n++)
            {
                int a=1+n,b=1+(n+1)%segments;
                mesh.AddTriangle(0,a,b);
                mesh.AddTriangle(a,a+segments,b+segments);
                mesh.AddTriangle(a,b+segments,b);
            }
        }
    }

    /// <summary>
    /// Decorative, non-interactive layers for the canonical march preparation.
    /// Uses the existing approved Eldoria frame atlas and native uGUI geometry.
    /// Never owns army state, statistics, controls, unlocks or save data.
    /// </summary>
    internal static class RoyalGoldV41MarchArt
    {
        static readonly Color Edge = new Color(.84f,.66f,.37f,.65f);
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
            rim.pixelsPerUnitMultiplier=3.5f;
            var rect=rim.rectTransform;
            rect.offsetMin=new Vector2(3f,3f);
            rect.offsetMax=new Vector2(-3f,-3f);
            rim.transform.SetAsLastSibling();
            // A separate, narrow identity plate makes the live hero label
            // legible without replacing the original portrait with a bitmap.
            var identity=Layer("March art / real-name enamel ribbon",card,
                new Color(.029f,.045f,.062f,.85f));
            identity.rectTransform.anchorMin=new Vector2(.035f,.02f);
            identity.rectTransform.anchorMax=new Vector2(.965f,RoyalGoldV2Hud.UseCoronaArt?.245f:.39f);
            identity.rectTransform.offsetMin=identity.rectTransform.offsetMax=Vector2.zero;
            identity.transform.SetSiblingIndex(name.transform.GetSiblingIndex());

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
            rim.pixelsPerUnitMultiplier=3.7f;
            rim.color=new Color(.84f,.66f,.37f,.53f);
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
            rim.pixelsPerUnitMultiplier=3.8f;
            rim.color=new Color(.84f,.66f,.37f,.49f);
            rim.rectTransform.offsetMin=new Vector2(3,3);
            rim.rectTransform.offsetMax=new Vector2(-3,-3);
            rim.transform.SetAsLastSibling();
        }
    }
}
