using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// A single art-directed engraved manuscript vignette, NOT a fake message
    /// or an image of a whole interface. Drawn directly into the real mailbox
    /// with smooth native vector geometry; no enlarged 128px item icons,
    /// downloaded textures, input handlers, text, or duplicate old Canvas.
    /// Activates ONLY when the authoritative mailbox category has no mail.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchMailboxEmptyArtwork : MaskableGraphic
    {
        public static BreachWatchMailboxEmptyArtwork Install(RectTransform window)
        {
            var go=new GameObject("Vigilia / illuminated empty post folio",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(BreachWatchMailboxEmptyArtwork));
            go.layer=window.gameObject.layer;
            var rt=(RectTransform)go.transform;
            rt.SetParent(window,false);
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
            rt.offsetMin=rt.offsetMax=Vector2.zero;
            // Above one continuous stone folio, behind ALL live labels,
            // categories, report panels and actual mailbox buttons.
            rt.SetSiblingIndex(1);
            var art=go.GetComponent<BreachWatchMailboxEmptyArtwork>();
            art.raycastTarget=false;
            art.color=Color.white;
            return art;
        }

        static Color C(float r,float g,float b,float a=1f)=>
            new Color(r,g,b,a);
        static readonly Color Shadow=C(.050f,.036f,.032f,.93f);
        static readonly Color Deep=C(.105f,.082f,.070f,.96f);
        static readonly Color Bronze=C(.53f,.35f,.18f,.83f);
        static readonly Color Gold=C(.79f,.57f,.30f,.93f);
        static readonly Color Light=C(.99f,.81f,.46f,.85f);
        static readonly Color Paper=C(.64f,.49f,.32f,.96f);
        static readonly Color Parchment=C(.83f,.71f,.54f,.93f);
        static readonly Color Wine=C(.37f,.082f,.105f,.99f);

        static void Quad(VertexHelper vh,Vector2 a,Vector2 b,
            Color leftBottom,Color rightBottom,Color rightTop,Color leftTop)
        {
            int k=vh.currentVertCount;
            vh.AddVert(new Vector2(a.x,a.y),leftBottom,Vector2.zero);
            vh.AddVert(new Vector2(b.x,a.y),rightBottom,Vector2.right);
            vh.AddVert(new Vector2(b.x,b.y),rightTop,Vector2.one);
            vh.AddVert(new Vector2(a.x,b.y),leftTop,Vector2.up);
            vh.AddTriangle(k,k+1,k+2);
            vh.AddTriangle(k,k+2,k+3);
        }
        static void Rect(VertexHelper vh,Vector2 a,Vector2 b,Color c)
            =>Quad(vh,a,b,c,c,c,c);
        static void Tri(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,
            Color c0,Color c1,Color c2)
        {
            int k=vh.currentVertCount;
            vh.AddVert(a,c0,Vector2.zero);
            vh.AddVert(b,c1,Vector2.right);
            vh.AddVert(c,c2,Vector2.one);
            vh.AddTriangle(k,k+1,k+2);
        }
        static void Stroke(VertexHelper vh,Vector2 from,Vector2 to,
            float width,Color c)
        {
            Vector2 direction=to-from;
            if(direction.sqrMagnitude<.001f)return;
            Vector2 n=new Vector2(-direction.y,direction.x).normalized*
                (Mathf.Max(.5f,width)*.5f);
            QuadPoints(vh,from-n,from+n,to+n,to-n,c);
        }
        static void QuadPoints(VertexHelper vh,Vector2 a,Vector2 b,
            Vector2 c,Vector2 d,Color tint)
        {
            int k=vh.currentVertCount;
            vh.AddVert(a,tint,Vector2.zero);
            vh.AddVert(b,tint,Vector2.right);
            vh.AddVert(c,tint,Vector2.one);
            vh.AddVert(d,tint,Vector2.up);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
        static void Ring(VertexHelper vh,Vector2 center,float rx,float ry,
            float width,Color c,int steps=48)
        {
            for(int i=0;i<steps;i++)
            {
                float a=i*2f*Mathf.PI/steps;
                float b=(i+1)*2f*Mathf.PI/steps;
                var aPoint=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);
                var bPoint=center+new Vector2(Mathf.Cos(b)*rx,Mathf.Sin(b)*ry);
                Stroke(vh,aPoint,bPoint,width,c);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=GetPixelAdjustedRect();
            float width=r.width,height=r.height;
            // On short landscape phones the actual message, tabs, pagination
            // and close action already occupy the available vertical space.
            if(width<285f||height<530f||width>height)return;
            float factor=Mathf.Min(width/900f,height/1600f);
            factor=Mathf.Clamp(factor,.38f,1.65f);
            float cx=r.center.x;
            float cy=r.yMin+height*.495f;
            Vector2 P(float x,float y)=>new Vector2(cx+x*factor,cy+y*factor);
            float k=factor;

            // Shadowed archival alcove: one illustration behind the existing
            // honest empty-state text. This is part of the stone wall, not a
            // second rectangular panel pasted onto it.
            for(int i=3;i>=0;i--)
            {
                float spread=1f+i*.11f;
                Color haze=C(.55f,.35f,.15f,.012f+(3-i)*.007f);
                Ring(vh,P(0,8),310f*spread*k,255f*spread*k,
                    (18f+i*5f)*k,haze,52);
            }
            Ring(vh,P(0,9),302*k,252*k,2.0f*k,C(.58f,.42f,.23f,.15f));
            Ring(vh,P(0,9),287*k,237*k,1.1f*k,C(.85f,.64f,.33f,.12f));

            // Recessed, perspective-cut stone desk with a pair of delicate
            // low-contrast gilded bevels. The illustration sits ON this desk
            // rather than floating in an empty charcoal void.
            Tri(vh,P(-350,-178),P(350,-178),P(0,-247),
                C(.20f,.145f,.11f,.40f),C(.20f,.145f,.11f,.40f),
                C(.05f,.045f,.043f,.06f));
            Stroke(vh,P(-350,-178),P(350,-178),2.3f*k,
                C(.66f,.48f,.26f,.36f));
            Stroke(vh,P(-312,-188),P(312,-188),.8f*k,
                C(.99f,.78f,.38f,.19f));

            // Manuscript envelope: deliberately larger than a UI button but
            // no blurry texture enlargement. Matte parchment, emboss, flap,
            // metal rim and burgundy seal share one physically coherent light.
            Quad(vh,P(-197,-101),P(208,119),Shadow,Shadow,Shadow,Shadow);
            Quad(vh,P(-203,-91),P(196,124),
                C(.46f,.31f,.18f,.98f),C(.57f,.41f,.23f,.98f),
                C(.92f,.78f,.52f,.98f),C(.77f,.64f,.44f,.98f));
            Quad(vh,P(-195,-83),P(189,114),
                C(.71f,.57f,.37f,.99f),C(.76f,.62f,.41f,.99f),
                Parchment,C(.89f,.76f,.55f,.99f));
            Stroke(vh,P(-203,-91),P(196,-91),4*k,Deep);
            Stroke(vh,P(-203,124),P(196,124),3*k,Light);
            Stroke(vh,P(-203,-91),P(-203,124),3*k,Bronze);
            Stroke(vh,P(196,-91),P(196,124),3*k,Gold);

            // Real folded edges and paper thickness; offset fold highlights
            // avoid a diagram-like outline and give the emblem depth.
            Tri(vh,P(-192,110),P(184,110),P(-4,-21),
                C(.96f,.83f,.62f,.93f),C(.89f,.73f,.51f,.97f),
                C(.55f,.39f,.22f,.96f));
            Tri(vh,P(-192,-77),P(-4,-16),P(184,-77),
                C(.70f,.54f,.34f,.89f),C(.92f,.75f,.50f,.96f),
                C(.71f,.56f,.35f,.89f));
            Stroke(vh,P(-188,109),P(-4,-18),2.0f*k,C(.62f,.43f,.23f,.67f));
            Stroke(vh,P(182,109),P(-4,-18),2.0f*k,C(.92f,.73f,.42f,.67f));
            Stroke(vh,P(-185,-76),P(-3,-13),1.2f*k,C(.39f,.27f,.17f,.48f));
            Stroke(vh,P(183,-76),P(-3,-13),1.2f*k,C(.98f,.77f,.49f,.45f));

            // Metal-and-wax heraldic seal. Multilayer relief is visible at
            // phone density, with a crown, not an invented notification count.
            Ring(vh,P(-4,-10),40*k,40*k,13*k,C(.19f,.10f,.07f,.91f));
            Ring(vh,P(-4,-10),36*k,36*k,13*k,Wine);
            Ring(vh,P(-4,-10),30*k,30*k,3.8f*k,Gold);
            Ring(vh,P(-4,-10),24*k,24*k,1.1f*k,Light);
            Stroke(vh,P(-18,-14),P(-15,3),2.3f*k,Light);
            Stroke(vh,P(-15,3),P(-4,-5),2.3f*k,Light);
            Stroke(vh,P(-4,-5),P(8,4),2.3f*k,Light);
            Stroke(vh,P(8,4),P(12,-14),2.3f*k,Light);
            Stroke(vh,P(-18,-14),P(12,-14),3.4f*k,Light);
            for(int i=-1;i<=1;i++)
                Ring(vh,P(-3+i*13,4),2.1f*k,2.1f*k,1.9f*k,Light,12);

            // A fountain quill grounded beside the sealed document. Feather
            // uses multiple separately graded vanes (not a 2D image flattened
            // over fake correspondence).
            Vector2 base0=P(220,-144),tip=P(311,194);
            Stroke(vh,P(222,-147),P(303,176),5.7f*k,
                C(.11f,.10f,.09f,.90f));
            Stroke(vh,P(223,-142),P(307,189),3.1f*k,Gold);
            for(int i=0;i<11;i++)
            {
                float t=i/11f;
                Vector2 shaft=Vector2.Lerp(P(247,-36),P(306,185),t);
                float radius=45f*(1f-t)*k;
                Vector2 left=shaft+new Vector2(-radius,16f*k*(1f-t));
                Vector2 right=shaft+new Vector2(radius*.77f,-6f*k*(1f-t));
                Tri(vh,shaft,left,
                    shaft+new Vector2(0,24f*k),Gold,
                    C(.67f,.52f,.32f,.74f),C(.94f,.76f,.49f,.81f));
                Tri(vh,shaft,right,
                    shaft+new Vector2(0,24f*k),
                    C(.82f,.64f,.38f,.80f),C(.39f,.31f,.23f,.72f),
                    C(.93f,.74f,.45f,.73f));
            }
            Tri(vh,P(222,-147),P(216,-127),P(229,-130),
                Light,Gold,Deep);

            // Small, subtle symmetric flourishes tie the one-piece art to
            // Vigilia's existing gilded chapter separators.
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*226f;
                Stroke(vh,P(x,-158),P(x+side*101f,-158),1.7f*k,
                    C(.83f,.63f,.33f,.41f));
                Stroke(vh,P(x+side*101f,-158),P(x+side*109f,-150),
                    1.4f*k,C(.85f,.61f,.31f,.36f));
                Ring(vh,P(x,-158),3.2f*k,3.2f*k,1.2f*k,
                    C(.99f,.81f,.43f,.52f),12);
            }
        }
    }
}
