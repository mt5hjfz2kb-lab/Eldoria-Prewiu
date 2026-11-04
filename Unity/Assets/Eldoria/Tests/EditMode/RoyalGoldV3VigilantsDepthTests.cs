using Eldoria.UI.RoyalGold.V2;
using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    /// <summary>
    /// Zero-gameplay art pass. Tests deliberately create no battle/gateway/save:
    /// layers must draw against existing art, never create an alternate scene.
    /// </summary>
    public sealed class RoyalGoldV3VigilantsDepthTests
    {
        [Test]
        public void ApprovedPainterlyArtIsReusedWithExactCoverCrop()
        {
            var scene=new GameObject("Existing Vigilants scene",typeof(RectTransform));
            var texture=new Texture2D(8,8,TextureFormat.RGBA32,false);
            try
            {
                var rect=(RectTransform)scene.transform;
                var original=new GameObject("Original approved RawImage",
                    typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));
                original.transform.SetParent(scene.transform,false);
                var source=original.GetComponent<RawImage>();
                source.texture=texture;
                source.uvRect=new Rect(.09f,.02f,.79f,.96f);
                var planes=RoyalGoldV3VigilantsParallax.Install(rect,1);
                planes.Configure(texture,source.uvRect);

                Assert.That(planes,Is.Not.Null);
                Assert.That(planes.raycastTarget,Is.False);
                Assert.That(planes.mainTexture,Is.SameAs(texture));
                Assert.That(planes.OriginalArtwork,Is.SameAs(source.texture));
                Assert.That(planes.CoverUv,Is.EqualTo(source.uvRect));
                Assert.That(planes.rectTransform.anchorMin,Is.EqualTo(Vector2.zero));
                Assert.That(planes.rectTransform.anchorMax,Is.EqualTo(Vector2.one));
                Assert.That(scene.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(scene.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(scene);
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void GuardianAndEngendroGroundingKeepTruePortraitAndAllTouches()
        {
            var frame=new GameObject("Existing encounter card",typeof(RectTransform));
            try
            {
                var rect=(RectTransform)frame.transform;
                var realPortrait=new GameObject("Real portrait",typeof(RectTransform),
                    typeof(CanvasRenderer),typeof(RawImage));
                realPortrait.transform.SetParent(frame.transform,false);
                var image=realPortrait.GetComponent<RawImage>();
                var back=RoyalGoldV3EncounterGrounding.Install(rect,false);
                var front=RoyalGoldV3EncounterGrounding.Install(rect,true);
                back.Configure(true,true);front.Configure(true,true);
                Assert.That(back.raycastTarget,Is.False);
                Assert.That(front.raycastTarget,Is.False);
                Assert.That(back.Front,Is.False);
                Assert.That(front.Front,Is.True);
                Assert.That(front.Breach,Is.True);
                Assert.That(front.Guardian,Is.True);
                Assert.That(frame.GetComponentsInChildren<RawImage>(true).Length,Is.EqualTo(1));
                Assert.That(frame.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(frame.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
                Assert.That(frame.GetComponentsInChildren<Graphic>(true)
                    .Where(g=>g.raycastTarget).Count(),Is.EqualTo(1),
                    "Only the unmodified real portrait may retain its original hit flag");
                // Original texture is not replaced by either layer.
                Assert.That(image,Is.SameAs(realPortrait.GetComponent<RawImage>()));
                back.Configure(false,false);front.Configure(false,false);
                Assert.That(front.Breach,Is.False);
                Assert.That(back.Guardian,Is.False);
            }
            finally { Object.DestroyImmediate(frame); }
        }

        [Test]
        public void PainterlyPortraitPreservesAuthenticRawImageApiAndUvForStage15()
        {
            var host=new GameObject("Encounter",typeof(RectTransform));
            var source=new Texture2D(8,8,TextureFormat.RGBA32,false);
            try
            {
                var portrait=new GameObject("Authentic illustrated encounter",
                    typeof(RectTransform),typeof(CanvasRenderer),
                    typeof(RoyalGoldV3VigilantsPortraitGraphic));
                portrait.transform.SetParent(host.transform,false);
                var renderer=portrait.GetComponent<RoyalGoldV3VigilantsPortraitGraphic>();
                renderer.texture=source;
                renderer.uvRect=new Rect(.01f,.51f,.48f,.48f);
                renderer.raycastTarget=false;
                renderer.Configure(false);
                Assert.That(renderer.TransparentCreature,Is.False);
                Assert.That(renderer.PatrolSilhouettes,Is.True,
                    "Normal patrols render separate authentic figures, not a full photo");
                Assert.That(renderer.mainTexture,Is.SameAs(source));
                Assert.That(renderer.uvRect,Is.EqualTo(new Rect(.01f,.51f,.48f,.48f)));
                Assert.That(portrait.GetComponent<RawImage>(),Is.SameAs(renderer));
                renderer.Configure(true);
                Assert.That(renderer.TransparentCreature,Is.True,
                    "The real alpha silhouette must NOT be clipped to a new mask");
                Assert.That(renderer.texture,Is.SameAs(source));
                Assert.That(renderer.raycastTarget,Is.False);
                Assert.That(host.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void ScenicEnemyAndTerrainReuseOriginalArtworkWithoutAdditionalCanvas()
        {
            var scene=new GameObject("Scenic illustration",typeof(RectTransform));
            var approved=new Texture2D(16,16,TextureFormat.RGBA32,false);
            try
            {
                var bounds=(RectTransform)scene.transform;
                var source=new GameObject("Original illustrated world / decorative only",
                    typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));
                source.transform.SetParent(scene.transform,false);
                var painting=source.GetComponent<RawImage>();
                painting.texture=approved;
                painting.uvRect=new Rect(.08f,.07f,.84f,.86f);
                var occlusion=RoyalGoldV3VigilantsTerrainOcclusion.Install(bounds,1);
                occlusion.Configure(approved,painting.uvRect);
                Assert.That(occlusion.OriginalArtwork,Is.SameAs(painting.texture));
                Assert.That(occlusion.mainTexture,Is.SameAs(approved));
                Assert.That(occlusion.CoverUv,Is.EqualTo(painting.uvRect));
                Assert.That(occlusion.raycastTarget,Is.False);
                Assert.That(occlusion.transform.parent,Is.SameAs(scene.transform));
                Assert.That(scene.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(scene.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(scene);
                Object.DestroyImmediate(approved);
            }
        }

        [Test]
        public void LiveStageLightUsesOnlyExistingRoadAndCannotTakeInput()
        {
            var room=new GameObject("Royal diorama",typeof(RectTransform));
            try
            {
                var light=RoyalGoldV3VigilantsWorldLight.Install(
                    (RectTransform)room.transform,0);
                light.SetState(new Rect(.10f,.12f,.73f,.76f),
                    14,15,new Vector2(25f,40f));
                Assert.That(light.raycastTarget,Is.False);
                Assert.That(light.LastCleared,Is.EqualTo(14));
                Assert.That(light.CurrentStage,Is.EqualTo(15));
                Assert.That(light.CoverUv,
                    Is.EqualTo(new Rect(.10f,.12f,.73f,.76f)));
                // Recycled live-stage markers move the illumination without
                // adding a second node or anything interactable.
                light.SetState(new Rect(.10f,.12f,.73f,.76f),
                    19,20,new Vector2(39f,56f));
                Assert.That(light.LastCleared,Is.EqualTo(19));
                Assert.That(light.CurrentStage,Is.EqualTo(20));
                Assert.That(light.raycastTarget,Is.False);
                Assert.That(room.GetComponentsInChildren<Canvas>(true).Length,
                    Is.Zero);
                Assert.That(room.GetComponentsInChildren<EventSystem>(true).Length,
                    Is.Zero);
            }
            finally { Object.DestroyImmediate(room); }
        }

        [Test]
        public void EngendroUsesOriginalAlphaWithSharedCardLighting()
        {
            var room=new GameObject("Royal encounter",typeof(RectTransform));
            try
            {
                var r=new GameObject("Shared stone and ambient backlight",
                    typeof(RectTransform),typeof(CanvasRenderer),
                    typeof(RoyalGoldV3GuardianReliefGraphic));
                r.transform.SetParent(room.transform,false);
                var graphic=r.GetComponent<RoyalGoldV3GuardianReliefGraphic>();
                graphic.raycastTarget=false;
                graphic.SetStage(15);
                graphic.SetBreach(true);
                graphic.UsePaintedFigure(true);
                Assert.That(graphic.StageStep,Is.Zero);
                Assert.That(graphic.PaintedFigure,Is.True);
                Assert.That(graphic.Breach,Is.True);
                Assert.That(graphic.raycastTarget,Is.False);
                Assert.That(room.GetComponentsInChildren<RawImage>(true).Length,
                    Is.Zero,"Card lighting does not contain a fake photo");
                graphic.SetBreach(false);
                Assert.That(graphic.Breach,Is.False);
            }
            finally { Object.DestroyImmediate(room); }
        }

        [TestCase(1,0)]
        [TestCase(5,5)]
        [TestCase(10,10)]
        [TestCase(15,0)]
        [TestCase(20,20)]
        [TestCase(25,25)]
        public void GuardianReliefIsOriginalNonPhotographicArtAndNeverCapturesTaps(
            int actualStage,int expectedRelief)
        {
            var host=new GameObject("Authentic royal manuscript",typeof(RectTransform));
            try
            {
                var relief=new GameObject("Guardian / embossed visual only",
                    typeof(RectTransform),typeof(CanvasRenderer),
                    typeof(RoyalGoldV3GuardianReliefGraphic));
                relief.transform.SetParent(host.transform,false);
                var graphic=relief.GetComponent<RoyalGoldV3GuardianReliefGraphic>();
                graphic.raycastTarget=false;
                graphic.SetStage(actualStage);
                Assert.That(graphic.StageStep,Is.EqualTo(expectedRelief));
                graphic.UsePaintedFigure(expectedRelief!=0);
                Assert.That(graphic.PaintedFigure,Is.EqualTo(expectedRelief!=0));
                graphic.UsePaintedFigure(false);
                Assert.That(graphic.PaintedFigure,Is.False,
                    "The stone/ink fallback remains when a candidate PNG is absent");
                Assert.That(graphic.raycastTarget,Is.False);
                Assert.That(relief.GetComponent<RawImage>(),Is.Null,
                    "The actual guardian must not be another pasted photographic rectangle");
                Assert.That(host.GetComponentsInChildren<RawImage>(true).Length,Is.Zero);
                Assert.That(host.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(host.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
            }
            finally { Object.DestroyImmediate(host); }
        }

        [Test]
        public void LivingLandscapeUsesOneApprovedTextureAndAuthoritativeRealStages()
        {
            var host=new GameObject("Original scenic illustration",typeof(RectTransform));
            var tex=new Texture2D(16,16,TextureFormat.RGBA32,false);
            try
            {
                var scene=(RectTransform)host.transform;
                scene.sizeDelta=new Vector2(900f,640f);
                var picture=new GameObject("Approved landscape",
                    typeof(RectTransform),typeof(CanvasRenderer),
                    typeof(RoyalGoldV3VigilantsLivingLandscape));
                picture.transform.SetParent(host.transform,false);
                var painting=picture.GetComponent<RoyalGoldV3VigilantsLivingLandscape>();
                Assert.That(typeof(RoyalGoldV3VigilantsLivingLandscape).Assembly,
                    Is.SameAs(typeof(RoyalGoldV3VigilantsParallax).Assembly),
                    "The one-texture landscape must be included in the existing Unity UI assembly");
                painting.texture=tex;
                painting.raycastTarget=false;
                painting.rectTransform.sizeDelta=new Vector2(900f,640f);
                painting.uvRect=new Rect(.08f,.04f,.84f,.92f);
                var stage=new GameObject("True stage trail",
                    typeof(RectTransform),typeof(CanvasRenderer),
                    typeof(RoyalGoldV3StageTrailGraphic));
                stage.transform.SetParent(host.transform,false);
                var trail=stage.GetComponent<RoyalGoldV3StageTrailGraphic>();
                trail.rectTransform.sizeDelta=new Vector2(900f,640f);
                trail.ArtworkUv=painting.uvRect;
                trail.raycastTarget=false;
                painting.SetLivingScene(true,trail,14);
                Assert.That(painting.LivingScene,Is.True);
                Assert.That(painting.RoadStageCount,Is.EqualTo(25));
                Assert.That(painting.ActualCleared,Is.EqualTo(14));
                Assert.That(painting.texture,Is.SameAs(tex),
                    "One original image remains the only texture");
                Assert.That(painting.uvRect,Is.EqualTo(new Rect(.08f,.04f,.84f,.92f)));
                Assert.That(painting.raycastTarget,Is.False);
                painting.SetLivingScene(false,null,14);
                Assert.That(painting.LivingScene,Is.False,
                    "Other Royal destinations must still use the original raw quad");
                Assert.That(host.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(host.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void RealRoadStoneFoundationFollowsEveryActualStageAndNoTouch()
        {
            var host=new GameObject("Existing scenic manuscript",typeof(RectTransform));
            try
            {
                var area=(RectTransform)host.transform;
                area.sizeDelta=new Vector2(900f,640f);
                var trailObject=new GameObject("True real stage road",
                    typeof(RectTransform),typeof(CanvasRenderer),
                    typeof(RoyalGoldV3StageTrailGraphic));
                var rt=(RectTransform)trailObject.transform;
                rt.SetParent(host.transform,false);
                rt.sizeDelta=new Vector2(900f,640f);
                var trail=trailObject.GetComponent<RoyalGoldV3StageTrailGraphic>();
                trail.raycastTarget=false;
                trail.ArtworkUv=new Rect(0f,0f,1f,1f);
                var foundation=RoyalGoldV3VigilantsRoadFoundation.Install(area,0);
                foundation.BindRealRoad(trail,4);
                Assert.That(foundation.BoundStageCount,Is.EqualTo(25));
                Assert.That(foundation.Cleared,Is.EqualTo(4));
                Assert.That(foundation.NextStage,Is.EqualTo(5));
                Assert.That(foundation.raycastTarget,Is.False);
                for(int step=1;step<=25;step++)
                {
                    var actual=foundation.rectTransform.InverseTransformPoint(
                        trail.transform.TransformPoint(trail.StagePosition(step)));
                    Assert.That(Vector2.Distance(foundation.StagePosition(step),actual),
                        Is.LessThan(.01f),
                        "Real stage "+step+" shares the existing in-world road");
                }
                foundation.BindRealRoad(trail,14);
                Assert.That(foundation.NextStage,Is.EqualTo(15));
                foundation.BindRealRoad(trail,24);
                Assert.That(foundation.NextStage,Is.EqualTo(25));
                Assert.That(host.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(host.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
                Assert.That(host.GetComponentsInChildren<RawImage>(true).Length,Is.Zero,
                    "Stone relief never adds another photograph to the scene");
            }
            finally { Object.DestroyImmediate(host); }
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(15)]
        [TestCase(20)]
        [TestCase(25)]
        public void OriginalStageProjectionAndFiveGuardianCoordinatesRemain(int stage)
        {
            var node=new GameObject("Original approved 25-stage road",
                typeof(RectTransform),typeof(CanvasRenderer),
                typeof(RoyalGoldV3StageTrailGraphic));
            try
            {
                var trail=node.GetComponent<RoyalGoldV3StageTrailGraphic>();
                // MaskableGraphic defaults to raycastTarget=true on direct
                // AddComponent; the real screen sets it false at installation.
                trail.raycastTarget=false;
                trail.rectTransform.sizeDelta=new Vector2(800f,620f);
                trail.ArtworkUv=new Rect(0f,0f,1f,1f);
                var point=trail.StagePosition(stage);
                Assert.That(float.IsNaN(point.x)||float.IsNaN(point.y),Is.False);
                Assert.That(trail.rectTransform.rect.Contains(point),Is.True);
                Assert.That(trail.raycastTarget,Is.False);
                Assert.That(trail.StagePosition(stage),Is.EqualTo(point),
                    "The independent environmental planes never move stage coordinates");
            }
            finally { Object.DestroyImmediate(node); }
        }
    }
}
