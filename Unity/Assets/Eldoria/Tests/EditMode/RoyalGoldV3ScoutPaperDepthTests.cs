using Eldoria.UI.RoyalGold.V2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    /// <summary>
    /// Real paper and picture-window material stack; no gameplay sprites,
    /// no duplicated event receivers, and only legitimate ScoutOrder states.
    /// </summary>
    public sealed class RoyalGoldV3ScoutPaperDepthTests
    {
        [Test]
        public void ThreeMaterialsRemainIndependentPassiveAndLiveStateDriven()
        {
            var holder=new GameObject("Scout contract in a real UI",
                typeof(RectTransform));
            try
            {
                var parent=(RectTransform)holder.transform;
                parent.sizeDelta=new Vector2(300f,410f);
                for(int kind=0;kind<3;kind++)
                {
                    var paper=RoyalGoldV3ScoutPaperDepth.Install(parent,
                        "Scout / physical parchment-to-desk shadow "+kind,
                        RoyalGoldV3ScoutPaperDepth.MaterialLayer.DeskContact,kind);
                    var under=RoyalGoldV3ScoutPaperDepth.Install(parent,
                        "Scout / inset picture stone and leather underlay "+kind,
                        RoyalGoldV3ScoutPaperDepth.MaterialLayer.InsetUnderlay,kind);
                    var rim=RoyalGoldV3ScoutPaperDepth.Install(parent,
                        "Scout / gilded inset edge and natural occlusion "+kind,
                        RoyalGoldV3ScoutPaperDepth.MaterialLayer.InsetLip,kind);
                    Assert.That(paper.Kind,Is.EqualTo(kind));
                    Assert.That(under.Role,
                        Is.EqualTo(RoyalGoldV3ScoutPaperDepth.MaterialLayer.InsetUnderlay));
                    Assert.That(rim.Role,
                        Is.EqualTo(RoyalGoldV3ScoutPaperDepth.MaterialLayer.InsetLip));
                    foreach(var layer in new[]{paper,under,rim})
                    {
                        Assert.That(layer.raycastTarget,Is.False);
                        Assert.That(layer.mainTexture,
                            Is.SameAs(Texture2D.whiteTexture));
                        Assert.That(layer.InProgress,Is.False);
                        Assert.That(layer.Completed,Is.False);
                        layer.SetLiveState(kind,true,false);
                        Assert.That(layer.InProgress,Is.True);
                        Assert.That(layer.Completed,Is.False);
                        layer.SetLiveState(kind,false,true);
                        Assert.That(layer.InProgress,Is.False);
                        Assert.That(layer.Completed,Is.True);
                        layer.SetLiveState(kind,false,false);
                        Assert.That(layer.Completed,Is.False);
                    }
                }
                Assert.That(holder.GetComponentsInChildren<RoyalGoldV3ScoutPaperDepth>()
                    .Length,Is.EqualTo(9));
                Assert.That(holder.GetComponentsInChildren<Canvas>(true).Length,
                    Is.Zero);
                Assert.That(holder.GetComponentsInChildren<EventSystem>(true).Length,
                    Is.Zero);
                Assert.That(holder.GetComponentsInChildren<Button>(true).Length,
                    Is.Zero);
            }
            finally { Object.DestroyImmediate(holder); }
        }

        [Test]
        public void ContractMaterialsClampKindAndNeverChangeMissionAuthority()
        {
            var holder=new GameObject("Nonfunctional decoration",typeof(RectTransform));
            try
            {
                var material=RoyalGoldV3ScoutPaperDepth.Install(
                    (RectTransform)holder.transform,
                    "paper",RoyalGoldV3ScoutPaperDepth.MaterialLayer.DeskContact,0);
                material.SetLiveState(-200,false,false);
                Assert.That(material.Kind,Is.EqualTo(0));
                material.SetLiveState(300,false,false);
                Assert.That(material.Kind,Is.EqualTo(3));
                Assert.That(material.raycastTarget,Is.False);
                Assert.That(material.GetComponent<RectMask2D>(),Is.Null);
            }
            finally { Object.DestroyImmediate(holder); }
        }
    }
}
