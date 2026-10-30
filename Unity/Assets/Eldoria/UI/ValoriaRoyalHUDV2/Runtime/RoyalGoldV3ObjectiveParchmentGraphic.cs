using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Crónica Real / approved Interface 1. A small aged manuscript with
    /// antique-gold filigree and heraldic left margin. Native UI geometry,
    /// not a crop from the reference (text, quests and input remain live).
    /// </summary>
    [AddComponentMenu("Eldoria/UI/Crónica Real Objective Manuscript")]
    public sealed class RoyalGoldV3ObjectiveParchmentGraphic : MaskableGraphic
    {
        static readonly Color Shadow = new Color(.055f, .049f, .047f, .84f);
        static readonly Color AgedEdge = new Color(.38f, .26f, .12f, 1f);
        static readonly Color Gold = new Color(.72f, .51f, .22f, 1f);
        static readonly Color ParchmentLow = new Color(.78f, .68f, .52f, .99f);
        static readonly Color ParchmentHigh = new Color(.972f, .920f, .796f, .99f);
        static readonly Color ShieldBlue = new Color(.071f, .151f, .263f, 1f);

        static Color Mix(Color value, Color tint) => new Color(
            value.r * tint.r, value.g * tint.g, value.b * tint.b,
            value.a * tint.a);

        void Quad(VertexHelper mesh, Rect r, Color low, Color high)
        {
            if (r.width <= 0 || r.height <= 0) return;
            int k = mesh.currentVertCount;
            mesh.AddVert(new Vector3(r.xMin, r.yMin), Mix(low, color), Vector2.zero);
            mesh.AddVert(new Vector3(r.xMin, r.yMax), Mix(high, color), Vector2.up);
            mesh.AddVert(new Vector3(r.xMax, r.yMax), Mix(high, color), Vector2.one);
            mesh.AddVert(new Vector3(r.xMax, r.yMin), Mix(low, color), Vector2.right);
            mesh.AddTriangle(k, k + 1, k + 2);
            mesh.AddTriangle(k, k + 2, k + 3);
        }

        void WornPaper(VertexHelper mesh, Rect r, Color low, Color high)
        {
            // Native rag-paper silhouette instead of an antiseptic rectangle;
            // all points are INSIDE the existing authored objective bounds.
            float t = Mathf.Min(5f, r.height * .07f);
            var vertices = new [] {
                new Vector2(r.xMin+t,r.yMin+t),
                new Vector2(r.xMin+r.width*.16f,r.yMin+1f),
                new Vector2(r.xMin+r.width*.36f,r.yMin+2.2f),
                new Vector2(r.xMin+r.width*.64f,r.yMin+.8f),
                new Vector2(r.xMax-r.width*.11f,r.yMin+2.5f),
                new Vector2(r.xMax-t,r.yMin+t),
                new Vector2(r.xMax-1f,r.yMin+r.height*.21f),
                new Vector2(r.xMax-2.1f,r.yMin+r.height*.57f),
                new Vector2(r.xMax-1f,r.yMax-r.height*.15f),
                new Vector2(r.xMax-t,r.yMax-t),
                new Vector2(r.xMin+r.width*.83f,r.yMax-1f),
                new Vector2(r.xMin+r.width*.52f,r.yMax-2.2f),
                new Vector2(r.xMin+r.width*.24f,r.yMax-1f),
                new Vector2(r.xMin+t,r.yMax-t),
                new Vector2(r.xMin+1.8f,r.yMax-r.height*.25f),
                new Vector2(r.xMin+1f,r.yMin+r.height*.33f)
            };
            int k=mesh.currentVertCount;
            var center=Mix(Color.Lerp(low,high,.50f),color);
            mesh.AddVert(r.center,center,Vector2.zero);
            foreach(var v in vertices)
                mesh.AddVert(v,Mix(Color.Lerp(low,high,Mathf.InverseLerp(r.yMin,r.yMax,v.y)),color),Vector2.zero);
            for(int i=0;i<vertices.Length;i++)
                mesh.AddTriangle(k,k+i+1,k+(i+1)%vertices.Length+1);
        }

        void Diamond(VertexHelper mesh, Vector2 p, float n, Color tint)
        {
            int k = mesh.currentVertCount;
            var c = Mix(tint, color);
            mesh.AddVert(new Vector3(p.x, p.y + n), c, Vector2.up);
            mesh.AddVert(new Vector3(p.x + n, p.y), c, Vector2.right);
            mesh.AddVert(new Vector3(p.x, p.y - n), c, Vector2.zero);
            mesh.AddVert(new Vector3(p.x - n, p.y), c, Vector2.left);
            mesh.AddTriangle(k, k + 1, k + 2);
            mesh.AddTriangle(k, k + 2, k + 3);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            if (r.width < 105f || r.height < 60f) return;
            // Four calibrated layers: parchment must read as parchment even
            // over bright meadow or deep-purple Brecha, never as a dark card.
            Quad(mesh, r, Shadow, Shadow);
            var rim = Rect.MinMaxRect(r.xMin + 1.4f, r.yMin + 1.4f,
                r.xMax - 1.4f, r.yMax - 1.4f);
            Quad(mesh, rim, AgedEdge, Gold);
            var paper = Rect.MinMaxRect(r.xMin + 3.6f, r.yMin + 3.6f,
                r.xMax - 3.6f, r.yMax - 3.6f);
            WornPaper(mesh, paper, ParchmentLow, ParchmentHigh);
            // Historical illuminated-manuscript edge, transparent decoration,
            // not part of the clickable action region.
            float crestWidth = Mathf.Min(54f, r.width * .18f);
            Quad(mesh, new Rect(paper.xMin + 3f, paper.yMin + 5f,
                crestWidth, paper.height - 10f), 
                new Color(.64f, .49f, .29f, .14f),
                new Color(.96f, .82f, .53f, .20f));
            Quad(mesh, new Rect(paper.xMin + crestWidth + 9f,
                paper.yMin + 13f, 1.1f, paper.height - 26f),
                AgedEdge, Gold);
            var fleur = new Color(.59f, .39f, .17f, .95f);
            Diamond(mesh, new Vector2(paper.xMin + 11f, paper.yMax - 11f), 4f, fleur);
            Diamond(mesh, new Vector2(paper.xMax - 11f, paper.yMax - 11f), 4f, fleur);
            Diamond(mesh, new Vector2(paper.xMin + 11f, paper.yMin + 11f), 3f, fleur);
            Diamond(mesh, new Vector2(paper.xMax - 11f, paper.yMin + 11f), 3f, fleur);
            // Top and bottom engraved rules.
            Quad(mesh, new Rect(paper.xMin + 19f, paper.yMax - 5f,
                Mathf.Max(1f, paper.width - 38f), 1.1f), Gold, AgedEdge);
            Quad(mesh, new Rect(paper.xMin + 19f, paper.yMin + 4f,
                Mathf.Max(1f, paper.width - 38f), 1f), AgedEdge, Gold);
        }
    }
}
