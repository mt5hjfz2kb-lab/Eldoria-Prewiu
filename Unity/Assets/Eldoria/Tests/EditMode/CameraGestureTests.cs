using Eldoria.Presentation;
using Eldoria.World4XPrototype;
using NUnit.Framework;

namespace Eldoria.Tests.EditMode
{
    public sealed class CameraGestureTests
    {
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
        public void WorldZoomPresetsFollowNearRegionalPanoramicWithoutCityOverlap()
        {
            Assert.Less(ZoneOneStrategicCamera.NearLandscape, ZoneOneStrategicCamera.NearPortrait);
            Assert.Less(ZoneOneStrategicCamera.NearPortrait, ZoneOneStrategicCamera.Regional);
            Assert.Less(ZoneOneStrategicCamera.Regional, ZoneOneStrategicCamera.Panoramic);
            Assert.That(ZoneOneStrategicCamera.NearLandscape, Is.GreaterThan(SlicePresenter.MaxOrthographicZoom),
                "World near scale is not the Valoria city scale.");
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
