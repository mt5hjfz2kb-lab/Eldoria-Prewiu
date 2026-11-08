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
            if(r.width<220||r.height<30)return;
            float y=r.center.y;
            float first=r.xMin+(r.width-5*55f-4*15f)*.5f+27.5f;
            float step=70f;
            float last=first+4*step;
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
            Line(new Vector2(r.xMin+3,y-2),new Vector2(r.xMax-3,y-2),5.5f,shadow);
            Line(new Vector2(r.xMin+3,y),new Vector2(r.xMax-3,y),1.9f,copper);
            Line(new Vector2(r.xMin+8,y+1.1f),new Vector2(r.xMax-8,y+1.1f),.75f,gold);
            Diamond(r.xMin+8,5);
            Diamond(r.xMax-8,5);
            for(int i=0;i<4;i++)
                Diamond(first+(i+.5f)*step,4.8f);
        }
    }
}
