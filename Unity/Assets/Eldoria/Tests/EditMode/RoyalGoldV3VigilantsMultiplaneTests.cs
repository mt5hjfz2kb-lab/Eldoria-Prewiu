using Eldoria.UI.RoyalGold.V2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    /// <summary>
    /// Exact materialized B10 painterly source proof. An outline/mask without
    /// the six real approved PNG resources must fail, not silently PASS as B10.
    /// </summary>
    public sealed class RoyalGoldV3VigilantsMultiplaneTests
    {
        static readonly string[] Layers = {
            "left_castle","bridge","right_castle",
            "left_rock","right_cliff","near_cliff"
        };

        static readonly Vector2Int[] Dimensions = {
            new Vector2Int(463,395),new Vector2Int(534,317),
            new Vector2Int(611,499),new Vector2Int(608,551),
            new Vector2Int(741,706),new Vector2Int(920,377)
        };

        [Test]
        public void SixSeparateOriginalPainterlyLayersCannotBecomeAnotherPhotoOrTouchBlocker()
        {
            var approved=Resources.Load<Texture2D>(
                "EldoriaRetention/vigilants-painterly-v2");
            Assert.That(approved,Is.Not.Null);
            var parent=new GameObject("Original real scene",typeof(RectTransform));
            try
            {
                var area=(RectTransform)parent.transform;
                area.sizeDelta=new Vector2(980f,620f);
                var parallax=RoyalGoldV3VigilantsParallax.Install(area,0);
                var uv=new Rect(.04f,.09f,.88f,.79f);
                parallax.Configure(approved,uv);
                Assert.That(parallax.HasMaterializedDepth,Is.True,
                    "The new depth test MUST NOT pass with only the old flat mesh");
                Assert.That(parallax.MaterializedPlaneCount,Is.EqualTo(6));
                Assert.That(parallax.OriginalArtwork,Is.SameAs(approved));
                Assert.That(parallax.CoverUv,Is.EqualTo(uv));
                Assert.That(parallax.raycastTarget,Is.False);
                Assert.That(parent.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(parent.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);

                var children=parallax.GetComponentsInChildren<RawImage>(true);
                Assert.That(children.Length,Is.EqualTo(6));
                for(int i=0;i<Layers.Length;i++)
                {
                    var art=Resources.Load<Texture2D>(
                        "EldoriaRetention/DepthB10/"+Layers[i]);
                    Assert.That(art,Is.Not.Null,Layers[i]+" source must be imported");
                    Assert.That(art.width,Is.EqualTo(Dimensions[i].x),Layers[i]);
                    Assert.That(art.height,Is.EqualTo(Dimensions[i].y),Layers[i]);
                    var plane=parallax.transform.Find(
                        "Vigilants / original paint plane / "+Layers[i]);
                    Assert.That(plane,Is.Not.Null);
                    var picture=plane.GetComponent<RawImage>();
                    Assert.That(picture,Is.Not.Null);
                    Assert.That(picture.texture,Is.SameAs(art));
                    Assert.That(picture.raycastTarget,Is.False,
                        "No parallax artwork can cover real controls");
                    Assert.That(picture.uvRect,Is.EqualTo(new Rect(0f,0f,1f,1f)));
                    var rect=picture.rectTransform;
                    var bounds=parallax.SourceUvForPlane(i);
                    Assert.That(rect.anchorMin.x,
                        Is.EqualTo((bounds.xMin-uv.xMin)/uv.width).Within(.00001f));
                    Assert.That(rect.anchorMin.y,
                        Is.EqualTo((bounds.yMin-uv.yMin)/uv.height).Within(.00001f));
                    Assert.That(rect.anchorMax.x,
                        Is.EqualTo((bounds.xMax-uv.xMin)/uv.width).Within(.00001f));
                    Assert.That(rect.anchorMax.y,
                        Is.EqualTo((bounds.yMax-uv.yMin)/uv.height).Within(.00001f));
                }
            }
            finally { Object.DestroyImmediate(parent); }
        }

        [Test]
        public void ScoutsNeverReceiveVigilantsDepthAssets()
        {
            var parent=new GameObject("Non-Vigilants reference room",
                typeof(RectTransform));
            var unrelated=new Texture2D(8,8,TextureFormat.RGBA32,false);
            unrelated.name="scout-view-fixture";
            try
            {
                var parallax=RoyalGoldV3VigilantsParallax.Install(
                    (RectTransform)parent.transform,0);
                parallax.Configure(unrelated,new Rect(0f,0f,1f,1f));
                Assert.That(parallax.HasMaterializedDepth,Is.False);
                Assert.That(parallax.MaterializedPlaneCount,Is.Zero);
                Assert.That(parallax.GetComponentsInChildren<RawImage>(true).Length,
                    Is.Zero);
                Assert.That(parallax.raycastTarget,Is.False);
            }
            finally
            {
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(unrelated);
            }
        }
    }
}
