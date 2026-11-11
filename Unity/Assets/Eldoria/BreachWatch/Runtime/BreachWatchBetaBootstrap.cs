using System;
using System.Collections;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Browser-only, explicit opt-in for testing the REAL Vigilia interface
    /// before any main/beta rollout. Loading a page without ?eldoria_vigilia=1
    /// leaves the current live HUD completely unchanged. No demo session,
    /// second save, fake rewards or duplicate controller is created.
    ///
    /// After visual/functional certification, release integration can switch
    /// the canonical presentation; this URL test hook is not a release gate.
    /// </summary>
    public sealed class BreachWatchBetaBootstrap : MonoBehaviour
    {
        const string Flag = "eldoria_vigilia";
        const float MaximumWaitSeconds = 45f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if(!IsOptedIn(Application.absoluteURL))
                return;
            if(FindFirstObjectByType<BreachWatchHud>())
                return;
            var watcher=new GameObject("VIGILIA / browser beta opt-in");
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

        IEnumerator Start()
        {
            float deadline=Time.realtimeSinceStartup+MaximumWaitSeconds;
            while(Time.realtimeSinceStartup<deadline)
            {
                var host=FindFirstObjectByType<ValoriaIntegratedWorld>();
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
                        Debug.Log("VIGILIA_WEBGL_EXPLICIT_OPT_IN_LIVE_SESSION");
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                        // Fail closed: if an asset/scene is missing, destroy
                        // this provisional UI. BreachWatchHud.OnDestroy
                        // restores original Canvas, raycaster and blocking.
                        var partial=FindFirstObjectByType<BreachWatchHud>();
                        if(partial)Destroy(partial.gameObject);
                        Debug.LogError("VIGILIA_OPT_IN_ROLLED_BACK_TO_CANONICAL_HUD");
                    }
                    Destroy(gameObject);
                    yield break;
                }
                yield return null;
            }
            Debug.LogWarning("VIGILIA_OPT_IN_CANONICAL_SESSION_NOT_READY");
            Destroy(gameObject);
        }
    }
}
