using Eldoria.Application;
using Eldoria.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Presentation
{
    public static class SliceBoot
    {
        private static bool recoveryNoticeVisible;
        private const string WebGlSaveKey = "eldoria-unity-slice-owner-i-ii-v1";
        public static string SavePath => System.IO.Path.Combine(UnityEngine.Application.persistentDataPath,
            Eldoria.Domain.SliceContentProfiles.ActiveRuntimeProfile==Eldoria.Domain.SliceContentProfiles.QaFastId
                ? "eldoria-unity-slice-v1.json"
                : "eldoria-unity-slice-owner-i-ii-v1.json");

        public static void ResetLocalSaveAndRestart()
        {
            if (recoveryNoticeVisible) return;
            DeleteLocalSave();
            var ui = Object.FindFirstObjectByType<SlicePresenter>();
            if (ui != null) ui.Initialize(new LocalGateway(new SystemClock(), CreateStore()));
            SceneManager.LoadScene(CitySceneName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Eldoria.Domain.SliceContentProfiles.SetRuntimeProfileOverride(Eldoria.Domain.SliceContentProfiles.OwnerIiiId);
#endif
            if (Object.FindFirstObjectByType<SlicePresenter>() != null) return;
            var obj = new GameObject("Eldoria composition root");
            Object.DontDestroyOnLoad(obj);
            var ui = obj.AddComponent<SlicePresenter>();
            var save = CreateStore();
            try { ui.Initialize(new LocalGateway(new SystemClock(), save)); }
            catch (System.Exception error)
            {
                Debug.LogError("Eldoria save load failed; existing save untouched: " + error);
                ui.Initialize(new LocalGateway(new SystemClock(), new VolatileStore()));
                ShowSaveRecoveryNotice();
            }
            SceneManager.sceneLoaded += ui.OnSceneLoaded;
            if (SceneManager.GetActiveScene().name == "Bootstrap") SceneManager.LoadScene("Valoria");
            else ui.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static string CitySceneName
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return "ValoriaWebGL";
#else
                return "Valoria";
#endif
            }
        }

        private static IStateStore CreateStore()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new WebGlPlayerPrefsStore(WebGlSaveKey);
#else
            return new FileStateStore(SavePath);
#endif
        }

        private static void DeleteLocalSave()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayerPrefs.DeleteKey(WebGlSaveKey);
            PlayerPrefs.Save();
#else
            new FileStateStore(SavePath).DeleteLocalState();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private sealed class WebGlPlayerPrefsStore : IStateStore
        {
            private readonly string key;
            public WebGlPlayerPrefsStore(string key) { this.key = key; }

            public Eldoria.Domain.PlayerState Load()
            {
                if (!PlayerPrefs.HasKey(key)) return null;
                var json = PlayerPrefs.GetString(key, "");
                if (string.IsNullOrEmpty(json)) return null;
                var state = JsonUtility.FromJson<Eldoria.Domain.PlayerState>(json);
                if (state == null || state.SchemaVersion != 1)
                    throw new System.IO.InvalidDataException("Unknown WebGL save version");
                return state;
            }

            public void Save(Eldoria.Domain.PlayerState state)
            {
                PlayerPrefs.SetString(key, JsonUtility.ToJson(state, true));
                // WebGL FileStateStore writes live in the virtual filesystem and can be lost
                // on an immediate browser reload before IDBFS sync. PlayerPrefs.Save drives
                // Unity's browser-backed persistence explicitly at each authoritative commit.
                PlayerPrefs.Save();
            }
        }
#endif


        private static void ShowSaveRecoveryNotice()
        {
            recoveryNoticeVisible = true;
            var presenter = Object.FindFirstObjectByType<SlicePresenter>();
            var priorEnabled = presenter != null && presenter.enabled;
            if (presenter != null) presenter.enabled = false;
            var message = SaveLoadNoticePolicy.Message(true);
            var root = new GameObject("Eldoria save recovery notice", typeof(RectTransform),
                typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            Object.DontDestroyOnLoad(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 390);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            canvas.scaleFactor = Mathf.Min(Screen.width, Screen.height) / 390f;
            var backdrop = NoticeElement("Backdrop", root.transform, Vector2.zero, Vector2.zero);
            var backdropRect = backdrop.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.sizeDelta = Vector2.zero;
            backdrop.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.65f);
            var card = NoticeElement("Save notice", root.transform, new Vector2(-32, 240), Vector2.zero, true);
            card.AddComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.12f, 0.18f, 1f);
            NoticeText(card.transform, "Problema con el guardado", new Vector2(-36, 42), new Vector2(0, 83), 20);
            NoticeText(card.transform, message, new Vector2(-36, 116), new Vector2(0, 10), 18);
            var buttonObject = NoticeElement("Continue without saving", card.transform, new Vector2(-36, 44), new Vector2(0, -86), true);
            buttonObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.18f, 0.35f, 0.50f, 1);
            var button = buttonObject.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => {
                recoveryNoticeVisible = false;
                if (presenter != null) presenter.enabled = priorEnabled;
                Debug.Log("ELDORIA_SAVE_RECOVERY_NOTICE acknowledged=true");
                Object.Destroy(root);
            });
            NoticeText(buttonObject.transform, "Continuar sin guardar", new Vector2(-12, 40), Vector2.zero, 18);
            Canvas.ForceUpdateCanvases();
            Debug.Log("ELDORIA_SAVE_RECOVERY_NOTICE shown=true message=" + message);
            Debug.Log("ELDORIA_SAVE_RECOVERY_LAYOUT width=" + card.GetComponent<RectTransform>().rect.width * canvas.scaleFactor
                + " height=" + 240 * canvas.scaleFactor + " font=" + 18 * canvas.scaleFactor
                + " scale=" + canvas.scaleFactor + " screen=" + Screen.width + "x" + Screen.height);
        }

        private static GameObject NoticeElement(string name, Transform parent, Vector2 size, Vector2 position, bool stretchWidth = false)
        {
            var element = new GameObject(name, typeof(RectTransform));
            var rect = element.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = stretchWidth ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0.5f);
            rect.anchorMax = stretchWidth ? new Vector2(1, 0.5f) : new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return element;
        }

        private static void NoticeText(Transform parent, string value, Vector2 size, Vector2 position, int fontSize)
        {
            var obj = NoticeElement("Notice text", parent, size, position, true);
            var text = obj.AddComponent<UnityEngine.UI.Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
        }

        private sealed class VolatileStore : IStateStore
        {
            private Eldoria.Domain.PlayerState state;
            public Eldoria.Domain.PlayerState Load() => state;
            public void Save(Eldoria.Domain.PlayerState s) { state = s; }
        }
    }
}
