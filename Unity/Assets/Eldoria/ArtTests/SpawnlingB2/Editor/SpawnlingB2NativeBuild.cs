#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLTFast;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    /// <summary>
    /// Imports the two actual Blender GLBs into PERSISTENT Unity mesh/material/
    /// texture assets. Saves a gameplay-loadable Resources prefab and performs
    /// a same-camera before/after on the genuine integrated I-II world.
    /// Does NOT save/edit the protected canonical scene.
    /// </summary>
    public static class SpawnlingB2NativeBuild
    {
        const string Origin = "../ArtSource/World4X/Spawnling_v1/Finished/";
        const string SourceNear = Origin + "Spawnling_B2_Near.glb";
        const string SourceFar = Origin + "Spawnling_B2_Far.glb";
        const string ResourceFolder = "Assets/Eldoria/Resources/SpawnlingB2";
        const string PrefabPath = ResourceFolder + "/SpawnlingB2.prefab";
        const string IntegratedScene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string Evidence = "../docs/evidence/spawnling-bastion2";
        static string generated;
        static int serial;
        static readonly Dictionary<Mesh, Mesh> meshCache = new Dictionary<Mesh, Mesh>();
        static readonly Dictionary<Material, Material> materialCache = new Dictionary<Material, Material>();
        static readonly Dictionary<string, Material> sharedMaterialByName = new Dictionary<string, Material>();
        static readonly Dictionary<Texture, Texture2D> textureCache = new Dictionary<Texture, Texture2D>();

        [Serializable] sealed class Audit
        {
            public string source_sha, scene, near_glb_sha256, far_glb_sha256;
            public bool canonical_scene_saved, immutable_world_hotspot, no_new_colliders,
                world_navigation_preserved, terrain_and_city_untouched, prefab_saved,
                original_pbr_textures_persisted, art_approved, physical_mobile_tested;
            public int near_meshes, far_meshes, near_triangles, far_triangles,
                near_renderers, far_renderers, materials, textures, original_hotspot_count;
            public string[] captures, limitations;
        }
        static void Need(bool valid, string reason)
        {
            if (!valid) throw new InvalidOperationException("SPAWNLING_B2_NATIVE: " + reason);
        }
        static string Hash(string p)
        {
            using (var s = System.Security.Cryptography.SHA256.Create())
            using (var f = File.OpenRead(p))
                return BitConverter.ToString(s.ComputeHash(f)).Replace("-", "").ToLowerInvariant();
        }
        static Bounds BoundsOf(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            Need(renderers.Length > 0, "No real Tripo mesh under " + root.name);
            var bounds = renderers[0].bounds;
            foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
            return bounds;
        }
        static Texture2D ReadableTexture(Texture source)
        {
            bool linear = !UnityEngine.Experimental.Rendering.GraphicsFormatUtility.IsSRGBFormat(source.graphicsFormat);
            int width = source.width, height = source.height;
            Need(width >= 16 && height >= 16 && width <= 2048 && height <= 2048, "Unexpected Tripo PBR texture dimensions");
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
                linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            var earlier = RenderTexture.active;
            try
            {
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;
                var result = new Texture2D(width, height, TextureFormat.RGBA32, true, linear)
                {
                    name = "Spawnling B2 / " + source.name,
                    filterMode = FilterMode.Trilinear,
                    wrapMode = source.wrapMode,
                    anisoLevel = 2
                };
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply(true, false);
                return result;
            }
            finally
            {
                RenderTexture.active = earlier;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
        static void Persist(GameObject root)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                Need(filter.sharedMesh, "Imported GLB MeshFilter is empty");
                if (!meshCache.TryGetValue(filter.sharedMesh, out var saved))
                {
                    saved = Object.Instantiate(filter.sharedMesh);
                    saved.name = "Spawnling B2 / " + filter.sharedMesh.name;
                    AssetDatabase.CreateAsset(saved, generated + "/mesh-" + serial++ + ".asset");
                    meshCache.Add(filter.sharedMesh, saved);
                }
                filter.sharedMesh = saved;
            }
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, 0);
                renderer.lightmapIndex = -1;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                var sourceMaterials = renderer.sharedMaterials;
                for (int index = 0; index < sourceMaterials.Length; index++)
                {
                    var original = sourceMaterials[index];
                    Need(original && original.shader &&
                        !original.shader.name.Contains("InternalErrorShader"),
                        "Pink / missing Tripo material");
                    if (!materialCache.TryGetValue(original, out var saved))
                    {
                        if (!sharedMaterialByName.TryGetValue(original.name, out saved))
                        {
                            saved = new Material(original) { name = "Spawnling B2 / " + original.name };
                            foreach (var property in original.GetTexturePropertyNames())
                            {
                                var texture = original.GetTexture(property);
                                if (!texture) continue;
                                if (!textureCache.TryGetValue(texture, out var copy))
                                {
                                    copy = ReadableTexture(texture);
                                    AssetDatabase.CreateAsset(copy, generated + "/texture-" + serial++ + ".asset");
                                    textureCache.Add(texture, copy);
                                }
                                saved.SetTexture(property, copy);
                            }
                            AssetDatabase.CreateAsset(saved, generated + "/material-" + serial++ + ".mat");
                            sharedMaterialByName.Add(original.name, saved);
                        }
                        materialCache.Add(original, saved);
                    }
                    sourceMaterials[index] = saved;
                }
                renderer.sharedMaterials = sourceMaterials;
            }
        }
        static async System.Threading.Tasks.Task<GameObject> Import(string file, string name, Transform parent)
        {
            var node = new GameObject(name);
            node.transform.SetParent(parent, false);
            using (var importer = new GltfImport(null, new UninterruptedDeferAgent()))
            {
                Need(await importer.Load(new Uri(Path.GetFullPath(file)).AbsoluteUri,
                    new ImportSettings { GenerateMipMaps = true, AnisotropicFilterLevel = 2 }),
                    "Unable to import existing authored GLB: " + file);
                Need(await importer.InstantiateMainSceneAsync(node.transform),
                    "Unable to instantiate original Tripo mesh");
                Persist(node);
            }
            Need(BoundsOf(node).size.y > .1f, "Tiny or invalid imported mesh");
            return node;
        }
        static void Capture(Camera camera, string output, int width, int height)
        {
            Directory.CreateDirectory(Evidence);
            // Native pixel bytes from the original Unity player camera / pipeline.
            ValoriaReferenceCleanBuilder.Capture(camera, Path.Combine(Evidence, output), width, height);
            Need(new FileInfo(Path.Combine(Evidence, output)).Length > 10000, "Blank Unity capture " + output);
        }
        static void CaptureAt(Camera camera, Vector3 target, float zoom, string label, int width, int height)
        {
            camera.transform.position = target - camera.transform.forward * 110f;
            camera.orthographicSize = zoom;
            Capture(camera, label, width, height);
        }
        public static async void Build()
        {
            int exit = 1;
            GameObject working = null;
            try
            {
                var near = Path.GetFullPath(SourceNear);
                var far = Path.GetFullPath(SourceFar);
                Need(File.Exists(near) && File.Exists(far), "Vetted Blender near/far source missing");
                Directory.CreateDirectory(ResourceFolder);
                Directory.CreateDirectory(Evidence);
                generated = ResourceFolder + "/Generated-" +
                    (Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? "local");
                Need(!Directory.Exists(generated), "Refuse to replace an earlier mobile prefab artifact");
                Directory.CreateDirectory(generated);
                AssetDatabase.Refresh();
                serial = 0;
                meshCache.Clear(); materialCache.Clear(); sharedMaterialByName.Clear(); textureCache.Clear();
                var audit = new Audit
                {
                    source_sha = Environment.GetEnvironmentVariable("GITHUB_SHA"),
                    near_glb_sha256 = Hash(near), far_glb_sha256 = Hash(far),
                    scene = IntegratedScene,
                    art_approved = false, physical_mobile_tested = false,
                    canonical_scene_saved = false,
                    terrain_and_city_untouched = true,
                    limitations = new[] {
                        "Real Unity A/B images are an independent artistic review, not automatic ART_APPROVED.",
                        "No skeletal rig exists; the runtime presentation applies only subtle transform breathing.",
                        "Actual Android/iPhone GPU, memory, thermals and rendering must be measured separately.",
                        "Combat, targeting, completion rewards and persistent player state are unchanged."
                    }
                };
                var temp = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                working = new GameObject("Engendro B2 / persistent near-far source");
                var close = await Import(near, "Near / detailed genuine GLB", working.transform);
                var distance = await Import(far, "Far / mobile true LOD mesh", working.transform);
                var nearFilters = close.GetComponentsInChildren<MeshFilter>(true);
                var farFilters = distance.GetComponentsInChildren<MeshFilter>(true);
                var nearRenderers = close.GetComponentsInChildren<MeshRenderer>(true);
                var farRenderers = distance.GetComponentsInChildren<MeshRenderer>(true);
                audit.near_meshes = nearFilters.Length;
                audit.far_meshes = farFilters.Length;
                audit.near_triangles = nearFilters.Sum(x => x.sharedMesh.triangles.Length / 3);
                audit.far_triangles = farFilters.Sum(x => x.sharedMesh.triangles.Length / 3);
                audit.near_renderers = nearRenderers.Length;
                audit.far_renderers = farRenderers.Length;
                audit.materials = sharedMaterialByName.Count;
                audit.textures = textureCache.Count;
                Need(audit.near_triangles >= 10000 && audit.near_triangles <= 13000 &&
                    audit.far_triangles >= 2500 && audit.far_triangles < 3500,
                    "True mobile mesh LOD budgets missing");
                Need(audit.materials >= 1 && audit.textures >= 2 &&
                    nearRenderers.Length >= 2 && farRenderers.Length >= 1,
                    "Actual three-map PBR materials / anchored eyes not present");
                var group = working.AddComponent<LODGroup>();
                group.SetLODs(new[] {
                    new LOD(.045f, nearRenderers.Cast<Renderer>().ToArray()),
                    new LOD(.004f, farRenderers.Cast<Renderer>().ToArray())
                });
                group.RecalculateBounds();
                Need(working.GetComponentsInChildren<Collider>(true).Length == 0 &&
                    working.GetComponentsInChildren<WorldHotspot>(true).Length == 0,
                    "Imported art created a new physical collider or enemy gameplay target");
                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(working, PrefabPath);
                Need(savedPrefab != null, "Failed to save real editable Unity Resources prefab");
                audit.prefab_saved = true;
                audit.original_pbr_textures_persisted = textureCache.Count >= 2;
                AssetDatabase.SaveAssets();
                Object.DestroyImmediate(working); working = null;
                EditorSceneManager.OpenScene(IntegratedScene, OpenSceneMode.Single);
                var host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>(FindObjectsInactive.Include);
                Need(host && host.WorldRoot && host.WorldCamera,
                    "Actual current integrated Valoria+Zone1 scene unavailable");
                var nodes = host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true);
                var spot = nodes.Single(x => x.Id == SpawnlingB2Presentation.EncounterId);
                var originalPosition = spot.transform.position;
                var city = Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                Need(city && city.BastionOne && city.BastionTwo, "Canonical Bastion I-II missing");
                var originalBastionI = city.BastionOne;
                var originalBastionII = city.BastionTwo;
                var camera = host.WorldCamera;
                var cameraPosition = camera.transform.position;
                var cameraRotation = camera.transform.rotation;
                var cameraSize = camera.orthographicSize;
                bool originalWorld = host.WorldRoot.activeSelf, enabled = camera.enabled;
                var cityEnabled = city.PlayerCamera.enabled;
                try
                {
                    host.WorldRoot.SetActive(true);
                    city.PlayerCamera.enabled = false;
                    camera.enabled = true;
                    var centre = spot.transform.position + Vector3.up;
                    CaptureAt(camera, centre, 12, "spawnling-before-local.png", 1448, 1086);
                    CaptureAt(camera, centre, 32, "spawnling-before-strategic-portrait.png", 900, 1600);
                    Need(SpawnlingB2Presentation.AttachVisual(host, out var err),
                        "Cannot bind only visual replacement to original enemy " + err);
                    var visual = host.WorldRoot.GetComponentsInChildren<Transform>(true)
                        .Single(x => x.name == SpawnlingB2Presentation.VisualRootName);
                    var body = visual.GetComponentsInChildren<MeshRenderer>(true);
                    Need(body.Length >= 2, "No actual visible near/far PBR in integrated world");
                    Need(visual.GetComponentsInChildren<WorldHotspot>(true).Length == 0 &&
                        visual.GetComponentsInChildren<Collider>(true).Length == 0,
                        "Pure-art replacement introduced physics or commands");
                    audit.no_new_colliders = true;
                    audit.immutable_world_hotspot = spot.transform.position == originalPosition &&
                        nodes.Length == host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Length &&
                        spot.GetComponent<BoxCollider>() != null;
                    Need(audit.immutable_world_hotspot, "Existing Engendro picking / collider was altered");
                    audit.world_navigation_preserved = host.WorldCamera == camera &&
                        city.BastionOne == originalBastionI &&
                        city.BastionTwo == originalBastionII;
                    Need(audit.world_navigation_preserved, "Canonical view or Bastion state changed");
                    CaptureAt(camera, centre, 12, "spawnling-after-local.png", 1448, 1086);
                    CaptureAt(camera, centre, 32, "spawnling-after-strategic-portrait.png", 900, 1600);
                }
                finally
                {
                    host.WorldRoot.SetActive(originalWorld);
                    camera.enabled = enabled;
                    city.PlayerCamera.enabled = cityEnabled;
                    camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
                    camera.orthographicSize = cameraSize;
                }
                audit.captures = new[] {
                    "spawnling-before-local.png", "spawnling-after-local.png",
                    "spawnling-before-strategic-portrait.png",
                    "spawnling-after-strategic-portrait.png"
                };
                File.WriteAllText(Path.Combine(Evidence, "native-prefab-result.json"),
                    JsonUtility.ToJson(audit, true));
                Debug.Log("ELDORIA_SPAWNLING_B2_PERSISTENT_PREFAB_NATIVE_PASS_ART_REVIEW_REQUIRED");
                exit = 0;
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory(Evidence);
                File.WriteAllText(Path.Combine(Evidence, "native-prefab-failure.txt"), ex.ToString());
                Debug.LogException(ex);
            }
            finally
            {
                if (working) Object.DestroyImmediate(working);
                EditorApplication.Exit(exit);
            }
        }
    }
}
#endif
