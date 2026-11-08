using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Authored metal inlay for the independent compact Vigilia HUD. Pure
    /// geometry with open transparent center; cannot hide the world or
    /// swallow input. Its asymmetric edge weights mimic a raised forged lip,
    /// not the old flat procedural rectangle or inherited RoyalHUD panel.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchBevelTrim : MaskableGraphic
    {
        public bool Highlight = false;
        protected override void Awake()
        {
            base.Awake();
            color=Color.white;
            raycastTarget=false;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=GetPixelAdjustedRect();
            if(r.width<40 || r.height<25)return;
            float l=r.xMin,rt=r.xMax,b=r.yMin,t=r.yMax;
            float cut=Mathf.Min(8f,Mathf.Min(r.width,r.height)*.15f);
            Vector2[] contour={
                new Vector2(l+cut,t-2),new Vector2(rt-cut,t-2),
                new Vector2(rt-2,t-cut),new Vector2(rt-2,b+cut),
                new Vector2(rt-cut,b+2),new Vector2(l+cut,b+2),
                new Vector2(l+2,b+cut),new Vector2(l+2,t-cut)
            };
            Color dark=new Color(.035f,.027f,.024f,.86f);
            Color copper=Highlight
                ?new Color(.76f,.43f,.19f,.91f)
                :new Color(.50f,.315f,.18f,.88f);
            Color edge=new Color(.80f,.66f,.40f,.86f);

            void Line(Vector2 a,Vector2 c,float w,Color color)
            {
                var d=c-a;
                if(d.sqrMagnitude<.0001f)return;
                var n=new Vector2(-d.y,d.x).normalized*(w*.5f);
                int v=vh.currentVertCount;
                vh.AddVert(a+n,color,Vector2.zero);
                vh.AddVert(a-n,color,Vector2.zero);
                vh.AddVert(c-n,color,Vector2.zero);
                vh.AddVert(c+n,color,Vector2.zero);
                vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v,v+2,v+3);
            }
            for(int i=0;i<contour.Length;i++)
            {
                var a=contour[i];var c=contour[(i+1)%contour.Length];
                Line(a+new Vector2(0,-1.4f),c+new Vector2(0,-1.4f),3.5f,dark);
                Line(a,c,1.65f,copper);
                if(i==0||i==7)
                    Line(a+Vector2.up*.7f,c+Vector2.up*.7f,.72f,edge);
            }
            if(r.width>95f)
            {
                float cx=r.center.x;
                // Lightly chiselled center notch, not a second huge banner.
                Line(new Vector2(cx-4,t-4),new Vector2(cx,t-1),1f,copper);
                Line(new Vector2(cx,t-1),new Vector2(cx+4,t-4),1f,edge);
            }
            if(r.height>48)
            {
                // Fine warm stress cuts etched into the upper inlay.
                Line(new Vector2(l+cut+6,t-5),new Vector2(l+cut+11,t-8),.8f,edge);
                Line(new Vector2(rt-cut-6,t-5),new Vector2(rt-cut-11,t-8),.8f,edge);
            }
        }
    }
}
