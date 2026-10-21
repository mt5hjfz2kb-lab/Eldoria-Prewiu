using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Artwork for V3 management destinations. Drawn as scalable, opaque
    /// hand-composed uGUI geometry (royal-blue military tapestry, gilt compass,
    /// shield and bow). Never loads a city scene or runtime texture.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/V3 Management Illustrated Backdrop")]
    public sealed class RoyalGoldV3ManagementBackdropGraphic : MaskableGraphic
    {
        public bool MarchTheme;
        static readonly Color Navy=new Color(.016f,.030f,.056f,1f);
        static readonly Color Slate=new Color(.048f,.081f,.128f,1f);
        static readonly Color Highlight=new Color(.103f,.15f,.196f,1f);
        static readonly Color Brass=new Color(.60f,.45f,.245f,.85f);
        static readonly Color DimBrass=new Color(.34f,.26f,.16f,.50f);
        static readonly Color Mist=new Color(.62f,.68f,.68f,.14f);
        Color Tint(Color c)=>new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
        void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color low,Color high)
        {
            int i=vh.currentVertCount;
            vh.AddVert(a,Tint(low),Vector2.zero);
            vh.AddVert(b,Tint(high),Vector2.up);
            vh.AddVert(c,Tint(high),Vector2.one);
            vh.AddVert(d,Tint(low),Vector2.right);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i+2,i+3,i);
        }
        void Box(VertexHelper vh,float x,float y,float w,float h,Color low,Color high)
        {
            if(w<=0||h<=0)return;
            Quad(vh,new Vector2(x,y),new Vector2(x,y+h),new Vector2(x+w,y+h),new Vector2(x+w,y),low,high);
        }
        void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color c)
        {
            var d=b-a;if(d.sqrMagnitude<.1f)return;
            var n=new Vector2(-d.y,d.x).normalized*width*.5f;
            Quad(vh,a-n,a+n,b+n,b-n,c,c);
        }
        void Disc(VertexHelper vh,Vector2 p,float radius,float thickness,Color c,int segments=64)
        {
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2f/segments,b=(i+1)*Mathf.PI*2f/segments;
                var va=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                var vb=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                Quad(vh,p+va*(radius-thickness),p+va*radius,p+vb*radius,p+vb*(radius-thickness),c,c);
            }
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float w=r.width,h=r.height;
            if(w<50||h<50)return;
            Box(mesh,r.xMin,r.yMin,w,h,Navy,Slate);
            // Military tapestry / quiet banding in the same approved V3 colors.
            for(int i=0;i<14;i++)
            {
                float x=r.xMin+w*(i/13f);
                Line(mesh,new Vector2(x,r.yMin),new Vector2(x+w*.11f,r.yMax),2f,
                    new Color(.44f,.51f,.53f,.045f));
            }
            float inset=Mathf.Min(w,h)*.022f;
            Box(mesh,r.xMin+inset,r.yMax-inset,w-2*inset,1.6f,DimBrass,DimBrass);
            Box(mesh,r.xMin+inset,r.yMin+inset,w-2*inset,1.6f,DimBrass,DimBrass);
            Box(mesh,r.xMin+inset,r.yMin+inset,1.6f,h-2*inset,DimBrass,DimBrass);
            Box(mesh,r.xMax-inset,r.yMin+inset,1.6f,h-2*inset,DimBrass,DimBrass);
            var p=new Vector2(r.xMin+w*.73f,r.yMin+h*.55f);
            float radius=Mathf.Min(w*.28f,h*.42f);
            // Engraved ornamental circles: map compass on marches, archer shield
            // and longbow on the training destination.
            Disc(mesh,p,radius,2.5f,Brass);
            Disc(mesh,p,radius*.87f,1.2f,DimBrass);
            Disc(mesh,p,radius*.48f,1f,Mist);
            if(MarchTheme)
            {
                for(int i=0;i<16;i++)
                {
                    float a=i*Mathf.PI/8;
                    var dir=new Vector2(Mathf.Sin(a),Mathf.Cos(a));
                    Line(mesh,p+dir*radius*.88f,p+dir*(i%4==0?radius*1.075f:radius*.98f),
                        i%4==0?3f:1f,Brass);
                }
                // Four winds, compass rose, expedition path.
                Line(mesh,p+Vector2.up*radius*.67f,p+Vector2.down*radius*.67f,3f,Brass);
                Line(mesh,p+Vector2.left*radius*.67f,p+Vector2.right*radius*.67f,3f,Brass);
                Line(mesh,p+new Vector2(-radius*.53f,-radius*.53f),p+new Vector2(radius*.53f,radius*.53f),1.6f,DimBrass);
                Line(mesh,p+new Vector2(radius*.53f,-radius*.53f),p+new Vector2(-radius*.53f,radius*.53f),1.6f,DimBrass);
                for(int i=0;i<6;i++)
                {
                    Vector2 a=new Vector2(r.xMin+w*(.14f+.11f*i),r.yMin+h*(.19f+.045f*(i%3)));
                    Vector2 b=new Vector2(r.xMin+w*(.25f+.11f*i),r.yMin+h*(.19f+.045f*((i+1)%3)));
                    Line(mesh,a,b,2f,Mist);
                }
            }
            else
            {
                // Stylized recurved bow and string: original military-tapestry
                // motif, not a new soldier unit, item or world object.
                Vector2 top=p+new Vector2(-radius*.30f,radius*.70f);
                Vector2 bottom=p+new Vector2(-radius*.30f,-radius*.70f);
                Vector2 mid=p+new Vector2(radius*.17f,0);
                Line(mesh,top,p+new Vector2(radius*.28f,radius*.42f),5f,Brass);
                Line(mesh,p+new Vector2(radius*.28f,radius*.42f),mid,5f,Brass);
                Line(mesh,mid,p+new Vector2(radius*.28f,-radius*.42f),5f,Brass);
                Line(mesh,p+new Vector2(radius*.28f,-radius*.42f),bottom,5f,Brass);
                Line(mesh,top,bottom,1.5f,Mist);
                Line(mesh,p+new Vector2(-radius*.53f,0),p+new Vector2(radius*.55f,0),3f,Brass);
                Line(mesh,p+new Vector2(radius*.55f,0),p+new Vector2(radius*.40f,radius*.12f),3f,Brass);
                Line(mesh,p+new Vector2(radius*.55f,0),p+new Vector2(radius*.40f,-radius*.12f),3f,Brass);
            }
            // Corner ornaments are present even when management content is tall.
            for(int k=0;k<4;k++)
            {
                var c=new Vector2(k%2==0?r.xMin+inset*1.6f:r.xMax-inset*1.6f,
                    k<2?r.yMin+inset*1.6f:r.yMax-inset*1.6f);
                Disc(mesh,c,5.5f,2f,Brass,12);
            }
        }
    }
}
