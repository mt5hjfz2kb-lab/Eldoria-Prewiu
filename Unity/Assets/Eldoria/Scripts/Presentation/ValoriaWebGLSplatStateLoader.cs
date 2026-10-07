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
        Texture2D webBackgroundTexture;

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

            var imageObject = new GameObject("Certified Valoria frame", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            imageObject.transform.SetParent(hud.transform, false);
            imageObject.transform.SetAsFirstSibling();
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(.5f, .5f);
            rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.one;
            var fitter = imageObject.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 1230f / 845f;
            webBackground = imageObject.GetComponent<RawImage>();
            webBackground.raycastTarget = false;
            if (webBackgroundTexture != null) webBackground.texture = webBackgroundTexture;
            Debug.Log("VALORIA_WEBGL_HUD_BACKGROUND_READY");
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
        }
    }
}
