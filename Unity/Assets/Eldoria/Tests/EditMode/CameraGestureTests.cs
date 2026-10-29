using Eldoria.Presentation;
using Eldoria.ReferenceClean;
using NUnit.Framework;

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
