using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// A non-interactive material finish for the single genuine city-local
    /// building offer and its projected operation timer. Layered shadows,
    /// raised parchment and relief are generated from reusable uGUI vertices.
    /// No baked labels, extra Canvas, game command or synthetic state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3BuildingCartoucheFinish : MaskableGraphic
    {
        public enum Phase { Ready, Unavailable, InProgress, Council }
        public Phase CurrentPhase { get; private set; }
        public bool TimerScale;
        public override Texture mainTexture => Texture2D.whiteTexture;
        static void Fill(VertexHelper v,Rect r,Color lower,Color upper)
        {
            if(r.width<=0||r.height<=0)return;
            int n=v.currentVertCount;
            v.AddVert(new Vector3(r.xMin,r.yMin),lower,Vector2.zero);
            v.AddVert(new Vector3(r.xMin,r.yMax),upper,Vector2.up);
            v.AddVert(new Vector3(r.xMax,r.yMax),upper,Vector2.one);
            v.AddVert(new Vector3(r.xMax,r.yMin),lower,Vector2.right);
            v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
        static void Stripe(VertexHelper v,Rect r,Color c)=>Fill(v,r,c,c);
        static void Diamond(VertexHelper v,Vector2 p,float size,Color c)
        {
            int n=v.currentVertCount;
            v.AddVert(new Vector3(p.x,p.y+size),c,Vector2.zero);
            v.AddVert(new Vector3(p.x+size,p.y),c,Vector2.right);
            v.AddVert(new Vector3(p.x,p.y-size),c,Vector2.one);
            v.AddVert(new Vector3(p.x-size,p.y),c,Vector2.up);
            v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
        public void SetPhase(Phase phase)
        {
            if(CurrentPhase==phase)return;
            CurrentPhase=phase;SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=rectTransform.rect;
            float w=r.width,h=r.height;
            if(w<40f||h<35f)return;
            Color contact=new Color(.08f,.055f,.028f,.46f);
            Fill(vh,new Rect(r.xMin+7f,r.yMin+1f,w-13f,h-9f),
                contact,new Color(.17f,.10f,.045f,.33f));
            Color low=CurrentPhase==Phase.InProgress?
                new Color(.80f,.748f,.604f,1f):new Color(.835f,.739f,.552f,1f);
            Color high=CurrentPhase==Phase.Unavailable?
                new Color(.882f,.82f,.702f,1f):new Color(.977f,.911f,.773f,1f);
            Fill(vh,new Rect(r.xMin+2f,r.yMin+8f,w-11f,h-15f),low,high);
            Fill(vh,new Rect(r.xMin+10f,r.yMin+14f,w-27f,h-27f),
                new Color(.953f,.863f,.694f,.80f),
                new Color(.996f,.942f,.827f,.86f));
            for(int i=0;i<12;i++)
            {
                float y=r.yMin+24f+i*Mathf.Max(9f,h/14f);
                if(y<r.yMax-19f)
                    Stripe(vh,new Rect(r.xMin+19f,y,w-49f,.55f),
                        new Color(.32f,.205f,.10f,.048f));
            }
            var brass=new Color(.66f,.435f,.194f,.73f);
            Stripe(vh,new Rect(r.xMin+11f,r.yMax-14f,w-30f,2f),brass);
            Stripe(vh,new Rect(r.xMin+11f,r.yMin+17f,w-30f,2f),brass);
            Stripe(vh,new Rect(r.xMin+10f,r.yMin+12f,2f,h-23f),brass);
            Stripe(vh,new Rect(r.xMax-16f,r.yMin+12f,2f,h-23f),brass);
            Color accent=CurrentPhase==Phase.InProgress?
                new Color(.43f,.52f,.335f,1f):
                CurrentPhase==Phase.Unavailable?new Color(.61f,.31f,.25f,1f):
                CurrentPhase==Phase.Council?new Color(.44f,.33f,.56f,1f):
                new Color(.68f,.455f,.215f,1f);
            Stripe(vh,new Rect(r.xMin+16f,r.yMax-10f,w-38f,3f),accent);
            for(int i=0;i<4;i++)
            {
                float x=i%2==0?r.xMin+15f:r.xMax-20f;
                float y=i<2?r.yMax-18f:r.yMin+23f;
                Diamond(vh,new Vector2(x,y),3.8f,new Color(.76f,.54f,.24f,1f));
            }
            if(TimerScale)
            {
                Stripe(vh,new Rect(r.xMin+19f,r.yMax-33f,2.5f,13f),accent);
                Stripe(vh,new Rect(r.xMax-23f,r.yMax-33f,2.5f,13f),accent);
            }
            else
            {
                // The actual 3D thumbnail occupies a physical inset, rather
                // than a disconnected dark rectangle on top of the interface.
                float socket=Mathf.Min(h-24f,w*.31f);
                Fill(vh,new Rect(r.xMin+13f,r.yMax-18f-socket,
                    socket+2f,socket+2f),
                    new Color(.29f,.19f,.10f,.35f),
                    new Color(.36f,.26f,.14f,.20f));
            }
        }
    }
}
