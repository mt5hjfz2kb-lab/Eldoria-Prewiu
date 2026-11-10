using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Original fine copper linkwork between five independent native uGUI
    /// medallions. No full-width HUD bar, no old navigation prefab or sprites.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchNavFiligree : MaskableGraphic
    {
        float medallionStep=70f;
        float medallionSize=55f;
        int visibleMedallions=5;
        public void Configure(float step,float diameter,int count=5)
        {
            medallionStep=Mathf.Max(20f,step);
            medallionSize=Mathf.Max(15f,diameter);
            visibleMedallions=Mathf.Clamp(count,0,5);
            SetVerticesDirty();
        }
        protected override void Awake()
        {
            base.Awake();
            raycastTarget=false;
            color=Color.white;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<80||r.height<30||visibleMedallions<2)return;
            float y=r.yMax-3f-medallionSize*.5f;
            float step=medallionStep;
            float first=r.xMin+medallionSize*.5f;
            float last=first+(visibleMedallions-1)*step;
            var shadow=new Color(.04f,.025f,.02f,.83f);
            var copper=new Color(.60f,.36f,.18f,.86f);
            var gold=new Color(.86f,.64f,.31f,.88f);

            void Line(Vector2 a,Vector2 b,float thickness,Color ink)
            {
                var d=b-a;
                if(d.sqrMagnitude<.0001f)return;
                var n=new Vector2(-d.y,d.x).normalized*(thickness*.5f);
                int idx=vh.currentVertCount;
                vh.AddVert(a+n,ink,Vector2.zero);
                vh.AddVert(a-n,ink,Vector2.zero);
                vh.AddVert(b-n,ink,Vector2.zero);
                vh.AddVert(b+n,ink,Vector2.zero);
                vh.AddTriangle(idx,idx+1,idx+2);
                vh.AddTriangle(idx,idx+2,idx+3);
            }
            void Diamond(float x,float radius)
            {
                Vector2[] p={
                    new Vector2(x,y+radius),new Vector2(x+radius*.7f,y),
                    new Vector2(x,y-radius),new Vector2(x-radius*.7f,y)
                };
                int idx=vh.currentVertCount;
                vh.AddVert(new Vector2(x,y),gold,Vector2.zero);
                for(int i=0;i<4;i++)vh.AddVert(p[i],copper,Vector2.zero);
                for(int i=0;i<4;i++)
                    vh.AddTriangle(idx,idx+i+1,idx+(i+1)%4+1);
                for(int i=0;i<4;i++)Line(p[i],p[(i+1)%4],.85f,gold);
            }
            // Fine open metalwork occupies only the gaps. A thick dark
            // strip behind all icons made the original dock look rigid.
            void Link(float a,float b)
            {
                Line(new Vector2(a,y-1),new Vector2(b,y-1),2.8f,shadow);
                Line(new Vector2(a,y),new Vector2(b,y),1.4f,copper);
                Line(new Vector2(a,y+.7f),new Vector2(b,y+.7f),.65f,gold);
            }
            void Finial(float edge,float direction)
            {
                float tip=edge+direction*10f;
                float neck=edge-direction*8f;
                Line(new Vector2(tip,y),new Vector2(neck,y+3.5f),.85f,gold);
                Line(new Vector2(neck,y+3.5f),new Vector2(edge-direction*3f,y),.8f,copper);
                Line(new Vector2(edge-direction*3f,y),new Vector2(neck,y-3.5f),.8f,copper);
                Line(new Vector2(neck,y-3.5f),new Vector2(tip,y),.85f,gold);
                Link(Mathf.Min(edge,neck),Mathf.Max(edge,neck));
                Diamond(edge-direction*1.5f,2.6f);
            }
            Finial(r.xMin,-1f);
            Finial(r.xMax,1f);
            for(int i=0;i<visibleMedallions-1;i++)
            {
                float midpoint=first+(i+.5f)*step;
                float a=first+i*step+medallionSize*.34f;
                float b=first+(i+1)*step-medallionSize*.34f;
                Link(a,b);
                Diamond(midpoint,3.8f);
                // Two engraved shoulders echo the approved gold dock.
                Line(new Vector2(midpoint-5,y),new Vector2(midpoint-8,y+2),.65f,gold);
                Line(new Vector2(midpoint+5,y),new Vector2(midpoint+8,y+2),.65f,gold);
            }
        }
    }
}
