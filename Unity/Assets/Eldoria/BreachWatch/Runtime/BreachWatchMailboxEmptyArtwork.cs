using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Authored parchment, wax and feather illustration for the authoritative
    /// empty mailbox. Never intercepts input or invents a message/reward.
    /// Replaces the rejected procedural envelope; no second panel or frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreachWatchMailboxEmptyArtwork : MaskableGraphic
    {
        const string ArtworkKey = "VigiliaArt/MailboxArtwork/sealed_letter";
        Sprite illustration;
        bool loadAttempted;

        public static BreachWatchMailboxEmptyArtwork Install(RectTransform window)
        {
            var go = new GameObject("Vigilia / authored empty post illustration",
                typeof(RectTransform), typeof(CanvasRenderer),
                typeof(BreachWatchMailboxEmptyArtwork));
            go.layer = window.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(window, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.SetSiblingIndex(1);
            var art = go.GetComponent<BreachWatchMailboxEmptyArtwork>();
            art.raycastTarget = false;
            art.color = Color.white;
            return art;
        }

        void LoadIllustration()
        {
            if (loadAttempted) return;
            loadAttempted = true;
            illustration = Resources.Load<Sprite>(ArtworkKey);
        }

        public override Texture mainTexture
        {
            get
            {
                LoadIllustration();
                return illustration ? illustration.texture : Texture2D.whiteTexture;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            LoadIllustration();
            if (!illustration) return; // honest empty state remains; no fake fallback
            var r = GetPixelAdjustedRect();
            if (r.width < 285f || r.height < 530f || r.width > r.height) return;

            // Preserve the original aspect, including its transparent margin.
            // Below real tabs and above the single authoritative empty sentence.
            float aspect = illustration.rect.width / illustration.rect.height;
            float width = Mathf.Min(r.width * .78f, r.height * .30f * aspect);
            float height = width / aspect;
            Vector2 center = new Vector2(r.center.x, r.yMin + r.height * .51f);
            Vector2 min = center - new Vector2(width, height) * .5f;
            Vector2 max = center + new Vector2(width, height) * .5f;
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(illustration);
            Color32 tint = color;
            vh.AddVert(new Vector3(min.x, min.y), tint, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(min.x, max.y), tint, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(max.x, max.y), tint, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(max.x, min.y), tint, new Vector2(uv.z, uv.y));
            vh.AddTriangle(0, 1, 2); vh.AddTriangle(2, 3, 0);
        }
    }
}
