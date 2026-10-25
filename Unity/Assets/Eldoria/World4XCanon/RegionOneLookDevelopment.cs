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
        Camera worldCamera; Light sun,configuredSun,fill; Quaternion rotation; LightShadows shadows; float strength,distance,depthBias,normalBias;int cascades;
        UniversalRenderPipelineAsset pipeline; bool scoped; Color background;
        readonly List<Material> owned=new List<Material>();
        readonly List<Material> minerals=new List<Material>();
        readonly List<Material> meadows=new List<Material>();
        readonly List<Renderer> fineDetails=new List<Renderer>();
        MaterialPropertyBlock fineProperties;float fineFade=-1f;
        readonly Dictionary<Material,Material> cache=new Dictionary<Material,Material>();
        Shader authoredShader;Vector3 origin;
        public static void Install(GameObject artwork,Camera camera)
        {
            var scope=artwork.AddComponent<RegionOneLookDevelopment>();scope.worldCamera=camera;
            var shader=Resources.Load<Shader>("World4X/RegionOneAuthoredSurface");
            if(!shader||!shader.isSupported)throw new InvalidOperationException("Authored AG surface shader missing");
            scope.authoredShader=shader;scope.origin=artwork.transform.position;scope.ApplyMaterials(artwork);
            var fillObject=new GameObject("Region 1 scoped sky fill");fillObject.transform.SetParent(artwork.transform,false);fillObject.layer=29;
            scope.fill=fillObject.AddComponent<Light>();scope.fill.type=LightType.Directional;scope.fill.shadows=LightShadows.None;
            scope.fill.color=new Color(.65f,.71f,.84f);scope.fill.intensity=.20f;scope.fill.cullingMask=1<<29;
            scope.fill.transform.rotation=Quaternion.Euler(30,145,0);scope.fill.enabled=false;
        }
        public void ApplyMaterials(GameObject artwork)
        {
            foreach(var renderer in artwork.GetComponentsInChildren<MeshRenderer>(true))
                renderer.sharedMaterials=renderer.sharedMaterials.Select(source=>{
                    if(cache.TryGetValue(source,out var existing))return existing;
                    var material=new Material(authoredShader){name=source.name+" AG look",enableInstancing=true};
                    Color colour=source.HasProperty("baseColorFactor")?source.GetColor("baseColorFactor"):source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):Color.white;
                    material.SetColor("_BaseColor",colour);material.SetVector("_RegionOrigin",origin);
                    if(source.name.StartsWith("Breach naturally graded exposed mineral",StringComparison.Ordinal)){material.SetFloat("_MineralEmission",.18f);minerals.Add(material);}
                    bool meadow=source.name.Contains("Region1 open meadows")||source.name.StartsWith("Ochre meadow",StringComparison.Ordinal)||
                        source.name.StartsWith("P connected meadow forest floor paths",StringComparison.Ordinal)||source.name.StartsWith("AI open meadow paths",StringComparison.Ordinal);
                    if(meadow){material.SetFloat("_MeadowFinish",1f);meadows.Add(material);}
                    if(meadow){
                        var detail=Resources.Load<Texture2D>("World4X/Blender/RegionOneGrassDetail");
                        if(!detail)throw new InvalidOperationException("Original AF-derived continental detail missing");
                        material.SetTexture("_GroundDetail",detail);// Native matched soil patches calibrate the linear detail to 0.9*sqrt(2).
                        if(source.name.Contains("Region1 open meadows"))material.SetFloat("_GroundDetailStrength",1.2727922f);
                    }
                    for(int p=0;p<source.shader.GetPropertyCount();p++)
                        if(source.shader.GetPropertyType(p)==ShaderPropertyType.Texture){string name=source.shader.GetPropertyName(p);if(name.ToLowerInvariant().Contains("basecolor"))material.SetTexture("_BaseMap",source.GetTexture(name));}
                    cache.Add(source,material);owned.Add(material);return material;
                }).ToArray();
            foreach(var renderer in artwork.GetComponentsInChildren<MeshRenderer>(true))
                if(IsCloseGroundDetail(renderer)&&!fineDetails.Contains(renderer))fineDetails.Add(renderer);
            // Instanced properties opt these shared crowns into GPU instancing;
            // no per-tree material duplication, no replacement canopy imagery.
            var properties=new MaterialPropertyBlock();properties.SetColor("_InstanceTint",Color.white);
            foreach(var renderer in artwork.GetComponentsInChildren<MeshRenderer>(true))
                if(renderer.name.StartsWith("BLD_Forest")||renderer.name.StartsWith("Pilot_Pine")||renderer.name.StartsWith("Far_Pilot_Pine"))renderer.SetPropertyBlock(properties);
        }
        public void SetMeadowFinish(bool enabled){foreach(var material in meadows)material.SetFloat("_MeadowFinish",enabled?1f:0f);}
        static bool IsCloseGroundDetail(Renderer renderer)
        {
            if(renderer.sharedMaterials.Any(material=>material&&(
                material.name.StartsWith("Near grass shade",StringComparison.Ordinal)||
                material.name.StartsWith("Near meadow dry grass",StringComparison.Ordinal)||
                material.name.StartsWith("Near bank soft olive",StringComparison.Ordinal)||
                material.name.StartsWith("Near underbrush",StringComparison.Ordinal)||
                material.name.StartsWith("Near riverbank living shrubs",StringComparison.Ordinal)||
                material.name.StartsWith("Near dry scrub",StringComparison.Ordinal)||
                material.name.StartsWith("Understory ",StringComparison.Ordinal)||
                material.name.StartsWith("Z woodland understory",StringComparison.Ordinal))))return true;
            // glTF multi-material primitives may live below their named source
            // node. Match that ancestry as well as the renderer itself.
            for(var node=renderer.transform;node;node=node.parent){
                string name=node.name;
                if(name=="Directed understory mesh"||name=="Z retained-sector woodland understory"||
                    name=="BLD_NearMeadowHabitat"||name=="BLD_NearUndergrowth")return true;
            }
            return renderer.name.StartsWith("Bank forest stone ",StringComparison.Ordinal)&&
                Mathf.Max(renderer.bounds.size.x,renderer.bounds.size.z)<7f;
        }

        public void RefreshForZoom()
        {
            if(!worldCamera)return;
            float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(75f,170f,worldCamera.orthographicSize));
            if(Mathf.Abs(fade-fineFade)<.0001f)return;fineFade=fade;
            if(fineProperties==null)fineProperties=new MaterialPropertyBlock();
            foreach(var renderer in fineDetails)if(renderer){
                renderer.enabled=fade<.999f;fineProperties.Clear();renderer.GetPropertyBlock(fineProperties);
                fineProperties.SetFloat("_Fade",fade);renderer.SetPropertyBlock(fineProperties);
            }
        }
        void OnEnable(){RenderPipelineManager.beginCameraRendering+=Begin;RenderPipelineManager.endCameraRendering+=End;}
        static float GroundShadowCoverage(Camera camera,Vector3 origin,float fadeBorder)
        {
            // Orthographic strategic cameras can stand hundreds of metres above
            // the map. Cover their actual ground footprint, including fade-out,
            // rather than exhausting a fixed 300m radius before reaching soil.
            var plane=new Plane(Vector3.up,origin);float furthest=300f;
            for(int x=0;x<2;x++)for(int y=0;y<2;y++){
                var ray=camera.ViewportPointToRay(new Vector3(x,y,0));
                if(plane.Raycast(ray,out float enter))furthest=Mathf.Max(furthest,Vector3.Distance(camera.transform.position,ray.GetPoint(enter)));
            }
            return Mathf.Min(camera.farClipPlane,furthest/Mathf.Max(.5f,1f-fadeBorder)*1.04f);
        }
        void Begin(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=worldCamera||scoped)return;
            RefreshForZoom();
            var navigation=camera.GetComponent<Eldoria.World4XPrototype.ZoneOneStrategicCamera>();
            float regional=navigation?navigation.regionalZoom:Mathf.Max(280,400/Mathf.Max(.1f,camera.aspect));
            // The physical outer edge enters the viewport during the orbit,
            // before the final overview. Fade only that same peripheral strip;
            // no fog is applied to the interior or to the retained AG sector.
            float atmosphere=Mathf.SmoothStep(0,1,Mathf.InverseLerp(regional*.75f,regional*1.25f,camera.orthographicSize));
            var fog=QualitySettings.activeColorSpace==ColorSpace.Linear?camera.backgroundColor.linear:camera.backgroundColor;
            foreach(var material in owned){material.SetFloat("_BoundaryWeight",atmosphere);material.SetVector("_BoundaryColour",new Vector4(fog.r,fog.g,fog.b,1));}
            float mineralEmission=Mathf.Lerp(.65f,.85f,Mathf.InverseLerp(36f,280f,camera.orthographicSize));
            if(camera.orthographicSize>280f)mineralEmission=Mathf.Lerp(.85f,1.10f,Mathf.InverseLerp(280f,navigation?navigation.globalZoom:2500f,camera.orthographicSize));
            foreach(var material in minerals)material.SetFloat("_MineralEmission",mineralEmission);
            background=camera.backgroundColor; // Preserve the existing continental clear colour; grey increased the tile silhouette.
            configuredSun=RenderSettings.sun;sun=configuredSun;
            if(!sun)sun=FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.enabled&&l.type==LightType.Directional).OrderByDescending(l=>l.intensity).FirstOrDefault();
            if(sun){rotation=sun.transform.rotation;shadows=sun.shadows;strength=sun.shadowStrength;depthBias=sun.shadowBias;normalBias=sun.shadowNormalBias;sun.transform.rotation=Quaternion.Euler(42,-35,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.9f;}
            pipeline=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(pipeline){
                distance=pipeline.shadowDistance;cascades=pipeline.shadowCascadeCount;
                if(camera.orthographicSize<110f)pipeline.shadowDistance=300f;
                else{
                    pipeline.shadowDistance=GroundShadowCoverage(camera,origin,pipeline.cascadeBorder);
                    // A parallel orthographic frustum does not need a perspective
                    // near cascade over empty air above the ground. Use the full
                    // existing atlas for this one world footprint.
                    pipeline.shadowCascadeCount=1;
                    if(sun){sun.shadowBias=.6f;sun.shadowNormalBias=.35f;}
                }
            }
            // The imported Tripo PBR materials need sky fill when the new real
            // shadows occlude the key light. Keep their albedo/normal/ORM intact.
            if(sun)RenderSettings.sun=sun;
            if(fill)fill.enabled=true;
            scoped=true;
        }
        void End(ScriptableRenderContext context,Camera camera){if(camera==worldCamera)Restore();}
        void Restore(){if(!scoped)return;if(fill)fill.enabled=false;RenderSettings.sun=configuredSun?configuredSun:null;if(sun){sun.transform.rotation=rotation;sun.shadows=shadows;sun.shadowStrength=strength;sun.shadowBias=depthBias;sun.shadowNormalBias=normalBias;}if(pipeline){pipeline.shadowDistance=distance;pipeline.shadowCascadeCount=cascades;}if(worldCamera)worldCamera.backgroundColor=background;scoped=false;}
        void OnDisable(){RenderPipelineManager.beginCameraRendering-=Begin;RenderPipelineManager.endCameraRendering-=End;Restore();}
        void OnDestroy(){foreach(var material in owned)if(material)Destroy(material);}
    }
}

