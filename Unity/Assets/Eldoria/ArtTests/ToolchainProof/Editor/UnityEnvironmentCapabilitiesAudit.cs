using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.EditorTools
{
    public static class UnityEnvironmentCapabilitiesAudit
    {
        [Serializable] class Check { public string name; public bool available; public string detail; }
        [Serializable] class Report {
            public string unityVersion;
            public string renderPipeline;
            public bool zeroSpend=true;
            public List<Check> checks=new List<Check>();
        }

        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var report=new Report {
                unityVersion=Application.unityVersion,
                renderPipeline=GraphicsSettings.currentRenderPipeline!=null?GraphicsSettings.currentRenderPipeline.GetType().FullName:"null"
            };

            Add(report,"URP_Lit",Shader.Find("Universal Render Pipeline/Lit")!=null,
                Shader.Find("Universal Render Pipeline/Lit")?.name ?? "missing");

            try {
                var mpb=new MaterialPropertyBlock();
                mpb.SetFloat("_EldoriaProof",1f);
                Add(report,"MaterialPropertyBlock",true,"runtime API available");
            } catch(Exception e){ Add(report,"MaterialPropertyBlock",false,e.Message); }

            try {
                var go=new GameObject("Decal probe");
                var decal=go.AddComponent<DecalProjector>();
                Add(report,"DecalProjector",decal!=null,decal!=null?decal.GetType().FullName:"missing");
                UnityEngine.Object.DestroyImmediate(go);
            } catch(Exception e){ Add(report,"DecalProjector",false,e.Message); }

            try {
                var go=new GameObject("Light probe group");
                var c=go.AddComponent<LightProbeGroup>();
                c.probePositions=new[]{Vector3.zero,Vector3.right,Vector3.forward,Vector3.up};
                Add(report,"LightProbeGroup",c!=null,"component available");
                UnityEngine.Object.DestroyImmediate(go);
            } catch(Exception e){ Add(report,"LightProbeGroup",false,e.Message); }

            try {
                var go=new GameObject("Reflection probe");
                var c=go.AddComponent<ReflectionProbe>();
                c.mode=ReflectionProbeMode.Baked;
                Add(report,"ReflectionProbe",c!=null,"baked probe component available");
                UnityEngine.Object.DestroyImmediate(go);
            } catch(Exception e){ Add(report,"ReflectionProbe",false,e.Message); }

            try {
                var go=new GameObject("LOD group");
                var c=go.AddComponent<LODGroup>();
                Add(report,"LODGroup",c!=null,"component available");
                UnityEngine.Object.DestroyImmediate(go);
            } catch(Exception e){ Add(report,"LODGroup",false,e.Message); }

            try {
                var data=new TerrainData();
                data.heightmapResolution=33;
                data.size=new Vector3(32,8,32);
                var go=Terrain.CreateTerrainGameObject(data);
                Add(report,"Terrain",go!=null,"TerrainData + Terrain creation available");
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(data);
            } catch(Exception e){ Add(report,"Terrain",false,e.Message); }

            try {
                var shader=Shader.Find("Universal Render Pipeline/Lit");
                var mat=shader!=null?new Material(shader):null;
                if(mat!=null) mat.enableInstancing=true;
                Add(report,"GPU_instancing_material_flag",mat!=null && mat.enableInstancing,
                    mat!=null?"Material.enableInstancing available":"URP Lit unavailable");
                if(mat!=null) UnityEngine.Object.DestroyImmediate(mat);
            } catch(Exception e){ Add(report,"GPU_instancing_material_flag",false,e.Message); }

            try {
                var volume=typeof(Volume);
                Add(report,"VolumeFramework",volume!=null,volume.FullName);
            } catch(Exception e){ Add(report,"VolumeFramework",false,e.Message); }

            Directory.CreateDirectory("UnityEnvironmentCapabilitiesAudit");
            File.WriteAllText("UnityEnvironmentCapabilitiesAudit/capabilities.json",JsonUtility.ToJson(report,true));
            Debug.Log("UNITY_ENVIRONMENT_CAPABILITIES_AUDIT="+JsonUtility.ToJson(report));
        }

        static void Add(Report r,string name,bool available,string detail)
        {
            r.checks.Add(new Check{name=name,available=available,detail=detail});
        }
    }
}
