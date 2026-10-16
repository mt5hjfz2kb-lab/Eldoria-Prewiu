using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eldoria.Presentation;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    // Non-destructive A/B of individually reviewed GLBs on the preserved playable
    // scene. Never regenerates geometry, rewrites original assets, changes gameplay
    // or overwrites ProductionSlice/Runtime/Valoria.unity.
    public static class Bastion1Modular63SceneReview
    {
        const string Original = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string Candidate = "Assets/Eldoria/ArtTests/Bastion1Assembly/Bastion1Modular63Candidate.unity";
        const string Inputs = "Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedSource/Modular63";
        const string Generated = "Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedModular63";
        const string Evidence = "Bastion1Modular63Review";
        static readonly List<MeshCollider> ContactColliders = new List<MeshCollider>();
        static readonly List<UnityEngine.Object> Pending = new List<UnityEngine.Object>();
        static readonly HashSet<UnityEngine.Object> Seen = new HashSet<UnityEngine.Object>();

        [MenuItem("Eldoria/Art/Bastion I verify reviewed 63 sources on playable scene")]
        public static async void Capture()
        {
            try
            {
                if (!File.Exists(Original)) throw new FileNotFoundException("Preserved playable scene missing", Original);
                Directory.CreateDirectory(Evidence);
                var scene = EditorSceneManager.OpenScene(Original, OpenSceneMode.Single);
                var presentation = UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
                if (presentation == null || !presentation.MeshMode || presentation.ProductionCamera == null ||
                    presentation.Bindings == null || presentation.Bindings.Length < 2 ||
                    presentation.Bindings[0].Ground == null || presentation.Bindings[0].Construction == null ||
                    presentation.Bindings[0].Built == null || presentation.Bindings[1].Built == null)
                    throw new Exception("Existing playable Bastion I/II parcel bindings not intact");
                var camera = presentation.ProductionCamera;
                var sourcePose = camera.transform.position;
                var sourceRotation = camera.transform.rotation;
                var sourceSize = camera.orthographicSize;
                RenderSet(camera, "before", sourceSize);
                var originalRoot = GameObject.Find("BastionI_ReferenceAssembly_CANDIDATE");
                if (originalRoot == null) throw new Exception("Original bastion assembly missing");
                var group = new GameObject("Modular63_VISUAL_CANDIDATE_NO_GAMEPLAY");
                var evidence = new List<string>();
                MakeGroundContactProbes(originalRoot);
                try
                {
                    // No gate, wall, road or stairs replacement before their scale
                    // and contact have been separately checked in Unity.
                    var pine = await Import("pine_medium");
                    var rock = await Import("cliff_rock_column");
                    int pinePlaced = 0, rockPlaced = 0;
                    foreach (var point in new[] {new Vector2(29f, -4f), new Vector2(-29f, -10f), new Vector2(30f, 14f)})
                        if (pinePlaced < 2 && TryPlace(pine, point, 11f, group.transform,
                            "PineReviewed" + pinePlaced, out var state))
                        {
                            pinePlaced++;
                            evidence.Add(state);
                        }
                    foreach (var point in new[] {new Vector2(32f, -21f), new Vector2(-32f, 19f), new Vector2(32f, 5f)})
                        if (rockPlaced < 1 && TryPlace(rock, point, 7f, group.transform,
                            "ScannedCliffReviewed" + rockPlaced, out var state))
                        {
                            rockPlaced++;
                            evidence.Add(state);
                        }
                    if (pinePlaced < 1 || rockPlaced < 1)
                        throw new Exception("No valid grass contact for pine or rock: no model was promoted");
                    // The imported templates are never renderable scene instances.
                    UnityEngine.Object.DestroyImmediate(pine);
                    UnityEngine.Object.DestroyImmediate(rock);
                }
                finally { RemoveGroundContactProbes(); }
                CollectOwnedAssets(group);
                PersistOwnedAssets();
                if (!EditorSceneManager.SaveScene(scene, Candidate, true))
                    throw new Exception("Isolated candidate scene could not be saved");
                if (sourcePose != camera.transform.position || sourceRotation != camera.transform.rotation ||
                    Mathf.Abs(sourceSize - camera.orthographicSize) > .0001f)
                    throw new Exception("Gameplay camera pose changed during review");
                RenderSet(camera, "after", sourceSize);
                File.WriteAllText(Path.Combine(Evidence, "modular63-scene-review.json"),
                    "{\n  \"schema\": \"eldoria-modular63-scene-review-v1\",\n" +
                    "  \"original_scene_unchanged\": true,\n  \"models_created\": 0,\n" +
                    "  \"runtime_gameplay_modified\": false,\n  \"art_approved\": false,\n" +
                    "  \"mobile_performance_approved\": false,\n  \"placements\": [" +
                    String.Join(",", evidence.ConvertAll(e => "\"" + e + "\"")) + "]\n}\n");
                Debug.Log("BASTION1_MODULAR63_AB_CAPTURE_COMPLETE new_scene=" + Candidate +
                          " placements=" + evidence.Count + " approval=REVIEW_REQUIRED");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                RemoveGroundContactProbes();
                Debug.LogError("BASTION1_MODULAR63_AB_REJECT " + exception);
                EditorApplication.Exit(1);
            }
        }

        static void RenderSet(Camera camera, string phase, float originalSize)
        {
            Bastion1ReferenceCapture.Render(camera, Path.GetFullPath(Evidence + "/" + phase + "-landscape.png"), 1440, 1000);
            Bastion1ReferenceCapture.Render(camera, Path.GetFullPath(Evidence + "/" + phase + "-mobile.png"), 780, 520);
            camera.orthographicSize = 76f;
            try { Bastion1ReferenceCapture.Render(camera, Path.GetFullPath(Evidence + "/" + phase + "-portrait.png"), 390, 844); }
            finally { camera.orthographicSize = originalSize; }
        }

        static async Task<GameObject> Import(string assetName)
        {
            string file = Path.GetFullPath(Inputs + "/" + assetName + ".glb");
            if (!File.Exists(file)) throw new FileNotFoundException("Pinned approved-for-study GLB unavailable", file);
            var importer = new GltfImport(null, new UninterruptedDeferAgent());
            if (!await importer.Load(new Uri(file).AbsoluteUri,
                    new ImportSettings { GenerateMipMaps = true, AnisotropicFilterLevel = 2 }))
                throw new Exception("Unity failed to load GLB " + assetName);
            var root = new GameObject("Template_" + assetName);
            if (!await importer.InstantiateMainSceneAsync(root.transform))
                throw new Exception("Unity failed to instantiate " + assetName);
            Bastion1MaterialBindings.Apply(root, importer, file);
            root.SetActive(false);
            return root;
        }

        static bool TryPlace(GameObject original, Vector2 anchor, float desiredHeight, Transform parent,
                            string name, out string evidence)
        {
            evidence = "";
            // Protect 14m axial transit and 10x10m building parcels plus 2m buffer.
            if (Mathf.Abs(anchor.x) < 23f) throw new Exception("Placement enters protected road/parcel envelope");
            if (!TryGroundY(anchor, out float groundY)) return false;
            var copy = UnityEngine.Object.Instantiate(original, parent);
            copy.name = name;
            copy.SetActive(true);
            var bounds = Bounds(copy);
            if (bounds.size.y < .05f) throw new Exception("Degenerate geometry " + name);
            var scale = Mathf.Clamp(desiredHeight / bounds.size.y, .25f, 3f);
            copy.transform.localScale *= scale;
            bounds = Bounds(copy);
            copy.transform.position += new Vector3(anchor.x - bounds.center.x, groundY - bounds.min.y, anchor.y - bounds.center.z);
            bounds = Bounds(copy);
            if (Mathf.Abs(bounds.min.y - groundY) > .04f)
                throw new Exception("Floating candidate " + name);
            if (bounds.size.x > 16f || bounds.size.z > 16f)
                throw new Exception("Candidate too wide for declared side parcel " + name);
            evidence = name + ":grounded";
            return true;
        }

        static Bounds Bounds(GameObject root)
        {
            var all = root.GetComponentsInChildren<Renderer>(true);
            if (all.Length < 1) throw new Exception("No visible renderer " + root.name);
            var bound = all[0].bounds;
            foreach (var r in all) bound.Encapsulate(r.bounds);
            return bound;
        }

        static void MakeGroundContactProbes(GameObject root)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var n = filter.name.ToLowerInvariant();
                if (!(n.Contains("soil") || n.Contains("grass") || n.Contains("plateau") ||
                      n.Contains("meadow") || n.Contains("hilltop") || n.Contains("earth"))) continue;
                if (filter.sharedMesh == null) continue;
                var collider = filter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
                ContactColliders.Add(collider);
            }
            if (ContactColliders.Count == 0) throw new Exception("Ground source collision probes unavailable");
            Physics.SyncTransforms();
        }
        static bool TryGroundY(Vector2 anchor, out float y)
        {
            y = 0;
            var hits = Physics.RaycastAll(new Vector3(anchor.x, 130f, anchor.y), Vector3.down, 270f,
                                          Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (!ContactColliders.Contains(hit.collider as MeshCollider)) continue;
                if (hit.point.y > y || y == 0) y = hit.point.y;
            }
            return hits.Length > 0 && ContactColliders.Exists(c => Array.Exists(hits, h => h.collider == c));
        }
        static void RemoveGroundContactProbes()
        {
            foreach (var collider in ContactColliders)
                if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            ContactColliders.Clear();
        }
        static void CollectOwnedAssets(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null) continue;
                    foreach (var property in mat.GetTexturePropertyNames()) Add(mat.GetTexture(property));
                    Add(mat);
                }
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true)) Add(filter.sharedMesh);
        }
        static void Add(UnityEngine.Object obj)
        {
            if (obj != null && !AssetDatabase.Contains(obj) && Seen.Add(obj)) Pending.Add(obj);
        }
        static void PersistOwnedAssets()
        {
            Directory.CreateDirectory(Generated);
            int number = 0;
            foreach (var obj in Pending)
            {
                var path = Generated + "/asset_" + number.ToString("D4") + ".asset";
                if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(obj, path);
                number++;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
