#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Eldoria.Branding.Editor
{
    public static class EonscarBrandingTrialEditor
    {
        [MenuItem("Eldoria/EONSCAR/Mostrar prueba sobre la partida")]
        public static void ShowInGame()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("EONSCAR: abre Valoria y pulsa Play; después selecciona Eldoria/EONSCAR/Mostrar prueba sobre la partida.");
                return;
            }
            EonscarBrandingTrialOverlay.Show();
        }
    }
}
#endif
