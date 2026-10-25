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
                    far.transform.localScale=new Vector3(scale.x*1.50f,scale.y*1.85f,scale.z*1.50f);
                    var size=far.bounds.size;float footprint=Mathf.Max(size.x,size.z);
                    if(footprint<12f){
                        // The source ecotone/castle/small plantation crowns are
                        // 1-4m high. A dense regional carpet makes the old sector
                        // legible as a rectangle even after its ground detail is
                        // culled. Keep its expected crown area, using fewer of
                        // the SAME anchors at the continental silhouette scale.
                        float coverage=footprint*footprint/(16f*16f);
                        retainedFarVisible=(identity%10000)/10000f<coverage;
                        if(retainedFarVisible){
                            scale=far.transform.localScale;
                            far.transform.localScale=new Vector3(scale.x*16f/Mathf.Max(.01f,footprint),
                                scale.y*14f/Mathf.Max(.01f,size.y),scale.z*16f/Mathf.Max(.01f,footprint));
                        }
                    }
                }
                else
                {
                    // Protected near crowns stay unchanged. The same distant
                    // anchors retain approximately their original covered area.
                    retainedFarVisible=identity%20==0;
                    var scale=far.transform.localScale;
                    if(retainedFarVisible)far.transform.localScale=new Vector3(scale.x*4.5f,scale.y*6.5f,scale.z*4.5f);
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
                float end=retained?Mathf.Clamp(lod.size/(2*47f),.0002f,.9f):.065f;
                float begin=retained?Mathf.Clamp(lod.size/(2*37f),end,.95f):.08f;
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
