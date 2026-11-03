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
                // Preserve every pixel of the authentic stage-15 alpha art.
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();
            Rect r=GetPixelAdjustedRect();
            if(r.width<16f || r.height<16f)return;

            Vector2 center=new Vector2(.50f,.51f);
            int first=vh.currentVertCount;
            Color32 tint=color;
            vh.AddVert(FromUnit(r,center),Modulate(tint,1f),OriginalUv(center));

            // The polygon has an irregular rounded-arch perimeter, not a
            // straight rectangular screenshot. Soft outside (alpha 0) and
            // strong central mass retain the original guardian scene subject.
            var core=new Vector2[Edges];
            var rim=new Vector2[Edges];
            for(int i=0;i<Edges;i++)
            {
                float a=i*Mathf.PI*2f/Edges;
                float shape=1f+.024f*Mathf.Sin(a*5f+.9f)
                    +.018f*Mathf.Sin(a*9f+1.7f);
                Vector2 boundary=center+new Vector2(
                    Mathf.Cos(a)*.49f*shape,
                    Mathf.Sin(a)*.49f*shape);
                boundary.x=Mathf.Clamp01(boundary.x);
                boundary.y=Mathf.Clamp01(boundary.y);
                rim[i]=boundary;
                core[i]=Vector2.Lerp(center,boundary,.79f);

                vh.AddVert(FromUnit(r,core[i]),
                    Modulate(tint,1f),OriginalUv(core[i]));
            }
            for(int i=0;i<Edges;i++)
                vh.AddVert(FromUnit(r,rim[i]),
                    Modulate(tint,0f),OriginalUv(rim[i]));

            for(int i=0;i<Edges;i++)
            {
                int j=(i+1)%Edges;
                // Centre/inner fan; outer ring feathers in physical
                // coordinates and uses the original atlas quadrant UV.
                vh.AddTriangle(first,first+1+i,first+1+j);
                vh.AddTriangle(first+1+i,first+1+Edges+i,first+1+Edges+j);
                vh.AddTriangle(first+1+i,first+1+Edges+j,first+1+j);
            }
        }
    }
}
