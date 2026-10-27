using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>Actual editable uGUI scenic illustration, not a flat mockup.
    /// Chapters depict the Vigilants' path; scouting depicts the herald's map.
    /// No external asset or texture download is required; GPU-safe vertex art.</summary>
    public sealed class RoyalGoldV3EarlyRetentionArtwork : MaskableGraphic
    {
        public bool Scouts;
        public int Chapter = 1;
        public bool BossEncounter;
        // Five boss seals show the ACTUAL completed milestones in this chapter.
        public int ChapterProgress;
        Color T(Color c)=>new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
        void Q(VertexHelper v,float x,float y,float w,float h,Color low,Color high)
        {
            if(w<=0||h<=0)return;int k=v.currentVertCount;
            v.AddVert(new Vector3(x,y),T(low),Vector2.zero);
            v.AddVert(new Vector3(x,y+h),T(high),Vector2.up);
            v.AddVert(new Vector3(x+w,y+h),T(high),Vector2.one);
            v.AddVert(new Vector3(x+w,y),T(low),Vector2.right);
            v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);
        }
        void Tri(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Color col)
        {int k=v.currentVertCount;v.AddVert(a,T(col),Vector2.zero);v.AddVert(b,T(col),Vector2.zero);v.AddVert(c,T(col),Vector2.zero);v.AddTriangle(k,k+1,k+2);}
        void Diamond(VertexHelper v,float x,float y,float radius,Color c)
        {Tri(v,new Vector2(x-radius,y),new Vector2(x,y+radius),new Vector2(x+radius,y),c);
         Tri(v,new Vector2(x-radius,y),new Vector2(x+radius,y),new Vector2(x,y-radius),c);}
        void Coin(VertexHelper v,float x,float y,float radius,Color inner,Color rim)
        {
            var outer=new Vector2[20];var paper=new Vector2[20];
            for(int i=0;i<20;i++)
            {
                float angle=i*Mathf.PI*.1f;
                outer[i]=new Vector2(x+Mathf.Cos(angle)*radius,y+Mathf.Sin(angle)*radius);
                paper[i]=new Vector2(x+Mathf.Cos(angle)*(radius-2.2f),
                    y+Mathf.Sin(angle)*(radius-2.2f));
            }
            int o=v.currentVertCount;
            foreach(var p in outer)v.AddVert(p,T(rim),Vector2.zero);
            foreach(var p in paper)v.AddVert(p,T(inner),Vector2.zero);
            for(int i=0;i<20;i++)
            {
                int j=(i+1)%20;
                v.AddTriangle(o+i,o+j,o+20+j);
                v.AddTriangle(o+i,o+20+j,o+20+i);
            }
            for(int i=1;i<19;i++)v.AddTriangle(o+20,o+20+i,o+20+i+1);
        }
        void Tree(VertexHelper v,float x,float y,float h,Color wood,Color leaves)
        {
            Q(v,x-h*.045f,y,h*.09f,h*.55f,wood,wood);
            Tri(v,new Vector2(x-h*.30f,y+h*.26f),new Vector2(x,y+h),
                new Vector2(x+h*.30f,y+h*.26f),leaves);
            Tri(v,new Vector2(x-h*.38f,y+h*.08f),new Vector2(x,y+h*.66f),
                new Vector2(x+h*.38f,y+h*.08f),leaves);
        }
        void Crest(VertexHelper v,float x,float y,float size,Color gold)
        {
            Q(v,x-size*.08f,y-size*.38f,size*.16f,size*.85f,gold,gold);
            Q(v,x-size*.36f,y-size*.11f,size*.72f,size*.16f,gold,gold);
            Tri(v,new Vector2(x-size*.36f,y+size*.06f),
                new Vector2(x,y+size*.50f),new Vector2(x+size*.36f,y+size*.06f),gold);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            if(r.width<80||r.height<70)return;
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            var shadow=new Color(.016f,.028f,.047f,1f);
            var navy=new Color(.063f,.110f,.156f,1f);
            var warm=new Color(.38f,.28f,.18f,1f);
            var antique=new Color(.76f,.60f,.35f,1f);
            var grass=new Color(.16f,.26f,.20f,1f);
            var deep=new Color(.08f,.14f,.14f,1f);
            Q(vh,x,y,w,h,shadow,navy);
            if(Scouts)
            {
                // A royal cartographer's leather chart, brass dividers and map pins.
                Q(vh,x+w*.10f,y+h*.11f,w*.80f,h*.77f,
                    new Color(.55f,.43f,.27f,1),new Color(.87f,.76f,.51f,1));
                Q(vh,x+w*.135f,y+h*.15f,w*.73f,h*.69f,
                    new Color(.77f,.64f,.42f,1),new Color(.94f,.84f,.63f,1));
                for(int i=1;i<6;i++)
                {
                    Q(vh,x+w*(.16f+i*.10f),y+h*.18f,w*.004f,h*.63f,
                        new Color(.42f,.35f,.23f,.28f),new Color(.42f,.35f,.23f,.28f));
                    Q(vh,x+w*.155f,y+h*(.20f+i*.09f),w*.69f,h*.004f,
                        new Color(.42f,.35f,.23f,.28f),new Color(.42f,.35f,.23f,.28f));
                }
                Tri(vh,new Vector2(x+w*.28f,y+h*.38f),new Vector2(x+w*.51f,y+h*.72f),
                    new Vector2(x+w*.67f,y+h*.50f),new Color(.32f,.42f,.34f,.78f));
                Tri(vh,new Vector2(x+w*.50f,y+h*.26f),new Vector2(x+w*.70f,y+h*.47f),
                    new Vector2(x+w*.78f,y+h*.25f),new Color(.25f,.36f,.30f,.8f));
                for(int i=0;i<4;i++){
                    float px=x+w*(.25f+i*.16f),py=y+h*(.33f+(i%3)*.12f);
                    Diamond(vh,px,py,Mathf.Min(w,h)*.035f,new Color(.49f,.21f,.16f,1));
                }
                Crest(vh,x+w*.51f,y+h*.49f,Mathf.Min(w,h)*.16f,antique);
            }
            else
            {
                // Four palettes cycle by chapter without changing scene geometry.
                bool ash=(Chapter-1)%4==0;
                var sky=ash?new Color(.14f,.19f,.23f,1):
                    new Color(.12f,.22f,.28f,1);
                Q(vh,x,y+h*.27f,w,h*.73f,deep,sky);
                Q(vh,x,y,w,h*.27f,new Color(.12f,.16f,.13f,1),grass);
                // A receding stone road and a single tower, with clear depth
                // even when the banner is shown on a narrow mobile viewport.
                Tri(vh,new Vector2(x+w*.18f,y),new Vector2(x+w*.51f,y+h*.44f),
                    new Vector2(x+w*.80f,y),new Color(.44f,.39f,.33f,1));
                for(int i=0;i<6;i++)
                {
                    float t=i/6f,baseY=y+h*(.04f+t*.37f);
                    Q(vh,x+w*(.28f+t*.23f),baseY,w*(.43f-t*.34f),h*.007f,
                        new Color(.25f,.25f,.24f,1),new Color(.57f,.52f,.41f,1));
                }
                Tree(vh,x+w*.15f,y+h*.21f,h*.42f,warm,deep);
                Tree(vh,x+w*.84f,y+h*.23f,h*.52f,warm,
                    new Color(.09f,.18f,.15f,1));
                Tree(vh,x+w*.72f,y+h*.28f,h*.28f,warm,deep);
                Q(vh,x+w*.47f,y+h*.39f,w*.13f,h*.37f,
                    new Color(.31f,.31f,.33f,1),new Color(.65f,.61f,.52f,1));
                Q(vh,x+w*.46f,y+h*.68f,w*.15f,h*.08f,
                    new Color(.42f,.37f,.30f,1),new Color(.78f,.66f,.47f,1));
                for(int k=0;k<4;k++)
                    Q(vh,x+w*(.46f+k*.043f),y+h*.76f,w*.027f,h*.055f,
                        new Color(.51f,.47f,.42f,1),new Color(.75f,.69f,.57f,1));
                Q(vh,x+w*.519f,y+h*.40f,w*.029f,h*.17f,shadow,shadow);
                Crest(vh,x+w*.53f,y+h*.59f,Mathf.Min(w,h)*.11f,antique);
                if(BossEncounter)
                {
                    // Purple Breach atmosphere belongs to the rare boss seal,
                    // not to every ordinary Vigilants encounter.
                    var purple=new Color(.71f,.44f,.87f,1);
                    Diamond(vh,x+w*.53f,y+h*.90f,Mathf.Min(w,h)*.055f,purple);
                    Diamond(vh,x+w*.53f,y+h*.90f,Mathf.Min(w,h)*.025f,
                        new Color(.95f,.81f,1f,1));
                    Q(vh,x+w*.528f,y+h*.79f,w*.004f,h*.07f,purple,purple);
                    Q(vh,x+w*.495f,y+h*.88f,w*.066f,h*.005f,purple,purple);
                }
                // The five illustrated boss milestones are an in-world trail,
                // never extra buttons or tutorial-locked destinations.
                // Displayed after clearing five, ten, fifteen, twenty,
                // and twenty-five stages in the player's own chapter.
                float trackY=y+h*.105f;
                float startX=x+w*.18f,endX=x+w*.82f;
                float step=(endX-startX)*.25f;
                float radius=Mathf.Clamp(h*.043f,5f,13f);
                Q(vh,startX,trackY-1f,endX-startX,2f,antique,antique);
                for(int i=0;i<5;i++)
                {
                    bool won=ChapterProgress>=(i+1)*5;
                    var ring=won?new Color(.96f,.77f,.45f,1):
                        new Color(.41f,.37f,.32f,1);
                    var centre=won?new Color(.72f,.46f,.21f,1):
                        new Color(.06f,.09f,.13f,1);
                    Coin(vh,startX+i*step,trackY,radius,centre,ring);
                    Diamond(vh,startX+i*step,trackY,radius*.47f,
                        won?new Color(.98f,.88f,.66f,1):
                        new Color(.50f,.50f,.47f,1));
                }
            }
            Q(vh,x+2,y+2,w-4,2,antique,antique);
            Q(vh,x+2,y+h-4,w-4,2,antique,antique);
            Diamond(vh,x+13,y+13,4,antique);
            Diamond(vh,x+w-13,y+h-13,4,antique);
        }
    }
}
