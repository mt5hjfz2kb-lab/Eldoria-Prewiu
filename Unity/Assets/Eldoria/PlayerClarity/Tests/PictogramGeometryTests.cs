using System.Collections.Generic;
using System.Reflection;
using Eldoria.PlayerClarity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class PictogramGeometryTests
    {
        [TestCase(ResourcePictogramKind.Wood)]
        [TestCase(ResourcePictogramKind.Stone)]
        [TestCase(ResourcePictogramKind.Food)]
        public void EveryResourceHasNontrivialMultitoneGeometry(ResourcePictogramKind kind)
        {
            var go = new GameObject("Geometry probe", typeof(RectTransform));
            try
            {
                var rect = (RectTransform)go.transform;
                rect.sizeDelta = new Vector2(24f, 24f);
                var graphic = go.AddComponent<RoyalResourcePictogram>();
                graphic.Kind = kind;
                using (var helper = new VertexHelper())
                {
                    var populate = typeof(RoyalResourcePictogram).GetMethod(
                        "OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.That(populate, Is.Not.Null);
                    populate.Invoke(graphic, new object[] {helper});
                    Assert.That(helper.currentVertCount, Is.GreaterThan(20));
                    Assert.That(helper.currentIndexCount, Is.GreaterThan(30));
                    var stream = new List<UIVertex>();
                    helper.GetUIVertexStream(stream);
                    var tones = new HashSet<Color32>();
                    foreach (var vertex in stream) tones.Add(vertex.color);
                    Assert.That(tones.Count, Is.GreaterThan(3));
                    foreach (var vertex in stream)
                    {
                        Assert.That(float.IsNaN(vertex.position.x), Is.False);
                        Assert.That(float.IsNaN(vertex.position.y), Is.False);
                        Assert.That(vertex.position.x, Is.InRange(-13f, 13f));
                        Assert.That(vertex.position.y, Is.InRange(-13f, 13f));
                    }
                }
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
