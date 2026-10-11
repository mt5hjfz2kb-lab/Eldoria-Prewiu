using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.ArtTests.ImageTo3D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Eldoria.EditorTools
{
    public static class TripoTextureImportDebug
    {
        const string Root = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Source = Root + "Source/Eldoria_Module_TowerWallRock_50K_SurfaceCleanupV4.glb";
        const string ExternalDir = Root + "TextureImportDebug/External/";
        const string MaterialDir = Root + "TextureImportDebug/Materials/";
        const string Output = "TripoTextureImportDebugCaptures";
        const string ScenePath = Root + "TripoTextureImportDebug.unity";

        [Serializable] class TextureEntry {
            public string name, assetPath;
            public int width, height, maxTextureSize, compression, npotScale;
            public bool mipmapEnabled, sRGBTexture;
        }
        [Serializable] class MaterialTextureEntry {
            public string material, shader, property, textureName;
            public int width, height;
        }
        [Serializable] class Report {
            public string source = Source;
            public int qualityLevel, globalTextureMipmapLimit;
            public string qualityName;
            public long triangles;
            public int meshes, materials, embeddedTextures, colliders;
            public bool positiveRaycast, emptySpaceDoesNotSelect;
            public TextureEntry[] externalTextures;
            public MaterialTextureEntry[] embeddedBindings;
            public string[] materialNames;
            public string[] cloneBindingNotes;
            public string[] unityMaterialBindingNotes;
            public long[] cloneCaptureMs, unityMaterialCaptureMs;
        }

        public static void Run()
        {
            Directory.CreateDirectory(Output);
            if (!File.Exists(Source)) throw new FileNotFoundException("V4 GLB missing: " + Source);
            foreach (var p in new [] {
                ExternalDir + "eldoria_stone_basecolor.png",
                ExternalDir + "eldoria_roof_basecolor.png",
                ExternalDir + "eldoria_rock_basecolor.png"
            }) if (!File.Exists(p)) throw new FileNotFoundException("Extracted external PNG missing: " + p);

            SceneSetup.SetupRenderPipeline();
            foreach (var p in new [] {
                ExternalDir + "eldoria_stone_basecolor.png",
                ExternalDir + "eldoria_roof_basecolor.png",
                ExternalDir + "eldoria_rock_basecolor.png"
            }) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(Source, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var report = new Report {
                qualityLevel = QualitySettings.GetQualityLevel(),
                qualityName = QualitySettings.names[QualitySettings.GetQualityLevel()],
                globalTextureMipmapLimit = QualitySettings.globalTextureMipmapLimit,
                externalTextures = MeasureExternalTextures()
            };

            foreach (var e in report.externalTextures)
                if (e.width != 1024 || e.height != 1024)
                    throw new Exception($"Direct PNG import failed for {e.name}: {e.width}x{e.height}");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (prefab == null) throw new Exception("glTFast failed to import V4.");
            var probeModule = UnityEngine.Object.Instantiate(prefab);
            probeModule.name = "V4 embedded import probe";
            Normalize(probeModule);
            report.embeddedBindings = MeasureEmbeddedBindings(probeModule, out var importedMaterials);
            report.materialNames = importedMaterials.ConvertAll(m => m.name).ToArray();
            report.materials = importedMaterials.Count;
            report.embeddedTextures = CountUniqueTextures(probeModule);
            MeasureGeometry(probeModule, report);
            UnityEngine.Object.DestroyImmediate(probeModule);

            // Route A: keep glTFast material/shader, replace only the BaseColor texture with the exact external PNG.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var clone = UnityEngine.Object.Instantiate(prefab);
            clone.name = "V4 glTFast materials + external PNGs";
            Normalize(clone);
            AddColliders(clone);
            report.cloneBindingNotes = BindExternalTexturesToClonedImportedMaterials(clone);
            var cloneCamera = BuildReviewContext(clone, "Clone-imported-material route");
            report.cloneCaptureMs = CaptureSet(cloneCamera, "clone");
            ValidateRaycast(cloneCamera, clone, report);

            // Route B: same GLB geometry/submesh order, but persistent Unity URP materials + external PNG assets.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var unityRoute = UnityEngine.Object.Instantiate(prefab);
            unityRoute.name = "V4 external Unity material route";
            Normalize(unityRoute);
            AddColliders(unityRoute);
            report.unityMaterialBindingNotes = BindPersistentUrpMaterials(unityRoute);
            var unityCamera = BuildReviewContext(unityRoute, "Persistent Unity material route");
            report.unityMaterialCaptureMs = CaptureSet(unityCamera, "unity_material");
            ValidateRaycast(unityCamera, unityRoute, report);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);

            File.WriteAllText(Output + "/texture-import-debug.json", JsonUtility.ToJson(report, true));
            Debug.Log($"Texture debug complete. External PNGs={report.externalTextures[0].width}x{report.externalTextures[0].height}; embedded bindings={report.embeddedBindings.Length}; quality={report.qualityName}; limit={report.globalTextureMipmapLimit}");
        }

        static TextureEntry[] MeasureExternalTextures()
        {
            var list = new List<TextureEntry>();
            foreach (var p in new [] {
                ExternalDir + "eldoria_stone_basecolor.png",
                ExternalDir + "eldoria_roof_basecolor.png",
                ExternalDir + "eldoria_rock_basecolor.png"
            }) {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                if (tex == null || imp == null) throw new Exception("Direct PNG TextureImporter missing: " + p);
                list.Add(new TextureEntry {
                    name = tex.name, assetPath = p, width = tex.width, height = tex.height,
                    maxTextureSize = imp.maxTextureSize, compression = (int)imp.textureCompression,
                    npotScale = (int)imp.npotScale, mipmapEnabled = imp.mipmapEnabled, sRGBTexture = imp.sRGBTexture
                });
            }
            return list.ToArray();
        }

        static MaterialTextureEntry[] MeasureEmbeddedBindings(GameObject module, out List<Material> uniqueMaterials)
        {
            var seen = new HashSet<Material>(); uniqueMaterials = new List<Material>();
            var rows = new List<MaterialTextureEntry>();
            foreach (var r in module.GetComponentsInChildren<Renderer>()) foreach (var m in r.sharedMaterials) {
                if (m == null || !seen.Add(m)) continue;
                uniqueMaterials.Add(m);
                foreach (var prop in m.GetTexturePropertyNames()) {
                    var t = m.GetTexture(prop);
                    if (t == null) continue;
                    rows.Add(new MaterialTextureEntry {
                        material = m.name, shader = m.shader != null ? m.shader.name : "<null>",
                        property = prop, textureName = t.name, width = t.width, height = t.height
                    });
                }
            }
            return rows.ToArray();
        }

        static string[] BindExternalTexturesToClonedImportedMaterials(GameObject module)
        {
            var notes = new List<string>();
            foreach (var r in module.GetComponentsInChildren<Renderer>()) {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) {
                    if (mats[i] == null) continue;
                    var original = mats[i];
                    var clone = new Material(original) { name = original.name + "_ExternalPNG_Debug" };
                    var tex = ExternalForMaterial(original.name);
                    bool assigned = false;
                    foreach (var prop in clone.GetTexturePropertyNames()) {
                        var old = clone.GetTexture(prop);
                        if (old != null && old.name.ToLowerInvariant().Contains("basecolor")) {
                            clone.SetTexture(prop, tex); assigned = true;
                            notes.Add($"{clone.name}: {prop} <- {tex.name} {tex.width}x{tex.height}");
                        }
                    }
                    foreach (var prop in new [] {"_BaseMap","_BaseColorTexture","_MainTex"}) {
                        if (!assigned && clone.HasProperty(prop)) {
                            clone.SetTexture(prop, tex); assigned = true;
                            notes.Add($"{clone.name}: {prop} <- {tex.name} {tex.width}x{tex.height}");
                        }
                    }
                    if (!assigned) throw new Exception("No BaseColor-compatible texture property found on " + original.name + " shader=" + original.shader.name);
                    mats[i] = clone;
                }
                r.sharedMaterials = mats;
            }
            return notes.ToArray();
        }

        static string[] BindPersistentUrpMaterials(GameObject module)
        {
            Directory.CreateDirectory(MaterialDir);
            var notes = new List<string>();
            foreach (var r in module.GetComponentsInChildren<Renderer>()) {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) {
                    var sourceName = mats[i] != null ? mats[i].name : $"slot_{i}";
                    var tex = ExternalForMaterial(sourceName);
                    var baseName = CanonicalMaterialName(sourceName);
                    var path = MaterialDir + baseName + ".mat";
                    var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (existing != null) AssetDatabase.DeleteAsset(path);
                    var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = baseName };
                    m.SetTexture("_BaseMap", tex);
                    m.SetColor("_BaseColor", Color.white);
                    m.SetFloat("_Metallic", 0f);
                    m.SetFloat("_Smoothness", SmoothnessFor(sourceName));
                    AssetDatabase.CreateAsset(m, path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    var persisted = AssetDatabase.LoadAssetAtPath<Material>(path);
                    mats[i] = persisted;
                    notes.Add($"{sourceName} -> {path} -> {tex.name} {tex.width}x{tex.height}");
                }
                r.sharedMaterials = mats;
            }
            AssetDatabase.SaveAssets();
            return notes.ToArray();
        }

        static string CanonicalMaterialName(string name) {
            var n = name.ToLowerInvariant();
            if (n.Contains("roof") || n.Contains("slate")) return "Eldoria_SlateRoof_External";
            if (n.Contains("rock")) return "Eldoria_RockBase_External";
            return "Eldoria_Stone_External";
        }
        static float SmoothnessFor(string name) {
            var n = name.ToLowerInvariant();
            if (n.Contains("roof") || n.Contains("slate")) return .32f;
            if (n.Contains("rock")) return .12f;
            return .22f;
        }
        static Texture2D ExternalForMaterial(string materialName) {
            var n = materialName.ToLowerInvariant();
            string p = n.Contains("roof") || n.Contains("slate")
                ? ExternalDir + "eldoria_roof_basecolor.png"
                : n.Contains("rock")
                    ? ExternalDir + "eldoria_rock_basecolor.png"
                    : ExternalDir + "eldoria_stone_basecolor.png";
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (t == null) throw new Exception("External texture missing: " + p);
            return t;
        }

        static int CountUniqueTextures(GameObject module) {
            var set = new HashSet<Texture>();
            foreach (var r in module.GetComponentsInChildren<Renderer>()) foreach (var m in r.sharedMaterials) {
                if (m == null) continue; foreach (var p in m.GetTexturePropertyNames()) { var t=m.GetTexture(p); if(t!=null) set.Add(t); }
            }
            return set.Count;
        }
        static void MeasureGeometry(GameObject module, Report report) {
            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) {
                if (f.sharedMesh == null) continue; report.meshes++;
                for (int s=0;s<f.sharedMesh.subMeshCount;s++) report.triangles += (long)f.sharedMesh.GetIndexCount(s)/3;
            }
            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) if (f.sharedMesh != null) {
                f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh; report.colliders++;
            }
            if (report.triangles != 50000) throw new Exception("Certified V4 geometry changed: " + report.triangles);
        }
        static void Normalize(GameObject module) {
            var b=BoundsOf(module); var span=Mathf.Max(b.size.x,b.size.z); var scale=14f/span; module.transform.localScale*=scale;
            b=BoundsOf(module); module.transform.position += new Vector3(-b.center.x,-b.min.y,-b.center.z);
        }
        static void AddColliders(GameObject module) {
            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) if (f.sharedMesh != null && f.GetComponent<Collider>() == null) {
                var c=f.gameObject.AddComponent<MeshCollider>(); c.sharedMesh=f.sharedMesh;
            }
        }
        static Camera BuildReviewContext(GameObject module, string label) {
            module.name = label;
            var terrain=GameObject.CreatePrimitive(PrimitiveType.Plane); terrain.name="Review terrain only"; terrain.transform.localScale=new Vector3(4,1,4); terrain.transform.position=new Vector3(0,-.08f,0); ApplyColor(terrain,new Color(.38f,.36f,.29f));
            var road=GameObject.CreatePrimitive(PrimitiveType.Cube); road.name="Review road only"; road.transform.position=new Vector3(0,.015f,-10); road.transform.localScale=new Vector3(3.2f,.08f,12); ApplyColor(road,new Color(.28f,.27f,.24f));
            var sun=new GameObject("Review light").AddComponent<Light>(); sun.type=LightType.Directional; sun.intensity=1.35f; sun.transform.rotation=Quaternion.Euler(42,-38,0); RenderSettings.ambientLight=new Color(.58f,.61f,.64f);
            var cam=new GameObject("Official zoom review camera").AddComponent<Camera>(); cam.tag="MainCamera"; cam.orthographic=true; cam.orthographicSize=12; cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.64f,.72f,.8f); cam.transform.position=new Vector3(16,13,-22); cam.transform.LookAt(new Vector3(0,6,0));
            var probe=cam.gameObject.AddComponent<BastionSelectionProbe>(); probe.ReviewCamera=cam; probe.BastionRoot=module.transform;
            return cam;
        }
        static long[] CaptureSet(Camera camera,string prefix) {
            var values=new long[4]; var sizes=new[]{19f,12f,9f}; var names=new[]{"strategic","city","detail"};
            for(int i=0;i<3;i++){camera.orthographicSize=sizes[i]; values[i]=Save(camera,$"{Output}/{prefix}_{names[i]}.png");}
            camera.orthographicSize=12; camera.transform.position=new Vector3(-19,16,-22); camera.transform.LookAt(new Vector3(0,6,0)); values[3]=Save(camera,$"{Output}/{prefix}_oblique.png");
            return values;
        }
        static void ValidateRaycast(Camera camera,GameObject module,Report report) {
            Physics.SyncTransforms(); var bounds=BoundsOf(module); var ray=new Ray(camera.transform.position,(bounds.center-camera.transform.position).normalized);
            report.positiveRaycast=Physics.Raycast(ray,out var hit,250)&&hit.transform.IsChildOf(module.transform);
            var empty=camera.ScreenPointToRay(new Vector3(4,716,0)); report.emptySpaceDoesNotSelect=!Physics.Raycast(empty,out var miss,250)||!miss.transform.IsChildOf(module.transform);
            if(!report.positiveRaycast||!report.emptySpaceDoesNotSelect) throw new Exception("Texture debug raycast failed.");
        }
        static void ApplyColor(GameObject go,Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;go.GetComponent<Renderer>().sharedMaterial=m;}
        static Bounds BoundsOf(GameObject go){var rs=go.GetComponentsInChildren<Renderer>();if(rs.Length==0)throw new Exception("No renderer");var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
        static long Save(Camera camera,string path){var sw=System.Diagnostics.Stopwatch.StartNew();var rt=new RenderTexture(1280,720,24);var prev=RenderTexture.active;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var img=new Texture2D(1280,720,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1280,720),0,0);img.Apply();File.WriteAllBytes(path,img.EncodeToPNG());UnityEngine.Object.DestroyImmediate(img);}finally{camera.targetTexture=null;RenderTexture.active=prev;rt.Release();UnityEngine.Object.DestroyImmediate(rt);}sw.Stop();return sw.ElapsedMilliseconds;}
    }
}
