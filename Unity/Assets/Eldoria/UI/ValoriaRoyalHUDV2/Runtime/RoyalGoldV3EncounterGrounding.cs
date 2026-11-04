using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Matched two-plane environmental light, contact shadow and soft occlusion
    /// for EXISTING encounter portraits. Behind/above the original RawImage;
    /// neither plane catches rays or changes encounter data.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3EncounterGrounding : MaskableGraphic
    {
        public bool Front {get; private set;}
        public bool Breach {get; private set;}
        public bool Guardian {get; private set;}

        public static RoyalGoldV3EncounterGrounding Install(
            RectTransform parent,bool front)
        {
            var go=new GameObject(front ?
                "Vigilants / portrait soft environmental occlusion" :
                "Vigilants / portrait contact shadow and bounce light",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(RoyalGoldV3EncounterGrounding));
            go.layer=parent.gameObject.layer;
            var rect=(RectTransform)go.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;
            rect.offsetMax=Vector2.zero;
            var graphic=go.GetComponent<RoyalGoldV3EncounterGrounding>();
            graphic.Front=front;
            graphic.raycastTarget=false;
            graphic.color=Color.white;
            if(front)rect.SetAsLastSibling();
            else rect.SetAsFirstSibling();
            return graphic;
        }

        public void Configure(bool breach,bool guardian)
        {
            if(Breach==breach && Guardian==guardian)return;
            Breach=breach;
            Guardian=guardian;
            SetVerticesDirty();
        }

        static void Rift(VertexHelper vh,Vector2 a,Vector2 b,
            float width,Color glow)
        {
            var dir=b-a;
            if(dir.sqrMagnitude<.0001f)return;
            var normal=new Vector2(-dir.y,dir.x).normalized*(width*.5f);
            int start=vh.currentVertCount;
            vh.AddVert(a-normal,glow,Vector2.zero);
            vh.AddVert(a+normal,glow,Vector2.zero);
            vh.AddVert(b+normal,new Color(glow.r,glow.g,glow.b,0f),Vector2.zero);
            vh.AddVert(b-normal,new Color(glow.r,glow.g,glow.b,0f),Vector2.zero);
            vh.AddTriangle(start,start+1,start+2);
            vh.AddTriangle(start,start+2,start+3);
        }

        static void Glow(VertexHelper vh,Vector2 center,float radiusX,
            float radiusY,Color inner)
        {
            int start=vh.currentVertCount;
            vh.AddVert(center,inner,Vector2.zero);
            const int count=24;
            for(int i=0;i<=count;i++)
            {
                float theta=(Mathf.PI*2f/count)*i;
                var p=center+new Vector2(Mathf.Cos(theta)*radiusX,
                    Mathf.Sin(theta)*radiusY);
                vh.AddVert(p,new Color(inner.r,inner.g,inner.b,0f),Vector2.zero);
                if(i>0)vh.AddTriangle(start,start+i,start+i+1);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=GetPixelAdjustedRect();
            if(r.width<24f||r.height<24f)return;
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            Color near=Breach ?
                new Color(.36f,.19f,.48f,.62f):
                Guardian?new Color(.37f,.26f,.14f,.57f):
                         new Color(.20f,.24f,.23f,.49f);
            Color fog=Breach ?
                new Color(.46f,.30f,.63f,.22f):
                Guardian?new Color(.57f,.48f,.30f,.14f):
                         new Color(.43f,.52f,.47f,.12f);
            if(!Front)
            {
                // Radial bounce light comes from the existing approved enemy.
                // No rectangular photo plates or full-viewport overlays.
                Glow(vh,new Vector2(x+w*.51f,y+h*.51f),w*.54f,h*.58f,
                    new Color(near.r,near.g,near.b,Breach ? .36f : .24f));
                Glow(vh,new Vector2(x+w*.51f,y+h*.14f),w*.44f,h*.125f,
                    new Color(.012f,.018f,.024f,.82f));
                Glow(vh,new Vector2(x+w*.54f,y+h*.18f),w*.45f,h*.18f,
                    new Color(near.r,near.g,near.b,Breach ? .47f : .30f));
                if(Breach)
                {
                    // The Breach is an atmospheric emergence from the real
                    // painted rocks. B2's four straight lavender polygon
                    // strokes were visible as artificial UI lines; remove
                    // them instead of concealing them behind another frame.
                    var origin=new Vector2(x+w*.51f,y+h*.13f);
                    Glow(vh,origin,w*.36f,h*.16f,
                        new Color(.53f,.22f,.79f,.29f));
                    Glow(vh,new Vector2(x+w*.36f,y+h*.19f),
                        w*.22f,h*.18f,new Color(.46f,.24f,.62f,.15f));
                    Glow(vh,new Vector2(x+w*.69f,y+h*.20f),
                        w*.17f,h*.15f,new Color(.66f,.39f,.85f,.17f));
                }
            }
            else
            {
                // Only soft organic shapes. The older rectangular gradients
                // created a visible purple block under the Engendro in the
                // first exact-source Unity A/B, and are intentionally removed.
                Glow(vh,new Vector2(x+w*.47f,y+h*.075f),w*.52f,h*.19f,
                    new Color(fog.r,fog.g,fog.b,Breach ? .25f : .17f));
                Glow(vh,new Vector2(x+w*.77f,y+h*.16f),w*.24f,h*.12f,
                    new Color(fog.r,fog.g,fog.b,Breach ? .11f : .08f));
            }
        }
    }
}
