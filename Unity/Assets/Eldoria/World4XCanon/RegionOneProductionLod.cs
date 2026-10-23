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
            foreach(var near in all.Where(x=>x.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)||x.name.StartsWith("Pilot_Pine",StringComparison.Ordinal)))
            {
                string farName=near.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)?near.name.Replace("BLD_ForestMass_","BLD_ForestFar_"):"Far_"+near.name;
                var far=all.SingleOrDefault(x=>x.name==farName);if(!far)continue;
                var group=new GameObject("LOD "+near.name);group.transform.SetParent(artwork.transform,false);
                // Reparent with world transforms preserved, including glTF handedness.
                near.transform.SetParent(group.transform,true);far.transform.SetParent(group.transform,true);
                var lod=group.AddComponent<LODGroup>();lod.fadeMode=LODFadeMode.CrossFade;lod.animateCrossFading=true;
                lod.SetLODs(new[]{new LOD(near.name.StartsWith("BLD_")?.20f:.065f,new Renderer[]{near}),new LOD(.0001f,new Renderer[]{far})});lod.RecalculateBounds();
            }
        }
    }
}
