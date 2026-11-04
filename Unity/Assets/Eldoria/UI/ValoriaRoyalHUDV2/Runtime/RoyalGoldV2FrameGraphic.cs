using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Resolution-independent Royal Gold V2 frame. Antique gold, slate-blue
    /// glass and restrained corner relief; all geometry is local uGUI vertices.
    /// No textures, paid assets, runtime allocations after mesh rebuild or
    /// new shaders. Editable on a UI prefab and compatible with raycasts.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/Royal Gold V2 Frame")]
    public sealed class RoyalGoldV2FrameGraphic : MaskableGraphic
    {
        public bool Primary;
        public bool AntiqueGoldAction;
        public bool DecorativeCorners = true;

        private static readonly Color Shadow = new Color(.006f, .012f, .021f, .80f);
        private static readonly Color RimLow = new Color(.27f, .21f, .125f, 1f);
        private static readonly Color RimHigh = new Color(.79f, .64f, .365f, 1f);
        private static readonly Color Edge = new Color(.065f, .095f, .135f, 1f);
        private static readonly Color Ink = new Color(.029f, .046f, .077f, .865f);
        private static readonly Color Slate = new Color(.065f, .105f, .159f, .925f);
        private static readonly Color ActionBase = new Color(.082f, .172f, .265f, 1f);
        private static readonly Color ActionTop = new Color(.13f, .30f, .435f, 1f);

        private void FillPolygon(VertexHelper mesh, Vector2[] shape, Color low, Color high)
        {
            float bottom = float.MaxValue, top = float.MinValue;
            Vector2 center = Vector2.zero;
            foreach (var p in shape)
            {
                center += p;
                bottom = Mathf.Min(bottom, p.y);
                top = Mathf.Max(top, p.y);
            }
            center /= shape.Length;
            int offset = mesh.currentVertCount;
            mesh.AddVert(center, UnityColor(Color.Lerp(low, high, .5f)), Vector2.zero);
            foreach (var p in shape)
                mesh.AddVert(p, UnityColor(Color.Lerp(low, high, Mathf.InverseLerp(bottom, top, p.y))), Vector2.zero);
            for (int i = 0; i < shape.Length; ++i)
                mesh.AddTriangle(offset, offset + i + 1, offset + ((i + 1) % shape.Length) + 1);
        }

        private Color UnityColor(Color original) =>
            new Color(original.r * color.r, original.g * color.g, original.b * color.b,
                original.a * color.a);

        private void Plate(VertexHelper mesh, Rect outer, float inset, Color low, Color high)
        {
            var r = new Rect(outer.xMin + inset, outer.yMin + inset,
                outer.width - 2f * inset, outer.height - 2f * inset);
            if (r.width < 3f || r.height < 3f) return;
            float cut = Mathf.Min(9.5f, Mathf.Min(r.width, r.height) * .12f);
            FillPolygon(mesh, new[]
            {
                new Vector2(r.xMin + cut, r.yMin),
                new Vector2(r.xMax - cut, r.yMin),
                new Vector2(r.xMax, r.yMin + cut),
                new Vector2(r.xMax, r.yMax - cut),
                new Vector2(r.xMax - cut, r.yMax),
                new Vector2(r.xMin + cut, r.yMax),
                new Vector2(r.xMin, r.yMax - cut),
                new Vector2(r.xMin, r.yMin + cut),
            }, low, high);
        }

        private void Diamond(VertexHelper mesh, float x, float y, float half, Color accent)
        {
            FillPolygon(mesh, new[]
            {
                new Vector2(x, y - half),
                new Vector2(x + half, y),
                new Vector2(x, y + half),
                new Vector2(x - half, y),
            }, accent, accent);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            if (r.width < 14f || r.height < 14f) return;
            Plate(vh, r, 0f, Shadow, Shadow);
            Plate(vh, r, 1.5f, RimLow, RimHigh);
            Plate(vh, r, 3.0f, Edge, Edge);
            Plate(vh, r, 4.4f,
                Primary ? (AntiqueGoldAction ? new Color(.23f, .135f, .048f, 1f) : ActionBase) : Ink,
                Primary ? (AntiqueGoldAction ? new Color(.55f, .36f, .13f, 1f) : ActionTop) : Slate);
            // Small gold corner marks communicate the approved medieval
            // identity without covering text or adding full-panel ornament.
            if (!DecorativeCorners || r.width < 110f || r.height < 45f) return;
            var gold = Primary ? new Color(.87f, .74f, .47f, 1f) :
                new Color(.68f, .55f, .32f, .84f);
            // Inlaid twin gilt rules and four engraved corner knots:
            // live controls remain on top, with no extra Canvas or raycasts.
            float rail = r.width - 61f;
            if (rail > 4f && r.height > 65f)
            {
                FillPolygon(vh, new[] {
                    new Vector2(r.xMin+30f,r.yMax-10f),
                    new Vector2(r.xMax-30f,r.yMax-10f),
                    new Vector2(r.xMax-30f,r.yMax-8.6f),
                    new Vector2(r.xMin+30f,r.yMax-8.6f)
                },gold,gold);
                FillPolygon(vh, new[] {
                    new Vector2(r.xMin+30f,r.yMin+8.6f),
                    new Vector2(r.xMax-30f,r.yMin+8.6f),
                    new Vector2(r.xMax-30f,r.yMin+10f),
                    new Vector2(r.xMin+30f,r.yMin+10f)
                },gold,gold);
                Diamond(vh,r.xMin+18f,r.yMin+18f,4.5f,gold);
                Diamond(vh,r.xMax-18f,r.yMin+18f,4.5f,gold);
                Diamond(vh,r.xMin+18f,r.yMax-18f,4.5f,gold);
                Diamond(vh,r.xMax-18f,r.yMax-18f,4.5f,gold);
                Diamond(vh,r.xMin+25f,r.yMin+25f,2f,RimHigh);
                Diamond(vh,r.xMax-25f,r.yMin+25f,2f,RimHigh);
                Diamond(vh,r.xMin+25f,r.yMax-25f,2f,RimHigh);
                Diamond(vh,r.xMax-25f,r.yMax-25f,2f,RimHigh);
            }
            Diamond(vh, r.xMin + 20f, r.yMax - 15f, 3f, gold);
            Diamond(vh, r.xMax - 20f, r.yMax - 15f, 3f, gold);
        }
    }
}
