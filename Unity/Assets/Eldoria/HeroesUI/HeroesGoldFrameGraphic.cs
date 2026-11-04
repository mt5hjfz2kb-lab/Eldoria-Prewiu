using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.HeroesUI
{
    /// <summary>Independent, scalable, editable Royal Gold panel mesh. No screenshot is used.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class HeroesGoldFrameGraphic : MaskableGraphic
    {
        public Color Border = new Color(.74f,.57f,.30f,1);
        public Color InnerBorder = new Color(.27f,.20f,.12f,1);
        public Color SurfaceTop = new Color(.105f,.118f,.142f,.97f);
        public Color SurfaceBottom = new Color(.024f,.036f,.055f,.98f);
        public float BorderWidth = 3;
        public float Inset = 6;
        public bool CornerOrnaments = true;
        public bool Divider = false;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r=rectTransform.rect;
            if(r.width<=0||r.height<=0)return;
            float b=Mathf.Min(BorderWidth,Mathf.Min(r.width,r.height)*.08f);
            Rim(vh,r.xMin,r.yMin,r.width,r.height,b,Border);
            Rim(vh,r.xMin+b,r.yMin+b,r.width-2*b,r.height-2*b,1f,InnerBorder);
            float p=b+Mathf.Max(1,Inset);
            Quad(vh,r.xMin+p,r.yMin+p,r.width-2*p,r.height-2*p,SurfaceBottom,SurfaceTop);
            if(CornerOrnaments){float d=Mathf.Min(14,Mathf.Min(r.width,r.height)*.13f);
                for(int ix=0;ix<2;ix++)for(int iy=0;iy<2;iy++){
                    float x=ix==0?r.xMin+p:r.xMax-p-d,y=iy==0?r.yMin+p:r.yMax-p-d;
                    Quad(vh,x,y,d,2,Border,Border);Quad(vh,x,y,2,d,Border,Border);
                }}
            if(Divider)Quad(vh,r.xMin+p+4,r.center.y-1,r.width-2*p-8,2,Border,InnerBorder);
        }
        void Rim(VertexHelper vh,float x,float y,float w,float h,float b,Color c)
        {
            Quad(vh,x,y,w,b,c,c);Quad(vh,x,y+h-b,w,b,c,c);
            Quad(vh,x,y+b,b,h-2*b,c,c);Quad(vh,x+w-b,y+b,b,h-2*b,c,c);
        }
        void Quad(VertexHelper vh,float x,float y,float w,float h,Color bottom,Color top)
        {
            if(w<=0||h<=0)return;int n=vh.currentVertCount;
            vh.AddVert(new Vector3(x,y),bottom,Vector2.zero);
            vh.AddVert(new Vector3(x,y+h),top,Vector2.up);
            vh.AddVert(new Vector3(x+w,y+h),top,Vector2.one);
            vh.AddVert(new Vector3(x+w,y),bottom,Vector2.right);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
        protected override void OnValidate(){base.OnValidate();SetVerticesDirty();}
    }
}
