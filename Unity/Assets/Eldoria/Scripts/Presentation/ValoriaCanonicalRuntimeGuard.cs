using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Production fail-closed guard for canonical Valoria.
    ///
    /// The current player-facing city must be driven by ValoriaParcelPresentation
    /// (SHARP beauty + runtime parcel states). VisualWorld is a legacy QA/fallback
    /// path only and must never be shown silently in a player build.
    ///
    /// Editor is intentionally excluded so historical/QA regression captures can
    /// continue to exercise VisualWorld without being mistaken for production.
    /// </summary>
    public static class ValoriaCanonicalRuntimeGuard
    {
        static bool registered;
        static bool failedClosed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
#if UNITY_EDITOR
            return;
#else
            if(registered)return;
            registered=true;
            SceneManager.sceneLoaded-=OnSceneLoaded;
            SceneManager.sceneLoaded+=OnSceneLoaded;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ValidateInitialScene()
        {
#if !UNITY_EDITOR
            Validate(SceneManager.GetActiveScene());
#endif
        }

        static void OnSceneLoaded(Scene scene,LoadSceneMode mode)
        {
#if !UNITY_EDITOR
            Validate(scene);
#endif
        }

        static void Validate(Scene scene)
        {
#if UNITY_EDITOR
            return;
#else
            if(scene.name!="Valoria"||failedClosed)return;
            var canonical=Object.FindFirstObjectByType<ValoriaParcelPresentation>(FindObjectsInactive.Include);
            if(canonical!=null)return;

            FailClosed(
                "CANONICAL VALORIA RUNTIME MISSING\n\n"+
                "This build did not load ValoriaParcelPresentation.\n"+
                "Legacy VisualWorld fallback is forbidden in production.\n\n"+
                "BUILD INVALID — DO NOT USE");
#endif
        }

        static void FailClosed(string message)
        {
#if !UNITY_EDITOR
            failedClosed=true;
            Debug.LogError("[VALORIA-AUTHORITY] "+message.Replace("\n"," "));

            // Never allow the legacy procedural fallback to remain visible.
            foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                camera.enabled=false;

            Time.timeScale=0f;

            var root=new GameObject("VALORIA_CANONICAL_RUNTIME_GUARD_FATAL");
            Object.DontDestroyOnLoad(root);

            var blockerCamera=root.AddComponent<Camera>();
            blockerCamera.clearFlags=CameraClearFlags.SolidColor;
            blockerCamera.backgroundColor=new Color(.015f,.015f,.02f,1f);
            blockerCamera.cullingMask=0;
            blockerCamera.depth=10000f;
            blockerCamera.orthographic=true;

            var canvasObject=new GameObject("Invalid build overlay");
            canvasObject.transform.SetParent(root.transform,false);
            var canvas=canvasObject.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=32767;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            var panelObject=new GameObject("Message");
            panelObject.transform.SetParent(canvasObject.transform,false);
            var rect=panelObject.AddComponent<RectTransform>();
            rect.anchorMin=new Vector2(.08f,.18f);
            rect.anchorMax=new Vector2(.92f,.82f);
            rect.offsetMin=Vector2.zero;
            rect.offsetMax=Vector2.zero;

            var text=panelObject.AddComponent<Text>();
            text.text=message;
            text.alignment=TextAnchor.MiddleCenter;
            text.color=Color.white;
            text.fontSize=24;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;
            text.verticalOverflow=VerticalWrapMode.Overflow;
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#endif
        }
    }
}
