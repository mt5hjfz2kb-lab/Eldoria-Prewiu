using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Tests
{
    // Test-assembly compatibility type for legacy unqualified Text references.
    // It is excluded from player builds with the rest of the test assembly.
    public sealed class Text : UnityEngine.UI.Text { }

    public static class UiTextTestCompatibility
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void MirrorRuntimeLabels()
        {
            foreach (var source in Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None))
            {
                if (source is Text) continue;
                var mirror = source.gameObject.GetComponent<Text>() ?? source.gameObject.AddComponent<Text>();
                mirror.text = source.text;
                mirror.font = source.font;
                mirror.fontSize = source.fontSize;
                mirror.color = source.color;
                mirror.alignment = source.alignment;
                mirror.raycastTarget = false;
                mirror.enabled = false;
            }
        }
    }
}
