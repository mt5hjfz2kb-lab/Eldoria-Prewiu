using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Artwork-space parallax cut-outs of the *existing* approved Vigilants
    /// landscape. Uses the original texture and its exact RawImage cover UV.
    /// This never creates a camera, duplicate art asset, stage or touch target.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3VigilantsParallax : MaskableGraphic
    {
        static readonly Vector2[] NearLeft =
        {
            new Vector2(.00f,.00f), new Vector2(.00f,.30f),
            new Vector2(.07f,.33f), new Vector2(.17f,.28f),
            new Vector2(.26f,.18f), new Vector2(.34f,.00f)
        };
        static readonly Vector2[] NearRight =
        {
            new Vector2(.69f,.00f), new Vector2(.77f,.20f),
            new Vector2(.85f,.29f), new Vector2(.94f,.33f),
            new Vector2(1.00f,.32f), new Vector2(1.00f,.00f)
        };
        static readonly Vector2[] MiddleCliff =
        {
            new Vector2(.00f,.29f), new Vector2(.00f,.54f),
            new Vector2(.11f,.60f), new Vector2(.24f,.54f),
            new Vector2(.37f,.43f), new Vector2(.30f,.32f)
        };
        static readonly Vector2[] FortressRidge =
        {
            new Vector2(.62f,.58f),new Vector2(.69f,.68f),
            new Vector2(.77f,.85f),new Vector2(.86f,.98f),
            new Vector2(1.00f,.96f),new Vector2(1.00f,.55f)
        };

        Texture2D artwork;
        Rect cover = new Rect(0f,0f,1f,1f);
        float nextRepaint;

        // The source-approved castle/bridge/cliff masks are individually
        // materialized in CI from the UNCHANGED original image. Cropped
        // rectangles use bottom-left source UV; no additional canvas/camera.
        static readonly string[] DepthNames = {
            "left_castle","bridge","right_castle",
            "left_rock","right_cliff","near_cliff"
        };
        static readonly Rect[] DepthUv = {
            new Rect(.05924479f,.58789062f,.30143229f,.38574219f),
            new Rect(.25195312f,.39550781f,.34765625f,.30957031f),
            new Rect(.52539062f,.48632812f,.39778646f,.48730469f),
            new Rect(0f,.30566406f,.39583333f,.53808594f),
            new Rect(.51757812f,.04199219f,.48242188f,.68945312f),
            new Rect(0f,0f,.59895833f,.36816406f)
        };
        static readonly float[] DepthMotion = {
            .0035f,.0018f,.0032f,.0055f,.0060f,.0011f
        };
        readonly RawImage[] planeImages=new RawImage[6];
        readonly RectTransform[] planeRects=new RectTransform[6];
        bool materialized,attemptedMaterialization;
        public bool HasMaterializedDepth => materialized;
        public int MaterializedPlaneCount => materialized ? planeImages.Length : 0;
        public Rect SourceUvForPlane(int index) => DepthUv[Mathf.Clamp(index,0,DepthUv.Length-1)];

        public Texture2D OriginalArtwork => artwork;
        public Rect CoverUv => cover;
        public override Texture mainTexture => artwork ? artwork : Texture2D.whiteTexture;

        public static RoyalGoldV3VigilantsParallax Install(RectTransform parent, int index)
        {
            var go = new GameObject("Vigilants / approved painting depth planes",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(RoyalGoldV3VigilantsParallax));
            go.layer=parent.gameObject.layer;
            var rect=(RectTransform)go.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;
            rect.offsetMax=Vector2.zero;
            rect.SetSiblingIndex(Mathf.Clamp(index,0,parent.childCount-1));
            var graphic=go.GetComponent<RoyalGoldV3VigilantsParallax>();
            graphic.raycastTarget=false;
            graphic.color=Color.white;
            return graphic;
        }

        public void Configure(Texture2D original, Rect exactRawImageCover)
        {
            exactRawImageCover.width=Mathf.Max(.0001f,exactRawImageCover.width);
            exactRawImageCover.height=Mathf.Max(.0001f,exactRawImageCover.height);
            bool changed=artwork!=original ||
                (cover.position-exactRawImageCover.position).sqrMagnitude>.0000001f ||
                (cover.size-exactRawImageCover.size).sqrMagnitude>.0000001f;
            artwork=original;
            cover=exactRawImageCover;
            if(!attemptedMaterialization)
                TryInstallDepthAssets();
            if(materialized)
            {
                PositionPlanesForCover();
                if(changed)SetVerticesDirty();
            }
            else if(changed)SetVerticesDirty();
        }

        void TryInstallDepthAssets()
        {
            attemptedMaterialization=true;
            // Tests and legacy installations keep their safe original shader
            // and approved texture. Never enable optional B10 material for
            // the Scouts or an arbitrary 8x8 fixture.
            if(!artwork || artwork.name!="vigilants-painterly-v2")return;
            var textures=new Texture2D[DepthNames.Length];
            for(int i=0;i<textures.Length;i++)
            {
                textures[i]=Resources.Load<Texture2D>(
                    "EldoriaRetention/DepthB10/"+DepthNames[i]);
                if(!textures[i])return; // all-or-nothing, never partial cut-outs
            }
            for(int i=0;i<textures.Length;i++)
            {
                var go=new GameObject("Vigilants / original paint plane / "+DepthNames[i],
                    typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));
                go.layer=gameObject.layer;
                var rect=(RectTransform)go.transform;
                rect.SetParent(transform,false);
                rect.pivot=new Vector2(.5f,.5f);
                var image=go.GetComponent<RawImage>();
                image.texture=textures[i];
                image.color=Color.white;
                image.raycastTarget=false;
                image.uvRect=new Rect(0f,0f,1f,1f);
                planeImages[i]=image;
                planeRects[i]=rect;
            }
            materialized=true;
            SetVerticesDirty();
        }

        void PositionPlanesForCover()
        {
            if(!materialized)return;
            for(int i=0;i<planeRects.Length;i++)
            {
                var r=planeRects[i];
                var bounds=DepthUv[i];
                r.anchorMin=new Vector2(
                    (bounds.xMin-cover.xMin)/cover.width,
                    (bounds.yMin-cover.yMin)/cover.height);
                r.anchorMax=new Vector2(
                    (bounds.xMax-cover.xMin)/cover.width,
                    (bounds.yMax-cover.yMin)/cover.height);
                r.offsetMin=r.offsetMax=Vector2.zero;
            }
        }

        void Update()
        {
            if(!artwork || !isActiveAndEnabled || !gameObject.activeInHierarchy ||
                Time.unscaledTime<nextRepaint)return;
            // Five mesh updates a second; mesh is small and reuses the
            // existing imported texture. The canonical route stays fixed.
            nextRepaint=Time.unscaledTime+.20f;
            if(materialized)
            {
                // Quiet camera drift: near rock is nearly fixed so the
                // original 25 stage stones NEVER detach from their road.
                float phase=Time.unscaledTime*.35f;
                float minSide=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);
                for(int i=0;i<planeRects.Length;i++)
                {
                    var r=planeRects[i];
                    var k=DepthMotion[i]*minSide;
                    var offset=new Vector2(
                        Mathf.Sin(phase+i*.47f)*k,
                        Mathf.Cos(phase*.72f+i*.51f)*k*.57f);
                    if((r.anchoredPosition-offset).sqrMagnitude>.001f)
                        r.anchoredPosition=offset;
                }
                return;
            }
            SetVerticesDirty();
        }

        Vector2 Map(Rect bounds,Vector2 pixelUv)
        {
            return new Vector2(
                bounds.xMin+bounds.width*(pixelUv.x-cover.x)/cover.width,
                bounds.yMin+bounds.height*(pixelUv.y-cover.y)/cover.height);
        }

        static Color WithAlpha(Color c,float a)
            => new Color(c.r,c.g,c.b,a);

        void Cutout(VertexHelper vh,Rect bounds,Vector2[] polygon,
            Vector2 displacement,Color tone)
        {
            // Each layer has an opaque-ish interior and feathered perimeter;
            // no rectangular photographic fragments may become visible.
            int count=polygon.Length;
            var center=Vector2.zero;
            for(int i=0;i<count;i++)center+=polygon[i];
            center/=count;
            int start=vh.currentVertCount;
            for(int i=0;i<count;i++)
            {
                Vector2 inside=Vector2.Lerp(center,polygon[i],.88f);
                vh.AddVert(Map(bounds,inside)+displacement,tone,inside);
            }
            for(int i=0;i<count;i++)
                vh.AddVert(Map(bounds,polygon[i])+displacement,
                    WithAlpha(tone,0f),polygon[i]);
            // Convex area contours authored against the reference image.
            for(int i=1;i<count-1;i++)
                vh.AddTriangle(start,start+i,start+i+1);
            for(int i=0;i<count;i++)
            {
                int j=(i+1)%count;
                vh.AddTriangle(start+i,start+count+i,start+count+j);
                vh.AddTriangle(start+i,start+count+j,start+j);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(materialized)return; // actual separated PNG depth planes render as children
            if(!artwork)return;
            var bounds=GetPixelAdjustedRect();
            if(bounds.width<36f||bounds.height<36f)return;
            float unit=Mathf.Min(bounds.width,bounds.height)/600f;
            float t=Time.unscaledTime;
            float slow=Mathf.Sin(t*.29f);
            float slower=Mathf.Sin(t*.17f+1.4f);
            // Far citadel moves almost imperceptibly, middle rock face
            // less, and near vegetation/waterfall banks more. Painting pixels
            // are sampled from their own original UV, not a second image.
            Cutout(vh,bounds,FortressRidge,
                new Vector2(.95f*slower,.44f*slow)*unit,
                new Color(1f,.93f,.85f,.41f));
            Cutout(vh,bounds,MiddleCliff,
                new Vector2(3.8f*slow,2.1f*slower)*unit,
                new Color(.96f,.98f,1f,.60f));
            Cutout(vh,bounds,NearLeft,
                new Vector2(-4.0f+12f*slow,5.0f*slower)*unit,
                new Color(1f,.96f,.88f,.91f));
            Cutout(vh,bounds,NearRight,
                new Vector2(4.4f-13f*slow,5.2f*slower)*unit,
                new Color(.94f,1f,.94f,.89f));
        }
    }

}
