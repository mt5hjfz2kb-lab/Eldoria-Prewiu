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
        float lastLoggedPanY = float.NaN;
        float lastLoggedFov = float.NaN;
        string loadedVisualKey = "";
        string requestedVisualKey = "";
        Coroutine visualLoading;
        const float CertifiedAspect = 1230f / 845f;
        const float CertifiedHalfPanWorld = .80f;
        const float CertifiedHalfPanPixels = 22.8f;
        const float CertifiedWebOverscan = 1.025f;


        // Both directions use the camera that produced the currently displayed texture.
        // EnvelopeParent cropping and the residual image transform participate in selection.
        public bool TryProjectPresentedPoint(Vector3 world,out Vector3 screen)
        {
            screen=Vector3.zero;
            if(webBackgroundRect==null||webBackgroundTexture==null||Presentation==null)return false;
            var view=PresentedViewMatrix();
            var local=view.MultiplyPoint(world);
            if(local.z>=0)return false;
            var camera=Presentation.ProductionCamera;
            var projection=Matrix4x4.Perspective(Presentation.HomeFov*LoadedZoom(),CertifiedAspect,camera.nearClipPlane,camera.farClipPlane);
            var clip=projection*new Vector4(local.x,local.y,local.z,1);
            var uv=new Vector2(clip.x/clip.w*.5f+.5f,clip.y/clip.w*.5f+.5f);
            var rect=webBackgroundRect.rect;
            var pixel=RectTransformUtility.WorldToScreenPoint(null,webBackgroundRect.TransformPoint(new Vector3(rect.xMin+uv.x*rect.width,rect.yMin+uv.y*rect.height,0)));
            screen=new Vector3(pixel.x,pixel.y,-local.z);
            return true;
        }
        public bool TryPresentedRay(Vector2 screen,out Ray ray)
        {
            ray=default;
            if(webBackgroundRect==null||webBackgroundTexture==null||Presentation==null)return false;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(webBackgroundRect,screen,null,out var local))return false;
            var rect=webBackgroundRect.rect;
            var uv=new Vector2((local.x-rect.xMin)/rect.width,(local.y-rect.yMin)/rect.height);
            if(uv.x<0||uv.x>1||uv.y<0||uv.y>1)return false;
            float tangent=Mathf.Tan(Presentation.HomeFov*LoadedZoom()*.5f*Mathf.Deg2Rad);
            var direction=new Vector3((uv.x*2-1)*tangent*CertifiedAspect,(uv.y*2-1)*tangent,1).normalized;
            ray=new Ray(new Vector3(LoadedPan(),0,0),direction);
            return true;
        }
        float LoadedPan()=>loadedVisualKey.Contains("pan-0.5")?-.5f:(loadedVisualKey.Contains("pan0.5")?.5f:0f);
        float LoadedZoom()=>loadedVisualKey.Contains("zoom0.9")?.9f:(loadedVisualKey.Contains("zoom1.1")?1.1f:1f);
        Matrix4x4 PresentedViewMatrix()=>Matrix4x4.Scale(new Vector3(1,1,-1))*Matrix4x4.TRS(new Vector3(LoadedPan(),0,0),Quaternion.identity,Vector3.one).inverse;

        public float WebHorizontalPanLimit
        {
            get
            {
                float width=Mathf.Max(Screen.width,Screen.height*CertifiedAspect);
                float margin=Mathf.Max(0f,(width*CertifiedWebOverscan-Screen.width)*.5f);
                float pixelsPerPan=width*(CertifiedHalfPanPixels/1230f)/CertifiedHalfPanWorld;
                return Mathf.Max(CertifiedHalfPanWorld,margin/Mathf.Max(1f,pixelsPerPan));
            }
        }
        public void FocusPresentedPoint(Vector3 world)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(!TryProjectPresentedPoint(world,out var point)||webBackgroundRect==null)return;
            var canvas=webBackgroundRect.GetComponentInParent<Canvas>();
            float pixelsPerPan=webBackgroundRect.rect.width*(CertifiedHalfPanPixels/1230f)/CertifiedHalfPanWorld*(canvas!=null?canvas.scaleFactor:1f);
            var camera=Presentation.ProductionCamera;
            var position=camera.transform.position;
            position.x=Mathf.Clamp(position.x+(point.x-Screen.width*.5f)/Mathf.Max(1f,pixelsPerPan),-WebHorizontalPanLimit,WebHorizontalPanLimit);
            camera.transform.position=position;
            SyncCertifiedWebView();
