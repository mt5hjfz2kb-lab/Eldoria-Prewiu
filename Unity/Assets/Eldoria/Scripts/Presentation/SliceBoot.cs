using Eldoria.Application;
using Eldoria.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Presentation
{
    public static class SliceBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindFirstObjectByType<SlicePresenter>() != null) return;
            var obj = new GameObject("Eldoria composition root");
            Object.DontDestroyOnLoad(obj);
            var ui = obj.AddComponent<SlicePresenter>();
            var save = new FileStateStore(System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "eldoria-unity-slice-v1.json"));
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
        private sealed class VolatileStore : IStateStore
        {
            private Eldoria.Domain.PlayerState state;
            public Eldoria.Domain.PlayerState Load() => state;
            public void Save(Eldoria.Domain.PlayerState s) { state = s; }
        }
    }
}
