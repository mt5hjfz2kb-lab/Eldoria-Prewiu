using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// A fine engraved metal rule and central Breach diamond, independent
    /// of any legacy HUD. Separates the live building narrative from its
    /// actual upgrade cost without occupying an opaque rectangle.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BreachWatchSectionDivider : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            color = Color.white;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width < 35f || r.height < 7f)return;
            var center=r.center;
            Color copper=new Color(.65f,.40f,.20f,.82f);
            Color gold=new Color(.95f,.73f,.39f,.92f);
            Color shadow=new Color(.055f,.029f,.02f,.75f);

            void Segment(Vector2 a, Vector2 b,float thickness,Color ink)
            {
                Vector2 direction=b-a;
                if(direction.sqrMagnitude<.0001f)return;
                Vector2 normal=new Vector2(-direction.y,direction.x).normalized*thickness*.5f;
                int i=vh.currentVertCount;
                vh.AddVert(a+normal,ink,Vector2.zero);
                vh.AddVert(a-normal,ink,Vector2.zero);
                vh.AddVert(b-normal,ink,Vector2.zero);
                vh.AddVert(b+normal,ink,Vector2.zero);
                vh.AddTriangle(i,i+1,i+2);
                vh.AddTriangle(i,i+2,i+3);
            }
            var originLeft=new Vector2(r.xMin+4,center.y);
            var originRight=new Vector2(r.xMax-4,center.y);
            float inset=Mathf.Min(r.width*.09f,10f);
            Vector2 leftBreak=new Vector2(center.x-inset,center.y);
            Vector2 rightBreak=new Vector2(center.x+inset,center.y);
            Segment(originLeft+Vector2.down,leftBreak+Vector2.down,2.9f,shadow);
            Segment(originRight+Vector2.down,rightBreak+Vector2.down,2.9f,shadow);
            Segment(originLeft,leftBreak,1.0f,copper);
            Segment(originRight,rightBreak,1.0f,copper);

            Vector2[] diamond={
                center+new Vector2(0,5),
                center+new Vector2(7,0),
                center+new Vector2(0,-5),
                center+new Vector2(-7,0)
            };
            int start=vh.currentVertCount;
            vh.AddVert(center,new Color(.30f,.15f,.07f,.96f),Vector2.zero);
            foreach(var pt in diamond)vh.AddVert(pt,copper,Vector2.zero);
            for(int i=0;i<4;i++)
            {
                vh.AddTriangle(start,start+i+1,start+(i+1)%4+1);
                Segment(diamond[i],diamond[(i+1)%4],1.25f,gold);
            }
            Segment(center+new Vector2(-2,0),center+new Vector2(2,0),.75f,gold);
        }
    }
}