#endif
        }

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
#if UNITY_WEBGL && !UNITY_EDITOR
            if (loadedVariant == variant)
            {
                RequestWebVisualForCurrentCamera();
                return;
            }
#else
            if (loadedVariant == variant && runtimeAsset != null)
            {
                if (Renderer != null && Renderer.GsplatAsset != runtimeAsset) Renderer.GsplatAsset = runtimeAsset;
                return;
            }
#endif
            if (loading == null) loading = StartCoroutine(LoadRequested());
        }

        IEnumerator LoadRequested()
        {
            while (requestedVariant != loadedVariant)
            {
                int variant = requestedVariant;
#if UNITY_WEBGL && !UNITY_EDITOR
                loadedVariant = variant;
                RequestWebVisualForCurrentCamera();
                yield return null;
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
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = CertifiedAspect;
            webBackground = imageObject.GetComponent<RawImage>();
            webBackground.raycastTarget = false;
            if (webBackgroundTexture != null) webBackground.texture = webBackgroundTexture;
            SyncCertifiedWebView();
            Debug.Log("VALORIA_WEBGL_HUD_BACKGROUND_READY policy=ENVELOPE_PARENT_CITY_CROP");
        }
        void SyncCertifiedWebView()
        {
            if (webBackgroundRect == null || Presentation == null || Presentation.ProductionCamera == null) return;
            var camera = Presentation.ProductionCamera;
            RequestWebVisualForCurrentCamera();

            float panLimit=WebHorizontalPanLimit;
            float panX = Mathf.Clamp(camera.transform.position.x,-panLimit,panLimit);
            float panY = Mathf.Clamp(camera.transform.position.y,
                -ValoriaParcelPresentation.VerticalPanHalfExtent,
                ValoriaParcelPresentation.VerticalPanHalfExtent);
            float panAnchor = LoadedPan();
            float residualPan = panX - panAnchor;
            float renderedWidth = Mathf.Max(1f, webBackgroundRect.rect.width);
            float renderedHeight = Mathf.Max(1f, webBackgroundRect.rect.height);
            float shift = -(residualPan / CertifiedHalfPanWorld) * renderedWidth * (CertifiedHalfPanPixels / 1230f);

            float zoomRatio = Mathf.Clamp(camera.fieldOfView / Mathf.Max(.01f, Presentation.HomeFov), .9f, 1.1f);
            float zoomAnchor = LoadedZoom();
            float residualScale = Mathf.Clamp(zoomAnchor / zoomRatio, .97f, 1.03f);
            float visualScale = residualScale * CertifiedWebOverscan;
            // The small overscan is only a safety margin for the deliberately smaller Y pan.
            // Vertical travel never exceeds the covered margin, so no un-authored edge can appear.
            float verticalMargin = renderedHeight * (visualScale - residualScale) * .5f;
            float verticalShift = -(panY / ValoriaParcelPresentation.VerticalPanHalfExtent) *
                Mathf.Max(0f, verticalMargin * .90f);
            webBackgroundRect.anchoredPosition = new Vector2(shift, verticalShift);
            webBackgroundRect.localScale = new Vector3(visualScale, visualScale, 1f);

            if (float.IsNaN(lastLoggedPanX) || Mathf.Abs(panX - lastLoggedPanX) >= .08f ||
                float.IsNaN(lastLoggedPanY) || Mathf.Abs(panY - lastLoggedPanY) >= .05f ||
                float.IsNaN(lastLoggedFov) || Mathf.Abs(camera.fieldOfView - lastLoggedFov) >= .5f)
            {
                lastLoggedPanX = panX;
                lastLoggedPanY = panY;
                lastLoggedFov = camera.fieldOfView;
                Debug.Log("ELDORIA_PLAYABLE_VIEW panX=" + panX.ToString("F3") +
                    " panY=" + panY.ToString("F3") +
                    " fov=" + camera.fieldOfView.ToString("F3") +
                    " key=" + requestedVisualKey +
                    " residualShift=" + shift.ToString("F2") +
                    " verticalShift=" + verticalShift.ToString("F2"));
            }
        }

        void RequestWebVisualForCurrentCamera()
        {
            if (loadedVariant < 0 || Presentation == null || Presentation.ProductionCamera == null) return;
            requestedVisualKey = VisualKey(loadedVariant, Presentation.ProductionCamera);
            if (requestedVisualKey == loadedVisualKey || visualLoading != null) return;
            visualLoading = StartCoroutine(LoadRequestedWebVisual());
        }

        IEnumerator LoadRequestedWebVisual()
        {
            while (!string.IsNullOrEmpty(requestedVisualKey) && requestedVisualKey != loadedVisualKey)
            {
                string key = requestedVisualKey;
                string imageUrl = ResolveUrl(key);
                using var imageRequest = UnityWebRequestTexture.GetTexture(imageUrl, true);
                yield return imageRequest.SendWebRequest();
                if (imageRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("VALORIA_WEBGL_BACKGROUND_LOAD_FAIL key=" + key + " url=" + imageUrl + " error=" + imageRequest.error);
                    visualLoading = null;
                    yield break;
                }
                if (key != requestedVisualKey) continue;
                var previousTexture = webBackgroundTexture;
                webBackgroundTexture = DownloadHandlerTexture.GetContent(imageRequest);
                webBackgroundTexture.name = "Valoria certified WebGL " + key;
                if (webBackground != null) webBackground.texture = webBackgroundTexture;
                loadedVisualKey = key;
                if (previousTexture != null) Destroy(previousTexture);
                Debug.Log("VALORIA_WEBGL_CERTIFIED_BACKGROUND_READY variant=" + loadedVariant +
                    " key=" + key + " bytes=" + imageRequest.downloadedBytes);
            }
            visualLoading = null;
        }

        string VisualKey(int variant, Camera camera)
        {
            // Bastion II can share the same parcel-bit variant as late Bastion I (sawmill
            // built, barracks not yet built). WebGL uses certified frame transport, so the
            // Bastion level must participate in the visual key instead of being lost behind
            // the HUD background.
            if (Presentation != null && Presentation.PresentedBastionLevel >= 2 && variant == 1)
                return "valoria-bastion-ii.png";
            if (variant != 0 && variant != 3) return "valoria-state-" + variant + ".png";
            float pan = VisualPanAnchor(Mathf.Clamp(camera.transform.position.x, -.5f, .5f), variant);
            float zoom = VisualZoomAnchor(Mathf.Clamp(camera.fieldOfView / Mathf.Max(.01f, Presentation.HomeFov), .9f, 1.1f), variant);
            return "valoria-state" + variant + "-pan" + PanToken(pan) + "-zoom" + ZoomToken(zoom) + ".png";
        }

        static float VisualPanAnchor(float panX, int variant)
        {
            if (variant != 0 && variant != 3) return 0f;
            if (panX <= -.25f) return -.5f;
            if (panX >= .25f) return .5f;
            return 0f;
        }

        static float VisualZoomAnchor(float zoomRatio, int variant)
        {
            if (variant != 0 && variant != 3) return 1f;
            if (zoomRatio <= .95f) return .9f;
            if (zoomRatio >= 1.05f) return 1.1f;
            return 1f;
        }

        static string PanToken(float pan)
            => pan < -.25f ? "-0.5" : (pan > .25f ? "0.5" : "0");

        static string ZoomToken(float zoom)
            => zoom < .95f ? "0.9" : (zoom > 1.05f ? "1.1" : "1");
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

