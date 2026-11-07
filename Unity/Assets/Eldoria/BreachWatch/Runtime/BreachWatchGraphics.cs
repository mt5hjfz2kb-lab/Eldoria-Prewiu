using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    // Entirely new vector geometry. No RoyalHUD/Forja sprites, plates or layout.
    // Independent UI meshes remain crisp at any physical resolution.
    public sealed class BreachWatchPlate : MaskableGraphic
    {
        public enum PlateType { Basalt, Obsidian, Resource, Action, Selected, Dangerous }
        public PlateType Type = PlateType.Basalt;
        public bool CopperEdge = true;
        protected override void Awake() { base.Awake(); raycastTarget = false; }
        static Color C(byte r, byte g, byte b, byte a=255) => new Color32(r,g,b,a);
        static Rect Shrink(Rect r, float d) => new Rect(r.xMin+d, r.yMin+d, Mathf.Max(1,r.width-2*d), Mathf.Max(1,r.height-2*d));
        static void Poly(VertexHelper mesh, Rect rect, float chamfer, Color top, Color bottom)
        {
            var r=rect; float cut=Mathf.Min(chamfer,Mathf.Min(r.width,r.height)*.22f);
            Vector2[] points = {
                new Vector2(r.xMin+cut,r.yMax),new Vector2(r.xMax-cut,r.yMax),
                new Vector2(r.xMax,r.yMax-cut),new Vector2(r.xMax,r.yMin+cut),
                new Vector2(r.xMax-cut,r.yMin),new Vector2(r.xMin+cut,r.yMin),
                new Vector2(r.xMin,r.yMin+cut),new Vector2(r.xMin,r.yMax-cut)};
            int start=mesh.currentVertCount;
            var center=new Vector2(r.center.x,r.center.y);
            mesh.AddVert(new UIVertex{position=center,color=Color.Lerp(top,bottom,.5f),uv0=Vector2.zero});
            foreach(var pt in points)mesh.AddVert(new UIVertex{position=pt,color=Color.Lerp(top,bottom,Mathf.InverseLerp(r.yMax,r.yMin,pt.y)),uv0=Vector2.zero});
            for(int i=0;i<8;i++)mesh.AddTriangle(start,start+i+1,start+(i+1)%8+1);
        }
        static void Line(VertexHelper mesh, Vector2 a,Vector2 b,float width,Color color)
        {
            Vector2 d=b-a;if(d.sqrMagnitude<.0001f)return;
            Vector2 n=new Vector2(-d.y,d.x).normalized*(width*.5f);
            int start=mesh.currentVertCount;
            mesh.AddVert(new UIVertex{position=a+n,color=color});mesh.AddVert(new UIVertex{position=a-n,color=color});
            mesh.AddVert(new UIVertex{position=b-n,color=color});mesh.AddVert(new UIVertex{position=b+n,color=color});
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Rect r=GetPixelAdjustedRect();if(r.width<2||r.height<2)return;
            bool glow=Type==PlateType.Selected||Type==PlateType.Action;
            Color rim=glow?C(160,93,63):C(77,67,61);
            Color mainTop=Type==PlateType.Resource?C(40,43,44):Type==PlateType.Obsidian?C(23,28,32):C(34,39,42);
            Color mainBottom=Type==PlateType.Resource?C(17,21,24):C(14,19,23);
            Poly(vh,new Rect(r.xMin+1,r.yMin-2,r.width,r.height),12,C(3,5,6,230),C(3,4,5,220));
            Poly(vh,r,12,C(9,12,15),C(7,9,11));
            Poly(vh,Shrink(r,1.8f),11,rim,C(40,39,38));
            Poly(vh,Shrink(r,4.1f),9,C(13,18,21),C(16,19,20));
            Poly(vh,Shrink(r,5.8f),7,mainTop,mainBottom);
            // Patinated bevel, scored basalt and restrained light at edges.
            Line(vh,new Vector2(r.xMin+14,r.yMax-6),new Vector2(r.xMax-14,r.yMax-6),1.1f,C(133,108,88,130));
            Line(vh,new Vector2(r.xMin+12,r.yMin+6),new Vector2(r.xMax-12,r.yMin+6),1f,C(2,6,8,230));
            Line(vh,new Vector2(r.xMin+8,r.yMax-15),new Vector2(r.xMin+8,r.yMin+18),.8f,C(87,92,91,85));
            if(CopperEdge) Line(vh,new Vector2(r.xMin+14,r.yMax-2),new Vector2(r.xMin+Mathf.Min(r.width*.48f,120),r.yMax-2),2f,glow?C(212,121,73):C(119,69,53,185));
            // The fissure is a quiet carved stone scar, never a neon effect.
            if(r.width>100&&r.height>46)
            {
                float x=r.xMax-20,y=r.yMax-10;
                Line(vh,new Vector2(x-13,y),new Vector2(x-7,y-7),.9f,C(143,91,70,85));
                Line(vh,new Vector2(x-7,y-7),new Vector2(x-11,y-15),.9f,C(143,91,70,85));
                Line(vh,new Vector2(x-11,y-15),new Vector2(x-4,y-20),.75f,C(143,91,70,72));
            }
            // Deliberate matte-grain rather than a flat app-style rectangle.
            for(int i=0;i<Mathf.Min(24,(int)(r.width*r.height/1300f));i++)
            {
                float u=((i*37)%97)/97f,v=((i*53+19)%89)/89f;
                float x=Mathf.Lerp(r.xMin+18,r.xMax-18,u),y=Mathf.Lerp(r.yMin+12,r.yMax-12,v);
                Line(vh,new Vector2(x,y),new Vector2(x+2.5f,y-.35f),.5f,C(124,114,108,22));
            }
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
                Edge(vh,p+new Vector2(1,-1),q+new Vector2(1,-1),thick*w,new Color(.02f,.035f,.04f,.83f));
                Edge(vh,p,q,thick*w,Ink);
            }
            void Path(params Vector2[] pts){for(int i=0;i<pts.Length-1;i++)Stroke(pts[i].x,pts[i].y,pts[i+1].x,pts[i+1].y);}
            switch(Type)
            {
                case Sigil.Realm:case Sigil.Castle:
                    Path(new Vector2(-.8f,-.75f),new Vector2(-.8f,.45f),new Vector2(-.5f,.45f),new Vector2(-.5f,.75f),new Vector2(-.2f,.75f),new Vector2(-.2f,.45f),new Vector2(.2f,.45f),new Vector2(.2f,.75f),new Vector2(.55f,.75f),new Vector2(.55f,.45f),new Vector2(.82f,.45f),new Vector2(.82f,-.75f),new Vector2(-.8f,-.75f));
                    Stroke(-.2f,-.75f,-.2f,-.12f);Stroke(.23f,-.75f,.23f,-.12f);Stroke(-.2f,-.12f,.23f,-.12f);break;
                case Sigil.Build:case Sigil.Hammer:
                    Stroke(-.7f,-.68f,.44f,.56f,1.5f);Stroke(.06f,.68f,.75f,.08f,1.8f);Stroke(-.24f,.40f,.36f,.92f,1.3f);break;
                case Sigil.Army:
                    Stroke(-.8f,.8f,.7f,-.68f,1.2f);Stroke(.8f,.8f,-.7f,-.68f,1.2f);
                    Stroke(-.75f,-.48f,-.38f,-.85f);Stroke(.75f,-.48f,.38f,-.85f);
                    Stroke(-.87f,-.66f,-.42f,-.23f);Stroke(.87f,-.66f,.42f,-.23f);break;
                case Sigil.World:
                    Path(new Vector2(0,1),new Vector2(.27f,.28f),new Vector2(1,0),new Vector2(.27f,-.28f),new Vector2(0,-1),new Vector2(-.27f,-.28f),new Vector2(-1,0),new Vector2(-.27f,.28f),new Vector2(0,1));
                    Stroke(0,.58f,0,-.58f);Stroke(-.58f,0,.58f,0);break;
                case Sigil.Chronicle:
                    Path(new Vector2(-.72f,.76f),new Vector2(-.72f,-.72f),new Vector2(0,-.46f),new Vector2(0,.82f),new Vector2(-.72f,.76f));
                    Path(new Vector2(0,.82f),new Vector2(.72f,.76f),new Vector2(.72f,-.72f),new Vector2(0,-.46f));
                    Stroke(-.52f,.38f,-.22f,.34f,.6f);Stroke(.2f,.34f,.52f,.38f,.6f);break;
                case Sigil.Wood:
                    for(int i=-1;i<=1;i++){float y=i*.38f;Stroke(-.8f,y-.05f,.75f,y+.18f,.92f);Stroke(-.63f,y+.15f,.7f,y+.38f,.65f);}
                    break;
                case Sigil.Stone:
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