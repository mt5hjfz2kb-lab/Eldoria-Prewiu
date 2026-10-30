using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Lightweight, transparent atmospheric foreground for the four approved
    /// royal destinations. It sits BETWEEN source illustration and live UI.
    /// No image downloads, secondary canvas, fake gameplay, or pointer interception.
    /// Animating this mesh never moves the map texture or its real stage markers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3LayeredFinish : MaskableGraphic
    {
        public enum Scene { Vigilants, Scouts, Training, March, Panel }
        [SerializeField] Scene scene;
        float nextRepaint;

        public static RoyalGoldV3LayeredFinish Install(
            RectTransform parent, Scene kind, int siblingIndex)
        {
            var node = new GameObject("Illustrated foreground / " + kind,
                typeof(RectTransform), typeof(CanvasRenderer),
                typeof(RoyalGoldV3LayeredFinish));
            node.layer = parent.gameObject.layer;
            var rect = (RectTransform)node.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
            var finish = node.GetComponent<RoyalGoldV3LayeredFinish>();
            finish.raycastTarget = false;
            finish.color = Color.white;
            finish.SetScene(kind);
            return finish;
        }

        public void SetScene(Scene next)
        {
            if (scene == next) return;
            scene = next;
            SetVerticesDirty();
        }

        void Update()
        {
            // Soft atmospheric movement, throttled; no expensive texture copies,
            // shadow casters, camera changes or new graphic allocations per frame.
            if (!isActiveAndEnabled || Time.unscaledTime < nextRepaint) return;
            nextRepaint = Time.unscaledTime + .12f;
            SetVerticesDirty();
        }

        static Color RGBA(float r,float g,float b,float a)
            => new Color(r,g,b,a);

        static void Quad(VertexHelper vh, float x0, float y0, float x1, float y1,
            Color lowerLeft, Color lowerRight, Color upperRight, Color upperLeft)
        {
            int i = vh.currentVertCount;
            vh.AddVert(new Vector2(x0,y0),lowerLeft,Vector2.zero);
            vh.AddVert(new Vector2(x1,y0),lowerRight,Vector2.zero);
            vh.AddVert(new Vector2(x1,y1),upperRight,Vector2.zero);
            vh.AddVert(new Vector2(x0,y1),upperLeft,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);
            vh.AddTriangle(i,i+2,i+3);
        }

        static void Solid(VertexHelper vh,float x0,float y0,float x1,float y1,Color c)
            => Quad(vh,x0,y0,x1,y1,c,c,c,c);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=GetPixelAdjustedRect();
            float x=r.xMin,y=r.yMin,w=r.width,h=r.height;
            if(w<12f || h<12f) return;
            float t=Time.unscaledTime;
            float breath=.5f+.5f*Mathf.Sin(t*.68f);
            bool landscape=scene==Scene.Vigilants || scene==Scene.Scouts;
            var clear=RGBA(0f,0f,0f,0f);
            var midnight=RGBA(.018f,.031f,.051f,landscape?.48f:.31f);

            // Four atmospheric planes: a soft shadow at the near edge and
            // transparent lateral falloff, NOT another opaque screenshot.
            Quad(vh,x,y,x+w,y+h*.24f,
                midnight,midnight,clear,clear);
            Quad(vh,x,y,x+w*.18f,y+h,
                midnight,clear,clear,midnight);
            Quad(vh,x+w*.82f,y,x+w,y+h,
                clear,midnight,midnight,clear);
            Quad(vh,x,y+h*.87f,x+w,y+h,
                clear,clear,RGBA(.02f,.025f,.04f,.16f),
                RGBA(.02f,.025f,.04f,.16f));

            if(scene==Scene.Vigilants)
            {
                // Separate luminous foreground mist and changing gold light.
                // Gameplay dots still align with the static authored road.
                var mist=RGBA(.68f,.79f,.78f,.055f+.045f*breath);
                Quad(vh,x,y+h*.03f,x+w,y+h*.17f,
                    mist,mist,clear,clear);
                float shaft=.32f+.08f*Mathf.Sin(t*.33f);
                Quad(vh,x+w*shaft,y+h*.52f,x+w*(shaft+.16f),y+h,
                    clear,clear,RGBA(1f,.80f,.43f,.06f+.035f*breath),
                    RGBA(1f,.80f,.43f,.10f+.035f*breath));
                for(int i=0;i<5;i++)
                {
                    float px=x+w*(.13f+i*.18f);
                    float py=y+h*(.17f+(i%3)*.12f)+Mathf.Sin(t*.55f+i)*h*.003f;
                    float sz=Mathf.Clamp(w*.0035f,1.4f,3.5f);
                    Solid(vh,px,py,px+sz,py+sz,
                        RGBA(1f,.82f,.46f,.12f+.10f*breath));
                }
            }
            else if(scene==Scene.Scouts)
            {
                // Candlelit map edge and moving paper-gold sheen. Contracts
                // remain three separate, tactile, authoritative controls.
                float glint=.39f+.025f*Mathf.Sin(t*.4f);
                Quad(vh,x+w*glint,y+h*.08f,x+w*(glint+.22f),y+h*.93f,
                    clear,clear,RGBA(1f,.78f,.43f,.045f+.025f*breath),
                    RGBA(1f,.78f,.43f,.09f+.025f*breath));
                Quad(vh,x+w*.08f,y+h*.86f,x+w*.92f,y+h*.96f,
                    clear,clear,RGBA(.95f,.68f,.29f,.12f),
                    RGBA(.95f,.68f,.29f,.12f));
            }
            else
            {
                // Military-room / parchment lighting remains underneath
                // controls, without obscuring live counts or hero portraits.
                Color warm=RGBA(.90f,.67f,.31f,.08f+.025f*breath);
                Quad(vh,x+w*.04f,y+h*.68f,x+w*.32f,y+h*.98f,
                    clear,clear,warm,warm);
                Quad(vh,x+w*.02f,y+h*.92f,x+w*.98f,y+h*.99f,
                    clear,clear,RGBA(.90f,.67f,.31f,.085f),
                    RGBA(.90f,.67f,.31f,.085f));
                if(scene==Scene.March)
                {
                    // Slight silver-blue raking light on tactical parchment.
                    Quad(vh,x+w*.72f,y+h*.20f,x+w*.95f,y+h*.80f,
                        clear,clear,RGBA(.46f,.68f,.85f,.065f+.02f*breath),
                        clear);
                }
                if(scene==Scene.Panel)
                {
                    // Nested thin gold lip and darker paper shadow; separate
                    // from the approved texture and responsive to any ratio.
                    var lip=RGBA(.83f,.64f,.34f,.19f);
                    Solid(vh,x+2,y+h-3,x+w-2,y+h-1,lip);
                    Solid(vh,x+2,y+1,x+w-2,y+3,RGBA(.01f,.02f,.03f,.28f));
                    Solid(vh,x+1,y+3,x+3,y+h-3,lip);
                    Solid(vh,x+w-3,y+3,x+w-1,y+h-3,lip);
                }
            }
        }
    }
}
