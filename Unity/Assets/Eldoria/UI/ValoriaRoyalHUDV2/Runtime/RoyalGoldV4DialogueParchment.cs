using UnityEngine;
using UnityEngine.UI;
namespace Eldoria.UI.RoyalGold.V2
{
    // Pure presentation mesh. The real Text, Button and portrait are children
    // of the SAME ModalWindow; no new canvas, events or gameplay state.
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV4DialogueParchment : MaskableGraphic
    {
        public bool Arcane {get;private set;}
        public bool Nocturne {get;private set;}
        public bool Cinematic {get;private set;}
        public float NameplateWidth {get;private set;}
        public float PortraitColumn {get;private set;}
        public override Texture mainTexture=>Texture2D.whiteTexture;
        static void Quad(VertexHelper h,Rect r,Color lo,Color hi)
        {
            if(r.width<=0||r.height<=0)return;
            int n=h.currentVertCount;
            h.AddVert(new Vector3(r.xMin,r.yMin),lo,Vector2.zero);
            h.AddVert(new Vector3(r.xMin,r.yMax),hi,Vector2.up);
            h.AddVert(new Vector3(r.xMax,r.yMax),hi,Vector2.one);
            h.AddVert(new Vector3(r.xMax,r.yMin),lo,Vector2.right);
            h.AddTriangle(n,n+1,n+2);h.AddTriangle(n,n+2,n+3);
        }
        static Rect Inset(Rect r,float d)=>new Rect(r.xMin+d,r.yMin+d,
            Mathf.Max(0,r.width-2*d),Mathf.Max(0,r.height-2*d));
        static void Seal(VertexHelper h,Vector2 centre,float size,Color c)
        {
            const int steps=20;
            for(int j=0;j<steps;j++)
            {
                float a=j*Mathf.PI*2/steps,b=(j+1)*Mathf.PI*2/steps;
                int n=h.currentVertCount;
                h.AddVert(centre,new Color(c.r,c.g,c.b,.25f),Vector2.zero);
                h.AddVert(centre+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size,c,Vector2.zero);
                h.AddVert(centre+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size,c,Vector2.zero);
                h.AddTriangle(n,n+1,n+2);
            }
        }
        public void Configure(bool orin,bool lyra,float column,bool cinematic=false)
        {
            if(Arcane==orin&&Nocturne==lyra&&Cinematic==cinematic&&
                Mathf.Abs(PortraitColumn-column)<.1f)return;
            Arcane=orin;Nocturne=lyra;Cinematic=cinematic;
            PortraitColumn=column;SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper h)
        {
            h.Clear();var r=rectTransform.rect;
            if(r.width<180||r.height<130)return;
            float col=Mathf.Clamp(PortraitColumn,0,r.width*.43f);
            var p=new Rect(r.xMin+col*.68f,r.yMin+5,r.width-col*.68f-8,r.height-12);
            var rim=Arcane?new Color(.61f,.38f,.74f):new Color(.72f,.52f,.26f);
            Color paperBottom=Arcane?new Color(.12f,.075f,.17f):
                Nocturne?new Color(.06f,.075f,.10f):new Color(.81f,.70f,.51f);
            Color paperTop=Arcane?new Color(.22f,.14f,.27f):
                Nocturne?new Color(.11f,.14f,.17f):new Color(.98f,.91f,.77f);
            Quad(h,new Rect(p.x+7,p.y-7,p.width,p.height),new Color(.07f,.03f,.015f,.48f),new Color(.10f,.055f,.03f,.27f));
            Quad(h,p,new Color(rim.r*.65f,rim.g*.65f,rim.b*.55f),rim);
            Quad(h,Inset(p,4f),paperBottom,paperTop);
            Quad(h,Inset(p,10f),
                new Color(paperBottom.r,paperBottom.g,paperBottom.b,.20f),
                new Color(paperTop.r,paperTop.g,paperTop.b,.18f));
            for(int i=0;i<11;i++)
            {
                float y=p.yMin+25f+i*Mathf.Max(8f,(p.height-50f)/11f);
                if(y<p.yMax-24f)Quad(h,new Rect(p.xMin+18f,y,p.width-38f,.6f),
                    new Color(rim.r,rim.g,rim.b,.045f),new Color(rim.r,rim.g,rim.b,.025f));
            }
            float left=r.xMin+(col>0?col+12f:Mathf.Min(r.width*.30f,230f));
            // The approved Diálogo 1 has a SHORT nameplate over the paper.
            // A full-width dark bar still looked like a management modal
            // even after the real character became cinematic.
            float fullTabWidth=Mathf.Max(56f,r.xMax-left-15f);
            NameplateWidth=Cinematic?Mathf.Min(fullTabWidth,360f):fullTabWidth;
            var tab=new Rect(left,p.yMax-80,NameplateWidth,61);
            Quad(h,tab,new Color(rim.r*.5f,rim.g*.43f,rim.b*.32f),rim);
            Quad(h,Inset(tab,2f),Arcane?new Color(.21f,.12f,.27f):
                Nocturne?new Color(.08f,.11f,.15f):new Color(.25f,.19f,.13f),
                Arcane?new Color(.34f,.22f,.42f):
                Nocturne?new Color(.14f,.19f,.22f):new Color(.39f,.29f,.17f));
            Quad(h,new Rect(p.xMin+18,p.yMin+17,p.width-36,2),rim,rim);
            // In the approved cinematic style the real character cuts
            // into the scene instead of sitting inside a large medallion.
            // Keep the ornamental seal for smaller integrated layouts.
            if(col>50&&!Cinematic)
            {
                var centre=new Vector2(r.xMin+col*.5f,r.yMin+r.height*.55f);
                float radius=Mathf.Min(col*.46f,r.height*.43f);
                Seal(h,centre+new Vector2(3,-5),radius+10,new Color(.20f,.11f,.045f,.38f));
                Seal(h,centre,radius+6,rim);
                Seal(h,centre,radius,new Color(.42f,.31f,.20f,.43f));
            }
        }
    }
}