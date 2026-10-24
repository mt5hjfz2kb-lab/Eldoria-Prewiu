using System;
using System.Linq;
using UnityEngine;
namespace Eldoria.World4XCanon
{
    // Pairs independently exported near/far geometry; no second geography.
    public static class RegionOneProductionLod
    {
        public static void Install(GameObject artwork)
        {
            var all=artwork.GetComponentsInChildren<MeshRenderer>(true);
            var byName=all.GroupBy(x=>x.name).ToDictionary(x=>x.Key,x=>x.Single());
            foreach(var near in all.Where(x=>x.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)||x.name.StartsWith("Pilot_Pine",StringComparison.Ordinal)))
            {
                string farName=near.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)?near.name.Replace("BLD_ForestMass_","BLD_ForestFar_"):"Far_"+near.name;
                if(!byName.TryGetValue(farName,out var far))continue;
                var group=new GameObject("LOD "+near.name);group.transform.SetParent(artwork.transform,false);
                // Reparent with world transforms preserved, including glTF handedness.
                near.transform.SetParent(group.transform,true);far.transform.SetParent(group.transform,true);
                var lod=group.AddComponent<LODGroup>();lod.fadeMode=LODFadeMode.CrossFade;lod.animateCrossFading=false;
                lod.SetLODs(new[]{new LOD(.065f,new Renderer[]{near}),new LOD(.0001f,new Renderer[]{far})});lod.RecalculateBounds();
                bool retained=near.name.StartsWith("Pilot_Pine",StringComparison.Ordinal);
                // A spatial crossfade reproduces AG's 37–47 band, including small
                // retained crowns. It is deterministic during actual Camera.Render.
                float end=retained?Mathf.Clamp(lod.size/(2*47f),.0002f,.9f):.065f;
                float begin=retained?Mathf.Clamp(lod.size/(2*37f),end,.95f):.08f;
                var close=new LOD(end,new Renderer[]{near}){fadeTransitionWidth=(begin-end)/(1-end)};
                lod.SetLODs(new[]{close,new LOD(.0001f,new Renderer[]{far})});
            }
        }
    }
}
