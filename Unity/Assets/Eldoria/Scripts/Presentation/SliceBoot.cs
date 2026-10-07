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
            SceneManager.LoadScene("Valoria");
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
            }
            SceneManager.sceneLoaded += ui.OnSceneLoaded;
            if (SceneManager.GetActiveScene().name == "Bootstrap") SceneManager.LoadScene("Valoria");
            else ui.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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

        private sealed class VolatileStore : IStateStore
        {
            private Eldoria.Domain.PlayerState state;
            public Eldoria.Domain.PlayerState Load() => state;
            public void Save(Eldoria.Domain.PlayerState s) { state = s; }
        }
    }
}
