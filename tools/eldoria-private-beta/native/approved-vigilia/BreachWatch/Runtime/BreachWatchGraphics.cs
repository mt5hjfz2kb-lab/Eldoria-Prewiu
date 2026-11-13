using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // Entirely new vector geometry. No RoyalHUD/Forja sprites, plates or layout.
    // Independent UI meshes remain crisp at any physical resolution.
    // Own original raster mineral materials generated once, not the flat
    // geometric surfaces used by the rejected native concept.
    // Uses Unity's real nine-slice renderer and independent raycast surfaces.
    public sealed class BreachWatchPlate : Image
    {
        public enum PlateType { Basalt, Obsidian, Resource, Action, Selected, Dangerous, Navigation, Portrait, Banner }
        PlateType plateType = PlateType.Basalt;
        Sprite approvedIndependentArt;
        bool useIndependentNineSlice;
        bool acceptsPointer;
        // Some authored controls are built below a deliberately inactive rail.
        // Their Awake runs only at first real unlock; retain the input intent
        // configured by the owning button before that activation.
        public void SetPointerInput(bool enabled)
        {
            acceptsPointer=enabled;
            raycastTarget=enabled;
        }
        public new PlateType Type
        {
            get => plateType;
            set
            {
                if(plateType == value && sprite) return;
                plateType = value;
                RefreshMaterial();
            }
        }
        public bool CopperEdge = true;
        float visualScale=1f;
        // Shrink only PAINTED vertices while retaining an accessible native
        // hit target. The approved medallions are small floating metalwork,
        // not five oversized round UI buttons swallowing the scene.
        public float VisualScale
        {
            get => visualScale;
            set
            {
                float next=Mathf.Clamp(value,.50f,1f);
                if(Mathf.Abs(next-visualScale)<.001f)return;
                visualScale=next;
                SetVerticesDirty();
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            if(visualScale>.999f)return;
            var center=GetPixelAdjustedRect().center;
            Vector3 pivot=new Vector3(center.x,center.y,0f);
            UIVertex v=default;
            for(int i=0;i<vh.currentVertCount;i++)
            {
                vh.PopulateUIVertex(ref v,i);
                v.position=pivot+(v.position-pivot)*visualScale;
                vh.SetUIVertex(v,i);
            }
        }
        public bool UsesOwnerReference => approvedIndependentArt && sprite==approvedIndependentArt;
        // These are five new sprites derived from the separately approved
        // concept. They belong to this canvas, not the old game HUD.
        public void ApplyIndependentArt(Sprite art,bool nineSlice=false)
        {
            approvedIndependentArt=art;
            useIndependentNineSlice=nineSlice;
            RefreshMaterial();
        }
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = acceptsPointer;
            RefreshMaterial();
        }
        public void RefreshMaterial()
        {
            var next = approvedIndependentArt
                ? approvedIndependentArt
                : BreachWatchMaterialLibrary.For(plateType);
            if(next != sprite) sprite = next;
            bool medallion = approvedIndependentArt ||
                plateType==PlateType.Navigation || plateType==PlateType.Selected;
            bool isolatedBanner = plateType==PlateType.Banner;
            type = useIndependentNineSlice ? Image.Type.Sliced :
                medallion || isolatedBanner ? Image.Type.Simple : Image.Type.Sliced;
            preserveAspect = !useIndependentNineSlice && (medallion || isolatedBanner);
            color = plateType==PlateType.Selected
                ? new Color(1f,.86f,.77f,1f)
                : plateType==PlateType.Dangerous
                    ? new Color(.63f,.60f,.57f,1f)
                    : Color.white;
        }
    }

    public sealed class BreachWatchGlyph : MaskableGraphic
    {
        public enum Sigil { Realm, Build, Army, World, Chronicle, Wood, Stone, Food, Bell, Settings, Cross, Breach, Hammer, Arrow, Castle }
        public Sigil Type;
        public Color Ink=new Color(.82f,.73f,.59f,1);
        protected override void Awake() { base.Awake(); raycastTarget=false; }
        static void Edge(VertexHelper vh,Vector2 a,Vector2 b,float width,Color color)
        {
            var d=b-a;if(d.sqrMagnitude<.00001f)return;
            var n=new Vector2(-d.y,d.x).normalized*width*.5f;
            int o=vh.currentVertCount;
            vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(a-n,color,Vector2.zero);
            vh.AddVert(b-n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);
            vh.AddTriangle(o,o+1,o+2);vh.AddTriangle(o,o+2,o+3);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=GetPixelAdjustedRect();float size=Mathf.Min(r.width,r.height)*.36f;
            Vector2 center=r.center;float thick=Mathf.Max(1.7f,size*.115f);
            void Stroke(float x,float y,float xx,float yy,float w=1)
            {
                var p=center+new Vector2(x,y)*size;var q=center+new Vector2(xx,yy)*size;
                // Three-tier carved stroke: engraved shadow, reflected edge
                // and aged-metal face. Not a flat, single-color pictogram.
                Edge(vh,p+new Vector2(1.5f,-1.4f),q+new Vector2(1.5f,-1.4f),
                    thick*w*1.34f,new Color(.01f,.025f,.03f,.95f));
                Edge(vh,p+new Vector2(-.75f,1),q+new Vector2(-.75f,1),
                    thick*w*.74f,new Color(Ink.r*.8f+.18f,Ink.g*.75f+.14f,Ink.b*.62f+.09f,Ink.a*.9f));
                Edge(vh,p,q,thick*w*.70f,Ink);
            }
            void Path(params Vector2[] pts){for(int i=0;i<pts.Length-1;i++)Stroke(pts[i].x,pts[i].y,pts[i+1].x,pts[i+1].y);}
            void Relief(Color upper,Color lower,params Vector2[] vertices)
            {
                if(vertices.Length<3)return;
                Vector2 avg=Vector2.zero;
                foreach(var v in vertices)avg+=v;
                avg/=vertices.Length;
                int first=vh.currentVertCount;
                vh.AddVert(new UIVertex{position=center+avg*size,
                    color=Color.Lerp(upper,lower,.55f),uv0=Vector2.zero});
                foreach(var v in vertices)vh.AddVert(new UIVertex{
                    position=center+v*size,color=Color.Lerp(lower,upper,Mathf.Clamp01((v.y+1f)*.5f)),
                    uv0=Vector2.zero});
                for(int n=0;n<vertices.Length;n++)
                    vh.AddTriangle(first,first+1+n,first+1+(n+1)%vertices.Length);
            }
            Color copper=new Color(.34f,.29f,.23f,Ink.a);
            Color highlight=new Color(.72f,.65f,.52f,Ink.a);
            switch(Type)
            {
                case Sigil.Realm:case Sigil.Castle:
                    Relief(highlight,copper,new Vector2(-.78f,-.72f),new Vector2(-.78f,.44f),
                        new Vector2(.80f,.44f),new Vector2(.80f,-.72f));
                    Relief(highlight,copper,new Vector2(-.5f,.42f),new Vector2(-.5f,.72f),
                        new Vector2(-.19f,.72f),new Vector2(-.19f,.42f));
                    Relief(highlight,copper,new Vector2(.20f,.42f),new Vector2(.20f,.72f),
                        new Vector2(.54f,.72f),new Vector2(.54f,.42f));
                    Path(new Vector2(-.8f,-.75f),new Vector2(-.8f,.45f),new Vector2(-.5f,.45f),new Vector2(-.5f,.75f),new Vector2(-.2f,.75f),new Vector2(-.2f,.45f),new Vector2(.2f,.45f),new Vector2(.2f,.75f),new Vector2(.55f,.75f),new Vector2(.55f,.45f),new Vector2(.82f,.45f),new Vector2(.82f,-.75f),new Vector2(-.8f,-.75f));
                    Stroke(-.2f,-.75f,-.2f,-.12f);Stroke(.23f,-.75f,.23f,-.12f);Stroke(-.2f,-.12f,.23f,-.12f);break;
                case Sigil.Build:case Sigil.Hammer:
                    Relief(highlight,copper,new Vector2(-.76f,-.57f),new Vector2(-.63f,-.73f),
                        new Vector2(.51f,.50f),new Vector2(.34f,.65f));
                    Relief(highlight,copper,new Vector2(.03f,.72f),new Vector2(.60f,.17f),
                        new Vector2(.79f,.30f),new Vector2(.20f,.90f));
                    Stroke(-.7f,-.68f,.44f,.56f,1.5f);Stroke(.06f,.68f,.75f,.08f,1.8f);Stroke(-.24f,.40f,.36f,.92f,1.3f);break;
                case Sigil.Army:
                    Relief(highlight,copper,new Vector2(-.86f,.81f),new Vector2(-.71f,.89f),
                        new Vector2(.77f,-.62f),new Vector2(.61f,-.77f));
                    Relief(highlight,copper,new Vector2(.87f,.81f),new Vector2(.69f,.88f),
                        new Vector2(-.77f,-.64f),new Vector2(-.62f,-.78f));
                    Stroke(-.8f,.8f,.7f,-.68f,1.2f);Stroke(.8f,.8f,-.7f,-.68f,1.2f);
                    Stroke(-.75f,-.48f,-.38f,-.85f);Stroke(.75f,-.48f,.38f,-.85f);
                    Stroke(-.87f,-.66f,-.42f,-.23f);Stroke(.87f,-.66f,.42f,-.23f);break;
                case Sigil.World:
                    Relief(highlight,copper,new Vector2(0,.94f),new Vector2(.26f,.22f),
                        new Vector2(.94f,0),new Vector2(.26f,-.22f),new Vector2(0,-.94f),
                        new Vector2(-.26f,-.22f),new Vector2(-.94f,0),new Vector2(-.26f,.22f));
                    Path(new Vector2(0,1),new Vector2(.27f,.28f),new Vector2(1,0),new Vector2(.27f,-.28f),new Vector2(0,-1),new Vector2(-.27f,-.28f),new Vector2(-1,0),new Vector2(-.27f,.28f),new Vector2(0,1));
                    Stroke(0,.58f,0,-.58f);Stroke(-.58f,0,.58f,0);break;
                case Sigil.Chronicle:
                    Relief(highlight,copper,new Vector2(-.70f,.73f),new Vector2(-.05f,.76f),
                        new Vector2(-.03f,-.42f),new Vector2(-.70f,-.68f));
                    Relief(highlight,copper,new Vector2(.05f,.76f),new Vector2(.70f,.73f),
                        new Vector2(.70f,-.68f),new Vector2(.03f,-.42f));
                    Path(new Vector2(-.72f,.76f),new Vector2(-.72f,-.72f),new Vector2(0,-.46f),new Vector2(0,.82f),new Vector2(-.72f,.76f));
                    Path(new Vector2(0,.82f),new Vector2(.72f,.76f),new Vector2(.72f,-.72f),new Vector2(0,-.46f));
                    Stroke(-.52f,.38f,-.22f,.34f,.6f);Stroke(.2f,.34f,.52f,.38f,.6f);break;
                case Sigil.Wood:
                    for(int i=-1;i<=1;i++){float y=i*.38f;Stroke(-.8f,y-.05f,.75f,y+.18f,.92f);Stroke(-.63f,y+.15f,.7f,y+.38f,.65f);}
                    break;
                case Sigil.Stone:
                    Relief(new Color(.66f,.69f,.68f,Ink.a),new Color(.22f,.29f,.32f,Ink.a),
                        new Vector2(-.78f,-.37f),new Vector2(-.35f,.49f),
                        new Vector2(.39f,.70f),new Vector2(.83f,-.15f),
                        new Vector2(.43f,-.70f),new Vector2(-.55f,-.70f));
                    Path(new Vector2(-.82f,-.4f),new Vector2(-.35f,.5f),new Vector2(.4f,.73f),new Vector2(.87f,-.18f),new Vector2(.45f,-.73f),new Vector2(-.56f,-.73f),new Vector2(-.82f,-.4f));
                    Stroke(-.35f,.5f,-.08f,-.24f);Stroke(-.08f,-.24f,.45f,-.73f);Stroke(-.08f,-.24f,.87f,-.18f);break;
                case Sigil.Food:
                    Path(new Vector2(-.75f,.12f),new Vector2(-.42f,.64f),new Vector2(.18f,.55f),new Vector2(.4f,.12f),new Vector2(.1f,-.36f),new Vector2(-.44f,-.3f),new Vector2(-.75f,.12f));
                    Stroke(.28f,-.24f,.77f,-.73f,1.4f);Stroke(.57f,-.78f,.86f,-.48f);break;
                case Sigil.Bell:
                    Path(new Vector2(-.6f,-.45f),new Vector2(-.38f,-.2f),new Vector2(-.38f,.35f),new Vector2(-.25f,.62f),new Vector2(0,.75f),new Vector2(.25f,.62f),new Vector2(.38f,.35f),new Vector2(.38f,-.2f),new Vector2(.6f,-.45f),new Vector2(-.6f,-.45f));
                    Stroke(-.15f,-.68f,.15f,-.68f);break;
                case Sigil.Settings:
                    for(int i=0;i<8;i++){float a=i*Mathf.PI/4f;Stroke(Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f,Mathf.Cos(a)*.87f,Mathf.Sin(a)*.87f,1.2f);}
                    for(int i=0;i<10;i++){float a=i*Mathf.PI/5,b=(i+1)*Mathf.PI/5;Stroke(Mathf.Cos(a)*.45f,Mathf.Sin(a)*.45f,Mathf.Cos(b)*.45f,Mathf.Sin(b)*.45f);}
                    break;
                case Sigil.Cross:Stroke(-.6f,-.6f,.6f,.6f);Stroke(-.6f,.6f,.6f,-.6f);break;
                case Sigil.Breach:
                    Path(new Vector2(-.1f,.93f),new Vector2(.18f,.39f),new Vector2(-.2f,.05f),new Vector2(.14f,-.28f),new Vector2(-.16f,-.9f));
                    Stroke(.18f,.39f,.5f,.16f,.65f);break;
                case Sigil.Arrow:Stroke(-.65f,0,.65f,0,1.3f);Stroke(.24f,.45f,.7f,0,1.3f);Stroke(.24f,-.45f,.7f,0,1.3f);break;
            }
        }
    }
}