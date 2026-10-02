using System;
using Eldoria.Domain;
using UnityEngine;

namespace Eldoria.Presentation
{
    // Integrated benchmark-facing composite: selected backdrop + removal of obsolete dark legacy flank.
    // Visual-only. Existing accepted production layers remain authoritative.
    public static class ValoriaBenchmarkCompositeV1
    {
        public static bool Enabled=false;

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;

            SuppressChain("Valoria · rescued hero flank");

            var camera=Camera.main;
            if(camera==null)return;

            ValoriaCameraBackdropV1.Enabled=true;
            ValoriaCameraBackdropV1.CropX=.55f;
            ValoriaCameraBackdropV1.CropY=.10f;
            ValoriaCameraBackdropV1.CropW=.34f;
            ValoriaCameraBackdropV1.CropH=.60f;
            if(!ValoriaCameraBackdropV1.Build(parent,camera))
                throw new InvalidOperationException("Benchmark composite backdrop unavailable.");

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.47f,.56f,.61f);
            camera.farClipPlane=Mathf.Max(camera.farClipPlane,500f);

            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.57f,.62f,.64f);
            RenderSettings.fogStartDistance=30f;
            RenderSettings.fogEndDistance=78f;
        }

        static void SuppressChain(string exactName)
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(!string.Equals(t.name,exactName,StringComparison.Ordinal))continue;
                    r.enabled=false;
                    break;
                }
            }
        }
    }
}
