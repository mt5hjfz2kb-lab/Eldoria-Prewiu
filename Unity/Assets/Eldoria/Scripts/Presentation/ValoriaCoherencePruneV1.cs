using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Removes overlapping legacy visual generations so one coherent urban family owns the frame.
    public static class ValoriaCoherencePruneV1
    {
        public static bool Enabled=false;
        public static int Suppressed{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled)return;
            Suppressed=0;

            foreach(var token in new[]{
                "Valoria · Mid-Tier District v1 · production visual only",
                "Valoria · Full Frame Architecture Batch v1",
                "Valoria · Full Frame Foreground Edge v1",
                "Valoria · reused civil house",
                "Valoria · hero frame inhabited roofline",
                "VPD · terrain seam · rock",
                "VPD · expansion edge geology · rock",
                "VPD · stair shoulder rock · rock",
                "Ruined imperial arch fall · rock"
            }) SuppressChainContains(token);
        }

        static void SuppressChainContains(string token)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                bool match=false;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name.IndexOf(token,StringComparison.OrdinalIgnoreCase)>=0){match=true;break;}
                }
                if(match){r.enabled=false;Suppressed++;}
            }
        }
    }
}
