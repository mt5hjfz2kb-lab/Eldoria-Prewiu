using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Decorative original Crónica Real hanging cloth from the approved
    /// chapter card: ink-blue fabric, ochre border, tapered swallowtail.
    /// Generates only native uGUI vertices, creates no clickable new UI.
    /// </summary>
    public sealed class RoyalGoldV3HeraldicBannerGraphic : MaskableGraphic
    {
        Color Tint(Color value) => new Color(
            value.r * color.r, value.g * color.g, value.b * color.b,
            value.a * color.a);
        void Poly(VertexHelper v,Color bottom,Color top,params Vector2[] p)
        {
            if(p.Length<3)return;
            float yMin=float.MaxValue,yMax=float.MinValue;
            foreach(var q in p){yMin=Mathf.Min(yMin,q.y);yMax=Mathf.Max(yMax,q.y);}
            int k=v.currentVertCount;
            Vector2 middle=Vector2.zero;
            foreach(var q in p)middle+=q;
            middle/=p.Length;
            v.AddVert(middle,Tint(Color.Lerp(bottom,top,.5f)),Vector2.zero);
            foreach(var q in p)
                v.AddVert(q,Tint(Color.Lerp(bottom,top,
                    Mathf.InverseLerp(yMin,yMax,q.y))),Vector2.zero);
            for(int i=0;i<p.Length;i++)v.AddTriangle(k,k+i+1,k+((i+1)%p.Length)+1);
        }
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var r=rectTransform.rect;
            if(r.width<20f||r.height<50f)return;
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            Color shadow=new Color(.025f,.027f,.036f,.81f),
                rim=new Color(.56f,.37f,.15f,1),
                gold=new Color(.85f,.67f,.35f,1),
                low=new Color(.017f,.044f,.091f,.99f),
                high=new Color(.074f,.139f,.246f,1);
            Poly(v,shadow,shadow,new Vector2(x+1,y+6),new Vector2(x+w*.5f,y+h*.11f),
                new Vector2(x+w-1,y+6),new Vector2(x+w,y+h-2),
                new Vector2(x,y+h-2));
            Poly(v,rim,gold,new Vector2(x+2,y+8),new Vector2(x+w*.5f,y+h*.15f),
                new Vector2(x+w-2,y+8),new Vector2(x+w-2,y+h-4),
                new Vector2(x+2,y+h-4));
            Poly(v,low,high,new Vector2(x+6,y+18),new Vector2(x+w*.5f,y+h*.20f),
                new Vector2(x+w-6,y+18),new Vector2(x+w-6,y+h-9),
                new Vector2(x+6,y+h-9));
            // Gold-thread hem, intentionally subtle rather than neon.
            for(int i=0;i<5;i++)
            {
                float t=(i+.5f)/5f;
                float lineY=y+h*.22f+t*h*.61f;
                Poly(v,new Color(.46f,.33f,.20f,.33f),new Color(.75f,.57f,.29f,.20f),
                    new Vector2(x+6,lineY),new Vector2(x+w-6,lineY),
                    new Vector2(x+w-6,lineY+1.2f),new Vector2(x+6,lineY+1.2f));
            }
        }
    }
}