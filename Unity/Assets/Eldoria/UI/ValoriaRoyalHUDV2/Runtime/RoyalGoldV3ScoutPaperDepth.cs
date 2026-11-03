using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Scout contract material stack: each parchment casts its own contact
    /// shadow onto the real map, and its already-authored mission thumbnail sits
    /// in a shallow, recessed and edge-lit picture window. This never paints
    /// mission words/pictures, never creates a Canvas/EventSystem and never
    /// handles a pointer. The canonical ScoutOrder controls all visible states.
    ///
    /// Eldoria art method: separate desk, paper, illustrated inset and diegetic
    /// seals as independent non-interactive layers; do not duplicate baked
    /// imagery, tint the whole scene, or add fake missions/rewards.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3ScoutPaperDepth : MaskableGraphic
    {
        public enum MaterialLayer { DeskContact, PaperBevel, InsetUnderlay, InsetLip }
        public MaterialLayer Role;
        public int Kind { get; private set; }
        public bool InProgress { get; private set; }
        public bool Completed { get; private set; }
        public override Texture mainTexture => Texture2D.whiteTexture;

        public static RoyalGoldV3ScoutPaperDepth Install(
            RectTransform parent, string name, MaterialLayer role, int kind)
        {
            var go = new GameObject(name,typeof(RectTransform),
                typeof(CanvasRenderer),typeof(RoyalGoldV3ScoutPaperDepth));
            go.layer=parent.gameObject.layer;
            var r=(RectTransform)go.transform;
            r.SetParent(parent,false);
            r.anchorMin=Vector2.zero;
            r.anchorMax=Vector2.one;
            r.offsetMin=r.offsetMax=Vector2.zero;
            var component=go.GetComponent<RoyalGoldV3ScoutPaperDepth>();
            component.Role=role;
            component.Kind=Mathf.Clamp(kind,0,3);
            component.raycastTarget=false;
            component.color=Color.white;
            return component;
        }

        public void SetLiveState(int kind,bool underway,bool complete)
        {
            kind=Mathf.Clamp(kind,0,3);
            if(Kind==kind && InProgress==underway && Completed==complete)return;
            Kind=kind;
            InProgress=underway;
            Completed=complete;
            SetVerticesDirty();
        }

        static Color Ink(float r,float g,float b,float a)
            => new Color(r,g,b,a);

        static void Quad(VertexHelper vh,Vector2 bottomLeft,
            Vector2 bottomRight,Vector2 topRight,Vector2 topLeft,
            Color bottom,Color top)
        {
            int k=vh.currentVertCount;
            vh.AddVert(bottomLeft,bottom,Vector2.zero);
            vh.AddVert(bottomRight,bottom,Vector2.zero);
            vh.AddVert(topRight,top,Vector2.zero);
            vh.AddVert(topLeft,top,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);
            vh.AddTriangle(k,k+2,k+3);
        }
        static void Box(VertexHelper v,float x,float y,float w,float h,
            Color lo,Color hi)
        {
            if(w<=.1f||h<=.1f)return;
            Quad(v,new Vector2(x,y),new Vector2(x+w,y),
                new Vector2(x+w,y+h),new Vector2(x,y+h),lo,hi);
        }
        static void Contour(VertexHelper v,Rect r,float dx,float dy,
            Color bottom,Color top)
        {
            // Subtle irregular cuts, not another hard rectangular photo.
            float l=r.xMin+dx,t=r.yMax+dy,ri=r.xMax+dx,b=r.yMin+dy;
            int k=v.currentVertCount;
            var points=new [] {
                new Vector2(l+9f,b+3f),new Vector2(ri-11f,b+1f),
                new Vector2(ri-2f,b+8f),new Vector2(ri-3f,t-11f),
                new Vector2(ri-13f,t-2f),new Vector2(l+8f,t-3f),
                new Vector2(l+3f,t-12f),new Vector2(l+2f,b+11f)};
            Vector2 center=Vector2.zero;
            foreach(var p in points)center+=p;
            center/=points.Length;
            vhGuard(v,k,center,Color.Lerp(bottom,top,.48f));
            for(int i=0;i<points.Length;i++)
                vhGuard(v,k+i+1,points[i],
                    Color.Lerp(bottom,top,Mathf.InverseLerp(b,t,points[i].y)));
            for(int i=0;i<points.Length;i++)
                v.AddTriangle(k,k+i+1,k+(i+1)%points.Length+1);
        }
        static void vhGuard(VertexHelper v,int unused,Vector2 xy,Color c)
        {
            v.AddVert(xy,c,Vector2.zero);
        }

        static void MedallionSeat(VertexHelper v,Vector2 center,float radius)
        {
            int k=v.currentVertCount;
            v.AddVert(center+new Vector2(1.5f,-2.5f),
                Ink(.09f,.07f,.06f,.43f),Vector2.zero);
            for(int n=0;n<=22;n++)
            {
                float t=n*Mathf.PI*2f/22f;
                var at=center+new Vector2(
                    Mathf.Cos(t)*radius,Mathf.Sin(t)*radius*.72f);
                v.AddVert(at,Ink(.05f,.04f,.03f,.03f),Vector2.zero);
                if(n>0)v.AddTriangle(k,k+n,k+n+1);
            }
        }

        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<52f||r.height<37f)return;
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            // Natural materials respond differently by mission family.
            Color edge=Kind==0?Ink(.35f,.28f,.19f,1f):
                Kind==1?Ink(.30f,.30f,.19f,1f):
                Kind==2?Ink(.38f,.25f,.15f,1f):
                        Ink(.28f,.24f,.32f,1f);
            Color lip=Kind==0?Ink(.85f,.69f,.39f,.77f):
                Kind==1?Ink(.77f,.70f,.43f,.77f):
                Kind==2?Ink(.84f,.58f,.31f,.77f):
                        Ink(.73f,.59f,.79f,.77f);
            if(Completed)lip=Ink(.71f,.81f,.49f,.82f);
            else if(InProgress)lip=Ink(.59f,.68f,.86f,.80f);

            if(Role==MaterialLayer.DeskContact)
            {
                // Four offset penumbra contours and the thick paper's lower
                // edge make an object resting on the painted cartographer desk.
                // All drawn behind the original nine-sliced parchment sprite.
                for(int i=4;i>=1;i--)
                {
                    float dx=2f+i*2.2f,dy=-1f-i*2.5f;
                    float a=.045f+i*.036f;
                    Contour(v,r,dx,dy,Ink(.015f,.012f,.009f,a),
                        Ink(.045f,.028f,.014f,a*.48f));
                }
                // B2 macro material: a dark, visible leather portfolio sits
                // BELOW each real paper sheet, not a flat overlay on the map.
                // Its real protruding edge and shadow change the silhouette.
                float side=-5.6f+(Kind-1)*1.2f;
                Contour(v,r,side,-7.7f,
                    Ink(.075f,.051f,.036f,.92f),
                    Ink(.23f,.145f,.075f,.79f));
                Box(v,x+side+1f,y+16f,4.5f,h-34f,
                    Ink(edge.r,edge.g,edge.b,.96f),
                    Ink(.64f,.42f,.20f,.68f));
                Box(v,x+15f,y-9f,w-30f,5f,
                    Ink(.085f,.06f,.04f,.86f),
                    Ink(.25f,.17f,.09f,.82f));
                // Actual paper thickness: warmer upper lip, dark lower face.
                Box(v,x+13f,y-5.1f,w-25f,7.5f,
                    Ink(.22f,.13f,.07f,.89f),Ink(.59f,.40f,.23f,.68f));
                Box(v,x+15f,y-1.4f,w-29f,2f,
                    Ink(.92f,.76f,.49f,.68f),Ink(.92f,.76f,.49f,.34f));
                Box(v,x+w-7f,y+12f,5.5f,h-26f,
                    Ink(.28f,.18f,.09f,.45f),Ink(.54f,.32f,.16f,.22f));
                // A low-relief leather binding ledge, varied by family, sits
                // underneath the paper without affecting the live ink/labels.
                Box(v,x+11f,y+h-9f,w-22f,3f,
                    Ink(edge.r,edge.g,edge.b,.27f),
                    Ink(lip.r,lip.g,lip.b,.23f));
                return;
            }

            if(Role==MaterialLayer.PaperBevel)
            {
                // Upper and lower rolled-paper edges have deliberately
                // DIFFERENT physical responses to the cartographer lamp.
                // Thin enough to leave all live ink/reward glyphs readable.
                float thickness=Mathf.Clamp(Mathf.Min(w,h)*.033f,2.5f,8.5f);
                Box(v,x+17f,y+h-thickness-3f,w-34f,thickness,
                    Ink(.75f,.52f,.26f,.10f),
                    Ink(1f,.85f,.53f,.40f));
                Box(v,x+17f,y+3f,w-34f,thickness*.83f,
                    Ink(.27f,.15f,.08f,.37f),
                    Ink(.60f,.42f,.24f,.10f));
                Box(v,x+4f,y+14f,thickness*.62f,h-28f,
                    Ink(.71f,.46f,.21f,.24f),
                    Ink(1f,.89f,.62f,.18f));
                Box(v,x+w-7f,y+15f,thickness*.63f,h-30f,
                    Ink(.22f,.12f,.065f,.36f),
                    Ink(.74f,.50f,.25f,.18f));
                // Each real embossed medallion now owns a contact shadow,
                // centred on the original live/painted seal geometry.
                float radius=Mathf.Clamp(w*.070f,17f,30f);
                MedallionSeat(v,new Vector2(x+w*.5f,y+h-22f),radius+8f);
                // The same upper edge gleams differently by mission family;
                // a subtle coloured bookbinder's thread, not a new ability.
                Box(v,x+16f,y+h-9f,w-32f,1.6f,
                    Ink(lip.r,lip.g,lip.b,.50f),
                    Ink(lip.r,lip.g,lip.b,.28f));
                return;
            }

            if(Role==MaterialLayer.InsetUnderlay)
            {
                // Deep, separate backing below the authentic pictorial scene;
                // it never rescales/retouches that existing RawImage texture.
                Box(v,x+3f,y-3f,w-2f,h-1f,
                    Ink(.055f,.042f,.033f,.53f),Ink(.055f,.04f,.03f,.23f));
                Box(v,x,y+1f,w,h-1f,
                    Ink(edge.r,edge.g,edge.b,.91f),
                    Ink(.51f,.38f,.24f,.91f));
                Box(v,x+4f,y+4f,w-8f,h-8f,
                    Ink(.09f,.07f,.055f,.98f),
                    Ink(.25f,.18f,.105f,.98f));
                Box(v,x+7f,y+7f,w-14f,h-14f,
                    Ink(.27f,.23f,.18f,.66f),
                    Ink(.055f,.047f,.039f,.65f));
                return;
            }

            if(Role==MaterialLayer.InsetLip)
            {
                // Recessed viewport: physically different shadowed top/left,
                // gilded lower/right edges, not a uniform outline frame.
                float border=Mathf.Clamp(Mathf.Min(w,h)*.055f,2f,5.5f);
                Box(v,x,y+h-border,w,border,
                    Ink(.13f,.09f,.048f,.71f),Ink(.025f,.022f,.018f,.68f));
                Box(v,x,y,border,h,
                    Ink(.065f,.045f,.029f,.60f),Ink(.18f,.13f,.065f,.72f));
                Box(v,x,y,w,border,
                    Ink(lip.r,lip.g,lip.b,.78f),
                    Ink(.59f,.37f,.21f,.51f));
                Box(v,x+w-border,y,border,h,
                    Ink(lip.r,lip.g,lip.b,.59f),Ink(.80f,.65f,.35f,.47f));
                // Corner fittings actually sit above the painted inset.
                float c=Mathf.Clamp(border*.83f,1.5f,3.5f);
                Box(v,x-1f,y-1f,c*2f,c*2f,lip,lip);
                Box(v,x+w-c*2f+1f,y-1f,c*2f,c*2f,lip,lip);
                Box(v,x-1f,y+h-c*2f+1f,c*2f,c*2f,lip,lip);
                Box(v,x+w-c*2f+1f,y+h-c*2f+1f,c*2f,c*2f,lip,lip);
            }
        }
    }
}
