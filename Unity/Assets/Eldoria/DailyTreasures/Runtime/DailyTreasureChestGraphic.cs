using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Reusable original 2D chest made of editable Unity geometry. No 3D model,
    /// stock image, paid asset or full-screen painted screenshot is necessary.
    /// Art still requires a genuine native screenshot review before approval.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DailyTreasureChestGraphic : MaskableGraphic
    {
        [Range(0f,1f)] public float OpenAmount;
        static readonly Color WoodDark=new Color(.11f,.065f,.041f,1f);
        static readonly Color Wood=new Color(.31f,.17f,.095f,1f);
        static readonly Color WoodLight=new Color(.43f,.25f,.14f,1f);
        static readonly Color Iron=new Color(.095f,.105f,.12f,1f);
        static readonly Color Gold=new Color(.72f,.48f,.20f,1f);
        static readonly Color Glint=new Color(1f,.82f,.42f,1f);
        void Quad(VertexHelper vh,float x1,float y1,float x2,float y2,Color tint)
        {
            int start=vh.currentVertCount;
            vh.AddVert(new Vector3(x1,y1),tint,new Vector2(0f,0f));
            vh.AddVert(new Vector3(x1,y2),tint,new Vector2(0f,1f));
            vh.AddVert(new Vector3(x2,y2),tint,new Vector2(1f,1f));
            vh.AddVert(new Vector3(x2,y1),tint,new Vector2(1f,0f));
            vh.AddTriangle(start,start+1,start+2);
            vh.AddTriangle(start,start+2,start+3);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect=GetPixelAdjustedRect();
            float unit=Mathf.Min(rect.width/170f,rect.height/120f);
            float x=rect.center.x-85f*unit,y=rect.center.y-60f*unit;
            void P(float a,float b,float c,float d,Color tint)
                =>Quad(vh,x+a*unit,y+b*unit,x+c*unit,y+d*unit,tint);
            // Recessed amber interior is visible only as the lid opens.
            if(OpenAmount>.01f)
            {
                var light=new Color(1f,.66f,.18f,.85f*OpenAmount);
                P(14,48,156,87,light);
                P(20,56,150,82,new Color(1f,.90f,.52f,.47f*OpenAmount));
            }
            // Chest feet, base shadow, carved oak and blackened-iron bands.
            P(14,4,49,17,Iron);P(121,4,156,17,Iron);
            P(8,15,162,71,Iron);
            P(13,21,157,66,Gold);
            P(18,24,152,64,WoodDark);
            P(19,27,150,58,Wood);
            for(int i=0;i<4;i++)
                P(20,31+i*7,150,32+i*7,
                    i%2==0?WoodLight:WoodDark);
            P(31,23,39,65,Iron);P(130,23,138,65,Iron);
            P(35,24,36,65,Gold);P(134,24,135,65,Gold);
            // A genuine movable lid, not an opacity trick on the entire image.
            float rise=OpenAmount*33f;
            P(5,63+rise,165,91+rise,Iron);
            P(10,68+rise,160,89+rise,Gold);
            P(14,70+rise,156,85+rise,WoodDark);
            P(18,73+rise,152,83+rise,Wood);
            P(18,79+rise,152,81+rise,WoodLight);
            P(27,69+rise,35,91+rise,Iron);
            P(135,69+rise,143,91+rise,Iron);
            // Central lock is on the body so it remains stable while opening.
            P(73,36,98,68,Iron);P(77,39,94,65,Gold);
            P(81,43,90,59,WoodDark);
            P(84,47,87,55,Glint);
            P(18,63,151,65,new Color(.95f,.75f,.40f,1f));
            // Bronze rivets remain sharp at phone resolution.
            for(int i=0;i<4;i++)
            {
                float cx=i%2==0?29:141;
                float cy=i<2?33:56;
                P(cx-2,cy-2,cx+2,cy+2,Gold);
                P(cx-1,cy,cx,cy+1,Glint);
            }
        }
        public void SetOpening(float amount)
        {
            amount=Mathf.Clamp01(amount);
            if(Mathf.Abs(OpenAmount-amount)<.003f)return;
            OpenAmount=amount;
            SetVerticesDirty();
        }
    }
}
