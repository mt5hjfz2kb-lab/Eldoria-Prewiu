using System;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// A small source-only adapter for the canonical integrated Royal HUD.
    /// Replaces drawing of three existing resource icon slots, not their
    /// RectTransforms, input/click handlers, resources or layout authority.
    /// </summary>
    [DefaultExecutionOrder(2200)]
    public sealed class RoyalResourcePictogramInstaller : MonoBehaviour
    {
        private static bool subscribed;
        private float nextScan;
        private int appliedHudId = int.MinValue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Subscribe()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            subscribed = true;
        }

        private static void OnLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.name.StartsWith("Valoria", StringComparison.OrdinalIgnoreCase))
                return;
            if (UnityEngine.Object.FindFirstObjectByType<RoyalResourcePictogramInstaller>() != null)
                return;
            var root = new GameObject("Valoria / resource pictograms (presentation only)");
            DontDestroyOnLoad(root);
            root.AddComponent<RoyalResourcePictogramInstaller>();
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + .75f;
            var huds = UnityEngine.Object.FindObjectsByType<RoyalHUD>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var hud in huds)
            {
                if (!hud || !hud.isActiveAndEnabled ||
                    !hud.GetComponent<RoyalGoldV2Hud>() || !hud.Resources)
                    continue;
                if (appliedHudId == hud.GetInstanceID()) return;
                var parent = hud.Resources.transform;
                var wood = Apply(parent, "Madera icon", ResourcePictogramKind.Wood);
                var stone = Apply(parent, "Piedra icon", ResourcePictogramKind.Stone);
                var food = Apply(parent, "Comida icon", ResourcePictogramKind.Food);
                if (wood && stone && food) appliedHudId = hud.GetInstanceID();
            }
        }

        private static bool Apply(Transform parent, string slot, ResourcePictogramKind kind)
        {
            var icon = parent.Find(slot);
            if (!icon) return false;
            var replacement = icon.GetComponent<RoyalResourcePictogram>();
            if (!replacement)
                replacement = icon.gameObject.AddComponent<RoyalResourcePictogram>();
            replacement.Kind = kind;
            replacement.color = Color.white;
            replacement.raycastTarget = false;
            // Disabling the former flat artwork preserves its source and
            // active/inactive state; Food still unlocks with its real wallet.
            foreach (var graphic in icon.GetComponents<Graphic>())
                if (graphic != replacement) graphic.enabled = false;
            return true;
        }
    }
}
