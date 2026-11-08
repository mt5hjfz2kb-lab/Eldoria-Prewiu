using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Open-air Breach emblem. The actual landscape remains visible between
    /// delicate copper inlay lines; unlike the previous black stone button,
    /// this mark is neither a navigation control nor a full-screen overlay.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchRiftSeparator : MaskableGraphic
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
            if(r.width<35f||r.height<15f)return;
            Vector2 c=r.center;
            float halfW=r.width*.5f;
            float halfH=r.height*.5f;

            void Line(Vector2 start,Vector2 end,float width,Color ink)
            {
                var d=end-start;
                if(d.sqrMagnitude<.001f)return;
                var n=new Vector2(-d.y,d.x).normalized*width*.5f;
                int i=vh.currentVertCount;
                vh.AddVert(start+n,ink,Vector2.zero);
                vh.AddVert(start-n,ink,Vector2.zero);
                vh.AddVert(end-n,ink,Vector2.zero);
                vh.AddVert(end+n,ink,Vector2.zero);
                vh.AddTriangle(i,i+1,i+2);
                vh.AddTriangle(i,i+2,i+3);
            }
            // Each segment is transparent outside its bright alloy relief.
            float left=r.xMin+2f,right=r.xMax-2f;
            Color trace=new Color(.82f,.50f,.22f,.72f);
            Color dark=new Color(.12f,.07f,.04f,.52f);
            Line(new Vector2(left,c.y-1),new Vector2(c.x-16,c.y-1),3.1f,dark);
            Line(new Vector2(c.x+16,c.y-1),new Vector2(right,c.y-1),3.1f,dark);
            Line(new Vector2(left,c.y),new Vector2(c.x-16,c.y),1.4f,trace);
            Line(new Vector2(c.x+16,c.y),new Vector2(right,c.y),1.4f,trace);
            // A tiny artful fissure, using angular broken strokes with a
            // bright opening, not a giant magical monument or neon control.
            var black=new Color(.07f,.03f,.025f,.91f);
            var ember=new Color(1f,.36f,.10f,.96f);
            var light=new Color(1f,.76f,.39f,.98f);
            Vector2[] stroke={
                c+new Vector2(-4,halfH*.82f),
                c+new Vector2(1,4),
                c+new Vector2(-2,-2),
                c+new Vector2(4,-halfH*.80f)
            };
            for(int i=0;i<stroke.Length-1;i++)
            {
                Line(stroke[i]+Vector2.right,stroke[i+1]+Vector2.right,4.3f,black);
                Line(stroke[i],stroke[i+1],2.1f,ember);
                Line(stroke[i]-Vector2.right*.55f,stroke[i+1]-Vector2.right*.55f,.7f,light);
            }
            Line(c+new Vector2(-11,2),c+new Vector2(11,-1),1.65f,ember);
            Line(c+new Vector2(-7,0),c+new Vector2(7,1),.8f,light);
            // Corner diamonds are restrained, only four tiny solid vertices.
            void Diamond(float x)
            {
                var pt=new[]{
                    new Vector2(x,c.y+2.6f),new Vector2(x+3.5f,c.y),
                    new Vector2(x,c.y-2.6f),new Vector2(x-3.5f,c.y)};
                for(int i=0;i<4;i++)
                    Line(pt[i],pt[(i+1)%4],.85f,trace);
            }
            Diamond(c.x-halfW*.61f);
            Diamond(c.x+halfW*.61f);
        }
    }
}
