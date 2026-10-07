using System;
using System.Collections;
using Gsplat;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Eldoria.Presentation
{
    // WebGL-only transport for the exact certified SHARP parcel variants.
    // The native/certified path continues to use serialized StateAssets unchanged.
    public sealed class ValoriaWebGLSplatStateLoader : MonoBehaviour
    {
        public ValoriaParcelPresentation Presentation;
        public GsplatRenderer Renderer;
        public string FilePrefix = "splat-state-";

        int requestedVariant = -1;
        int loadedVariant = -1;
        Coroutine loading;
        GsplatAsset runtimeAsset;
        RawImage webBackground;
        RectTransform webBackgroundRect;
        Image webLetterbox;
        Texture2D webBackgroundTexture;
        float lastLoggedPanX = float.NaN;
        float lastLoggedFov = float.NaN;
        const float CertifiedAspect = 1230f / 845f;
        const float CertifiedHalfPanWorld = .5f;
        const float CertifiedHalfPanPixels = 22.8f;

        void Start()
        {
            if (Presentation == null) Presentation = GetComponent<ValoriaParcelPresentation>();
            if (Renderer == null && Presentation != null) Renderer = Presentation.SceneSplats;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Renderer != null) Renderer.enabled = false;
            StartCoroutine(CreateWebBackgroundWhenHudReady());
#endif
            RequestVariant(Presentation != null ? Presentation.ActiveVariant : 0);
        }

        void LateUpdate()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SyncCertifiedWebView();
#endif
        }

        public void RequestVariant(int variant)
        {
            if (variant < 0 || variant > 3) throw new ArgumentOutOfRangeException(nameof(variant));
            requestedVariant = variant;
            if (loadedVariant == variant && runtimeAsset != null)
            {
                if (Renderer != null && Renderer.GsplatAsset != runtimeAsset) Renderer.GsplatAsset = runtimeAsset;
                return;
            }
            if (loading == null) loading = StartCoroutine(LoadRequested());
        }

        IEnumerator LoadRequested()
        {
            while (requestedVariant != loadedVariant)
            {
                int variant = requestedVariant;
#if UNITY_WEBGL && !UNITY_EDITOR
                string imageUrl = ResolveUrl("valoria-state-" + variant + ".png");
                using var imageRequest = UnityWebRequestTexture.GetTexture(imageUrl, true);
                yield return imageRequest.SendWebRequest();
                if (imageRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("VALORIA_WEBGL_BACKGROUND_LOAD_FAIL variant=" + variant + " url=" + imageUrl + " error=" + imageRequest.error);
                    loading = null;
                    yield break;
                }
                if (variant != requestedVariant) continue;
                var previousTexture = webBackgroundTexture;
                webBackgroundTexture = DownloadHandlerTexture.GetContent(imageRequest);
                webBackgroundTexture.name = "Valoria certified WebGL state " + variant;
                if (webBackground != null) webBackground.texture = webBackgroundTexture;
                loadedVariant = variant;
                if (previousTexture != null) Destroy(previousTexture);
                Debug.Log("VALORIA_WEBGL_CERTIFIED_BACKGROUND_READY variant=" + variant + " bytes=" + imageRequest.downloadedBytes);
#else
                string url = ResolveUrl(FilePrefix + variant + ".ply");
                using var request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("VALORIA_WEBGL_SPLAT_LOAD_FAIL variant=" + variant + " url=" + url + " error=" + request.error);
                    loading = null;
                    yield break;
                }

                var asset = GsplatRuntimeLoader.Load(request.downloadHandler.data, GsplatFileFormat.Ply,
                    CompressionMode.Spark, SourceCoordinates.RDF);
                asset.name = "Valoria WebGL SHARP state " + variant;

                if (variant != requestedVariant)
                {
                    Destroy(asset);
                    continue;
                }

                var previous = runtimeAsset;
                runtimeAsset = asset;
                loadedVariant = variant;
                if (Renderer == null) throw new InvalidOperationException("Valoria WebGL SHARP renderer is missing.");
                Renderer.GsplatAsset = runtimeAsset;
                if (previous != null) Destroy(previous);
                Debug.Log("VALORIA_WEBGL_SPLAT_READY variant=" + variant + " bytes=" + request.downloadedBytes);
#endif
            }
            loading = null;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        IEnumerator CreateWebBackgroundWhenHudReady()
        {
            GameObject hud = null;
            for (var i = 0; i < 300 && hud == null; i++)
            {
                hud = GameObject.Find("Eldoria HUD");
                if (hud == null) yield return null;
            }
            if (hud == null)
            {
                Debug.LogError("VALORIA_WEBGL_HUD_BACKGROUND_FAIL missing Eldoria HUD");
                yield break;
            }

            var backdropObject = new GameObject("Valoria WebGL letterbox", typeof(RectTransform), typeof(Image));
            backdropObject.transform.SetParent(hud.transform, false);
            backdropObject.transform.SetAsFirstSibling();
            var backdropRect = backdropObject.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            webLetterbox = backdropObject.GetComponent<Image>();
            webLetterbox.color = new Color(.018f, .026f, .034f, 1f);
            webLetterbox.raycastTarget = false;

            var imageObject = new GameObject("Certified Valoria frame", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            imageObject.transform.SetParent(hud.transform, false);
            imageObject.transform.SetSiblingIndex(1);
            webBackgroundRect = imageObject.GetComponent<RectTransform>();
            webBackgroundRect.anchorMin = new Vector2(.5f, .5f);
            webBackgroundRect.anchorMax = new Vector2(.5f, .5f);
            webBackgroundRect.anchoredPosition = Vector2.zero;
            webBackgroundRect.sizeDelta = Vector2.one;
            var fitter = imageObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = CertifiedAspect;
            webBackground = imageObject.GetComponent<RawImage>();
            webBackground.raycastTarget = false;
            if (webBackgroundTexture != null) webBackground.texture = webBackgroundTexture;
            SyncCertifiedWebView();
            Debug.Log("VALORIA_WEBGL_HUD_BACKGROUND_READY policy=FIT_IN_PARENT");
        }
        void SyncCertifiedWebView()
        {
            if (webBackgroundRect == null || Presentation == null || Presentation.ProductionCamera == null) return;
            var camera = Presentation.ProductionCamera;
            float panX = Mathf.Clamp(camera.transform.position.x, -CertifiedHalfPanWorld, CertifiedHalfPanWorld);
            float panNormalized = panX / CertifiedHalfPanWorld;
            float renderedWidth = Mathf.Max(1f, webBackgroundRect.rect.width);
            float shift = -panNormalized * renderedWidth * (CertifiedHalfPanPixels / 1230f);
            webBackgroundRect.anchoredPosition = new Vector2(shift, 0f);

            float homeFov = Mathf.Max(.01f, Presentation.HomeFov);
            float zoomScale = Mathf.Clamp(homeFov / Mathf.Max(.01f, camera.fieldOfView), .9f, 1.1f);
            webBackgroundRect.localScale = new Vector3(zoomScale, zoomScale, 1f);

            if (float.IsNaN(lastLoggedPanX) || Mathf.Abs(panX - lastLoggedPanX) >= .08f ||
                float.IsNaN(lastLoggedFov) || Mathf.Abs(camera.fieldOfView - lastLoggedFov) >= .5f)
            {
                lastLoggedPanX = panX;
                lastLoggedFov = camera.fieldOfView;
                Debug.Log("ELDORIA_PLAYABLE_VIEW panX=" + panX.ToString("F3") +
                    " fov=" + camera.fieldOfView.ToString("F3") +
                    " shift=" + shift.ToString("F2"));
            }
        }
#endif

        static string ResolveUrl(string file)
        {
            if (string.IsNullOrWhiteSpace(UnityEngine.Application.absoluteURL))
                return file;
            return new Uri(new Uri(UnityEngine.Application.absoluteURL), file).AbsoluteUri;
        }

        void OnDestroy()
        {
            if (runtimeAsset != null) Destroy(runtimeAsset);
            if (webBackgroundTexture != null) Destroy(webBackgroundTexture);
            if (webLetterbox != null) Destroy(webLetterbox.gameObject);
        }
    }
}
