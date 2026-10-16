using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    // Unity-only, reversible existing-scene finishing test: NO geometry creation,
    // NO asset source mutation, NO gameplay code changes, NO scene publication.
    public static class Bastion1UnityFinishLookReview
    {
        const string Original = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string Candidate = "Assets/Eldoria/ArtTests/Bastion1Assembly/Bastion1UnityFinishLookCandidate.unity";
        const string Materials = "Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedFinishLook";
        const string Evidence = "Bastion1UnityFinishLookReview";

        [MenuItem("Eldoria/Art/Bastion I Unity-only lighting and materials A-B")]
        public static void Capture()
        {
            try
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Original) == null)
                    throw new FileNotFoundException("Existing native playable scene unavailable", Original);
                Directory.CreateDirectory(Evidence);
                var scene = EditorSceneManager.OpenScene(Original, OpenSceneMode.Single);
                var presentation = UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
                if (presentation == null || !presentation.MeshMode ||
                    presentation.ProductionCamera == null ||
                    presentation.Bindings == null || presentation.Bindings.Length < 2 ||
                    presentation.Bindings[0].Ground == null ||
                    presentation.Bindings[0].Construction == null ||
                    presentation.Bindings[0].Built == null ||
                    presentation.Bindings[1].Built == null)
                    throw new Exception("Original playable player-city binding contract failed");

                var camera = presentation.ProductionCamera;
                var pose = camera.transform.position;
                var rotation = camera.transform.rotation;
                var size = camera.orthographicSize;
                var beforeMeshes = MeshCount();
                RenderSet(camera,"before",size);

                var sun = FindSun();
                if(sun == null)throw new Exception("Original directional sun missing");
                // Source: warm 1.25 intensity, sky .37/.46/.61, almost black schist.
                // Target: brighter terrain readability and warm stone, while retaining
                // the reference's evening blue shadow/cool depth contrast.
                sun.intensity = 1.52f;
                sun.color = new Color(1f,.86f,.72f);
                sun.transform.rotation = Quaternion.Euler(43f,-37f,0f);
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = .84f;
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(.49f,.54f,.63f);
                RenderSettings.ambientEquatorColor = new Color(.46f,.46f,.43f);
                RenderSettings.ambientGroundColor = new Color(.38f,.34f,.29f);
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = .0023f;
                RenderSettings.fogColor = new Color(.33f,.40f,.48f);
                int instances, uniqueMaterials;
                RetintOriginalMaterials(out instances,out uniqueMaterials);
                if (instances < 10 || uniqueMaterials < 3)
                    throw new Exception("Original targeted PBR material coverage insufficient");
                if (MeshCount()!=beforeMeshes)
                    throw new Exception("Original geometry set changed during Unity-only lookdev");
                if (camera.transform.position!=pose || camera.transform.rotation!=rotation ||
                    Mathf.Abs(camera.orthographicSize-size)>.0001f)
                    throw new Exception("Camera changed; A/B invalid");
                if (!EditorSceneManager.SaveScene(scene,Candidate,true))
                    throw new Exception("Could not save separate isolated finish-look candidate");
                RenderSet(camera,"after",size);
                File.WriteAllText(Path.Combine(Evidence,"finishlook-result.json"),
                    "{\n  \"schema\": \"eldoria.bastion1.unity-only-finish-v1\",\n" +
                    "  \"source_scene_preserved\": true,\n" +
                    "  \"new_meshes\": 0,\n  \"original_mesh_filters\": " + beforeMeshes + ",\n" +
                    "  \"material_instances_rebound\": " + instances + ",\n" +
                    "  \"new_isolated_material_variants\": " + uniqueMaterials + ",\n" +
                    "  \"player_camera_unchanged\": true,\n" +
                    "  \"gameplay_code_unchanged\": true,\n" +
                    "  \"art_approved\": false,\n" +
                    "  \"mobile_performance_approved\": false\n}\n");
                Debug.Log("BASTION1_UNITY_FINISH_AB_COMPLETE original_scene_preserved=true"+
                          " meshes="+beforeMeshes+" material_bindings="+instances+
                          " variants="+uniqueMaterials+" ART_REVIEW_REQUIRED");
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                Debug.LogError("BASTION1_UNITY_FINISH_AB_FAILED "+e);
                EditorApplication.Exit(1);
            }
        }

        static int MeshCount()
        {
            int count=0;
            foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(
                        FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(f.sharedMesh!=null)count++;
            return count;
        }

        static Light FindSun()
        {
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(
                        FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(l.type==LightType.Directional && l.enabled)return l;
            return null;
        }

        static int GroupIndex(string node)
        {
            const string token="_material_group_";
            int k=node.IndexOf(token,StringComparison.Ordinal);
            if(k<0)return -1;
            string tail=node.Substring(k+token.Length);
            int dot=tail.IndexOf('.');
            if(dot>=0)tail=tail.Substring(0,dot);
            return int.TryParse(tail,out int i)?i:-1;
        }

        static void RetintOriginalMaterials(out int instances,out int unique)
        {
            instances=0;
            var map=new Dictionary<Material,Material>();
            Directory.CreateDirectory(Materials);
            foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(
                        FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var source=renderer.sharedMaterials;
                bool changed=false;
                var target=(Material[])source.Clone();
                for(int i=0;i<source.Length;i++)
                {
                    var material=source[i];
                    if(material==null)continue;
                    float r=1,g=1,b=1;
                    // Saved Unity materials are named 0000.asset, 0001.asset, etc.
                    // Material.name no longer identifies historical GLB families.
                    // Classify by original renderer/mesh node names and its
                    // canonical glTF primitive material ordering instead.
                    string node=renderer.gameObject.name.ToLowerInvariant();
                    int group=GroupIndex(node);
                    if(node.Contains("natural playable hilltop and rock scarps"))
                    {
                        if(i==0){r=1.06f;g=1.13f;b=1.035f;} // terrace grass
                        else if(i==1){r=1.25f;g=1.27f;b=1.31f;} // schist cliffs
                        else if(i==2){r=1.06f;g=1.13f;b=1.035f;} // valley earth
                        else continue;
                    }
                    else if(node.Contains("lower valley continuous terrain"))
                    {r=1.06f;g=1.13f;b=1.035f;}
                    else if(node.StartsWith("wall_straight_material_group_") ||
                            node.StartsWith("wall_connections_material_group_") ||
                            node.StartsWith("wall_corner_material_group_"))
                    {
                        if(group<0 || (group>5 && group!=7))continue;
                        r=1.055f;g=.965f;b=.89f; // dressed limestone, not dark joints
                    }
                    else if(node.StartsWith("watchtower_material_group_"))
                    {
                        if(!((group>=4&&group<=9)||group==11))continue;
                        r=1.055f;g=.965f;b=.89f; // do not retint blue banner or iron
                    }
                    else if(node.StartsWith("hero_gate_v3_material_group_"))
                    {
                        if(group<0||group>6)continue;
                        r=1.055f;g=.965f;b=.89f; // keep fire, banners, wood intact
                    }
                    else continue;
                    if(!material.HasProperty("_BaseColor"))continue;
                    if(!map.TryGetValue(material,out var clone))
                    {
                        clone=new Material(material){name="FINISH_LOOK_"+material.name};
                        var c=material.GetColor("_BaseColor");
                        clone.SetColor("_BaseColor",
                            new Color(c.r*r,c.g*g,c.b*b,c.a));
                        var path=Materials+"/finishlook_"+map.Count.ToString("D3")+".mat";
                        if(AssetDatabase.LoadAssetAtPath<Material>(path)!=null)
                            AssetDatabase.DeleteAsset(path);
                        AssetDatabase.CreateAsset(clone,path);
                        map.Add(material,clone);
                    }
                    target[i]=clone;
                    changed=true;instances++;
                }
                if(changed)renderer.sharedMaterials=target;
            }
            unique=map.Count;
            AssetDatabase.SaveAssets();
        }

        static void RenderSet(Camera camera,string name,float originalSize)
        {
            Bastion1ReferenceCapture.Render(camera,
                Path.GetFullPath(Evidence+"/"+name+"-landscape.png"),1440,1000);
            Bastion1ReferenceCapture.Render(camera,
                Path.GetFullPath(Evidence+"/"+name+"-mobile.png"),780,520);
            camera.orthographicSize=76f;
            try
            {
                Bastion1ReferenceCapture.Render(camera,
                    Path.GetFullPath(Evidence+"/"+name+"-portrait.png"),390,844);
            }
            finally{camera.orthographicSize=originalSize;}
        }
    }
}
