using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Owner-approved Dialogue 1: aged parchment with a midnight-blue portrait
    /// column. All primitives are uGUI vertex colours: no textures, shaders,
    /// additional Canvas, external assets or new input surface.
    /// Draws BEHIND the retained real narrative buttons/text/portrait.
    /// </summary>
    [AddComponentMenu("Eldoria/UI/V3 Cinematic Manuscript")]
    public sealed class RoyalGoldV3ParchmentGraphic : MaskableGraphic
    {
        public bool Arcane;
        public bool ObjectiveCard;
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
            if (ObjectiveCard)
            {
                if (r.width < 200 || r.height < 95) return;
                // Option 1: a restrained antique-gold frame, parchment reading
                // field and midnight-blue heraldic column; all genuine
                // chapter text / progress / actions remain separate uGUI.
                Fill(vh, r, new Color(.015f, .018f, .028f, .97f),
                    new Color(.023f, .030f, .045f, .98f));
                Fill(vh, Inset(r, 2f), GoldLow, GoldHigh);
                Fill(vh, Inset(r, 4f), new Color(.18f, .125f, .067f, 1f),
                    new Color(.38f, .26f, .13f, 1f));
                var page = Inset(r, 7f);
                Fill(vh, page, PaperLow, PaperHigh);
                var heraldry = new Rect(page.xMin, page.yMin, 66f, page.height);
                Fill(vh, heraldry, new Color(.025f, .064f, .115f, 1f),
                    new Color(.070f, .155f, .27f, 1f));
                Fill(vh, new Rect(heraldry.xMax, page.yMin, 2f, page.height),
                    GoldLow, GoldHigh);
                Diamond(vh, new Vector2(page.xMin + 12f, page.yMax - 12f), 3f, GoldHigh);
                Diamond(vh, new Vector2(page.xMax - 12f, page.yMax - 12f), 3f, GoldHigh);
                return;
            }
            if (r.width < 200 || r.height < 135) return;
            // Deep opaque drop shadow and two thin, antique-gold strokes.
            Fill(vh, r, new Color(.012f, .012f, .019f, .98f),
                new Color(.017f, .022f, .035f, .96f));
            Fill(vh, Inset(r, 2), Arcane ? Violet : GoldLow,
                Arcane ? new Color(.89f, .74f, 1f, 1f) : GoldHigh);
            Fill(vh, Inset(r, 4), Midnight, Slate);

            // The left portrait column belongs to the royal-blue livery; only
            // the actual reading surface is parchment (not an opaque full HUD).
            Rect inner = Inset(r, 7);
            float column = Mathf.Clamp(r.width * PortraitColumn, 120f, 270f);
            Rect left = new Rect(inner.xMin, inner.yMin, column, inner.height);
            Rect parchment = new Rect(inner.xMin + column + 3f, inner.yMin,
                Mathf.Max(4f, inner.width - column - 3f), inner.height);
            Fill(vh, left, new Color(.028f, .052f, .084f, 1f),
                new Color(.065f, .119f, .182f, 1f));
            Fill(vh, parchment, Arcane ? RiftLow : PaperLow,
                Arcane ? RiftHigh : PaperHigh);
            // Fine rule between portrait and text, and one restrained lower
            // ink line. These are not overlays that intercept game gestures.
            Fill(vh, new Rect(left.xMax, inner.yMin, 2.0f, inner.height),
                Arcane ? Violet : GoldLow, Arcane ? Violet : GoldHigh);
            Fill(vh, new Rect(parchment.xMin + 14f, parchment.yMin + 10f,
                Mathf.Max(4f, parchment.width - 28f), 1f),
                Arcane ? Violet : GoldLow, Arcane ? Violet : GoldLow);
            var ornament = Arcane ? Violet : GoldHigh;
            Diamond(vh, new Vector2(inner.xMin + 13f, inner.yMax - 13f), 3.5f, ornament);
            Diamond(vh, new Vector2(inner.xMax - 13f, inner.yMax - 13f), 3.5f, ornament);
        }

        private static Rect Inset(Rect r, float n)
        {
            return new Rect(r.xMin + n, r.yMin + n,
                Mathf.Max(0, r.width - 2f * n), Mathf.Max(0, r.height - 2f * n));
        }
    }
}
