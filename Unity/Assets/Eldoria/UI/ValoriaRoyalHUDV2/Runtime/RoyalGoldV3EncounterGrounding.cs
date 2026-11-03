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

        static void Quad(VertexHelper vh,Vector2 a,Vector2 b,
            Color bottomLeft,Color bottomRight,Color topRight,Color topLeft)
        {
            int k=vh.currentVertCount;
            vh.AddVert(new Vector2(a.x,a.y),bottomLeft,Vector2.zero);
            vh.AddVert(new Vector2(b.x,a.y),bottomRight,Vector2.zero);
            vh.AddVert(new Vector2(b.x,b.y),topRight,Vector2.zero);
            vh.AddVert(new Vector2(a.x,b.y),topLeft,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);
            vh.AddTriangle(k,k+2,k+3);
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
            var clear=new Color(0f,0f,0f,0f);
            if(!Front)
            {
                // Large bloom is BEHIND the authentic guardian: it gives the
                // purple or amber light a plausible environmental origin.
                Glow(vh,new Vector2(x+w*.54f,y+h*.50f),w*.53f,h*.60f,
                    new Color(near.r,near.g,near.b,Breach ? 0.27f : 0.19f));
                // Receding stony floor and an actual grounding/contact ellipse.
                Quad(vh,new Vector2(x+w*.07f,y+h*.045f),
                    new Vector2(x+w*.93f,y+h*.18f),
                    new Color(.045f,.065f,.075f,.35f),
                    new Color(.06f,.08f,.08f,.35f),clear,clear);
                Glow(vh,new Vector2(x+w*.51f,y+h*.14f),w*.43f,h*.12f,
                    new Color(.015f,.021f,.028f,.79f));
                Glow(vh,new Vector2(x+w*.54f,y+h*.17f),w*.43f,h*.18f,
                    new Color(near.r,near.g,near.b,.29f));
                // Light is strongest where the creature meets the ground.
                Glow(vh,new Vector2(x+w*.55f,y+h*.22f),w*.29f,h*.24f,
                    new Color(fog.r,fog.g,fog.b,Breach ? 0.18f : 0.12f));
            }
            else
            {
                // Front mattes hide the hard crop edges of both full-painting
                // enemy portraits and transparent figures; eyes/torso remain
                // visible and the button/text live OUTSIDE this frame.
                Color edge=new Color(.025f,.035f,.060f,.52f);
                Quad(vh,new Vector2(x,y),new Vector2(x+w*.20f,y+h),
                    edge,clear,clear,edge);
                Quad(vh,new Vector2(x+w*.80f,y),new Vector2(x+w,y+h),
                    clear,edge,edge,clear);
                Quad(vh,new Vector2(x,y),new Vector2(x+w,y+h*.25f),
                    new Color(near.r,near.g,near.b,.40f),
                    new Color(near.r,near.g,near.b,.40f),clear,clear);
                Quad(vh,new Vector2(x,y+h*.89f),new Vector2(x+w,y+h),
                    clear,clear,
                    new Color(.015f,.025f,.040f,.27f),
                    new Color(.015f,.025f,.040f,.27f));
                // A translucent foreground fog bank makes feet/terrain meet,
                // not a rectangular photograph standing in front of the HUD.
                Quad(vh,new Vector2(x+w*.13f,y+h*.09f),
                    new Vector2(x+w*.88f,y+h*.19f),
                    new Color(fog.r,fog.g,fog.b,fog.a),
                    new Color(fog.r,fog.g,fog.b,fog.a),clear,clear);
            }
        }
    }
}
