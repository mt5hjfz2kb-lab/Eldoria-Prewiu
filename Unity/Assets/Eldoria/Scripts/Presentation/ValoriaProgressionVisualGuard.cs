using Eldoria.Domain;
using Eldoria.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Presentation
{
    // Product-parity guard: the web vertical slice remains the canonical progression contract.
    // Production art may exist in the scene for composition/certification, but the player must
    // only see and interact with it when its canonical progression state has been reached.
    public sealed class ValoriaProgressionVisualGuard : MonoBehaviour
    {
        float nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureInstalled()
        {
            if (Object.FindFirstObjectByType<ValoriaProgressionVisualGuard>() != null) return;
            var root = new GameObject("Valoria progression visual guard");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<ValoriaProgressionVisualGuard>();
        }

        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .20f;
            if (SceneManager.GetActiveScene().name != "Valoria") return;

            PlayerState state = null;
            try { state = new FileStateStore(SliceBoot.SavePath).Load(); }
            catch { return; }
            if (state == null) return;

            Apply(state);
        }

        public static void Apply(PlayerState state)
        {
            if (state == null) return;

            // Bastion I: Aserradero is a reconstruction objective, never a completed building
            // before SawmillLevel becomes active. Keep its independent target/hotspot untouched.
            SetArchitectureVisible("Aserradero", state.SawmillLevel > 0);

            // Bastion II: Cuartel must not leak into Bastion I merely because its production
            // asset is already present for art composition. The completed architecture appears
            // only once construction has actually completed.
            SetArchitectureVisible("Cuartel", state.BarracksLevel > 0);
            SetScaffoldVisible("Cuartel scaffold", state.BastionLevel >= 2 && state.BarracksLevel == 0);

            // The Cuartel interaction itself is also progression-gated. Imported visual colliders
            // remain disabled; only the certified invisible target may become interactive.
            foreach (var hotspot in Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (hotspot == null || hotspot.Id != "barracks" ||
                    !hotspot.gameObject.name.EndsWith("· target", System.StringComparison.Ordinal)) continue;
                var collider = hotspot.GetComponent<Collider>();
                if (collider != null) collider.enabled = state.BastionLevel >= 2;
            }
        }

        static void SetArchitectureVisible(string prefix, bool visible)
        {
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!HasAncestorPrefix(renderer.transform, prefix)) continue;
                if (HasAncestorSuffix(renderer.transform, "· target")) continue;
                if (HasAncestorPrefix(renderer.transform, prefix + " scaffold")) continue;
                renderer.enabled = visible;
            }
        }

        static void SetScaffoldVisible(string prefix, bool visible)
        {
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (HasAncestorPrefix(renderer.transform, prefix)) renderer.enabled = visible;
        }

        static bool HasAncestorPrefix(Transform current, string prefix)
        {
            for (var t = current; t != null; t = t.parent)
                if (t.name.StartsWith(prefix, System.StringComparison.Ordinal)) return true;
            return false;
        }

        static bool HasAncestorSuffix(Transform current, string suffix)
        {
            for (var t = current; t != null; t = t.parent)
                if (t.name.EndsWith(suffix, System.StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
