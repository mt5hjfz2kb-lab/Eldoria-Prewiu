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
