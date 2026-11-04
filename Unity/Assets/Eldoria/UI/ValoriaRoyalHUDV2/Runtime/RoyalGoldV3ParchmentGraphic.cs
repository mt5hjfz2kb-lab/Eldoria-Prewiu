using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Dialogue 1 composition: aged parchment beside an unframed portrait.
    /// The implementation remains subject to comparison with the approved concept. All primitives are uGUI vertex colours: no textures, shaders,
    /// additional Canvas, external assets or new input surface.
    /// Draws BEHIND the retained real narrative buttons/text/portrait.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/V3 Cinematic Manuscript")]
    public sealed class RoyalGoldV3ParchmentGraphic : MaskableGraphic
    {
        public bool Arcane;
        public float PortraitColumn = 0.245f;

        private static readonly Color Midnight = new Color(.024f, .039f, .067f, .99f);
        private static readonly Color Slate = new Color(.063f, .102f, .158f, 1f);
        private static readonly Color GoldLow = new Color(.37f, .255f, .115f, 1f);
        private static readonly Color GoldHigh = new Color(.84f, .70f, .44f, 1f);
        private static readonly Color PaperLow = new Color(.72f, .628f, .47f, 1f);
        private static readonly Color PaperHigh = new Color(.961f, .893f, .729f, 1f);
        private static readonly Color RiftLow = new Color(.095f, .063f, .14f, .99f);
        private static readonly Color RiftHigh = new Color(.20f, .139f, .275f, 1f);
        private static readonly Color Violet = new Color(.69f, .49f, .83f, 1f);

        private Color Tint(Color c) => new Color(c.r * color.r, c.g * color.g,
            c.b * color.b, c.a * color.a);

        private void Fill(VertexHelper vh, Rect r, Color low, Color high)
        {
            if (r.width < 2 || r.height < 2) return;
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(r.xMin, r.yMin), Tint(low), Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), Tint(high), Vector2.up);
            vh.AddVert(new Vector3(r.xMax, r.yMax), Tint(high), Vector2.one);
            vh.AddVert(new Vector3(r.xMax, r.yMin), Tint(low), Vector2.right);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }

        private void Diamond(VertexHelper vh, Vector2 center, float size, Color c)
        {
            int start = vh.currentVertCount;
            var col = Tint(c);
            vh.AddVert(center + new Vector2(0, size), col, Vector2.zero);
            vh.AddVert(center + new Vector2(size, 0), col, Vector2.zero);
            vh.AddVert(center + new Vector2(0, -size), col, Vector2.zero);
            vh.AddVert(center + new Vector2(-size, 0), col, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            if (r.width < 200 || r.height < 135) return;
            // The approved normal dialogue leaves the city visible behind the
            // portrait. Only the reading surface receives an opaque paper fill.
            float column = PortraitColumn <= 0f ? 0f : Mathf.Clamp(r.width * PortraitColumn, 132f, 360f);
            Rect paper = Arcane ? r : new Rect(r.xMin + column, r.yMin, r.width - column, r.height);
            Fill(vh, paper, new Color(.012f, .012f, .019f, .98f), new Color(.017f, .022f, .035f, .96f));
            Fill(vh, Inset(paper, 2), Arcane ? Violet : GoldLow, Arcane ? Violet : GoldHigh);
            Rect inner = Inset(paper, 5);
            Fill(vh, inner, Arcane ? RiftLow : PaperLow, Arcane ? RiftHigh : PaperHigh);
            var ornament = Arcane ? Violet : GoldHigh;
            Fill(vh, new Rect(inner.xMin + 14f, inner.yMin + 10f, Mathf.Max(4f, inner.width - 28f), 1f),
                Arcane ? Violet : GoldLow, Arcane ? Violet : GoldLow);
            Diamond(vh, new Vector2(inner.xMax - 13f, inner.yMax - 13f), 3.5f, ornament);
            // Compact royal nameplate, independent of the genuine text control.
            float titleLeft = r.xMin + Mathf.Clamp(r.width * .29f, 132f, 360f) + 14f;
            Rect badge = new Rect(titleLeft, r.yMax - 73f, Mathf.Max(50f, r.xMax-titleLeft-15f), 56f);
            Fill(vh, badge, GoldLow, ornament);
            Fill(vh, Inset(badge, 2), Midnight, Slate);

        }

        private static Rect Inset(Rect r, float n)
        {
            return new Rect(r.xMin + n, r.yMin + n,
                Mathf.Max(0, r.width - 2f * n), Mathf.Max(0, r.height - 2f * n));
        }
    }
}
