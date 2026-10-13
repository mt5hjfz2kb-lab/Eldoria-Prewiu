using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Eldoria.AssetLibrary;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.AssetLibrary.Editor
{
    // Manual/batch isolated builder; never invoked from gameplay or from Bastion I integration.
    // Creates visual-only prefabs by referencing unchanged, hash-verified historical GLB imports.
    public static class EldoriaSelectedAssetLibraryBuilderV1
    {
        // Staging prefabs live outside Resources and therefore are not auto-packed into player builds.
        const string PrefabFolder = "Assets/Eldoria/AssetLibrarySelectionV1/GeneratedPrefabsV1";
        const string AuditOutput = "SelectedAssetLibraryV1Captures";

        [Serializable] sealed class EntryAudit
        {
            public string id, source_relative, sha256, prefab_path;
            public int renderers, colliders, enabled_behaviours, materials, mesh_filters;
            public long imported_triangles, imported_vertices;
            public int authored_source_triangles;
            public bool mobile_review_required, mobile_profile_pass;
            public string[] imported_shader_names;
            public bool prefab_ok;
        }

        [Serializable] sealed class Audit
        {
            public string verdict = "UNITY_IMPORT_PENDING";
            public string source = "26 legacy SHA-verified GLB inventory";
            public bool bastion_scene_changed = false;
            public bool gameplay_modified = false;
            public bool physical_mobile_pass = false;
            public EntryAudit[] entries;
        }

        [MenuItem("Eldoria/Art Library/Build Selected Prefabs V1 (isolated)")]
        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var catalog = EldoriaSelectedAssetLibraryV1.LoadCatalog();
            Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath,"..",PrefabFolder)));
            Directory.CreateDirectory(AuditOutput);
            var reports = new List<EntryAudit>();
            foreach (var e in catalog.entries.Where(x => x.tier == "SELECTED" && x.origin == "legacy_canonical"))
            {
                var input = e.source_relative;
                if (!input.StartsWith("Unity/Assets/Eldoria/Resources/Valoria/", StringComparison.Ordinal))
                    throw new InvalidOperationException("Non-canonical source rejected: " + input);

                string unityAssetPath = input.Substring("Unity/".Length);
                string actualDisk = Path.GetFullPath(Path.Combine(Application.dataPath, "..", unityAssetPath));
                if (!File.Exists(actualDisk) || !string.Equals(Hash(actualDisk), e.sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Source SHA mismatch or GLB missing: " + e.id);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(unityAssetPath);
                if (model == null) throw new InvalidOperationException("glTF imported GameObject missing: " + e.id);
                if (model.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                    throw new InvalidOperationException("Source has gameplay scripts and cannot enter a visual-only prefab: " + e.id);
                if(e.source_triangles <= 0 || e.mobile_performance_verified)
                    throw new InvalidOperationException("Incorrect source cost or invented mobile approval: " + e.id);

                var wrapper = new GameObject("Eldoria selected visual · " + e.id);
                try
                {
                    Object instantiated = Object.Instantiate((Object)model);
                    var child = instantiated as GameObject;
                    if (child == null && instantiated is Component c) child = c.gameObject;
                    if (child == null) throw new InvalidOperationException("Model instantiate type unsupported: " + e.id);
                    child.name = e.id + " · unchanged audited model";
                    child.transform.SetParent(wrapper.transform, false);
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localRotation = Quaternion.identity;

                    foreach (var component in child.GetComponentsInChildren<Collider>(true))
                        Object.DestroyImmediate(component);
                    foreach (var behaviour in child.GetComponentsInChildren<MonoBehaviour>(true))
                        Object.DestroyImmediate(behaviour);

                    var renderers = wrapper.GetComponentsInChildren<Renderer>(true);
                    if (renderers.Length == 0 || renderers.Any(r => r.sharedMaterials.Any(m => m == null || m.shader == null)))
                        throw new InvalidOperationException("No renders or invalid material: " + e.id);
                    // Some legacy Standard shaders silently render magenta in URP.
                    // Preserve authored PBR; reject unsupported imports instead of repainting.
                    var shaders=renderers.SelectMany(x=>x.sharedMaterials).Select(x=>x.shader.name)
                        .Distinct().OrderBy(x=>x,StringComparer.Ordinal).ToArray();
                    foreach(var shaderName in shaders)
                    {
                        if(!shaderName.StartsWith("Universal Render Pipeline/",StringComparison.Ordinal)
                            && !shaderName.StartsWith("Shader Graphs/",StringComparison.Ordinal)
                            && !shaderName.StartsWith("glTF/",StringComparison.Ordinal))
                            throw new InvalidOperationException("Unverified URP material shader for " + e.id + ": " + shaderName);
                    }
                    long triangles = 0, vertices = 0;
                    foreach(var mf in wrapper.GetComponentsInChildren<MeshFilter>(true))
                    {
                        var mesh = mf.sharedMesh;
                        if(mesh==null)continue;
                        vertices += mesh.vertexCount;
                        for(int sub=0;sub<mesh.subMeshCount;sub++)
                        {
                            if(mesh.GetTopology(sub)!=MeshTopology.Triangles)
                                throw new InvalidOperationException("Unexpected non-triangle mesh: " + e.id);
                            triangles += (long)mesh.GetIndexCount(sub)/3;
                        }
                    }
                    if(triangles < 1 || Math.Abs(triangles - e.source_triangles) > Math.Max(250L, e.source_triangles / 100))
                        throw new InvalidOperationException("Unity imported mesh topology differs from Blender reference: " + e.id + " Unity=" + triangles + " source=" + e.source_triangles);

                    var path = PrefabFolder + "/" + e.id + ".prefab";
                    bool success;
                    PrefabUtility.SaveAsPrefabAsset(wrapper, path, out success);
                    if (!success) throw new InvalidOperationException("Prefab generation failed: " + e.id);
                    var saved = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (saved == null) throw new InvalidOperationException("Prefab import validation failed: " + e.id);
                    reports.Add(new EntryAudit {
                        id=e.id, source_relative=input, sha256=e.sha256, prefab_path=path,
                        renderers=renderers.Length,
                        colliders=wrapper.GetComponentsInChildren<Collider>(true).Length,
                        enabled_behaviours=wrapper.GetComponentsInChildren<MonoBehaviour>(true).Count(b=>b.enabled),
                        materials=renderers.Sum(r=>r.sharedMaterials.Length),
                        mesh_filters=wrapper.GetComponentsInChildren<MeshFilter>(true).Length,
                        imported_triangles=triangles, imported_vertices=vertices,
                        authored_source_triangles=e.source_triangles,
                        mobile_review_required=e.asset_budget_review_required,
                        mobile_profile_pass=false,
                        imported_shader_names=shaders,
                        prefab_ok=true
                    });
                }
                finally { Object.DestroyImmediate(wrapper); }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (reports.Count != 10 || reports.Any(x=>x.colliders>0 || x.enabled_behaviours>0 || x.renderers==0))
                throw new InvalidOperationException("Selected library does not meet visual-only contract.");

            var report = new Audit { verdict="SHA_VERIFIED_PREFABS_BUILT_UNITY_VISUAL_REVIEW_PENDING", entries=reports.ToArray() };
            File.WriteAllText(Path.Combine(AuditOutput, "import-evidence.json"), JsonUtility.ToJson(report,true));
            Debug.Log("Selected asset library: built " + reports.Count + " SHA-pinned prefabs outside canonical scenes.");
        }

        [MenuItem("Eldoria/Art Library/Capture Selected Prefab Gallery (isolated)")]
        public static void Capture()
        {
            Build();
            // Deliberately make an unsaved scratch scene to avoid touching any playable Scene.
            // Do not call SceneSetup.SetupRenderPipeline: it can write shared renderer assets.
            // This isolated test consumes the already configured project pipeline only.
            if (GraphicsSettings.defaultRenderPipeline == null && QualitySettings.renderPipeline == null)
                throw new InvalidOperationException("No configured URP; refusing to modify canonical render settings.");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var catalog=EldoriaSelectedAssetLibraryV1.LoadCatalog();
            int i=0;
            foreach(var e in catalog.entries.Where(x => x.tier == "SELECTED" && x.origin == "legacy_canonical"))
            {
                var prefabPath=PrefabFolder+"/"+e.id+".prefab";
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if(prefab==null)throw new InvalidOperationException("Staged prefab not found: "+prefabPath);
                var go=Object.Instantiate(prefab);
                go.name="Selected prefab gallery · "+e.id;
                var rs=go.GetComponentsInChildren<Renderer>(true);
                if(rs.Length==0)throw new InvalidOperationException("Prefab appears empty: "+e.id);
                var b=rs[0].bounds;
                foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
                var scale=4.2f/Mathf.Max(0.001f,b.size.x,b.size.y,b.size.z);
                go.transform.localScale*=scale;
                rs=go.GetComponentsInChildren<Renderer>(true);
                b=rs[0].bounds;
                foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
                var target=new Vector3((i%5-2)*6f,0,(i/5)*7f);
                go.transform.position+=target-new Vector3(b.center.x,b.min.y,b.center.z);
                i++;
            }
            if(i!=10)throw new InvalidOperationException("Gallery must contain exactly 10 selected prefabs");
            var light = new GameObject("Independent audit sunlight").AddComponent<Light>();
            light.type=LightType.Directional;
            light.color=new Color(1f,.87f,.74f);
            light.intensity=1.4f;
            light.shadows=LightShadows.Soft;
            light.transform.rotation=Quaternion.Euler(45f,-35f,0);
            RenderSettings.ambientLight=new Color(.52f,.55f,.61f);
            var camera = new GameObject("Isolated comparison camera").AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.35f,.40f,.46f,1f);
            camera.orthographic=true;
            camera.orthographicSize=12.8f;
            camera.transform.position=new Vector3(21,20,-30);
            camera.transform.LookAt(new Vector3(0,1,3.5f));
            var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32){antiAliasing=2};
            var old=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;
                camera.Render();camera.Render();
                RenderTexture.active=rt;
                var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,1280,720),0,0);
                tex.Apply();
                Directory.CreateDirectory(AuditOutput);
                File.WriteAllBytes(Path.Combine(AuditOutput, "selected-prefabs-unity.png"),tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
            }
            finally
            {
                RenderTexture.active=old;
                camera.targetTexture=null;
                rt.Release();
                Object.DestroyImmediate(rt);
            }
            Debug.Log("Selected asset library Unity 10-piece gallery captured; not an artistic/mobile PASS.");
        }

        static string Hash(string path)
        {
            using(var sha=SHA256.Create())
            using(var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
    }
}
