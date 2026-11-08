using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Original editable, layered medieval oak/iron/aged-bronze chest.
    /// A true hinged lid mesh, not a whole-image movement or an opaque overlay.
    /// The current art remains a candidate until genuine Unity image approval.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DailyTreasureChestGraphic : MaskableGraphic
    {
        [Range(0f,1f)] public float OpenAmount;
        static readonly Color Deep=new Color(.040f,.034f,.036f,1f);
        static readonly Color Iron=new Color(.11f,.12f,.14f,1f);
        static readonly Color IronLight=new Color(.28f,.285f,.29f,1f);
        static readonly Color WoodBlack=new Color(.092f,.047f,.035f,1f);
        static readonly Color WoodBrown=new Color(.26f,.125f,.080f,1f);
        static readonly Color WoodHighlight=new Color(.39f,.215f,.13f,1f);
        static readonly Color Bronze=new Color(.55f,.365f,.17f,1f);
        static readonly Color BronzeBright=new Color(.90f,.68f,.32f,1f);
        static readonly Color Amber=new Color(1f,.75f,.29f,1f);
        VertexHelper vh;
        float X,Y,U;
        void Polygon(float x0,float y0,float x1,float y1,float x2,float y2,
            float x3,float y3,Color low,Color high)
        {
            int n=vh.currentVertCount;
            vh.AddVert(new Vector3(X+x0*U,Y+y0*U),low,Vector2.zero);
            vh.AddVert(new Vector3(X+x1*U,Y+y1*U),low,Vector2.right);
            vh.AddVert(new Vector3(X+x2*U,Y+y2*U),high,Vector2.one);
            vh.AddVert(new Vector3(X+x3*U,Y+y3*U),high,Vector2.up);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
        void Quad(float x0,float y0,float x1,float y1,Color low,Color high)
            =>Polygon(x0,y0,x1,y0,x1,y1,x0,y1,low,high);
        void Quad(float x0,float y0,float x1,float y1,Color tint)
            =>Quad(x0,y0,x1,y1,tint,tint);
        void Line(float x0,float y0,float x1,float y1,float thick,Color color)
        {
            float vx=x1-x0,vy=y1-y0,len=Mathf.Sqrt(vx*vx+vy*vy);
            if(len<.001f)return;
            float dx=-vy/len*thick*.5f,dy=vx/len*thick*.5f;
            Polygon(x0-dx,y0-dy,x1-dx,y1-dy,x1+dx,y1+dy,x0+dx,y0+dy,color,color);
        }
        void Disc(float x,float y,float radius,Color center,Color edge,int sides=14)
        {
            int n=vh.currentVertCount;
            vh.AddVert(new Vector3(X+x*U,Y+y*U),center,Vector2.one*.5f);
            for(int i=0;i<=sides;i++)
            {
                float angle=i*Mathf.PI*2f/sides;
                vh.AddVert(new Vector3(X+(x+Mathf.Cos(angle)*radius)*U,
                    Y+(y+Mathf.Sin(angle)*radius)*U),edge,Vector2.zero);
                if(i>0)vh.AddTriangle(n,n+i,n+i+1);
            }
        }
        void Rivet(float x,float y,float radius)
        {
            Disc(x+.9f,y-.8f,radius+.6f,Deep,Iron,12);
            Disc(x,y,radius,BronzeBright,Bronze,12);
            Disc(x-.65f,y+.8f,radius*.35f,Amber,BronzeBright,10);
        }
        void BreachSigil(float x,float y,float scale)
        {
            Line(x,y+11*scale,x-2*scale,y+3*scale,1.3f*scale,BronzeBright);
            Line(x-2*scale,y+3*scale,x+3*scale,y-1*scale,1.4f*scale,BronzeBright);
            Line(x+3*scale,y-1*scale,x-2*scale,y-8*scale,1.4f*scale,Amber);
            Line(x-2*scale,y-8*scale,x,y-11*scale,1.4f*scale,BronzeBright);
            Line(x-10*scale,y,x-3*scale,y,1f*scale,Bronze);
            Line(x+4*scale,y,x+10*scale,y,1f*scale,Bronze);
        }
        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vh=vertices;vh.Clear();
            var rect=GetPixelAdjustedRect();
            U=Mathf.Min(rect.width/204f,rect.height/154f);
            X=rect.center.x-102f*U;Y=rect.center.y-77f*U;
            float open=Mathf.Clamp01(OpenAmount);
            Disc(102,18,65,new Color(0f,0f,0f,.26f),new Color(0f,0f,0f,0f),26);
            Quad(22,18,55,30,Deep,Iron);Quad(151,18,184,30,Deep,Iron);
            Polygon(10,40,22,32,25,96,12,105,Deep,Iron);
            Polygon(181,33,193,42,190,106,178,96,Iron,Deep);
            Quad(18,33,185,95,WoodBlack,WoodBrown);
            Quad(22,37,181,89,WoodBrown,WoodHighlight);
            for(int i=0;i<6;i++)
            {
                float y=42f+i*8f;
                Line(28,y,176,y+.8f,1.4f,WoodBlack);
                float shift=8f+(i*29)%46;
                Line(27+shift,y+2,74+shift,y+2.6f,.65f,
                    new Color(.59f,.34f,.19f,.44f));
                Line(121-(i*13)%39,y+5,165,y+4.7f,.50f,
                    new Color(.075f,.044f,.036f,.48f));
            }
            Quad(17,31,185,37,Iron,IronLight);
            Quad(20,35,182,37,Bronze,BronzeBright);
            Quad(16,89,187,98,Iron,IronLight);
            Quad(20,93,183,96,Bronze,BronzeBright);
            for(int i=0;i<4;i++)
            {
                float px=i==0?34:i==1?72:i==2?132:169;
                Line(px,40,px,90,8f,Iron);
                Line(px-2,42,px-2,89,1.2f,Bronze);
                Line(px+2.5f,44,px+2.5f,88,.65f,IronLight);
                Rivet(px,49,2.5f);Rivet(px,84,2.5f);
            }
            if(open>.015f)
            {
                float a=open*.90f;
                Quad(25,92,178,105+open*6,new Color(.51f,.20f,.055f,a),
                    new Color(1f,.73f,.26f,.65f*a));
                for(int i=0;i<5;i++)
                {
                    float yy=95+i*2.1f;
                    Line(30,yy,173,yy,1.1f,new Color(1f,.71f,.22f,a*(.7f-i*.10f)));
                }
            }
            float lidBase=87+open*18f,lidTop=113+open*27f;
            float pinch=open*10f;
            Polygon(8+pinch,lidBase,193-pinch,lidBase,
                186-pinch*.5f,lidTop,16+pinch*.5f,lidTop,Iron,IronLight);
            Polygon(13+pinch,lidBase+4,187-pinch,lidBase+4,
                181-pinch*.5f,lidTop-5,21+pinch*.5f,lidTop-5,WoodBlack,WoodHighlight);
            Polygon(18+pinch,lidBase+6,182-pinch,lidBase+6,
                175-pinch*.5f,lidTop-10,26+pinch*.5f,lidTop-10,WoodBrown,WoodBlack);
            for(int i=0;i<3;i++)
            {
                float y=lidBase+9+i*4;
                Line(28+pinch,y,174-pinch,y+.3f,.65f,
                    i==1?WoodHighlight:WoodBlack);
            }
            for(int i=0;i<3;i++)
            {
                float px=i==0?34:i==1?102:170;
                Line(px,lidBase+5,px,lidTop-3,7f,Iron);
                Line(px-2,lidBase+6,px-2,lidTop-4,1f,Bronze);
                Rivet(px,lidBase+9,2.4f);Rivet(px,lidTop-7,2.4f);
            }
            Line(16+pinch,lidTop-3,184-pinch,lidTop-3,1.7f,BronzeBright);
            Quad(84,54,120,89,Deep,IronLight);
            Quad(88,57,116,86,Iron,Bronze);
            Quad(91,59,113,83,WoodBlack,Bronze);
            Disc(102,72,8,Bronze,BronzeBright,18);
            Disc(102,72,6,WoodBlack,Iron,18);
            BreachSigil(102,72,.45f);
            for(int i=0;i<4;i++)Rivet(i%2==0?24:180,i<2?40:86,2.2f);
        }
        public void SetOpening(float amount)
        {
            amount=Mathf.Clamp01(amount);
            if(Mathf.Abs(OpenAmount-amount)<.003f)return;
            OpenAmount=amount;
            SetVerticesDirty();
        }
    }
}
