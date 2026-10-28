using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Illustrated contract parchment for the Scout Board. Editable native
    /// uGUI vector art, including hand-cut edge, blue heraldic tab, inked
    /// trail illustration and red wax insignia. Not a second touch target.
    /// Uses the selected real contract type; no gameplay state in the art.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/Crónica Real / Carta de encargo")]
    public sealed class RoyalGoldV3ScoutContractGraphic : MaskableGraphic
    {
        public int Kind;
        public bool EmblemOnly;
        public bool Completed;
        public bool InProgress;
        Color C(float r,float g,float b,float a=1f)=>new Color(r,g,b,a);
        Color T(Color c)=>new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
        void Polygon(VertexHelper v,Color bottom,Color top,params Vector2[] pts)
        {
            if(pts.Length<3)return;
            float ymin=float.MaxValue,ymax=float.MinValue;Vector2 center=Vector2.zero;
            foreach(var p in pts){center+=p;ymin=Mathf.Min(ymin,p.y);ymax=Mathf.Max(ymax,p.y);}
            center/=pts.Length;
            int k=v.currentVertCount;
            v.AddVert(center,T(Color.Lerp(bottom,top,.5f)),Vector2.zero);
            foreach(var p in pts)
                v.AddVert(p,T(Color.Lerp(bottom,top,Mathf.InverseLerp(ymin,ymax,p.y))),Vector2.zero);
            for(int i=0;i<pts.Length;i++)v.AddTriangle(k,k+i+1,k+(i+1)%pts.Length+1);
        }
        void Quad(VertexHelper v,float x,float y,float w,float h,Color c)
        {
            if(w<=0||h<=0)return;
            Polygon(v,c,c,new Vector2(x,y),new Vector2(x+w,y),
                new Vector2(x+w,y+h),new Vector2(x,y+h));
        }
        void Diamond(VertexHelper v,float x,float y,float n,Color c)
        {
            Polygon(v,c,c,new Vector2(x-n,y),new Vector2(x,y+n),
                new Vector2(x+n,y),new Vector2(x,y-n));
        }
        void Circle(VertexHelper v,float x,float y,float rx,float ry,Color lo,Color hi)
        {
            var pts=new Vector2[32];
            for(int i=0;i<pts.Length;i++)
            {
                float angle=i*Mathf.PI*2f/pts.Length;
                pts[i]=new Vector2(x+Mathf.Cos(angle)*rx,y+Mathf.Sin(angle)*ry);
            }
            Polygon(v,lo,hi,pts);
        }
        void Stroke(VertexHelper v,Vector2 a,Vector2 b,float width,Color c)
        {
            var d=(b-a).normalized*width*.5f;
            Vector2 n=new Vector2(-d.y,d.x);
            Polygon(v,c,c,a+n,b+n,b-n,a-n);
        }
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();
            Rect r=rectTransform.rect; if(r.width<100f||r.height<44f)return;
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            var ink=C(.29f,.20f,.12f);
            var edge=C(.36f,.23f,.11f);
            var brass=C(.75f,.55f,.28f);
            var cream=C(.965f,.886f,.705f);
            var dusk=C(.70f,.55f,.34f);
            var navy=C(.048f,.10f,.165f);
            var blue=C(.13f,.23f,.31f);
            var wax=Completed?C(.26f,.39f,.24f):InProgress?C(.34f,.28f,.42f):C(.52f,.15f,.13f);
            if(EmblemOnly)
            {
                // The textured paper is a separate sliced Image. Preserve a
                // distinct live heraldic medallion without painting over it.
                float cx=x+w*.5f,cy=y+h-22f;
                float radius=Mathf.Clamp(w*.065f,15f,28f);
                Circle(v,cx,cy,radius+3f,radius+3f,edge,brass);
                var face=Kind==0?C(.18f,.33f,.50f):Kind==1?C(.22f,.38f,.17f):C(.32f,.18f,.42f);
                Circle(v,cx,cy,radius,radius,face,navy);
                for(int n=0;n<(Kind==1?4:8);n++)
                {
                    float angle=n*Mathf.PI*2f/(Kind==1?4f:8f);
                    var end=new Vector2(cx+Mathf.Cos(angle)*radius*.65f,cy+Mathf.Sin(angle)*radius*.65f);
                    Stroke(v,new Vector2(cx,cy),end,Kind==1?3f:2f,brass);
                }
                Circle(v,cx,cy,3f,3f,cream,brass);
                return;
            }
            // Gilt drop shadow, layered paper edging, and hand-torn silhouette.
            Quad(v,x+2,y,w-2,h-2,C(.014f,.021f,.034f,.92f));
            Polygon(v,edge,brass,new Vector2(x+5,y+3),new Vector2(x+w-10,y+1),
                new Vector2(x+w-3,y+11),new Vector2(x+w-3,y+h-9),
                new Vector2(x+w-11,y+h-2),new Vector2(x+7,y+h-2),
                new Vector2(x+2,y+h-10),new Vector2(x+2,y+10));
            Polygon(v,dusk,cream,new Vector2(x+9,y+7),new Vector2(x+w-15,y+6),
                new Vector2(x+w-8,y+13),new Vector2(x+w-7,y+h-13),
                new Vector2(x+w-15,y+h-8),new Vector2(x+10,y+h-7),
                new Vector2(x+8,y+h-15),new Vector2(x+8,y+14));
            float tab=Mathf.Clamp(w*.14f,35f,65f);
            // Deep blue, suspended royal dispatch ribbon in its own narrow gutter.
            Polygon(v,navy,blue,new Vector2(x+15,y+10),new Vector2(x+tab,y+10),
                new Vector2(x+tab,y+h-8),new Vector2(x+15,y+h-8));
            Stroke(v,new Vector2(x+18,y+15),new Vector2(x+18,y+h-16),1.4f,brass);
            Stroke(v,new Vector2(x+tab-3,y+15),new Vector2(x+tab-3,y+h-16),1.4f,brass);
            float px=x+(tab+15)*.5f,py=y+h*.55f;
            // Four distinctive inked illustrations; reuse the same geometry
            // without fake random objectives or invisible images.
            if(Kind==0)
            {
                // Patrol: crossed swords and stone keep.
                Stroke(v,new Vector2(px-12,py-12),new Vector2(px+12,py+14),3,brass);
                Stroke(v,new Vector2(px+12,py-12),new Vector2(px-12,py+14),3,brass);
                Stroke(v,new Vector2(px-16,py-5),new Vector2(px-7,py-13),3,cream);
                Stroke(v,new Vector2(px+16,py-5),new Vector2(px+7,py-13),3,cream);
            }
            else if(Kind==1)
            {
                // Rescue: stylized beacon flame above a two-line lantern.
                Quad(v,px-9,py-9,18,14,brass);
                Quad(v,px-5,py-5,10,8,navy);
                Polygon(v,brass,cream,new Vector2(px-7,py+8),
                    new Vector2(px,py+19),new Vector2(px+7,py+8));
            }
            else if(Kind==2)
            {
                // Supply cache: timber chest and bright gold lock.
                Quad(v,px-15,py-12,30,21,brass);
                Quad(v,px-12,py-8,24,12,navy);
                Quad(v,px-2,py-8,4,19,cream);
                Quad(v,px-8,py+9,16,4,dusk);
            }
            else
            {
                // Breach traces: amethyst star with fissure rays.
                var purple=C(.73f,.47f,.84f);
                Diamond(v,px,py+3,15,purple);
                Diamond(v,px,py+3,8,navy);
                Stroke(v,new Vector2(px-17,py+17),new Vector2(px-23,py+22),2,purple);
                Stroke(v,new Vector2(px+17,py-11),new Vector2(px+22,py-17),2,purple);
            }
            float sealX=x+w-27,sealY=y+h*.53f;
            // Wax seal of Valoria, doubled for embossed relief.
            Circle(v,sealX+1,sealY-2,13f,13f,C(.16f,.09f,.08f),edge);
            Circle(v,sealX,sealY,12f,12f,edge,wax);
            Circle(v,sealX,sealY,8.6f,8.6f,wax,brass);
            Diamond(v,sealX,sealY,4.5f,cream);
            Stroke(v,new Vector2(sealX,sealY-5),new Vector2(sealX,sealY+5),1.2f,cream);
            // Two fine gold engravings with small irregular ornamental cuts.
            float left=x+tab+12,right=x+w-46;
            if(right-left>22f)
            {
                Quad(v,left,y+h-12,right-left,1.3f,brass);
                Quad(v,left,y+10,right-left,1f,edge);
                Diamond(v,left+7,y+h-12,2.6f,brass);
                Diamond(v,right-7,y+10,2.6f,brass);
            }
            // Ink veins around corners make the visual read as handcrafted.
            Stroke(v,new Vector2(x+w-15,y+h-17),
                new Vector2(x+w-10,y+h-22),1.5f,dusk);
            Stroke(v,new Vector2(x+tab+8,y+15),
                new Vector2(x+tab+13,y+19),1.5f,dusk);
        }
    }
}
