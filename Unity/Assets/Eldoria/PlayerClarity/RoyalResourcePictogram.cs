using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Three original, layered resource silhouettes. No emoji, font glyphs,
    /// texture dependencies, raycast targets or fabricated resource values.
    /// The real Royal HUD still owns layout, wallet and accessibility text.
    /// This is a Unity source candidate, not artistic approval.
    /// </summary>
    public enum ResourcePictogramKind { Wood, Stone, Food }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RoyalResourcePictogram : MaskableGraphic
    {
        [SerializeField] private ResourcePictogramKind kind;
        public ResourcePictogramKind Kind
        {
            get => kind;
            set { if (kind == value) return; kind = value; SetVerticesDirty(); }
        }

        static Vector2 P(float x, float y) => new Vector2(x, y);
        private Vector3 Place(Vector2 p)
        {
            var r = rectTransform.rect;
            return new Vector3(r.xMin + p.x * r.width / 100f,
                r.yMin + p.y * r.height / 100f, 0f);
        }

        private Color32 Shade(Color32 c)
        {
            var tint = color;
            return (Color32)new Color(c.r / 255f * tint.r,
                c.g / 255f * tint.g, c.b / 255f * tint.b,
                c.a / 255f * tint.a);
        }

        private void Poly(VertexHelper v, Color32 ink, params Vector2[] p)
        {
            if (p == null || p.Length < 3) return;
            var first = v.currentVertCount;
            var c = Shade(ink);
            foreach (var point in p) v.AddVert(Place(point), c, Vector2.zero);
            for (var i = 1; i < p.Length - 1; i++)
                v.AddTriangle(first, first + i, first + i + 1);
        }

        private void Line(VertexHelper v, Color32 ink, Vector2 a, Vector2 b, float width)
        {
            var d = (b - a);
            if (d.sqrMagnitude < .0001f) return;
            var n = new Vector2(-d.y, d.x).normalized * (width * .5f);
            Poly(v, ink, a - n, b - n, b + n, a + n);
        }

        private void Oval(VertexHelper v, Color32 ink, Vector2 p, float rx, float ry)
        {
            var ring = new Vector2[20];
            for (var i = 0; i < ring.Length; i++)
            {
                var angle = Mathf.PI * 2f * i / ring.Length;
                ring[i] = p + new Vector2(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry);
            }
            Poly(v, ink, ring);
        }

        private static readonly Color32 Shadow = new Color32(22, 20, 23, 118);
        private static readonly Color32 Outline = new Color32(31, 35, 35, 255);

        private void Wood(VertexHelper v)
        {
            // Read as two sawn timber logs at 20 px, not as crossed swords or twigs.
            Line(v, Shadow, P(18, 27), P(75, 67), 37);
            Line(v, Outline, P(22, 31), P(72, 68), 30);
            Line(v, new Color32(105, 57, 32, 255), P(24, 31), P(73, 66), 24);
            Line(v, new Color32(182, 109, 54, 255), P(25, 36), P(69, 67), 10);
            Line(v, new Color32(232, 163, 86, 255), P(30, 42), P(72, 71), 4);
            Oval(v, Outline, P(23, 31), 16, 15);
            Oval(v, new Color32(204, 151, 86, 255), P(23, 31), 12, 11);
            Oval(v, new Color32(127, 78, 42, 255), P(23, 31), 8, 7);
            Oval(v, new Color32(234, 188, 110, 255), P(23, 31), 4, 3);

            Line(v, Outline, P(40, 20), P(83, 52), 25);
            Line(v, new Color32(121, 73, 43, 255), P(41, 20), P(81, 50), 19);
            Line(v, new Color32(201, 133, 68, 255), P(44, 24), P(81, 50), 5);
            Oval(v, Outline, P(39, 21), 13, 12);
            Oval(v, new Color32(216, 167, 91, 255), P(39, 21), 10, 9);
            Oval(v, new Color32(132, 83, 49, 255), P(39, 21), 6, 5);
            Oval(v, new Color32(240, 194, 109, 255), P(39, 21), 3, 2);
        }

        private void Stone(VertexHelper v)
        {
            // One broad quarry-rock silhouette with distinguishable cut facets.
            Poly(v, Shadow, P(15, 19), P(83, 18), P(95, 44), P(79, 81),
                P(40, 90), P(11, 63));
            Poly(v, Outline, P(12, 26), P(31, 74), P(59, 89),
                P(87, 75), P(93, 43), P(73, 18), P(28, 18));
            Poly(v, new Color32(114, 131, 135, 255), P(17, 38), P(34, 74),
                P(58, 82), P(52, 49), P(29, 24));
            Poly(v, new Color32(177, 192, 189, 255), P(34, 74), P(60, 84),
                P(85, 74), P(67, 53), P(52, 49));
            Poly(v, new Color32(89, 107, 113, 255), P(29, 24), P(52, 49),
                P(67, 53), P(74, 20));
            Poly(v, new Color32(123, 145, 149, 255), P(67, 53), P(85, 74),
                P(90, 44), P(74, 20));
            Line(v, new Color32(223, 224, 202, 250), P(34, 73), P(59, 82), 3);
            Line(v, new Color32(227, 220, 190, 218), P(59, 82), P(84, 73), 2);
            Line(v, new Color32(58, 68, 73, 220), P(52, 49), P(67, 53), 3);
        }

        private void Food(VertexHelper v)
        {
            // Golden cereal sheaf; a conventional food silhouette. Binds at the base.
            Line(v, Shadow, P(50, 15), P(50, 88), 12);
            var stem = new Color32(115, 93, 42, 255);
            var gold = new Color32(228, 172, 65, 255);
            var light = new Color32(255, 212, 107, 255);
            Line(v, stem, P(50, 16), P(51, 91), 5);
            Line(v, stem, P(50, 20), P(26, 73), 5);
            Line(v, stem, P(50, 20), P(75, 74), 5);
            for (var i = 0; i < 5; i++)
            {
                var y = 36 + i * 10;
                var spread = 19f - i * 2.1f;
                Oval(v, Outline, P(50 - spread, y + 1), 8, 5.5f);
                Oval(v, gold, P(51 - spread, y + 2), 6.2f, 4f);
                Oval(v, Outline, P(50 + spread, y + 1), 8, 5.5f);
                Oval(v, light, P(49 + spread, y + 2), 6.2f, 4f);
            }
            Oval(v, gold, P(50, 84), 7, 12);
            Poly(v, Outline, P(30, 27), P(70, 27), P(61, 18), P(40, 18));
            Poly(v, new Color32(165, 82, 43, 255),
                P(34, 28), P(65, 28), P(58, 20), P(43, 20));
            Line(v, light, P(40, 25), P(58, 25), 2);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            switch (kind)
            {
                case ResourcePictogramKind.Wood: Wood(vh); break;
                case ResourcePictogramKind.Stone: Stone(vh); break;
                case ResourcePictogramKind.Food: Food(vh); break;
            }
        }
    }
}
