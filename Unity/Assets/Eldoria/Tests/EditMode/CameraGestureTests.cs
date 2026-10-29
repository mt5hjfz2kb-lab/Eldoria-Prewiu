using Eldoria.Presentation;
using Eldoria.ReferenceClean;
using NUnit.Framework;
using Eldoria.World4XPrototype;
using UnityEngine;

namespace Eldoria.Tests.EditMode
{
    public sealed class CameraGestureTests
    {
        [TestCase(36f,110f)]
        [TestCase(64f,110f)]
        [TestCase(36f,280f)]
        [TestCase(64f,280f)]
        public void WorldZoomPresetsRemainDistinctAndOrdered(float near,float panoramic)
        {
            float regional=ValoriaIntegratedWorld.RegionalWorldZoom(near,panoramic);
            Assert.Less(near,regional,"Close world view must remain closest");
            Assert.Less(regional,panoramic,"Panoramic world view must remain farthest");
        }

        [Test]
        public void CityZoomAndWorldPresetsNeverMutateEachOther()
        {
            var city=new GameObject("Isolated city camera");
            var world=new GameObject("Isolated world camera");
            try
            {
                var cityCamera=city.AddComponent<Camera>();
                cityCamera.orthographic=true;
                cityCamera.orthographicSize=30f;
                var cityControls=city.AddComponent<ValoriaReferenceCleanInteraction>();
                cityControls.playerCamera=cityCamera;
                cityControls.gameplayOwnsInput=true;
                var worldCamera=world.AddComponent<Camera>();
                worldCamera.orthographic=true;
                worldCamera.orthographicSize=90f;
                var nav=world.AddComponent<ZoneOneStrategicCamera>();
                float worldBefore=worldCamera.orthographicSize;
                cityControls.ZoomBy(3f);
                Assert.That(worldCamera.orthographicSize,Is.EqualTo(worldBefore).Within(.001f),
                    "Zoom in Valoria must not change the world camera");
                float cityAfter=cityCamera.orthographicSize;
                nav.AllowWholeRegionOverview=true;
                nav.localZoom=36f;nav.regionalZoom=73f;nav.globalZoom=110f;
                nav.SetZoomLevel(2,true);
                Assert.That(worldCamera.orthographicSize,Is.EqualTo(36f).Within(.001f));
                Assert.That(cityCamera.orthographicSize,Is.EqualTo(cityAfter).Within(.001f),
                    "World zoom presets must never change the city camera");
            }
            finally
            {
                Object.DestroyImmediate(world);
                Object.DestroyImmediate(city);
            }
        }

        [Test]
        public void PinchApartZoomsIn()
        {
            var result=SlicePresenter.CalculatePinchZoom(12f,200f,300f,1000f);
            Assert.Less(result,12f);
        }

        [Test]
        public void PinchTogetherZoomsOut()
        {
            var result=SlicePresenter.CalculatePinchZoom(12f,300f,200f,1000f);
            Assert.Greater(result,12f);
        }

        [Test]
        public void PinchZoomClampsToApprovedRange()
        {
            Assert.AreEqual(SlicePresenter.MinOrthographicZoom,
                SlicePresenter.CalculatePinchZoom(9.2f,100f,1000f,1000f),.0001f);
            Assert.AreEqual(SlicePresenter.MaxOrthographicZoom,
                SlicePresenter.CalculatePinchZoom(18.8f,1000f,100f,1000f),.0001f);
        }

        [Test]
        public void InvalidPinchDataKeepsAClampedCamera()
        {
            Assert.AreEqual(12f,SlicePresenter.CalculatePinchZoom(12f,0f,200f,1000f),.0001f);
            Assert.AreEqual(SlicePresenter.MaxOrthographicZoom,
                SlicePresenter.CalculatePinchZoom(25f,0f,200f,1000f),.0001f);
        }
    }
}
