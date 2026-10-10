using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Experimental, isolated mesh-only proof. Never used by the canonical runtime or publication.
    // Excludes SHARP PLY, SHARP splats, SHARP-generated textures and reference projection.
    public static class ValoriaMeshOnlyBastionOneProof
    {
        const string OutputScene = "Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyBastionOne.unity";
        readonly struct Family
        {
            public readonly string Name, Path;
            public readonly Vector2 Uv;
            public readonly float Width, Height, Depth;
            public Family(string name, string path, float x, float y, float w, float h, float depth)
            { Name=name; Path=path; Uv=new Vector2(x,y); Width=w; Height=h; Depth=depth; }
        }
        static readonly Family[] Families = {
            new Family("Bridge","Assets/Eldoria/ProductionSlice/BridgeFamilyV1.glb", .288618f,.100592f,.28f,.22f,36f),
            new Family("LowerGate","Assets/Eldoria/ProductionSlice/LowerGateFamilyV1.glb", .394309f,.319527f,.21f,.225f,42f),
            new Family("MainRoad","Assets/Eldoria/ProductionSlice/RoadFamilyV1.glb", .467480f,.508876f,.14f,.25f,48f),
            new Family("CentralStair","Assets/Eldoria/ProductionSlice/StairFamilyV1.glb", .534959f,.673373f,.126f,.12f,54f),
            new Family("UpperWalls","Assets/Eldoria/ProductionSlice/WallFamilyV1.glb", .788618f,.801183f,.51f,.21f,60f),
            new Family("Bastion","Assets/Eldoria/ProductionSlice/BastionFamilyV1.glb", .604878f,.842604f,.24f,.21f,61f),
            new Family("TerrainCliffSupport","Assets/Eldoria/ProductionSlice/RockTerrainFamilyV1.glb", .72f,.36f,.90f,.60f,65f)
        };
        [MenuItem("Eldoria/Experimental/Create Bastion I Mesh-Only Proof")]
        public static void Create()
        {
            // Preflight before touching any scene, with explicit independent asset availability.
            foreach(var f in Families)
                if(AssetDatabase.LoadAssetAtPath<GameObject>(f.Path)==null)
                    throw new FileNotFoundException("Mesh-only proof missing independently licensed GLB: "+f.Path);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraGo=new GameObject("MeshOnlyProofCamera");
            var camera=cameraGo.AddComponent<Camera>();
            camera.tag="MainCamera"; camera.fieldOfView=44.42281f; camera.nearClipPlane=.1f; camera.farClipPlane=250f;
            camera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.075f,.105f,.125f,1f);
            var root=new GameObject("UNLICENSED_SOURCE_FREE__MESH_ONLY_GEOMETRY");
            float tan=Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad), aspect=1230f/845f;
            var results=new List<string>();
            foreach(var f in Families)
            {
                var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(f.Path));
                go.transform.SetParent(root.transform,false);go.name=f.Name+"_GLB";
                // Transform a GLB authored for the original approved oblique camera into the
                // isolated identity-camera layout. This is a composition proof, not a final world rig.
                var originalPosition=new Vector3(30.8183f,63.0934f,-84.6726f);
                var rotation=Quaternion.LookRotation((-originalPosition).normalized,Vector3.up);
                go.transform.rotation=Quaternion.Inverse(rotation);
                var renderers=go.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length==0)throw new InvalidOperationException("Mesh-only proof GLB has no renderers: "+f.Name);
                var bounds=renderers[0].bounds;
                for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                float sx=(f.Width*2f*f.Depth*tan*aspect)/Mathf.Max(.001f,bounds.size.x);
                float sy=(f.Height*2f*f.Depth*tan)/Mathf.Max(.001f,bounds.size.y);
                float scale=Mathf.Min(sx,sy);
                if(float.IsNaN(scale)||float.IsInfinity(scale)||scale<=0)throw new InvalidOperationException("Invalid scale "+f.Name);
                go.transform.localScale*=scale;
                bounds=renderers[0].bounds;
                for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                var target=new Vector3((f.Uv.x-.5f)*2f*f.Depth*tan*aspect,(f.Uv.y-.5f)*2f*f.Depth*tan,f.Depth);
                go.transform.position+=target-bounds.center;
                foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
                results.Add(f.Name+" renderers="+renderers.Length);
            }
            var lightGo=new GameObject("CoolAmbientDirectional");
            var light=lightGo.AddComponent<Light>();
            light.type=LightType.Directional;light.color=new Color(.88f,.91f,1f);light.intensity=1.25f;
            lightGo.transform.rotation=Quaternion.Euler(35f,-28f,0f);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.31f,.34f,.39f);
            Directory.CreateDirectory(Path.GetDirectoryName(OutputScene));
            EditorSceneManager.SaveScene(scene,OutputScene);
            Debug.Log("[VALORIA MESH ONLY] Authored isolated geometry proof; no SHARP assets loaded. "+string.Join("; ",results));
        }
    }
}
