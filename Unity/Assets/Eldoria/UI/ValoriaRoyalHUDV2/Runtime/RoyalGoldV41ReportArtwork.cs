using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Independent manuscript, embossed outcome seal and four authentic discovery
    /// family motifs. Visual-only vertex geometry: no screenshots, mock statistics,
    /// extra Canvas, interaction, hidden enemy knowledge or allocations in saves.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV41ReportArtwork : MaskableGraphic
    {
        public enum Motif { BattleVictory, BattleDefeat, Frontier, Road, Supply, Rift, Archive }
        Motif motif=Motif.Archive;
        int metricCount,columns=4;
        float metricTop,metricHeight,narrativeTop,narrativeHeight;
        static readonly Color Paper=new Color(.94f,.859f,.706f,1f);
        static readonly Color Shadow=new Color(.16f,.095f,.048f,.35f);
        static readonly Color Brass=new Color(.65f,.424f,.18f,.85f);
        static readonly Color Ink=new Color(.33f,.208f,.113f,.7f);
        public override Texture mainTexture=>Texture2D.whiteTexture;

        public void Configure(Motif newMotif,int count,int cols,float top,float cardH,
            float bodyTop,float bodyH)
        {
            if(motif==newMotif&&metricCount==count&&columns==cols&&
                Mathf.Approximately(metricTop,top)&&Mathf.Approximately(metricHeight,cardH)&&
                Mathf.Approximately(narrativeTop,bodyTop)&&Mathf.Approximately(narrativeHeight,bodyH))return;
            motif=newMotif;metricCount=count;columns=Mathf.Max(1,cols);
            metricTop=top;metricHeight=cardH;narrativeTop=bodyTop;narrativeHeight=bodyH;
            SetVerticesDirty();
        }
        static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,
            Color low,Color high)
        {
            int i=vh.currentVertCount;
            vh.AddVert(a,low,Vector2.zero);vh.AddVert(b,high,Vector2.up);
            vh.AddVert(c,high,Vector2.one);vh.AddVert(d,low,Vector2.right);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
        static void Fill(VertexHelper vh,Rect r,Color low,Color high)
        {
            if(r.width<=0f||r.height<=0f)return;
            Quad(vh,new Vector2(r.xMin,r.yMin),new Vector2(r.xMin,r.yMax),
                new Vector2(r.xMax,r.yMax),new Vector2(r.xMax,r.yMin),low,high);
        }
        static void Line(VertexHelper vh,Vector2 a,Vector2 b,float size,Color tint)
        {
            Vector2 d=b-a;
            if(d.sqrMagnitude<.001f)return;
            d.Normalize();var n=new Vector2(-d.y,d.x)*size*.5f;
            Quad(vh,a-n,a+n,b+n,b-n,tint,tint);
        }
        static void Disc(VertexHelper vh,Vector2 mid,float radius,Color center,Color edge)
        {
            const int steps=18;
            for(int i=0;i<steps;i++)
            {
                float a=i*Mathf.PI*2f/steps,b=(i+1)*Mathf.PI*2f/steps;
                var p=new Vector2(mid.x+Mathf.Cos(a)*radius,mid.y+Mathf.Sin(a)*radius);
                var q=new Vector2(mid.x+Mathf.Cos(b)*radius,mid.y+Mathf.Sin(b)*radius);
                int first=vh.currentVertCount;
                vh.AddVert(mid,center,Vector2.zero);
                vh.AddVert(p,edge,Vector2.zero);
                vh.AddVert(q,edge,Vector2.zero);
                vh.AddTriangle(first,first+1,first+2);
            }
        }
        void DrawGlyph(VertexHelper vh,Vector2 center,float radius,Color motifInk)
        {
            float s=radius*.68f;
            switch(motif)
            {
                case Motif.BattleVictory:
                    Line(vh,center+new Vector2(-s,-s),center+new Vector2(s,s),3f,motifInk);
                    Line(vh,center+new Vector2(s,-s),center+new Vector2(-s,s),3f,motifInk);
                    Line(vh,center+new Vector2(-s*.85f,-s*.40f),center+new Vector2(-s*.38f,-s*.85f),3f,motifInk);
                    Line(vh,center+new Vector2(s*.85f,-s*.40f),center+new Vector2(s*.38f,-s*.85f),3f,motifInk);
                    break;
                case Motif.BattleDefeat:
                    Line(vh,center+new Vector2(-s*.65f,s*.8f),
                        center+new Vector2(s*.65f,s*.8f),3f,motifInk);
                    Line(vh,center+new Vector2(-s*.65f,s*.8f),
                        center+new Vector2(-s*.5f,-s*.4f),3f,motifInk);
                    Line(vh,center+new Vector2(s*.65f,s*.8f),
                        center+new Vector2(s*.5f,-s*.4f),3f,motifInk);
                    Line(vh,center+new Vector2(-s*.5f,-s*.4f),
                        center+new Vector2(0,-s),3f,motifInk);
                    Line(vh,center+new Vector2(s*.5f,-s*.4f),
                        center+new Vector2(0,-s),3f,motifInk);
                    Line(vh,center+new Vector2(-s*.18f,s*.55f),
                        center+new Vector2(s*.14f,-s*.12f),2.5f,motifInk);
                    break;
                case Motif.Frontier:
                    Fill(vh,new Rect(center.x-s*.45f,center.y-s*.8f,s*.9f,s*1.15f),
                        motifInk,motifInk);
                    Fill(vh,new Rect(center.x-s*.7f,center.y+s*.30f,s*1.4f,s*.27f),
                        motifInk,motifInk);
                    Fill(vh,new Rect(center.x-s*.16f,center.y-s*.8f,s*.32f,s*.61f),
                        new Color(.94f,.84f,.63f),new Color(.94f,.84f,.63f));
                    break;
                case Motif.Road:
                    Disc(vh,center,s*.75f,new Color(.89f,.75f,.47f),motifInk);
                    Line(vh,center+new Vector2(-s,0),center+new Vector2(s,0),2f,motifInk);
                    Line(vh,center+new Vector2(0,-s),center+new Vector2(0,s),2f,motifInk);
                    Line(vh,center+new Vector2(-s*.42f,-s*.62f),
                        center+new Vector2(s*.42f,s*.62f),3f,motifInk);
                    break;
                case Motif.Supply:
                    Fill(vh,new Rect(center.x-s*.70f,center.y-s*.60f,s*1.4f,s*1.22f),
                        new Color(.66f,.43f,.21f),new Color(.80f,.60f,.32f));
                    Line(vh,center+new Vector2(-s*.68f,-s*.6f),
                        center+new Vector2(s*.68f,s*.6f),3f,motifInk);
                    Line(vh,center+new Vector2(-s*.68f,s*.6f),
                        center+new Vector2(s*.68f,-s*.6f),3f,motifInk);
                    break;
                case Motif.Rift:
                    Line(vh,center+new Vector2(-s*.55f,s),
                        center+new Vector2(s*.08f,s*.25f),3.5f,motifInk);
                    Line(vh,center+new Vector2(s*.08f,s*.25f),
                        center+new Vector2(-s*.28f,-s*.05f),3.5f,motifInk);
                    Line(vh,center+new Vector2(-s*.28f,-s*.05f),
                        center+new Vector2(s*.48f,-s),3.5f,motifInk);
                    break;
                default:
                    Line(vh,center+new Vector2(-s*.67f,-s*.52f),
                        center+new Vector2(s*.63f,s*.72f),4f,motifInk);
                    Line(vh,center+new Vector2(s*.63f,s*.72f),
                        center+new Vector2(s*.20f,s*.62f),2f,motifInk);
                    break;
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=rectTransform.rect;
            float w=r.width,h=r.height;
            if(w<80f||h<80f)return;
            Rect Area(float x,float y,float width,float height)=>
                new Rect(r.xMin+x,r.yMax-y-height,width,height);
            // Raised warm manuscript: distinct contact shadow, paper, inlay and
            // fine grain. Text and controls are children, never painted pixels.
            Fill(vh,Area(4f,5f,w-8f,h-10f),Shadow,Shadow);
            Fill(vh,Area(1f,0f,w-8f,h-13f),
                new Color(.83f,.728f,.555f),Paper);
            Fill(vh,Area(9f,9f,w-24f,h-34f),
                new Color(.96f,.884f,.739f,.40f),
                new Color(.98f,.924f,.804f,.60f));
            Fill(vh,Area(10f,11f,w-28f,2f),Brass,Brass);
            Fill(vh,Area(10f,h-22f,w-28f,2f),Brass,Brass);
            for(int n=0;n<13;n++)
            {
                float y=26f+n*(Mathf.Max(80f,h-60f)/13f);
                Fill(vh,Area(17f,y,w-49f,.7f),
                    new Color(.36f,.20f,.095f,.065f),
                    new Color(.36f,.20f,.095f,.025f));
            }
            // The header follows actual wrapped report titles, including
            // narrow screens and translated/long discovery names.
            float headerHeight=Mathf.Max(125f,metricTop-25f);
            Fill(vh,Area(15f,16f,w-39f,headerHeight),
                new Color(.24f,.166f,.108f,.95f),
                new Color(.39f,.276f,.16f,.97f));
            Fill(vh,Area(20f,21f,w-49f,2f),Brass,Brass);
            Fill(vh,Area(20f,16f+headerHeight-5f,w-49f,1.8f),Brass,Brass);
            var wax=motif==Motif.BattleDefeat?new Color(.58f,.19f,.18f):
                motif==Motif.Rift?new Color(.40f,.26f,.52f):
                motif==Motif.BattleVictory?new Color(.43f,.26f,.14f):
                new Color(.58f,.32f,.20f);
            Vector2 seal=new Vector2(r.xMin+w-64f,r.yMax-67f);
            Disc(vh,seal+new Vector2(3f,-4f),43f,Shadow,Shadow);
            Disc(vh,seal,41f,wax,new Color(wax.r*.75f,wax.g*.75f,wax.b*.75f,1f));
            Disc(vh,seal,31f,new Color(wax.r+.12f,wax.g+.11f,wax.b+.07f,1f),wax);
            DrawGlyph(vh,seal,35f,new Color(.95f,.76f,.43f,1f));
            for(int i=0;i<4;i++)
            {
                float x=i%2==0?20f:w-32f;
                float y=i<2?17f:h-29f;
                Line(vh,new Vector2(r.xMin+x,r.yMax-y),
                    new Vector2(r.xMin+x+(i%2==0?13f:-13f),r.yMax-y),2f,Ink);
            }
            int count=Mathf.Clamp(metricCount,0,4);
            int cols=Mathf.Max(1,columns);
            float side=24f,gap=11f;
            if(count>0)
            {
                float cardW=(w-side*2f-gap*(cols-1))/cols;
                for(int i=0;i<count;i++)
                {
                    int cx=i%cols,cy=i/cols;
                    float x=side+cx*(cardW+gap),y=metricTop+cy*(metricHeight+gap);
                    Fill(vh,Area(x+3f,y+4f,cardW,metricHeight),
                        new Color(.30f,.19f,.09f,.25f),new Color(.30f,.19f,.09f,.25f));
                    Fill(vh,Area(x,y,cardW,metricHeight),
                        new Color(.82f,.706f,.51f,.98f),new Color(.96f,.861f,.69f,.98f));
                    Fill(vh,Area(x+7f,y+6f,cardW-14f,1.4f),Brass,Brass);
                }
            }
            float storyX=24f,storyW=w-48f;
            Fill(vh,Area(storyX+3f,narrativeTop+4f,storyW,narrativeHeight),
                new Color(.23f,.11f,.06f,.22f),new Color(.23f,.11f,.06f,.22f));
            Fill(vh,Area(storyX,narrativeTop,storyW,narrativeHeight),
                new Color(.91f,.817f,.659f,.97f),
                new Color(.985f,.919f,.793f,.98f));
            Fill(vh,Area(storyX+10f,narrativeTop+11f,
                Mathf.Max(0f,storyW-20f),1.5f),Brass,Brass);
            Fill(vh,Area(storyX+10f,narrativeTop+narrativeHeight-13f,
                Mathf.Max(0f,storyW-20f),1f),new Color(.55f,.38f,.20f,.36f),
                new Color(.55f,.38f,.20f,.36f));
        }
    }
}
