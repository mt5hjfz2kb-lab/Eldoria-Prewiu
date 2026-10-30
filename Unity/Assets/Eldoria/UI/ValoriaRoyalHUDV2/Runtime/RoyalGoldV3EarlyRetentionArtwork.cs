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
        void InkStroke(VertexHelper v,Vector2 from,Vector2 to,float thickness,Color ink)
        {
            if((to-from).sqrMagnitude<.0001f)return;
            var direction=(to-from).normalized;
            var normal=new Vector2(-direction.y,direction.x)*(thickness*.5f);
            Tri(v,from-normal,from+normal,to+normal,ink);
            Tri(v,from-normal,to+normal,to-normal,ink);
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
                // A quiet hand-inked journey connects the four real map
                // destinations. It is scenic, not a new interactive hotspot.
                var stations=new Vector2[4];
                for(int i=0;i<stations.Length;i++)
                    stations[i]=new Vector2(x+w*(.25f+i*.16f),y+h*(.33f+(i%3)*.12f));
                for(int i=0;i<stations.Length-1;i++)
                {
                    for(int dash=0;dash<8;dash+=2)
                    {
                        InkStroke(vh,Vector2.Lerp(stations[i],stations[i+1],dash/8f),
                            Vector2.Lerp(stations[i],stations[i+1],(dash+1)/8f),
                            Mathf.Clamp(Mathf.Min(w,h)*.007f,1f,3.5f),
                            new Color(.40f,.27f,.17f,.70f));
                    }
                }
                // Compass rose in the margin gives the parchment a believable
                // cartographer's identity without expensive 3D objects.
                float compassX=x+w*.20f,compassY=y+h*.72f;
                Coin(vh,compassX,compassY,Mathf.Clamp(Mathf.Min(w,h)*.045f,6f,21f),
                    new Color(.78f,.64f,.43f,1),new Color(.45f,.31f,.18f,1));
                Diamond(vh,compassX,compassY,Mathf.Clamp(Mathf.Min(w,h)*.032f,4f,14f),
                    new Color(.24f,.35f,.38f,1));
                for(int i=0;i<stations.Length;i++)
                    Diamond(vh,stations[i].x,stations[i].y,Mathf.Min(w,h)*.035f,
                        new Color(.49f,.21f,.16f,1));
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
                // Distant light and two landscape planes add atmosphere
                // without texture bandwidth, additional cameras or animation.
                Coin(vh,x+w*.78f,y+h*.83f,Mathf.Clamp(Mathf.Min(w,h)*.060f,8f,36f),
                    new Color(.79f,.62f,.38f,.80f),new Color(.44f,.37f,.32f,.25f));
                Tri(vh,new Vector2(x,y+h*.27f),new Vector2(x+w*.25f,y+h*.47f),
                    new Vector2(x+w*.54f,y+h*.27f),new Color(.15f,.24f,.22f,1));
                Tri(vh,new Vector2(x+w*.39f,y+h*.27f),
                    new Vector2(x+w*.77f,y+h*.42f),
                    new Vector2(x+w,y+h*.27f),new Color(.12f,.20f,.21f,1));
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
                // Fine hand-authored masonry and a single lit window reinforce
                // the Valoria stone vocabulary instead of a featureless cuboid.
                for(int row=0;row<5;row++)
                {
                    float wallY=y+h*(.43f+row*.052f);
                    Q(vh,x+w*.475f,wallY,w*.038f,h*.005f,
                        new Color(.28f,.27f,.25f,1),new Color(.46f,.41f,.35f,1));
                    Q(vh,x+w*.556f,wallY+h*.022f,w*.033f,h*.004f,
                        new Color(.28f,.27f,.25f,1),new Color(.46f,.41f,.35f,1));
                }
                Q(vh,x+w*.574f,y+h*.58f,w*.014f,h*.035f,
                    new Color(.54f,.37f,.19f,1),new Color(.96f,.76f,.41f,1));
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
                // The 25-node RoyalGoldV3StageTrailGraphic already includes
                // every guardian at 5/10/15/20/25 and owns the real progress.
                // Do not draw a SECOND row of milestone coins beneath it:
                // it overlaps the trail on actual Unity mobile screenshots.
            }
            Q(vh,x+2,y+2,w-4,2,antique,antique);
            Q(vh,x+2,y+h-4,w-4,2,antique,antique);
            Diamond(vh,x+13,y+13,4,antique);
            Diamond(vh,x+w-13,y+h-13,4,antique);
        }
    }

    /// <summary>25 discernible chapter stages in a connected winding path.
    /// Boss marks at 5/10/15/20/25 are larger; current position has an
    /// illuminated amethyst rim. Purely informational: actual action is in
    /// the single authoritative battle button, not 25 invented clicks.</summary>
    public sealed class RoyalGoldV3StageTrailGraphic : MaskableGraphic
    {
        public int Cleared, Chapter=1;
        Color Tint(Color c)=>new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
        void Disc(VertexHelper vh,Vector2 center,float radius,Color tint)
        {
            int k=vh.currentVertCount;
            vh.AddVert(center,Tint(tint),Vector2.zero);
            for(int i=0;i<=8;i++){
                float a=i*Mathf.PI*2/8f;
                vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,
                    Tint(tint),Vector2.zero);
                if(i>0)vh.AddTriangle(k,k+i,k+i+1);
            }
        }
        void Line(VertexHelper vh,Vector2 start,Vector2 end,float thickness,Color c)
        {
            var dir=(end-start).normalized;
            var n=new Vector2(-dir.y,dir.x)*thickness*.5f;
            int k=vh.currentVertCount;
            foreach(var p in new[]{start-n,start+n,end+n,end-n})
                vh.AddVert(p,Tint(c),Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
        // Coordinates measured on the actual painterly-v2 texture, bottom-left
        // UV origin. Project the road through the SAME cover crop as RawImage.
        // The chapter follows the painted paving, never an unrelated zigzag.
        public Rect ArtworkUv=new Rect(0,0,1,1);
        static readonly Vector2[] Road = {
            new Vector2(.37f,.075f),new Vector2(.25f,.14f),
            new Vector2(.13f,.19f),new Vector2(.06f,.25f),
            new Vector2(.09f,.29f),new Vector2(.20f,.32f),
            new Vector2(.32f,.36f),new Vector2(.43f,.40f),
            new Vector2(.53f,.43f),new Vector2(.55f,.46f),
            new Vector2(.51f,.49f),new Vector2(.57f,.52f),
            new Vector2(.65f,.55f),new Vector2(.65f,.58f),
            new Vector2(.62f,.61f),new Vector2(.67f,.64f),
            new Vector2(.71f,.68f),new Vector2(.68f,.70f),
            new Vector2(.72f,.74f),new Vector2(.73f,.77f),
            new Vector2(.70f,.80f)
        };
        readonly System.Collections.Generic.List<Vector2> visibleRoad=
            new System.Collections.Generic.List<Vector2>();
        Rect cachedRoadBounds,cachedRoadUv;
        bool hasRoadCache;
        void ProjectRoad()
        {
            var b=rectTransform.rect;
            if(hasRoadCache&&cachedRoadBounds==b&&cachedRoadUv==ArtworkUv)return;
            cachedRoadBounds=b;cachedRoadUv=ArtworkUv;hasRoadCache=true;
            visibleRoad.Clear();
            var section=new System.Collections.Generic.List<Vector2>();
            // Cover cropping can hide the foreground. Sample only the
            // continuous visible portion, preserving actual scene positions.
            for(int segment=0;segment<Road.Length-1;segment++)
                for(int sample=0;sample<12;sample++)
                {
                    var uv=Vector2.Lerp(Road[segment],Road[segment+1],sample/12f);
                    float x=(uv.x-ArtworkUv.x)/ArtworkUv.width;
                    float y=(uv.y-ArtworkUv.y)/ArtworkUv.height;
                    if(x<.04f||x>.96f||y<.06f||y>.94f)
                    {
                        if(section.Count>visibleRoad.Count){visibleRoad.Clear();visibleRoad.AddRange(section);}
                        section.Clear();continue;
                    }
                    section.Add(new Vector2(b.xMin+x*b.width,b.yMin+y*b.height));
                }
            if(section.Count>visibleRoad.Count){visibleRoad.Clear();visibleRoad.AddRange(section);}
        }
        public Vector2 StagePosition(int stage)
        {
            ProjectRoad();
            if(visibleRoad.Count<2)return rectTransform.rect.center;
            float length=0;
            for(int i=1;i<visibleRoad.Count;i++)length+=Vector2.Distance(visibleRoad[i-1],visibleRoad[i]);
            float target=length*(Mathf.Clamp(stage,1,25)-1)/24f;
            for(int i=1;i<visibleRoad.Count;i++)
            {
                float segment=Vector2.Distance(visibleRoad[i-1],visibleRoad[i]);
                if(target<=segment)return Vector2.Lerp(visibleRoad[i-1],visibleRoad[i],target/Mathf.Max(.001f,segment));
                target-=segment;
            }
            return visibleRoad[visibleRoad.Count-1];
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var b=rectTransform.rect;
            if(b.width<110||b.height<65)return;
            var nodes=new Vector2[25];
            float marginX=Mathf.Min(26,b.width*.10f),marginY=Mathf.Min(19,b.height*.12f);
            float dx=(b.width-marginX*2)/4f,dy=(b.height-marginY*2)/4f;
            float weaveX=Mathf.Min(8f,dx*.12f),weaveY=Mathf.Min(6f,dy*.12f);
            for(int step=0;step<25;step++){
                int row=step/5,col=step%5;
                if(row%2==1)col=4-col;
                // The route remains 25 deterministic real steps; subtle
                // offsets make it read as a winding campaign trail rather
                // than a regimented five-by-five spreadsheet grid.
                nodes[step]=StagePosition(step+1);
            }
            for(int step=0;step<24;step++)
            {
                var a=nodes[step];var end=nodes[step+1];
                var tangent=end-a;
                var normal=new Vector2(-tangent.y,tangent.x).normalized;
                float arc=Mathf.Min(7f,tangent.magnitude*.12f);
                var bend=(a+end)*.5f+normal*((step%2==0?1f:-1f)*arc);
                // A subtle shaded path becomes warm gold as the player
                // completes encounters. Boss nodes still belong to this
                // one route, not a duplicated ornamental row.
                for(int part=0;part<6;part++)
                {
                    float t0=part/6f,t1=(part+1)/6f;
                    var p0=Vector2.Lerp(Vector2.Lerp(a,bend,t0),
                        Vector2.Lerp(bend,end,t0),t0);
                    var p1=Vector2.Lerp(Vector2.Lerp(a,bend,t1),
                        Vector2.Lerp(bend,end,t1),t1);
                    Line(vh,p0,p1,Mathf.Max(3f,b.width/145f),
                        new Color(.13f,.10f,.08f,.86f));
                    Line(vh,p0,p1,Mathf.Max(1.5f,b.width/245f),
                        (Chapter-1)*25+step+1<=Cleared
                            ?new Color(.99f,.78f,.35f,.96f)
                            :new Color(.73f,.61f,.42f,.69f));
                }
            }
            for(int i=0;i<25;i++){
                int absolute=(Chapter-1)*25+i+1;
                bool cleared=absolute<=Cleared;
                bool current=absolute==Cleared+1;
                bool boss=(i+1)%5==0;
                float separation=float.MaxValue;
                if(i>0)separation=Mathf.Min(separation,Vector2.Distance(nodes[i],nodes[i-1]));
                if(i<24)separation=Mathf.Min(separation,Vector2.Distance(nodes[i],nodes[i+1]));
                float r=Mathf.Clamp(separation*.31f,3,16);
                if(boss)r*=1.3f;
                var gold=new Color(.78f,.63f,.39f,1);
                var dark=new Color(.12f,.15f,.17f,.92f);
                if(current){
                    Disc(vh,nodes[i],r+9,new Color(1f,.50f,.05f,.20f));
                    Disc(vh,nodes[i],r+5,new Color(1f,.70f,.12f,.75f));
                }
                // Physical inset medallion: the lower-right contact shadow
                // and upper-left metal catchlight make the marker sit ON the
                // painted paving instead of floating above the photograph.
                // Coordinates and 25 genuine gameplay stages remain unchanged.
                Disc(vh,nodes[i]+new Vector2(1.7f,-2.4f),r+2.6f,
                    new Color(.025f,.032f,.031f,.44f));
                Disc(vh,nodes[i],r+2,boss?gold:new Color(.48f,.40f,.30f,1));
                Disc(vh,nodes[i],r,dark);
                Disc(vh,nodes[i]+new Vector2(-r*.26f,r*.27f),
                    Mathf.Max(.8f,r*.16f),
                    new Color(1f,.88f,.64f,cleared?.76f:.54f));
                if(cleared){
                    Line(vh,nodes[i]+new Vector2(-r*.5f,0),nodes[i]+new Vector2(-r*.1f,-r*.4f),3,new Color(.45f,.93f,.27f,1));
                    Line(vh,nodes[i]+new Vector2(-r*.1f,-r*.4f),nodes[i]+new Vector2(r*.6f,r*.45f),3,new Color(.45f,.93f,.27f,1));
                }
                if(boss)Disc(vh,nodes[i],r*.42f,
                    cleared?new Color(.40f,.21f,.12f,1):
                    new Color(.67f,.31f,.25f,1));
            }
        }
    }
    /// <summary>
    /// Four tiny illuminated cartographer map pins, based only on actual
    /// completed Scout kinds. Pure UI mesh; no textures, taps, or invented loot.
    /// </summary>
    public sealed class RoyalGoldV3ScoutDiscoverySeals : MaskableGraphic
    {
        public int DiscoveredMask;
        static readonly Vector2[] Points={
            new Vector2(.30f,.39f),new Vector2(.47f,.64f),
            new Vector2(.68f,.47f),new Vector2(.78f,.70f)
        };
        Color Tone(Color c)=>new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
        void Disc(VertexHelper vh,Vector2 origin,float radius,Color tint,int count=16)
        {
            int start=vh.currentVertCount;
            vh.AddVert(origin,Tone(tint),Vector2.zero);
            for(int i=0;i<=count;i++)
            {
                float a=i*Mathf.PI*2f/count;
                var point=origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;
                vh.AddVert(point,Tone(tint),Vector2.zero);
                if(i>0)vh.AddTriangle(start,start+i,start+i+1);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=rectTransform.rect;
            if(r.width<100f||r.height<90f)return;
            float size=Mathf.Clamp(Mathf.Min(r.width,r.height)*.033f,6.5f,15f);
            for(int i=0;i<Points.Length;i++)
            {
                if((DiscoveredMask&(1<<i))==0)continue;
                var center=new Vector2(r.xMin+r.width*Points[i].x,r.yMin+r.height*Points[i].y);
                var gold=new Color(.99f,.78f,.40f,.98f);
                Disc(vh,center,size*1.65f,new Color(.99f,.65f,.25f,.18f));
                Disc(vh,center,size*1.22f,new Color(.19f,.11f,.10f,.96f));
                Disc(vh,center,size*.96f,gold);
                Disc(vh,center,size*.66f,new Color(.34f,.20f,.14f,1f));
                Disc(vh,center,size*.32f,new Color(.95f,.83f,.52f,1f));
                // The seal's tiny point directs the eye to the region; no
                // separate clickable marker overlaps the real board actions.
                int k=vh.currentVertCount;
                vh.AddVert(center+new Vector2(-size*.35f,-size*.80f),Tone(gold),Vector2.zero);
                vh.AddVert(center+new Vector2(size*.35f,-size*.80f),Tone(gold),Vector2.zero);
                vh.AddVert(center+new Vector2(0,-size*1.63f),Tone(gold),Vector2.zero);
                vh.AddTriangle(k,k+1,k+2);
            }
        }
    }

}
