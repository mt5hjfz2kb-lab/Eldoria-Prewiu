using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Original, resolution-independent engraved envelope seal for the real
    /// mailbox's empty state. Draws only when the canonical empty-state Text
    /// is active. No fake messages, counters, lore, or UI screenshot.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchEmptyMailSeal : MaskableGraphic
    {
        public bool Compact {get;set;}
        static readonly Color Gold=new Color(.82f,.60f,.31f,.89f);
        static readonly Color Bright=new Color(.96f,.79f,.48f,.90f);
        static readonly Color Shadow=new Color(.16f,.12f,.10f,.93f);
        static readonly Color Slate=new Color(.095f,.083f,.080f,.95f);
        static readonly Color Burgundy=new Color(.34f,.095f,.13f,.96f);

        public static BreachWatchEmptyMailSeal Install(RectTransform emptyState)
        {
            if(!emptyState)return null;
            var go=new GameObject("Vigilia / empty genuine mail wax seal",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(BreachWatchEmptyMailSeal));
            go.layer=emptyState.gameObject.layer;
            var rt=(RectTransform)go.transform;
            rt.SetParent(emptyState,false);
            rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);
            // The real empty-category sentence is centred inside this
            // RectTransform. The previous 104-unit icon offset caused the
            // gold ring to touch the text at 900x1600. Use a deliberately
            // smaller seal with a generous visual break; no second message.
            float headroom=Mathf.Clamp(emptyState.rect.height*.31f,112f,172f);
            rt.anchoredPosition=new Vector2(0,headroom);
            rt.sizeDelta=new Vector2(120f,90f);
            var art=go.GetComponent<BreachWatchEmptyMailSeal>();
            art.raycastTarget=false;
            art.color=Color.white;
            return art;
        }

        static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Color c)
        {
            int k=vh.currentVertCount;
            vh.AddVert(new Vector2(a.x,a.y),c,Vector2.zero);
            vh.AddVert(new Vector2(b.x,a.y),c,Vector2.right);
            vh.AddVert(new Vector2(b.x,b.y),c,Vector2.one);
            vh.AddVert(new Vector2(a.x,b.y),c,Vector2.up);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }

        static void Line(VertexHelper vh,Vector2 a,Vector2 b,float weight,Color c)
        {
            Vector2 delta=b-a;
            if(delta.sqrMagnitude<.01f)return;
            Vector2 n=new Vector2(-delta.y,delta.x).normalized*(weight*.5f);
            int k=vh.currentVertCount;
            vh.AddVert(a-n,c,Vector2.zero);
            vh.AddVert(a+n,c,Vector2.zero);
            vh.AddVert(b+n,c,Vector2.one);
            vh.AddVert(b-n,c,Vector2.one);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }

        static void Circle(VertexHelper vh,Vector2 center,float radius,
            float width,Color c)
        {
            for(int i=0;i<40;i++)
            {
                float a=(i/40f)*Mathf.PI*2f;
                float b=((i+1)/40f)*Mathf.PI*2f;
                Line(vh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,
                    center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,
                    width,c);
            }
        }

        static void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color tint)
        {
            int k=vh.currentVertCount;
            vh.AddVert(a,tint,Vector2.zero);
            vh.AddVert(b,tint,Vector2.zero);
            vh.AddVert(c,tint,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect=GetPixelAdjustedRect();
            if(Compact)
            {
                // Designed for the 37-unit mailbox entry: broad envelope,
                // solid metal rim and readable folds without miniature text.
                float unit=Mathf.Min(rect.width,rect.height)/40f;
                Vector2 Q(float x,float y)=>rect.center+new Vector2(x,y)*unit;
                Circle(vh,Q(0,0),8f*unit,16f*unit,Slate);
                Circle(vh,Q(0,0),18f*unit,2f*unit,Gold);
                Circle(vh,Q(0,0),16.3f*unit,.8f*unit,Bright);
                Quad(vh,Q(-13,-8),Q(13,9),Gold);
                Quad(vh,Q(-11.5f,-6.5f),Q(11.5f,7.5f),Shadow);
                Line(vh,Q(-11.5f,7.5f),Q(0,-1),1.2f*unit,Bright);
                Line(vh,Q(11.5f,7.5f),Q(0,-1),1.2f*unit,Bright);
                Line(vh,Q(-11.5f,-6.5f),Q(-3,0),1f*unit,Gold);
                Line(vh,Q(11.5f,-6.5f),Q(3,0),1f*unit,Gold);
                Circle(vh,Q(0,-1),2.6f*unit,4f*unit,Burgundy);
                Circle(vh,Q(0,-1),2.6f*unit,.8f*unit,Bright);
                return;
            }
            float s=Mathf.Min(rect.width/146f,rect.height/108f);
            float px=rect.center.x,py=rect.center.y;
            Vector2 P(float x,float y)=>new Vector2(px+x*s,py+y*s);
            // Engraved backing medallion: metal ring, warm bevel and ash
            // shadow. The envelope is dimensional, not a flat icon glyph.
            Circle(vh,P(0,0),49f*s,2.0f*s,Gold);
            Circle(vh,P(0,0),43f*s,.85f*s,new Color(.96f,.83f,.55f,.43f));
            Circle(vh,P(0,0),55f*s,.85f*s,new Color(.58f,.43f,.28f,.19f));

            Quad(vh,P(-50,-28),P(50,27),Shadow);
            Quad(vh,P(-48,-26),P(48,28),Gold);
            Quad(vh,P(-45,-23),P(45,25),Slate);
            Line(vh,P(-45,25),P(0,-6),1.8f*s,Bright);
            Line(vh,P(45,25),P(0,-6),1.8f*s,Bright);
            Line(vh,P(-45,-23),P(-3,2),1.25f*s,Gold);
            Line(vh,P(45,-23),P(3,2),1.25f*s,Gold);
            Line(vh,P(-45,25),P(45,25),.9f*s,
                new Color(.98f,.83f,.58f,.77f));
            Line(vh,P(-45,-23),P(45,-23),.85f*s,Shadow);

            // Real wax-like heraldic seal at the flap intersection.
            Circle(vh,P(0,-5),13.5f*s,7f*s,Burgundy);
            Circle(vh,P(0,-5),13.5f*s,1.5f*s,Bright);
            Circle(vh,P(0,-5),9f*s,.85f*s,Gold);
            // Small crown: Valoria's heraldry, not an unopened mail badge.
            Line(vh,P(-5,-8),P(-5,-3),1.4f*s,Bright);
            Line(vh,P(-5,-3),P(0,-6),1.4f*s,Bright);
            Line(vh,P(0,-6),P(5,-3),1.4f*s,Bright);
            Line(vh,P(5,-3),P(5,-8),1.4f*s,Bright);
            Line(vh,P(-5,-8),P(5,-8),1.9f*s,Bright);
            for(int i=-1;i<=1;i++)
                Circle(vh,P(i*5,-2.5f),.75f*s,.95f*s,Bright);

            // One subtle gilded rule each side; live empty message remains
            // the only text and is still owned by Royal mailbox controller.
            Line(vh,P(-70,-46),P(-37,-46),1f*s,Gold);
            Line(vh,P(37,-46),P(70,-46),1f*s,Gold);
            Triangle(vh,P(-35,-46),P(-32,-48),P(-32,-44),Gold);
            Triangle(vh,P(35,-46),P(32,-48),P(32,-44),Gold);
        }
    }
}
