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
    /// <summary>
    /// Zero-cost isolated Unity URP finishing study. Does not replace authored 3D assets.
    /// Every family shares a deterministic style kit and camera-based verification.
    /// </summary>
    public static class ValoriaUnityFinishKit
    {
        static Material stageMaterial;
        static Texture2D stageAlbedo;

        static readonly Dictionary<Material,Material> Finished = new Dictionary<Material,Material>();
        const string Study = "Assets/Eldoria/Experimental/ValoriaFinishV1";

        public static void ConfigureIsolatedPipeline()
        {
            Directory.CreateDirectory(Study);
            var original=GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if(original==null)throw new Exception("Actual URP asset required");
            var pipeline=UnityEngine.Object.Instantiate(original);
            pipeline.name="Valoria isolated art URP (desktop evidence, not mobile certification)";
            pipeline.shadowDistance=260f;
            pipeline.shadowCascadeCount=4;
            pipeline.msaaSampleCount=4;
            pipeline.supportsHDR=true;
            pipeline.useSRPBatcher=true;
            var serialized=new SerializedObject(pipeline);
            var resolution=serialized.FindProperty("m_MainLightShadowmapResolution");
            if(resolution!=null)resolution.intValue=4096;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            string path=Study+"/ValoriaStudyURP.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(pipeline,path);
            GraphicsSettings.defaultRenderPipeline=pipeline;
            QualitySettings.renderPipeline=pipeline;
        }

        // Cache by original source material, not by renderer: the gate has >1000 parts.
        public static Material FinishMaterial(Material source,string piece)
        {
            if(Finished.TryGetValue(source,out var cached))return cached;
            var m=new Material(source){name="ValoriaFinish_"+source.name};
            ApplyArchitecturalMaterial(m,piece);
            // glTFast shaders already implement glTF channel conventions. Preserve their
            // authored maps rather than copying packed roughness into URP smoothness.
            foreach(string slot in new[]{"_BaseMap","_BaseColorTexture","_MainTex"})
            {
                if(!m.HasProperty(slot))continue;
                var texture=m.GetTexture(slot);
                if(texture==null)continue;
                texture.filterMode=FilterMode.Trilinear;
                texture.anisoLevel=4;
            }
            Finished[source]=m;
            return m;
        }

        public static void SaveStudyScene()
        {
            Directory.CreateDirectory(Study+"/Generated");
            var saved=new Dictionary<UnityEngine.Object,string>();
            int index=0;
            Action<UnityEngine.Object> save=null;
            save=obj=>{
                if(obj==null||AssetDatabase.Contains(obj)||saved.ContainsKey(obj))return;
                if(!(obj is Mesh)&&!(obj is Material)&&!(obj is Texture))return;
                string path=Study+"/Generated/"+(index++).ToString("D5")+".asset";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(obj,path); saved[obj]=path;
            };
            foreach(var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach(var filter in go.GetComponentsInChildren<MeshFilter>(true))save(filter.sharedMesh);
                foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)
                {
                    if(material==null)continue;
                    foreach(string property in material.GetTexturePropertyNames())save(material.GetTexture(property));
                    save(material);
                }
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Study+"/ValoriaFortification.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("VALORIA_REPRODUCIBLE_SCENE_SAVED assets="+saved.Count);
        }

        public static void ApplyArchitecturalMaterial(Material m, string piece)
        {
            if (m == null) return;
            var id=(m.name??string.Empty).ToLowerInvariant();
            var metal=id.Contains("iron") || id.Contains("steel") || id.Contains("metal");
            var cloth=id.Contains("cloth") || id.Contains("banner") || id.Contains("fabric");
            var wood=id.Contains("oak") || id.Contains("wood") || id.Contains("timber");
            var stone=!metal&&!cloth&&!wood;
            if (m.HasProperty("_Metallic"))
                m.SetFloat("_Metallic", metal?.65f:0f);
            if (m.HasProperty("_Smoothness"))
                m.SetFloat("_Smoothness", metal?.40f:cloth?.10f:wood?.16f:.11f);
            if (m.HasProperty("_BumpScale"))
                m.SetFloat("_BumpScale",stone?.72f:wood?.9f:1f);
            if (m.HasProperty("_OcclusionStrength"))
                m.SetFloat("_OcclusionStrength",stone?1f:.85f);
            if (m.HasProperty("_BaseColor"))
            {
                var c=m.GetColor("_BaseColor");
                var rock=piece=="cliff_rocks" && id.Contains("rock");
                var masonry=stone && piece!="cliff_rocks";
                var warmth=rock?.66f:masonry?.82f:1f;
                m.SetColor("_BaseColor",new Color(
                    Mathf.Clamp01(c.r*warmth),Mathf.Clamp01(c.g*(rock?.69f:masonry?.79f:.99f)),
                    Mathf.Clamp01(c.b*(rock?.74f:masonry?.74f:stone?.95f:1f)),c.a));
            }
        }

        public static GameObject MakeContextFloor(Bounds b,string piece)
        {
            var isCliff=piece=="cliff_rocks";
            var isPath=piece=="stone_path_platform";
            float width=Mathf.Max(48f,b.size.x*(isPath?6f:7f));
            float depth=Mathf.Max(48f,b.size.z*(isPath?6f:7f));
            var stage=new GameObject("STUDIO_CONTEXT_NOT_SOURCE_"+piece);
            var mf=stage.AddComponent<MeshFilter>();
            var mr=stage.AddComponent<MeshRenderer>();
            int n=192;
            var v=new Vector3[(n+1)*(n+1)];
            var uv=new Vector2[v.Length];
            var triangles=new int[n*n*6];
            float level=b.min.y-.065f;
            bool assembly=piece=="assembled_family";
            if(assembly)level=-.08f;
            int ix=0;
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)
            {
                float u=(float)x/n, w=(float)z/n;
                float px=(u-.5f)*width, pz=(w-.5f)*depth;
                float jitter=Mathf.PerlinNoise(u*7.91f+3.12f,w*6.44f+2.65f)-.5f;
                float erode=Mathf.PerlinNoise(u*16.51f+1.1f,w*17.23f+5.4f)-.5f;
                // Near the asset, keep ground support accurate and avoid intersecting its mesh.
                float slope=(isCliff?.23f:.075f)*(jitter+erode*.24f);
                if(assembly) {
                    float wx=px+b.center.x, wz=pz+b.center.z;
                    // Irregular escarpment follows the foreground cliff assets; soften
                    // the back into the neighboring world instead of a raised rectangle.
                    float edge=-18f + 1.5f*Mathf.Sin(wx*.22f) + .7f*Mathf.Sin(wx*.53f);
                    float front=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((edge-wz)/6.8f));
                    float side=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(wx)-28f)/9f));
                    slope-=4.15f*Mathf.Max(front,side*.65f);
                    slope+=Mathf.PerlinNoise(wx*.14f+12f,wz*.16f+9f)*.18f;
                    // Carry a tapered earthen causeway under the stone approach.
                    // The former hard x/z cutoff left the last platform in mid-air.
                    float roadAcross=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((Mathf.Abs(wx)-3f)/5f));
                    float roadEnd=1f-Mathf.SmoothStep(0f,1f,Mathf.Clamp01((-wz-28f)/7f));
                    if(wz<2f)slope=Mathf.Lerp(slope,0f,roadAcross*roadEnd);
                }
                v[ix]=new Vector3(px,level+slope,pz);
                uv[ix]=new Vector2(u*width*.07f,w*depth*.07f);
                ix++;
            }
            int ti=0;
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                int a=z*(n+1)+x, d=a+n+1;
                triangles[ti++]=a;triangles[ti++]=d;triangles[ti++]=a+1;
                triangles[ti++]=a+1;triangles[ti++]=d;triangles[ti++]=d+1;
            }
            var mesh=new Mesh();
            mesh.name="Valoria editorial staging terrain 32";
            mesh.vertices=v;mesh.uv=uv;
            // Lithology comes from the authored cliff GLBs. Per-cell rock selection
            // made a bright sawtooth stripe at the terrace lip in actual Unity PNGs.
            mesh.triangles=triangles;mr.sharedMaterial=GetStageMaterial();
            mesh.RecalculateNormals();
            mf.sharedMesh=mesh;
            stage.transform.position=new Vector3(b.center.x,0f,b.center.z);
            return stage;
        }

        static Material GetStageMaterial()
        {
            if(stageMaterial!=null)return stageMaterial;
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)throw new Exception("URP Lit shader missing in Valoria lookdev");
            stageMaterial=new Material(shader);
            stageMaterial.name="Valoria editorial stage — earth and broken limestone";
            stageMaterial.SetColor("_BaseColor",Color.white);
            stageMaterial.SetFloat("_Smoothness",.09f);
            stageMaterial.SetFloat("_Metallic",0f);
            stageAlbedo=new Texture2D(256,256,TextureFormat.RGB24,true,false);
            stageAlbedo.wrapMode=TextureWrapMode.Repeat;
            stageAlbedo.filterMode=FilterMode.Trilinear;
            stageAlbedo.anisoLevel=4;
            var pixels=new Color[256*256];
            for(int y=0;y<256;y++)for(int x=0;x<256;x++)
            {
                float a=Mathf.PerlinNoise(x*.035f+27f,y*.035f+34f);
                float b=Mathf.PerlinNoise(x*.14f+18f,y*.14f+76f);
                float c=Mathf.Clamp01(.64f*a+.36f*b);
                var dark=new Color(.22f,.25f,.105f);
                var light=new Color(.46f,.43f,.235f);
                pixels[y*256+x]=Color.Lerp(dark,light,c);
            }
            stageAlbedo.SetPixels(pixels);
            stageAlbedo.Apply(true,false);
            stageMaterial.SetTexture("_BaseMap",stageAlbedo);
            return stageMaterial;
        }

        public static Light MakeBounce(Bounds b)
        {
            var go=new GameObject("Valoria warm bounce for isolated art test");
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;
            l.shadows=LightShadows.None;
            l.range=Mathf.Max(8f,b.extents.magnitude*3f);
            l.intensity=.68f;
            l.color=new Color(1f,.72f,.46f);
            go.transform.position=b.center+new Vector3(-b.extents.x-3f,b.extents.y+1.5f,-b.extents.z-2f);
            return l;
        }

        public static GameObject MakeGateWarmth(Bounds gate)
        {
            var root=new GameObject("Valoria gate warm practical lights — study quality");
            foreach(float x in new[]{gate.min.x+2.2f,gate.max.x-2.2f})
            {
                var go=new GameObject("Brazier bounce");
                go.transform.SetParent(root.transform,false);
                go.transform.position=new Vector3(x,gate.min.y+2.1f,gate.min.z-1.25f);
                var light=go.AddComponent<Light>();
                light.type=LightType.Point;
                light.color=new Color(1f,.49f,.23f);
                light.intensity=2.0f;
                light.range=6.5f;
                light.shadows=LightShadows.None;
            }
            return root;
        }
    }
}
