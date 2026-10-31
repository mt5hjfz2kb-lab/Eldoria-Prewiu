using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>
    /// Independent painted-style architectural layer for the approved Valoria hall.
    /// This is geometry, NOT a flattened mock-up. It draws no hero, text, data or
    /// interactive surfaces and runs inside the single existing HUD Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class HeroesHallScenicGraphic : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            float x = r.xMin, y = r.yMin, w = r.width, h = r.height;
            if (w < 2f || h < 2f) return;
            Color night = new Color(.023f, .029f, .042f, 1f);
            Color stone = new Color(.16f, .145f, .139f, 1f);
            Color stoneEdge = new Color(.32f, .264f, .205f, 1f);
            Color gold = new Color(.62f, .405f, .19f, 1f);
            Color light = new Color(.88f, .73f, .48f, .62f);
            Color floor = new Color(.07f, .065f, .065f, 1f);

            // A full-height vaulted chamber, not the five thin separators
            // that used to leave most of the hero gallery on a black field.
            Quad(vh, x, y, w, h, night, new Color(.08f, .085f, .103f, 1f));
            Quad(vh, x, y, w, h * .49f, floor, new Color(.15f, .123f, .104f, 1f));
            Quad(vh, x, y + h * .47f, w, h * .018f, stone, stoneEdge);

            for (int i = 0; i < 5; i++)
            {
                float center = x + w * (i + .5f) / 5f;
                float half = w * .071f;
                // Recessed pointed-arch alcove with soft daylight from Valoria.
                Arch(vh, center, y + h * .47f, half, h * .23f,
                    new Color(.12f, .16f, .19f, 1f),
                    new Color(.31f, .285f, .255f, 1f));
                // Supporting stone rib, visible even below the picture strip.
                float pillarX = center + half + w * .005f;
                Quad(vh, pillarX, y + h * .43f, w * .015f, h * .46f,
                    stone, stoneEdge);
                Quad(vh, pillarX + w * .004f, y + h * .43f,
                    w * .003f, h * .46f,
                    new Color(.45f, .33f, .19f, .40f),
                    new Color(.62f, .49f, .31f, .52f));
                Quad(vh, pillarX - w * .007f, y + h * .43f,
                    w * .032f, h * .018f, stoneEdge, gold);
            }

            // Floor tiles converge towards the central hall, adding true
            // compositional depth without copying any UI from the reference.
            float vanishingX = x + w * .5f, horizon = y + h * .48f;
            for (int i = -5; i <= 5; i++)
            {
                float bottomX = x + w * (.5f + i * .16f);
                Stroke(vh, vanishingX + w * i * .024f, horizon,
                    bottomX, y, Mathf.Max(1f, w * .0016f),
                    new Color(.52f, .38f, .20f, .28f));
            }
            for (int j = 0; j < 5; j++)
            {
                float yy = y + h * (.045f + j * j * .021f);
                Quad(vh, x, yy, w, Mathf.Max(1, h * .002f),
                    new Color(.42f, .32f, .22f, .20f),
                    new Color(.42f, .32f, .22f, .20f));
            }

            // Royal banners and warm torchlight evoke the approved hall's
            // banners/bronze rather than adding fake ornamental buttons.
            for (int side = 0; side < 2; side++)
            {
                float cx = x + w * (side == 0 ? .075f : .925f);
                float bannerW = w * .043f, top = y + h * .85f;
                Quad(vh, cx - bannerW * .5f, top - h * .21f,
                    bannerW, h * .21f,
                    new Color(.085f, .104f, .183f, .93f),
                    new Color(.11f, .15f, .23f, .99f));
                Triangle(vh, new Vector2(cx - bannerW*.5f, top - h*.21f),
                    new Vector2(cx + bannerW*.5f, top - h*.21f),
                    new Vector2(cx, top - h*.26f),
                    new Color(.085f,.104f,.183f,.93f));
                Quad(vh, cx - bannerW*.5f, top - h*.015f,
                    bannerW, h*.008f, gold, gold);
                Torch(vh, cx, y + h * .60f, Mathf.Min(w,h)*.10f, light);
            }
        }

        void Arch(VertexHelper vh, float cx, float floorY, float rx, float rh,
            Color inside, Color top)
        {
            const int sections = 16;
            float prevX = cx-rx;
            for (int i = 1; i <= sections; i++)
            {
                float nextX = cx-rx+2f*rx*i/sections;
                float mid = (prevX + nextX)*.5f;
                float normalized = Mathf.Abs(mid-cx)/rx;
                float height = rh*(1f - normalized*normalized);
                float topY = floorY + Mathf.Max(0, height);
                Quad(vh, prevX, floorY, nextX-prevX,
                    Mathf.Max(1f, topY-floorY), inside, top);
                prevX = nextX;
            }
            // Stone lintel follows the entire pointed vault.
            for (int i = 0; i < sections; i++)
            {
                float a = (float)i/sections, b = (float)(i+1)/sections;
                float ax = cx-rx+2f*rx*a, bx=cx-rx+2f*rx*b;
                float ay = floorY + rh*(1f-Mathf.Pow(Mathf.Abs((ax-cx)/rx),1.6f));
                float by = floorY + rh*(1f-Mathf.Pow(Mathf.Abs((bx-cx)/rx),1.6f));
                Stroke(vh,ax,ay,bx,by,Mathf.Max(1.5f,rx*.06f),
                    new Color(.55f,.39f,.22f,.85f));
            }
        }

        void Torch(VertexHelper vh, float cx, float cy, float radius, Color flame)
        {
            for (int i=0;i<20;i++)
            {
                float a = Mathf.PI*2f*i/20f, b=Mathf.PI*2f*(i+1)/20f;
                TriangleGradient(vh,new Vector2(cx,cy),
                    new Vector2(cx+Mathf.Cos(a)*radius,cy+Mathf.Sin(a)*radius*1.4f),
                    new Vector2(cx+Mathf.Cos(b)*radius,cy+Mathf.Sin(b)*radius*1.4f),
                    flame,new Color(flame.r,flame.g,flame.b,0));
            }
            Quad(vh,cx-radius*.12f,cy-radius*.30f,radius*.24f,
                radius*.56f,new Color(.37f,.22f,.13f),new Color(.75f,.52f,.23f));
        }

        void Stroke(VertexHelper vh, float ax,float ay,float bx,float by,float size,Color c)
        {
            Vector2 d=new Vector2(bx-ax,by-ay).normalized*size*.5f;
            Vector2 n=new Vector2(-d.y,d.x);
            int k=vh.currentVertCount;
            vh.AddVert(new Vector3(ax+n.x,ay+n.y),c,Vector2.zero);
            vh.AddVert(new Vector3(ax-n.x,ay-n.y),c,Vector2.zero);
            vh.AddVert(new Vector3(bx-n.x,by-n.y),c,Vector2.zero);
            vh.AddVert(new Vector3(bx+n.x,by+n.y),c,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }

        void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color color)
        {
            int k=vh.currentVertCount;
            vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.one);
            vh.AddVert(c,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);
        }

        void TriangleGradient(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,
            Color center,Color edge)
        {
            int k=vh.currentVertCount;
            vh.AddVert(a,center,Vector2.zero);
            vh.AddVert(b,edge,Vector2.one);
            vh.AddVert(c,edge,Vector2.zero);vh.AddTriangle(k,k+1,k+2);
        }

        void Quad(VertexHelper vh,float x,float y,float w,float h,Color bottom,Color top)
        {
            if(w<=0||h<=0)return;
            int k=vh.currentVertCount;
            vh.AddVert(new Vector3(x,y),bottom,Vector2.zero);
            vh.AddVert(new Vector3(x,y+h),top,Vector2.up);
            vh.AddVert(new Vector3(x+w,y+h),top,Vector2.one);
            vh.AddVert(new Vector3(x+w,y),bottom,Vector2.right);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
    }
}
