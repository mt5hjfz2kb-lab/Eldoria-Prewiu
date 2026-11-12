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
            // The canonical empty-message collection board sits above old
            // index 1 and cropped the REAL parchment's top half (native
            // 43109761153). Preserve its genuine transparency instead.
            // This illustration is disabled when mail is populated and
            // never receives raycasts, so it cannot steal message input.
            rt.SetAsLastSibling();
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
            // A genuine 1600x900 desktop mailbox had over half the visible
            // surface empty: portrait-only painting was NOT good composition.
            // Use the SAME approved sealed-letter texture at desktop scale;
            // compact landscape (844x390) still has no room under the tabs.
            bool portrait = r.width >= 285f && r.height >= 530f &&
                r.height > r.width;
            bool spaciousLandscape = r.width >= 1100f &&
                r.height >= 650f && r.width > r.height;
            if (!portrait && !spaciousLandscape) return;

            // Fit within the actual canonical mailbox window; no stretching,
            // new panel, extra message, click target or different art.
            float aspect = illustration.rect.width / illustration.rect.height;
            float width = portrait
                ? Mathf.Min(r.width * .78f, r.height * .30f * aspect)
                : Mathf.Min(r.width * .32f, r.height * .41f * aspect);
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
