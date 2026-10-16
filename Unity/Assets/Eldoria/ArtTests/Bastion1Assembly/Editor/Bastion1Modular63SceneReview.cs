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
                // STRUCTURAL FIRST: wall and gate elements are in the original
                // main composition, not scattered props at the image corners.
                // Historical renderer, trigger and collider ownership stays intact.
                var wall=await Import("wall_panel_regular_4m");
                var ironLarge=await Import("portcullis_large");
                var ironSmall=await Import("portcullis_small");
                var oak=await Import("reinforced_oak_gate");
                var cliff=await Import("cliff_rock_column");
                foreach(var wing in new[]{"UpperLeftWing","UpperRightWing","LowerGateRightWing"})
                    MountWallFace(originalRoot,wall,group.transform,wing,evidence);
                GateLower(originalRoot,ironLarge,oak,group.transform,evidence);
                GateUpper(ironSmall,group.transform,evidence);
                FrontCliff(cliff,group.transform,evidence);
                foreach(var unused in new[]{wall,ironLarge,ironSmall,oak,cliff})
                    UnityEngine.Object.DestroyImmediate(unused);
                if(evidence.Count<12)throw new Exception("Structural main-composition coverage insufficient: "+evidence.Count);
                CollectOwnedAssets(group);
                PersistOwnedAssets();
                if (!EditorSceneManager.SaveScene(scene, Candidate, true))
                    throw new Exception("Isolated candidate scene could not be saved");
                if (sourcePose != camera.transform.position || sourceRotation != camera.transform.rotation ||
                    Mathf.Abs(sourceSize - camera.orthographicSize) > .0001f)
                    throw new Exception("Gameplay camera pose changed during review");
                RenderSet(camera, "after", sourceSize);
                // Second, independently labelled A/B: Unity-only lighting trial
                // on this disposable candidate. Geometry and material sources
                // remain unchanged; the original playable scene stays preserved.
                ApplyGoldenHourLookdev();
                RenderSet(camera, "lookdev", sourceSize);
                if (sourcePose != camera.transform.position || sourceRotation != camera.transform.rotation ||
                    Mathf.Abs(sourceSize - camera.orthographicSize) > .0001f)
                    throw new Exception("Golden-hour comparison changed protected player camera");
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

        static void ApplyGoldenHourLookdev()
        {
            // Edit light and atmosphere parameters only. Never generate meshes,
            // textures, gameplay hotspots, cameras or new material assets.
            int sunCount = 0, torchCount = 0;
            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    light.color = new Color(1f, .83f, .69f);
                    light.intensity = Mathf.Min(light.intensity * 1.10f, 1.55f);
                    light.shadows = LightShadows.Soft;
                    sunCount++;
                }
                else if (light.type == LightType.Point &&
                         light.name.IndexOf("amber", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    light.color = new Color(1f, .48f, .20f);
                    light.intensity *= 1.12f;
                    light.range = Mathf.Max(light.range, 7.5f);
                    torchCount++;
                }
            }
            if (sunCount != 1)
                throw new Exception("Expected exactly one original Bastion I directional sun, got " + sunCount);
            RenderSettings.ambientSkyColor = new Color(.47f, .52f, .59f);
            RenderSettings.ambientEquatorColor = new Color(.43f, .41f, .37f);
            RenderSettings.ambientGroundColor = new Color(.36f, .31f, .24f);
            // Keep existing natural atmospheric depth, avoid old over-fogging.
            RenderSettings.fogColor = new Color(.34f, .40f, .45f);
            RenderSettings.fogDensity = .0026f;
            Debug.Log("BASTION1_GOLDEN_UNITY_LOOKDEV_ACTIVE sun=" + sunCount +
                      " torches=" + torchCount +
                      " geometry_modified=false source_materials_modified=false gameplay_modified=false " +
                      "status=ART_REVIEW_REQUIRED");
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


        // Always target true historical Unity renderer bounds, rather than
        // blindly guessing the size/position of a new modular asset.
        static Transform FindNode(GameObject original,string name)
        {
            foreach(var tr in original.GetComponentsInChildren<Transform>(true))
                if(tr.name==name)return tr;
            throw new Exception("Existing historic node is missing: "+name);
        }
        static GameObject CopyOn(GameObject template,Transform parent,string name)
        {
            var copy=UnityEngine.Object.Instantiate(template,parent);
            copy.name=name; copy.SetActive(true); return copy;
        }
        static void MatchWidth(GameObject copy,float metres)
        {
            var bounds=Bounds(copy);
            if(bounds.size.x<.01f)throw new Exception("Invalid source width "+copy.name);
            float s=metres/bounds.size.x;
            if(s<.2f || s>4.5f)throw new Exception("Implausible structural module scale "+copy.name+" "+s);
            copy.transform.localScale*=s;
        }
        static void Centre(GameObject copy,Vector3 target)
        {
            copy.transform.position+=target-Bounds(copy).center;
        }
        static readonly Dictionary<Material,Material> StoneTone=new Dictionary<Material,Material>();
        static void RebindNeutralLimestone(GameObject wall)
        {
            foreach(var render in wall.GetComponentsInChildren<Renderer>(true))
            {
                var materials=render.sharedMaterials;
                bool changed=false;
                for(int i=0;i<materials.Length;i++)
                {
                    var original=materials[i];
                    if(original==null)continue;
                    if(!StoneTone.TryGetValue(original,out var tinted))
                    {
                        // Only isolated Unity copies; never change the artist's
                        // GLB, texture/PBR normal maps or original city materials.
                        tinted=new Material(original){name="V6_neutral_limestone_candidate"};
                        if(tinted.HasProperty("_BaseColor"))
                        {
                            var c=tinted.GetColor("_BaseColor");
                            tinted.SetColor("_BaseColor",
                                new Color(c.r*.88f,c.g*1.045f,c.b*1.16f,c.a));
                        }
                        StoneTone.Add(original,tinted);
                    }
                    materials[i]=tinted;changed=true;
                }
                if(changed)render.sharedMaterials=materials;
            }
        }

        static void MountWallFace(GameObject original,GameObject module,Transform parent,
                                  string wing,List<string> notes)
        {
            var node=FindNode(original,wing);
            var bounds=Bounds(node.gameObject);
            if(bounds.size.x<3.4f || bounds.size.x>15f || bounds.size.y<1.5f)
                throw new Exception("Unexpected historic masonry bounds: "+wing+" "+bounds);
            int segments=Mathf.CeilToInt(bounds.size.x/3.8f);
            if(segments<1 || segments>4)throw new Exception("Invalid authored wall-module count");
            float width=bounds.size.x/segments;
            for(int i=0;i<segments;i++)
            {
                var copy=CopyOn(module,parent,"NEW_V6_WALL_"+wing+"_"+i);
                MatchWidth(copy,width);
                // The first Unity render found entire freestanding masonry slabs.
                // Reuse exactly the same 91-mesh V6 source as a thin, aligned
                // facing course, embedded into the original battlement wall.
                copy.transform.localScale=new Vector3(copy.transform.localScale.x,
                    copy.transform.localScale.y,copy.transform.localScale.z*.16f);
                RebindNeutralLimestone(copy);
                var b=Bounds(copy);
                Vector3 at=new Vector3(bounds.min.x+(i+.5f)*width,
                    bounds.min.y+b.size.y*.5f,
                    bounds.max.z+b.size.z*.5f+.018f);
                Centre(copy,at);
                notes.Add("authored_wall_V6_"+wing+"_"+i);
            }
        }
        static Bounds OldGateLeaves(GameObject original)
        {
            var gate=FindNode(original,"LowerGate");
            foreach(var renderer in gate.GetComponentsInChildren<Renderer>(true))
                if(renderer.name.StartsWith("hero_gate_v3_material_group_11",StringComparison.Ordinal))
                    return renderer.bounds;
            throw new Exception("Historic lower gate oak-reference bounds missing");
        }
        static void GateLower(GameObject original,GameObject iron,GameObject oak,
                              Transform parent,List<string> notes)
        {
            var opening=OldGateLeaves(original);
            if(opening.size.x<1.8f || opening.size.x>6.0f)
                throw new Exception("Historical gate protection envelope mismatch");
            var newIron=CopyOn(iron,parent,"NEW_CHARCOAL_LARGE_GATE_RASTRILLO");
            MatchWidth(newIron,Mathf.Min(opening.size.x,3.1f));
            Centre(newIron,new Vector3(opening.center.x,opening.min.y+1.6f,opening.max.z+.45f));
            notes.Add("authored_black_iron_lower_rastrillo");
            var newOak=CopyOn(oak,parent,"NEW_AUTHENTIC_OAK_GATE_V1");
            MatchWidth(newOak,2.45f);
            Centre(newOak,new Vector3(opening.center.x,opening.min.y+1.55f,opening.center.z-.16f));
            notes.Add("authored_oak_lower_gate_V1");
        }
        static void GateUpper(GameObject iron,Transform parent,List<string> notes)
        {
            var newIron=CopyOn(iron,parent,"NEW_CHARCOAL_SMALL_UPPER_RASTRILLO");
            MatchWidth(newIron,2.6f);
            // Exact upper citadel portal location is pinned in the original GLB;
            // use visible front, not the blocked central transit corridor.
            Centre(newIron,new Vector3(0f,5.2f,-21.0f));
            notes.Add("authored_black_iron_upper_rastrillo");
        }
        static void FrontCliff(GameObject cliff,Transform parent,List<string> notes)
        {
            // Original A/B: white boulder columns dangled in front of dark
            // scarp. This candidate embeds the same real scans deeper and
            // lower, exposing only natural relief along the original rock.
            var points=new[]{new Vector3(-19,-8,23),new Vector3(-12,-9,24),
                             new Vector3(12,-9,24),new Vector3(19,-8,23)};
            for(int i=0;i<points.Length;i++)
            {
                var copy=CopyOn(cliff,parent,"NEW_NEUTRAL_PHOTO_SCAN_FRONT_SCARP_"+i);
                var b=Bounds(copy);
                if(b.size.y<.1f)throw new Exception("Scanned geological asset height invalid");
                float s=4.35f/b.size.y;
                if(s<.25f||s>3f)throw new Exception("Scanned cliff scale unacceptable");
                copy.transform.localScale*=s;
                Centre(copy,points[i]);
                notes.Add("real_scanned_cliff_front_"+i);
            }
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
