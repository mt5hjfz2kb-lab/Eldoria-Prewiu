using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Carved iron / aged-bronze ornament for the real reward panel and cards.
    /// Keeps all corner dimensions fixed when the panel is resized.
    /// Presentation-only, does not intercept player interaction.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DailyTreasureReliefGraphic : MaskableGraphic
    {
        public bool Major,Primary;
        VertexHelper mesh;
        void Quad(float x0,float y0,float x1,float y1,Color c0,Color c1)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(x0,y0),c0,Vector2.zero);
            mesh.AddVert(new Vector3(x1,y0),c0,Vector2.right);
            mesh.AddVert(new Vector3(x1,y1),c1,Vector2.one);
            mesh.AddVert(new Vector3(x0,y1),c1,Vector2.up);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
        void Line(float x0,float y0,float x1,float y1,float width,Color c)
        {
            float dx=x1-x0,dy=y1-y0,len=Mathf.Sqrt(dx*dx+dy*dy);
            if(len<.01f)return;
            float nx=-dy/len*width*.5f,ny=dx/len*width*.5f;
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(x0-nx,y0-ny),c,Vector2.zero);
            mesh.AddVert(new Vector3(x1-nx,y1-ny),c,Vector2.right);
            mesh.AddVert(new Vector3(x1+nx,y1+ny),c,Vector2.one);
            mesh.AddVert(new Vector3(x0+nx,y0+ny),c,Vector2.up);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
        void Rivet(float x,float y,float r)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(x,y),new Color(.82f,.62f,.30f,1f),Vector2.zero);
            for(int i=0;i<=12;i++)
            {
                float a=i*Mathf.PI/6f;
                mesh.AddVert(new Vector3(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r),
                    new Color(.24f,.18f,.11f,1f),Vector2.zero);
                if(i>0)mesh.AddTriangle(n,n+i,n+i+1);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            mesh=vertices;mesh.Clear();
            var r=GetPixelAdjustedRect();
            if(r.width<32f || r.height<32f)return;
            float x0=r.xMin,y0=r.yMin,x1=r.xMax,y1=r.yMax;
            const float min=1.5f;
            var iron=new Color(.065f,.075f,.086f,1f);
            var copper=new Color(.60f,.39f,.19f,1f);
            var light=new Color(.88f,.65f,.32f,1f);
            var muted=new Color(.37f,.27f,.19f,.95f);
            float o=Major?8f:Primary?5f:3.2f;
            // Stepped beveled borders: dark backing, copper inlay, worn highlight.
            Quad(x0,y0,x1,y0+o,iron,muted);
            Quad(x0,y1-o,x1,y1,muted,iron);
            Quad(x0,y0,x0+o,y1,iron,muted);
            Quad(x1-o,y0,x1,y1,muted,iron);
            float inner=o*.67f;
            Line(x0+inner,y0+inner,x1-inner,y0+inner,1.35f,copper);
            Line(x0+inner,y1-inner,x1-inner,y1-inner,1.35f,copper);
            Line(x0+inner,y0+inner,x0+inner,y1-inner,1.2f,copper);
            Line(x1-inner,y0+inner,x1-inner,y1-inner,1.2f,copper);
            Line(x0+o,y1-o,x1-o,y1-o,.6f,new Color(.91f,.73f,.39f,.8f));
            if(Major)
            {
                float m=(x0+x1)*.5f;
                Line(m-52,y1-o-3,m-11,y1-o-3,1.3f,copper);
                Line(m+11,y1-o-3,m+52,y1-o-3,1.3f,copper);
                Line(m,y1-3,m+5,y1-9,2.0f,light);
                Line(m+5,y1-9,m,y1-15,1.8f,copper);
                Line(m,y1-15,m-5,y1-9,1.8f,copper);
                Line(m-5,y1-9,m,y1-3,1.8f,light);
                Line(m-37,y0+o+3,m-8,y0+o+3,1f,muted);
                Line(m+8,y0+o+3,m+37,y0+o+3,1f,muted);
            }
            // Corner cuts stay intact at phone sizes; no stretched baroque UVs.
            for(int i=0;i<4;i++)
            {
                float x=i%2==0?x0+o+2:x1-o-2;
                float y=i<2?y0+o+2:y1-o-2;
                float signX=i%2==0?1f:-1f,signY=i<2?1f:-1f;
                Line(x,y,x+signX*10,y,1.8f,light);
                Line(x,y,x,y+signY*10,1.8f,light);
                Line(x+signX*9,y,x+signX*13,y+signY*4,.95f,copper);
                Rivet(x,y,2.5f);
            }
            if(Primary)
            {
                float m=(x0+x1)*.5f;
                Line(m-55,y1-6,m+55,y1-6,1.4f,light);
                Line(m-30,y0+5,m+30,y0+5,.8f,copper);
            }
            if(!Major&&!Primary)
            {
                // Small progress card gets a distinct top medallion.
                float mid=(x0+x1)*.5f;
                Line(mid-11,y1-4,mid-4,y1-4,1.3f,copper);
                Line(mid+4,y1-4,mid+11,y1-4,1.3f,copper);
                Rivet(mid,y1-4,2f);
            }
        }
    }
}
