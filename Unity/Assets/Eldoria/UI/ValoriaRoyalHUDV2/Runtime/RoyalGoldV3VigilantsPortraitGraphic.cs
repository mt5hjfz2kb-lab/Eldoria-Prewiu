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
        public bool PatrolSilhouettes => !transparentCreature;

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

        // Actual silhouettes lifted out of the AUTHORISED painted patrol
        // quadrant. Each crop contains an existing soldier, not a newly
        // invented guardian. NO castle, sky, screenshot or rectangular photo.
        // Local UV coordinates are inside the original atlas quadrant.
        static readonly Rect[] PatrolSource={
            new Rect(.57f,.18f,.12f,.29f),
            new Rect(.64f,.17f,.12f,.29f),
            new Rect(.72f,.15f,.13f,.31f),
            new Rect(.80f,.14f,.12f,.30f),
            new Rect(.87f,.13f,.11f,.29f)
        };
        static readonly Rect[] PatrolPlacement={
            new Rect(.055f,.12f,.19f,.69f),
            new Rect(.22f,.10f,.19f,.74f),
            new Rect(.39f,.07f,.20f,.79f),
            new Rect(.58f,.09f,.19f,.71f),
            new Rect(.76f,.12f,.17f,.65f)
        };
        static readonly Vector2[] Silhouette={
            new Vector2(.48f,.99f),new Vector2(.64f,.93f),
            new Vector2(.69f,.80f),new Vector2(.79f,.72f),
            new Vector2(.93f,.62f),new Vector2(.80f,.54f),
            new Vector2(.73f,.44f),new Vector2(.78f,.26f),
            new Vector2(.69f,.03f),new Vector2(.51f,.08f),
            new Vector2(.37f,.04f),new Vector2(.28f,.30f),
            new Vector2(.26f,.51f),new Vector2(.08f,.60f),
            new Vector2(.25f,.74f),new Vector2(.35f,.81f)
        };

        void PatrolFigure(VertexHelper vh,Rect frame,Rect source,Rect placement,
            float opacity)
        {
            var core=new Vector2(.50f,.52f);
            int first=vh.currentVertCount;
            Color tint=color;
            Vector2 ToWorld(Vector2 unit)=>new Vector2(
                frame.xMin+frame.width*(placement.x+placement.width*unit.x),
                frame.yMin+frame.height*(placement.y+placement.height*unit.y));
            Vector2 ToUv(Vector2 unit)=>OriginalUv(new Vector2(
                source.x+source.width*unit.x,
                source.y+source.height*unit.y));
            vh.AddVert(ToWorld(core),
                new Color(tint.r,tint.g,tint.b,tint.a*opacity),ToUv(core));
            int count=Silhouette.Length;
            for(int i=0;i<count;i++)
            {
                var p=Vector2.Lerp(core,Silhouette[i],.79f);
                vh.AddVert(ToWorld(p),
                    new Color(tint.r,tint.g,tint.b,tint.a*opacity),ToUv(p));
            }
            for(int i=0;i<count;i++)
            {
                var p=Silhouette[i];
                vh.AddVert(ToWorld(p),
                    new Color(tint.r,tint.g,tint.b,0f),ToUv(p));
            }
            for(int i=0;i<count;i++)
            {
                int next=(i+1)%count;
                vh.AddTriangle(first,first+1+i,first+1+next);
                vh.AddTriangle(first+1+i,first+1+count+i,first+1+count+next);
                vh.AddTriangle(first+1+i,first+1+count+next,first+1+next);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if(transparentCreature)
            {
                // The stage-15 Engendro already has AUTHENTIC premultiplied
                // alpha in its production art and must remain unchanged.
                base.OnPopulateMesh(vh);
                return;
            }
            vh.Clear();
            var rect=GetPixelAdjustedRect();
            if(rect.width<16f||rect.height<16f)return;
            // The previous full-scene patch duplicated a fortress on the
            // lower-right landscape. Use only original five patrol silhouettes
            // individually instead; the environment is ONE continuous scene.
            for(int i=0;i<PatrolSource.Length;i++)
                PatrolFigure(vh,rect,PatrolSource[i],PatrolPlacement[i],.93f);
        }

    }
}
