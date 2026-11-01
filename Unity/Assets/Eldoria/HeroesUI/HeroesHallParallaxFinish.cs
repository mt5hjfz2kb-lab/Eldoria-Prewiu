using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>
    /// True 2.5D architectural foreground, in front of the distant painted
    /// hall and behind every live hero card. Separate mesh, real perspective,
    /// independent lighting, no raycasts, no baked labels or fake heroes.
    /// Follows the previously certified RoyalGoldV3LayeredFinish pattern.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class HeroesHallParallaxFinish : MaskableGraphic
    {
        float nextFrame;
        public static HeroesHallParallaxFinish Install(RectTransform parent)
        {
            var go=new GameObject("Hall / 2.5D live architectural foreground",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(HeroesHallParallaxFinish));
            go.layer=parent.gameObject.layer;
            var rt=(RectTransform)go.transform;
            rt.SetParent(parent,false);
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
            rt.offsetMin=rt.offsetMax=Vector2.zero;
            var g=go.GetComponent<HeroesHallParallaxFinish>();
            g.color=Color.white;
            g.raycastTarget=false;
            return g;
        }
        void Update()
        {
            if(!isActiveAndEnabled||Time.unscaledTime<nextFrame)return;
            nextFrame=Time.unscaledTime+.16f;
            SetVerticesDirty();
        }
        static Color C(float r,float g,float b,float a)=>new Color(r,g,b,a);
        static void Q(VertexHelper vh,float x0,float y0,float x1,float y1,
            Color bottomLeft,Color bottomRight,Color topRight,Color topLeft)
        {
            if(x1<=x0||y1<=y0)return;
            int k=vh.currentVertCount;
            vh.AddVert(new Vector2(x0,y0),bottomLeft,Vector2.zero);
            vh.AddVert(new Vector2(x1,y0),bottomRight,Vector2.right);
            vh.AddVert(new Vector2(x1,y1),topRight,Vector2.one);
            vh.AddVert(new Vector2(x0,y1),topLeft,Vector2.up);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
        static void Fill(VertexHelper vh,float x0,float y0,float x1,float y1,Color c)
            =>Q(vh,x0,y0,x1,y1,c,c,c,c);
        static void Stroke(VertexHelper vh,Vector2 a,Vector2 b,float thickness,Color c)
        {
            Vector2 delta=b-a;
            if(delta.sqrMagnitude<.001f)return;
            Vector2 n=new Vector2(-delta.y,delta.x).normalized*(thickness*.5f);
            int k=vh.currentVertCount;
            vh.AddVert(a-n,c,Vector2.zero);
            vh.AddVert(a+n,c,Vector2.zero);
            vh.AddVert(b+n,c,Vector2.zero);
            vh.AddVert(b-n,c,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
        static void Tri(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color tint)
        {
            int k=vh.currentVertCount;
            vh.AddVert(a,tint,Vector2.zero);
            vh.AddVert(b,tint,Vector2.zero);
            vh.AddVert(c,tint,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);
        }
        static void Ember(VertexHelper vh,Vector2 center,float rad,float breathe)
        {
            Color inner=C(.97f,.62f,.22f,.11f+breathe*.075f);
            Color outer=C(.97f,.62f,.22f,0f);
            for(int j=0;j<16;j++){
                float a=j*Mathf.PI/8f,b=(j+1)*Mathf.PI/8f;
                Vector2 pa=center+new Vector2(Mathf.Cos(a)*rad,Mathf.Sin(a)*rad*1.7f);
                Vector2 pb=center+new Vector2(Mathf.Cos(b)*rad,Mathf.Sin(b)*rad*1.7f);
                int k=vh.currentVertCount;
                vh.AddVert(center,inner,Vector2.zero);
                vh.AddVert(pa,outer,Vector2.zero);
                vh.AddVert(pb,outer,Vector2.zero);
                vh.AddTriangle(k,k+1,k+2);
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=GetPixelAdjustedRect();
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            if(w<50||h<50)return;
            float t=Time.unscaledTime;
            float breathe=.5f+.5f*Mathf.Sin(t*.9f);
            var clear=C(0,0,0,0);
            var blue=C(.008f,.025f,.052f,.46f);
            var gold=C(.83f,.55f,.22f,.53f);
            var warm=C(1f,.74f,.39f,.13f+breathe*.045f);

            // Plane 1: distant chamber fades at top; do NOT cover its detail.
            Q(vh,x,y+h*.85f,x+w,y+h,
                clear,clear,C(.013f,.024f,.047f,.23f),C(.013f,.024f,.047f,.23f));

            // Plane 2: receding floor in separate geometry, with camera-style
            // perspective converging toward a far central vanishing point.
            float horizon=y+h*.46f,center=x+w*.5f;
            Q(vh,x,y,x+w,horizon,
                C(.012f,.016f,.031f,.21f),C(.012f,.016f,.031f,.21f),clear,clear);
            for(int i=-5;i<=5;i++){
                float bx=center+i*w*.165f;
                float hx=center+i*w*.027f;
                Stroke(vh,new Vector2(hx,horizon),new Vector2(bx,y),
                    Mathf.Max(.8f,w*.0017f),C(.68f,.43f,.17f,.16f));
            }
            for(int k=0;k<6;k++){
                float yy=y+h*(.03f+k*k*.012f);
                Stroke(vh,new Vector2(x,yy),new Vector2(x+w,yy),
                    Mathf.Max(.8f,h*.0022f),C(.75f,.51f,.23f,.10f));
            }

            // Plane 3: two large side pilasters and recessed mouldings
            // preserve the middle 82% for six genuine hero cards.
            float near=Mathf.Min(w*.069f,110f);
            Q(vh,x,y+h*.08f,x+near,y+h*.88f,
                C(.055f,.046f,.052f,.50f),C(.21f,.17f,.14f,.31f),
                C(.40f,.30f,.20f,.28f),C(.04f,.045f,.07f,.44f));
            Q(vh,x+w-near,y+h*.08f,x+w,y+h*.88f,
                C(.21f,.17f,.14f,.31f),C(.055f,.046f,.052f,.50f),
                C(.04f,.045f,.07f,.44f),C(.40f,.30f,.20f,.28f));
            Stroke(vh,new Vector2(x+near*.87f,y+h*.12f),
                new Vector2(x+near*.87f,y+h*.87f),
                Mathf.Max(1.5f,w*.002f),gold);
            Stroke(vh,new Vector2(x+w-near*.87f,y+h*.12f),
                new Vector2(x+w-near*.87f,y+h*.87f),
                Mathf.Max(1.5f,w*.002f),gold);
            // Architectural lintel is a real raised foreground plane.
            float lintel=y+h*.83f;
            Q(vh,x+near*.45f,lintel,x+w-near*.45f,lintel+h*.025f,
                C(.09f,.08f,.085f,.20f),C(.09f,.08f,.085f,.20f),
                C(.71f,.52f,.26f,.24f),C(.71f,.52f,.26f,.24f));

            // Two near translucent royal banners, authored independently
            // with gold hem and heraldic peak. NO texture with baked UI.
            for(int side=0;side<2;side++){
                float c=x+w*(side==0?.032f:.968f);
                float bw=Mathf.Min(w*.034f,55f);
                float top=y+h*.83f,bot=y+h*.57f;
                Q(vh,c-bw*.5f,bot,c+bw*.5f,top,
                    C(.021f,.057f,.13f,.48f),C(.022f,.068f,.16f,.52f),
                    C(.07f,.13f,.29f,.70f),C(.05f,.10f,.23f,.70f));
                Tri(vh,new Vector2(c-bw*.5f,bot),
                    new Vector2(c+bw*.5f,bot),
                    new Vector2(c,bot-h*.035f),C(.02f,.06f,.15f,.59f));
                Stroke(vh,new Vector2(c-bw*.47f,top-h*.01f),
                    new Vector2(c+bw*.47f,top-h*.01f),
                    Mathf.Max(1.2f,bw*.085f),gold);
                Ember(vh,new Vector2(c,y+h*.44f),Mathf.Min(w,h)*.10f,breathe);
            }

            // Plane 4: atmospheric rays and smoke, subtly animated and
            // disjoint from the stage controls/hit targets.
            float drift=Mathf.Sin(t*.29f)*w*.016f;
            Q(vh,x+w*.10f+drift,y+h*.34f,x+w*.24f+drift,y+h*.82f,
                clear,clear,warm,clear);
            Q(vh,x+w*.76f-drift,y+h*.34f,x+w*.90f-drift,y+h*.82f,
                clear,clear,clear,warm);
            Q(vh,x,y,x+w,y+h*.13f,
                blue,blue,clear,clear);
        }
    }
}
