using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Reprojects the APPROVED foreground rock and foliage from the source
    /// painting over an encounter standing INSIDE the world. This is genuine
    /// terrain occlusion, not a studio matte, rectangular overlay or new photo.
    /// Stage markers and UI are always drawn above the terrain.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3VigilantsTerrainOcclusion : MaskableGraphic
    {
        Texture2D source;
        Rect cover=new Rect(0f,0f,1f,1f);
        public Texture2D OriginalArtwork=>source;
        public Rect CoverUv=>cover;
        public override Texture mainTexture=>source?source:Texture2D.whiteTexture;

        // Hand-identified existing near-ground banks and scattered trees.
        // UV coordinates are in the approved picture, not on the screen.
        static readonly Vector2[] RightForeground={
            new Vector2(.58f,0f),new Vector2(1f,0f),
            new Vector2(1f,.24f),new Vector2(.97f,.21f),
            new Vector2(.90f,.18f),new Vector2(.83f,.24f),
            new Vector2(.78f,.18f),new Vector2(.68f,.19f)
        };
        static readonly Vector2[] LowerBoulders={
            new Vector2(.48f,0f),new Vector2(.83f,0f),
            new Vector2(.83f,.13f),new Vector2(.75f,.17f),
            new Vector2(.68f,.12f),new Vector2(.61f,.16f),
            new Vector2(.53f,.10f)
        };
        static readonly Vector2[] FrontTrees={
            new Vector2(.83f,.03f),new Vector2(1f,.03f),
            new Vector2(1f,.46f),new Vector2(.965f,.41f),
            new Vector2(.925f,.34f),new Vector2(.89f,.25f)
        };

        public static RoyalGoldV3VigilantsTerrainOcclusion Install(
            RectTransform parent, int sibling)
        {
            var obj=new GameObject("Vigilants / same-landscape front rock and foliage",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(RoyalGoldV3VigilantsTerrainOcclusion));
            obj.layer=parent.gameObject.layer;
            var rect=(RectTransform)obj.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            rect.SetSiblingIndex(Mathf.Clamp(sibling,0,parent.childCount-1));
            var g=obj.GetComponent<RoyalGoldV3VigilantsTerrainOcclusion>();
            g.raycastTarget=false;
            g.color=Color.white;
            return g;
        }

        public void Configure(Texture2D original, Rect uv)
        {
            uv.width=Mathf.Max(.0001f,uv.width);
            uv.height=Mathf.Max(.0001f,uv.height);
            if(source==original && cover==uv)return;
            source=original;
            cover=uv;
            SetVerticesDirty();
        }

        void Fragment(VertexHelper vh,Rect rect,Vector2[] contour,float opacity)
        {
            var centroid=Vector2.zero;
            for(int i=0;i<contour.Length;i++)centroid+=contour[i];
            centroid/=contour.Length;
            var start=vh.currentVertCount;
            Vector2 ToPosition(Vector2 uv)=>new Vector2(
                rect.xMin+rect.width*(uv.x-cover.x)/cover.width,
                rect.yMin+rect.height*(uv.y-cover.y)/cover.height);
            for(int i=0;i<contour.Length;i++)
            {
                var interior=Vector2.Lerp(centroid,contour[i],.80f);
                vh.AddVert(ToPosition(interior),
                    new Color(1f,1f,1f,opacity),interior);
            }
            for(int i=0;i<contour.Length;i++)
                vh.AddVert(ToPosition(contour[i]),new Color(1f,1f,1f,0f),contour[i]);
            for(int i=1;i<contour.Length-1;i++)
                vh.AddTriangle(start,start+i,start+i+1);
            for(int i=0;i<contour.Length;i++)
            {
                int next=(i+1)%contour.Length, count=contour.Length;
                vh.AddTriangle(start+i,start+count+i,start+count+next);
                vh.AddTriangle(start+i,start+count+next,start+next);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(!source)return;
            var r=GetPixelAdjustedRect();
            if(r.width<48f||r.height<48f)return;
            // The separate near-ground polygons are actual paint sampled
            // from matching coordinates. Only their soft edges are new.
            Fragment(vh,r,LowerBoulders,.91f);
            Fragment(vh,r,RightForeground,.97f);
            Fragment(vh,r,FrontTrees,.88f);
        }
    }
}
