using System;
using System.Collections;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Installs the exclusive Vigilia presentation on the canonical live
    /// session in every player build. No URL flag, extra save or duplicate
    /// controller is needed. A failed install cannot restore retired art.
    /// </summary>
    public sealed class BreachWatchBetaBootstrap : MonoBehaviour
    {
        const string Flag = "eldoria_vigilia";
        const float MaximumWaitSeconds = 45f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {
#if !UNITY_EDITOR
            if(FindFirstObjectByType<BreachWatchHud>())
                return;
            var watcher=new GameObject("VIGILIA / production presentation");
            watcher.AddComponent<BreachWatchBetaBootstrap>();
#endif
        }

        public static bool IsOptedIn(string url)
        {
            if(string.IsNullOrEmpty(url))return false;
            int q=url.IndexOf('?');
            if(q<0)return false;
            int hash=url.IndexOf('#',q);
            string query=hash<0?url.Substring(q+1):
                url.Substring(q+1,hash-q-1);
            foreach(var item in query.Split('&'))
            {
                if(string.IsNullOrEmpty(item))continue;
                int eq=item.IndexOf('=');
                if(eq<=0)continue;
                var key=item.Substring(0,eq);
                var val=item.Substring(eq+1);
                // Do not enable Vigilia because of an unrelated 'debug=1'
                // or strings containing the flag inside a UTM parameter.
                if(key==Flag && val=="1")return true;
            }
            return false;
        }

        void OnEnable(){Canvas.willRenderCanvases+=SuppressRetiredDuringStartup;}
        void OnDisable(){Canvas.willRenderCanvases-=SuppressRetiredDuringStartup;}
        void SuppressRetiredDuringStartup()
        {
            // Async scene initialization can create the old controller after
            // this watcher's Update. Suppress its Canvas before that very
            // first render, including slow devices still loading after 45s.
            var host=FindFirstObjectByType<ValoriaIntegratedWorld>();
            if(host && host.Hud)
            {
                var canvas=host.Hud.GetComponent<Canvas>();
                if(canvas)canvas.enabled=false;
            }
        }

        IEnumerator Start()
        {
            float deadline=Time.realtimeSinceStartup+MaximumWaitSeconds;
            while(true)
            {
                var host=FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(host && host.Hud)
                {
                    var retired=host.Hud.GetComponent<Canvas>();
                    if(retired)retired.enabled=false;
                }
                if(host && host.Ready && host.Hud)
                {
                    try
                    {
                        var installed=BreachWatchHud.Install(host);
                        if(!installed || !installed.IndependentCanvas ||
                            !installed.LegacyPresentationHidden ||
                            !installed.CanonicalModalVisibleOnIndependentCanvas)
                            throw new InvalidOperationException(
                                "Vigilia UI did not replace original Canvas");
                        Debug.Log("VIGILIA_PRODUCTION_ONLY_LIVE_SESSION");
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                        // Production never falls back to the retired interface.
                        // Preserve the failure for the release gate; no save is changed.
                        var partial=FindFirstObjectByType<BreachWatchHud>();
                        if(partial && !partial.ProductionPresentation)Destroy(partial.gameObject);
                        var oldCanvas=host.Hud.GetComponent<Canvas>();
                        if(oldCanvas)oldCanvas.enabled=false;
                        Debug.LogError("VIGILIA_PRODUCTION_PRESENTATION_FAILED_NO_LEGACY_FALLBACK");
                    }
                    Destroy(gameObject);
                    yield break;
                }
                if(Time.realtimeSinceStartup>=deadline)
                {
                    Debug.LogError("VIGILIA_PRODUCTION_CANONICAL_SESSION_NOT_READY");
                    deadline=float.PositiveInfinity;
                }
                yield return null;
            }
        }
    }
}
