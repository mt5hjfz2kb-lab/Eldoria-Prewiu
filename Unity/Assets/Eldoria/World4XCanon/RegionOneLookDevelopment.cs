using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Eldoria.World4XCanon
{
    // Restores the actual AG look-development recipe only while the WORLD camera renders.
    // City cameras, imported assets, player state and shared source materials are untouched.
    public sealed class RegionOneLookDevelopment : MonoBehaviour
    {
        Camera worldCamera; Light sun; Quaternion rotation; LightShadows shadows; float strength,distance;
        UniversalRenderPipelineAsset pipeline; bool scoped; Color background;
        readonly List<Material> owned=new List<Material>();
        readonly Dictionary<Material,Material> cache=new Dictionary<Material,Material>();
        Shader authoredShader;Vector3 origin;
        public static void Install(GameObject artwork,Camera camera)
        {
            var scope=artwork.AddComponent<RegionOneLookDevelopment>();scope.worldCamera=camera;
            var shader=Resources.Load<Shader>("World4X/RegionOneAuthoredSurface");
            if(!shader||!shader.isSupported)throw new InvalidOperationException("Authored AG surface shader missing");
            scope.authoredShader=shader;scope.origin=artwork.transform.position;scope.ApplyMaterials(artwork);
        }
        public void ApplyMaterials(GameObject artwork)
        {
            foreach(var renderer in artwork.GetComponentsInChildren<MeshRenderer>(true))
                renderer.sharedMaterials=renderer.sharedMaterials.Select(source=>{
                    if(cache.TryGetValue(source,out var existing))return existing;
                    var material=new Material(authoredShader){name=source.name+" AG look",enableInstancing=true};
                    Color colour=source.HasProperty("baseColorFactor")?source.GetColor("baseColorFactor"):source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):Color.white;
                    material.SetColor("_BaseColor",colour);material.SetVector("_RegionOrigin",origin);
                    if(source.name.Contains("Region1 open meadows")){
                        var detail=Resources.Load<Texture2D>("World4X/Blender/RegionOneGrassDetail");
                        if(!detail)throw new InvalidOperationException("Original AF-derived continental detail missing");
                        material.SetTexture("_GroundDetail",detail);material.SetFloat("_GroundDetailStrength",.85f);
                    }
                    for(int p=0;p<source.shader.GetPropertyCount();p++)
                        if(source.shader.GetPropertyType(p)==ShaderPropertyType.Texture){string name=source.shader.GetPropertyName(p);if(name.ToLowerInvariant().Contains("basecolor"))material.SetTexture("_BaseMap",source.GetTexture(name));}
                    cache.Add(source,material);owned.Add(material);return material;
                }).ToArray();
            // Instanced properties opt these shared crowns into GPU instancing;
            // no per-tree material duplication, no replacement canopy imagery.
            var properties=new MaterialPropertyBlock();properties.SetColor("_InstanceTint",Color.white);
            foreach(var renderer in artwork.GetComponentsInChildren<MeshRenderer>(true))
                if(renderer.name.StartsWith("BLD_Forest")||renderer.name.StartsWith("Pilot_Pine")||renderer.name.StartsWith("Far_Pilot_Pine"))renderer.SetPropertyBlock(properties);
        }
        void OnEnable(){RenderPipelineManager.beginCameraRendering+=Begin;RenderPipelineManager.endCameraRendering+=End;}
        void Begin(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=worldCamera||scoped)return;
            float overview=Mathf.Max(910/Mathf.Max(.1f,camera.aspect),1625)*1.04f;
            float atmosphere=Mathf.SmoothStep(0,1,Mathf.InverseLerp(overview*.85f,overview,camera.orthographicSize));
            foreach(var material in owned)material.SetFloat("_BoundaryWeight",atmosphere);
            background=camera.backgroundColor; // Preserve the existing continental clear colour; grey increased the tile silhouette.
            sun=RenderSettings.sun;
            if(!sun)sun=FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.enabled&&l.type==LightType.Directional).OrderByDescending(l=>l.intensity).FirstOrDefault();
            if(sun){rotation=sun.transform.rotation;shadows=sun.shadows;strength=sun.shadowStrength;sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.9f;}
            pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(pipeline){distance=pipeline.shadowDistance;pipeline.shadowDistance=300;}
            scoped=true;
        }
        void End(ScriptableRenderContext context,Camera camera){if(camera==worldCamera)Restore();}
        void Restore(){if(!scoped)return;if(sun){sun.transform.rotation=rotation;sun.shadows=shadows;sun.shadowStrength=strength;}if(pipeline)pipeline.shadowDistance=distance;if(worldCamera)worldCamera.backgroundColor=background;scoped=false;}
        void OnDisable(){RenderPipelineManager.beginCameraRendering-=Begin;RenderPipelineManager.endCameraRendering-=End;Restore();}
        void OnDestroy(){foreach(var material in owned)if(material)Destroy(material);}
    }
}
