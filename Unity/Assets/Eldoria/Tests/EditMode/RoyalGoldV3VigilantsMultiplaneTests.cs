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
                    "The depth test MUST NOT pass with only the old flat mesh");
                Assert.That(parallax.HasStructuralB11,Is.True,
                    "Reject previous B10: six genuine lit planes must be paired with the clean backplate");
                Assert.That(Resources.Load<Texture2D>(
                    "EldoriaRetention/DepthB11/clean_backdrop"),Is.Not.Null,
                    "A single complete original paint cannot remain underneath all foreground planes");
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
                        "EldoriaRetention/DepthB11/"+Layers[i]);
                    Assert.That(art,Is.Not.Null,Layers[i]+" source must be imported");
                    Assert.That(art.name,Is.EqualTo(Layers[i]),
                        "Exact original cut-out must not silently use a different asset");
                    Assert.That(art.width,Is.EqualTo(Dimensions[i].x),Layers[i]);
                    Assert.That(art.height,Is.EqualTo(Dimensions[i].y),Layers[i]);
                    RawImage picture=null;
                    // Unity's Transform.Find treats the slash as a path
                    // delimiter, but the actual imported source child name
                    // itself contains slashes. Match direct children by exact
                    // name: do not weaken the asset-identity assertion.
                    foreach(var item in children)
                        if(item.name=="Vigilants / original paint plane / "+Layers[i])
                        { picture=item; break; }
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
        public void TwentyFourActualStoneTerracesFollowExactlyTwentyFiveRealProgressionPositions()
        {
            var room=new GameObject("Real 25-stage terrace fixture",
                typeof(RectTransform));
            try
            {
                var area=(RectTransform)room.transform;
                area.sizeDelta=new Vector2(900f,600f);
                var stage=new GameObject("Actual route",typeof(RectTransform),
                    typeof(CanvasRenderer),typeof(RoyalGoldV3StageTrailGraphic));
                var stageRect=(RectTransform)stage.transform;
                stageRect.SetParent(area,false);
                stageRect.anchorMin=Vector2.zero;
                stageRect.anchorMax=Vector2.one;
                stageRect.offsetMin=stageRect.offsetMax=Vector2.zero;
                var route=stage.GetComponent<RoyalGoldV3StageTrailGraphic>();
                route.ArtworkUv=new Rect(0f,0f,1f,1f);
                var foundation=RoyalGoldV3VigilantsRoadFoundation.Install(area,0);
                foundation.BindRealRoad(route,14);
                Assert.That(foundation.BoundStageCount,Is.EqualTo(25));
                Assert.That(foundation.ActualReliefSpans,Is.EqualTo(24),
                    "The physical terrain must have all 24 continuous real spans");
                Assert.That(Vector2.Distance(
                    foundation.StagePosition(1),foundation.StagePosition(25)),
                    Is.GreaterThan(40f),
                    "All real stages must extend across the route, not collapse into the card");
                Assert.That(foundation.Cleared,Is.EqualTo(14));
                Assert.That(foundation.NextStage,Is.EqualTo(15));
                for(int i=1;i<=25;i++)
                {
                    var actual=foundation.rectTransform.InverseTransformPoint(
                        stageRect.TransformPoint(route.StagePosition(i)));
                    Assert.That(Vector2.Distance(actual,foundation.StagePosition(i)),
                        Is.LessThan(.1f),
                        "Terrace "+i+" must be bound to a true gameplay stage");
                }
                Assert.That(foundation.raycastTarget,Is.False);
                Assert.That(room.GetComponentsInChildren<Canvas>(true).Length,
                    Is.Zero);
                Assert.That(room.GetComponentsInChildren<EventSystem>(true).Length,
                    Is.Zero);
            }
            finally { Object.DestroyImmediate(room); }
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
                Assert.That(parallax.HasStructuralB11,Is.False);
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
