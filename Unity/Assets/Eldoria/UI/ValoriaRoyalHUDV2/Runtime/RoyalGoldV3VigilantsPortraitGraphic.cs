using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// An authentic RawImage-compatible portrait renderer with feathered,
    /// painterly contours. Regular enemy illustrations fade into the encounter
    /// room instead of ending in an obvious rectangular photograph. The stage
    /// 15 transparent creature keeps its FULL approved silhouette, including
    /// every tentacle, without duplicated textures or new cameras.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3VigilantsPortraitGraphic : RawImage
    {
        [SerializeField] bool transparentCreature;
        const int Edges=28;

        public bool TransparentCreature => transparentCreature;

        public void Configure(bool sourceHasOriginalCutout)
        {
            if(transparentCreature==sourceHasOriginalCutout)return;
            transparentCreature=sourceHasOriginalCutout;
            SetVerticesDirty();
        }

        static Color32 Modulate(Color baseTint,float fade)
        {
            return new Color(baseTint.r,baseTint.g,baseTint.b,
                baseTint.a*Mathf.Clamp01(fade));
        }

        Vector2 FromUnit(Rect frame,Vector2 uv)
        {
            return new Vector2(frame.xMin+uv.x*frame.width,
                frame.yMin+uv.y*frame.height);
        }

        Vector2 OriginalUv(Vector2 uv)
        {
            return new Vector2(uvRect.x+uv.x*uvRect.width,
                uvRect.y+uv.y*uvRect.height);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if(transparentCreature)
            {
                // Keep the exact approved translucent Engendro image; it is
                // now standing in the LANDSCAPE, not on a separate combat card.
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();
            var r=GetPixelAdjustedRect();
            if(r.width<16f||r.height<16f)return;
            // Painted encounter fragment, NOT the circular peephole seen in
            // B2: a broad, asymmetrically feathered terrain patch whose
            // exterior dissolves into the same approved world composition.
            const int columns=22,rows=18;
            Color c=color;
            for(int j=0;j<=rows;j++)
            {
                float v=(float)j/rows;
                float ragged=.021f*Mathf.Sin(v*19.2f+1.6f)
                    +.013f*Mathf.Sin(v*43f+2.1f);
                float edgeLeft=.025f+ragged;
                float edgeRight=.965f+.018f*Mathf.Sin(v*26.1f+.7f);
                for(int i=0;i<=columns;i++)
                {
                    float u=(float)i/columns;
                    float irregularTop=.965f-.013f*Mathf.Sin(u*21.3f);
                    float irregularBottom=.025f+.024f*Mathf.Sin(u*16.2f);
                    float innerEdge=Mathf.Min(
                        Mathf.Min(u-edgeLeft,edgeRight-u),
                        Mathf.Min(v-irregularBottom,irregularTop-v));
                    float strength=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01(innerEdge/.17f));
                    // Tiny ragged edge variation avoids a perfect photo
                    // rectangle, but the patrol scene remains authoritative.
                    var tone=new Color(c.r*.93f,c.g*.99f,c.b*.96f,
                        c.a*strength*.83f);
                    var uv=new Vector2(u,v);
                    vh.AddVert(FromUnit(r,uv),tone,OriginalUv(uv));
                }
            }
            for(int j=0;j<rows;j++)
            for(int i=0;i<columns;i++)
            {
                int k=j*(columns+1)+i;
                vh.AddTriangle(k,k+1,k+columns+2);
                vh.AddTriangle(k,k+columns+2,k+columns+1);
            }
        }
    }
}
