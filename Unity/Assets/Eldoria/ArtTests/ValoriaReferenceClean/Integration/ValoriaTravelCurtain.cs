using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean
{
    // One light, transient canvas survives the single-scene load. The old
    // integrated city never flashes while its canonical map prepares.
    public sealed class ValoriaTravelCurtain : MonoBehaviour
    {
        static ValoriaTravelCurtain current;
#if UNITY_WEBGL && !UNITY_EDITOR
        static int cityTextureLimit = -1;
#endif
        Text caption;
        Button back;
        float started;
        bool travelling;
        string destination;

        public static void Travel(string scene)
        {
            if (!ValoriaTravelSession.CombinedPlayer) return;
            if (!current)
            {
                var root = new GameObject("Paso entre Valoria y la Región", typeof(RectTransform), typeof(Canvas),
                    typeof(CanvasScaler), typeof(GraphicRaycaster));
                DontDestroyOnLoad(root);
                current = root.AddComponent<ValoriaTravelCurtain>();
                current.Build();
            }
            if (current.travelling) return;
            current.Open(scene);
        }

        static RectTransform Box(Transform parent, string name, float y, Vector2 size)
        {
            var rect = (RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            rect.SetParent(parent,false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = new Vector2(0,y);
            rect.sizeDelta = size;
            return rect;
        }

        Text Write(RectTransform parent,string message,int size)
        {
            var text = parent.gameObject.AddComponent<Text>();
            text.font = Resources.Load<Font>("ValoriaModular25D/UI/Fonts/SourceSans3-Semibold");
            if (!text.font) text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color32(246,219,168,255);
            text.text = message;
            text.raycastTarget = false;
            return text;
        }

        void Build()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390,390);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var background = gameObject.AddComponent<Image>();
            background.color = new Color32(30,23,39,255);
            var crest = Box(transform,"Corona",42,new Vector2(80,80)).gameObject.AddComponent<Image>();
            crest.sprite = Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/identity_crest");
            crest.preserveAspect = true;
            crest.raycastTarget = false;
            caption = Write(Box(transform,"Viaje",-25,new Vector2(320,56)),"",20);
            var target = Box(transform,"Volver",-97,new Vector2(180,48));
            var face = target.gameObject.AddComponent<Image>();
            face.sprite = Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/action_arcane");
            back = target.gameObject.AddComponent<Button>();
            back.targetGraphic = face;
            Write(Box(target,"Destino",0,new Vector2(150,44)),"Volver a Valoria",16);
            back.onClick.AddListener(() => Travel(ValoriaTravelSession.CityScene));
        }

        void Open(string scene)
        {
            destination = scene;
            started = Time.realtimeSinceStartup;
            caption.text = scene == ValoriaTravelSession.CityScene ? "Regresando a Valoria…" : "Abriendo Región 1…";
            back.gameObject.SetActive(false);
            travelling = true;
            StartCoroutine(ChangeScene(scene));
        }

        IEnumerator ChangeScene(string scene)
        {
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return new WaitForEndOfFrame();
            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(scene)) { travelling=false; Failed(); yield break; }
            var leaving = SceneManager.GetActiveScene();
            if (leaving.name == ValoriaTravelSession.CityScene || leaving.name == ValoriaTravelSession.RegionScene)
            {
                // Single-mode loading can retain both large scenes until activation.
                // Keep only the curtain alive while releasing city-only art.
                var transit = SceneManager.CreateScene("Valoria en tránsito");
                SceneManager.SetActiveScene(transit);
                var camera = new GameObject("Fondo del viaje").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color32(30,23,39,255);
                camera.cullingMask = 0;
                yield return SceneManager.UnloadSceneAsync(leaving);
                yield return Resources.UnloadUnusedAssets();
                System.GC.Collect();
#if UNITY_WEBGL && !UNITY_EDITOR
                // The integrated region has a different material budget than the
                // modular city. Change residency only when neither scene is alive.
                if (scene == ValoriaTravelSession.RegionScene)
                {
                    if (cityTextureLimit < 0) cityTextureLimit = QualitySettings.globalTextureMipmapLimit;
                    QualitySettings.globalTextureMipmapLimit = Mathf.Max(2,cityTextureLimit);
                }
                else if (cityTextureLimit >= 0)
                {
                    QualitySettings.globalTextureMipmapLimit = cityTextureLimit;
                    cityTextureLimit = -1;
                }
#endif
                yield return null;
            }
            AsyncOperation operation = null;
            try { operation = SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single); }
            catch (Exception e) { Debug.LogException(e); }
            if (operation == null) { travelling=false; Failed(); yield break; }
            yield return operation;
            travelling = false;
        }

        static public void Arrived(string scene)
        {
            if (!current || current.destination != scene) return;
            var old = current;
            current = null;
            // Destroy is deferred until end of frame. Disable the overlay and
            // raycaster immediately so the newly arrived canonical HUD can
            // receive its very first touch, including repeated world returns.
            old.gameObject.SetActive(false);
            Destroy(old.gameObject);
        }

        public static void Failed()
        {
            if (!current) return;
            current.caption.text = "No se ha podido abrir el destino.";
            current.back.gameObject.SetActive(true);
        }

        void Update()
        {
            if (!back.gameObject.activeSelf && Time.realtimeSinceStartup-started>125f) Failed();
        }
    }
}
