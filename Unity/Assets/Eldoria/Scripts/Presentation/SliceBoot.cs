using Eldoria.Application;
using Eldoria.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Presentation
{
    public static class SliceBoot
    {
        private const string WebGlSaveKey = "eldoria-unity-slice-owner-i-ii-v1";
        public static string SavePath => System.IO.Path.Combine(UnityEngine.Application.persistentDataPath,
            Eldoria.Domain.SliceContentProfiles.ActiveRuntimeProfile==Eldoria.Domain.SliceContentProfiles.QaFastId
                ? "eldoria-unity-slice-v1.json"
                : "eldoria-unity-slice-owner-i-ii-v1.json");

        public static void ResetLocalSaveAndRestart()
        {
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
            var message = SaveLoadNoticePolicy.Message(true);
            var root = new GameObject("Eldoria save recovery notice", typeof(RectTransform),
                typeof(Canvas), typeof(UnityEngine.UI.GraphicRaycaster));
            Object.DontDestroyOnLoad(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var backdrop = NoticeElement("Backdrop", root.transform, new Vector2(Screen.width, Screen.height), Vector2.zero);
            var shade = backdrop.AddComponent<UnityEngine.UI.Image>();
            shade.color = new Color(0, 0, 0, 0.65f);
            var width = Mathf.Min(Screen.width - 32f, 560f);
            var card = NoticeElement("Save notice", root.transform, new Vector2(width, 240), Vector2.zero);
            card.AddComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.12f, 0.18f, 1f);
            NoticeText(card.transform, "Problema con el guardado", new Vector2(width - 36, 42), new Vector2(0, 83), 20);
            NoticeText(card.transform, message, new Vector2(width - 36, 116), new Vector2(0, 10), 18);
            var buttonObject = NoticeElement("Continue without saving", card.transform, new Vector2(width - 36, 44), new Vector2(0, -86));
            buttonObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.18f, 0.35f, 0.50f, 1);
            var button = buttonObject.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => {
                Debug.Log("ELDORIA_SAVE_RECOVERY_NOTICE acknowledged=true");
                Object.Destroy(root);
            });
            NoticeText(buttonObject.transform, "Continuar sin guardar", new Vector2(width - 48, 40), Vector2.zero, 18);
            Debug.Log("ELDORIA_SAVE_RECOVERY_NOTICE shown=true message=" + message);
            Debug.Log("ELDORIA_SAVE_RECOVERY_LAYOUT width=" + width + " height=240 font=18 screen=" + Screen.width + "x" + Screen.height);
        }

        private static GameObject NoticeElement(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var element = new GameObject(name, typeof(RectTransform));
            var rect = element.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return element;
        }

        private static void NoticeText(Transform parent, string value, Vector2 size, Vector2 position, int fontSize)
        {
            var obj = NoticeElement("Notice text", parent, size, position);
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
