#if UNITY_EDITOR
using System;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEngine;

namespace Eldoria.BreachWatch.Editor
{
    /// <summary>
    /// Optional editor-only installation of the entirely new Vigilia Canvas.
    /// This does not attach a skin to the old HUD or persist an edited scene.
    /// </summary>
    public static class BreachWatchLivePreview
    {
        const string Menu = "Eldoria/Vigilia de la Brecha/";

        [MenuItem(Menu + "Activar HUD independiente (Play)")]
        public static void Enable()
        {
            if(!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[VIGILIA] Enter Play in real Valoria before activating the new HUD.");
                return;
            }
            var host=UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if(!host || !host.Ready)
                throw new InvalidOperationException("The original Valoria gameplay session is not ready");
            var hud=BreachWatchHud.Install(host);
            if(!hud || !hud.IndependentCanvas || !hud.LegacyPresentationHidden)
                throw new InvalidOperationException("The preview did not create an independent Canvas");
            Debug.Log("[VIGILIA] Independent live Unity HUD installed in Play; no scene asset modified.");
        }

        [MenuItem(Menu + "Restaurar interfaz publicada (Play)")]
        public static void Restore()
        {
            var hud=UnityEngine.Object.FindFirstObjectByType<BreachWatchHud>();
            if(!hud)return;
            UnityEngine.Object.Destroy(hud.gameObject);
            Debug.Log("[VIGILIA] Returning to original production UI; do not save temporary Play hierarchy.");
        }
    }
}
#endif
