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
                // Prefer more legible low-poly conifer silhouettes at strategic
                // distance, not an extra carpet or enlarged close-up donor.
                // The far mesh is independently exported, keeps its original
                // ground anchor, and still crossfades through the existing LOD.
                bool retained=near.name.StartsWith("Pilot_Pine",StringComparison.Ordinal);
                bool retainedFarVisible=true;
                uint identity=2166136261;
                foreach(char character in near.name)identity=unchecked((identity^character)*16777619);
                if(!retained)
                {
                    var scale=far.transform.localScale;
                    far.transform.localScale=new Vector3(scale.x*2.10f,scale.y*2.60f,scale.z*2.10f);
                    var size=far.bounds.size;float footprint=Mathf.Max(size.x,size.z);
                    if(footprint>=12f){
                        float crown=20f+(identity%7);
                        far.transform.localScale=new Vector3(far.transform.localScale.x*crown/footprint,
                            far.transform.localScale.y*(34f+identity%8)/Mathf.Max(.01f,size.y),
                            far.transform.localScale.z*crown/footprint);
                    }
                    if(footprint<12f){
                        // The source ecotone/castle/small plantation crowns are
                        // 1-4m high. A dense regional carpet makes the old sector
                        // legible as a rectangle even after its ground detail is
                        // culled. Keep its expected crown area, using fewer of
                        // the SAME anchors at the continental silhouette scale.
                        float coverage=footprint*footprint/(22f*22f);
                        retainedFarVisible=(identity%10000)/10000f<coverage;
                        if(retainedFarVisible){
                            scale=far.transform.localScale;
                            far.transform.localScale=new Vector3(scale.x*22f/Mathf.Max(.01f,footprint),
                                scale.y*34f/Mathf.Max(.01f,size.y),scale.z*22f/Mathf.Max(.01f,footprint));
                        }
                    }
                }
                else
                {
                    // Preserve every close donor. At strategic distance use the
                    // same crown envelope as the adjacent continental ecotone;
                    // sparse 60m crowns made the retained patch look monumental.
                    retainedFarVisible=identity%5==0;
                    if(retainedFarVisible){
                        var size=far.bounds.size;
                        float footprint=Mathf.Max(size.x,size.z);
                        float crown=20f+((identity/5)%5);
                        var scale=far.transform.localScale;
                        far.transform.localScale=new Vector3(scale.x*crown/Mathf.Max(.01f,footprint),
                            scale.y*(34f+identity%7)/Mathf.Max(.01f,size.y),scale.z*crown/Mathf.Max(.01f,footprint));
                    }
                }
                // Scale about the existing canopy base, not the ground-origin
                // pivot: these protected pine meshes start above a separate stem.
                // This keeps the same contact height through the LOD switch.
                if(retainedFarVisible)
                    far.transform.position+=Vector3.up*(near.bounds.min.y-far.bounds.min.y);
                var lod=group.AddComponent<LODGroup>();lod.fadeMode=LODFadeMode.CrossFade;lod.animateCrossFading=false;
                lod.SetLODs(new[]{new LOD(.065f,new Renderer[]{near}),new LOD(.0001f,new Renderer[]{far})});lod.RecalculateBounds();
                // A spatial crossfade reproduces AG's 37–47 band, including small
                // retained crowns. It is deterministic during actual Camera.Render.
                float end=Mathf.Clamp(lod.size/(2*47f),.0002f,.9f);
                float begin=Mathf.Clamp(lod.size/(2*37f),end,.95f);
                var close=new LOD(end,new Renderer[]{near}){fadeTransitionWidth=(begin-end)/(1-end)};
                if(retainedFarVisible)lod.SetLODs(new[]{close,new LOD(.0001f,new Renderer[]{far})});
                else
                {
                    lod.SetLODs(new[]{close});
                    far.enabled=false;
                    if(retained)far.gameObject.SetActive(false);
                }
            }
        }
    }
}

