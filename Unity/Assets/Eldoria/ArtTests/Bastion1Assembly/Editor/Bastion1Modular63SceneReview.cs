using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
        static readonly List<string> SourceManifest = new List<string>();

        [MenuItem("Eldoria/Art/Bastion I verify reviewed 63 sources on playable scene")]
        public static async void Capture()
        {
            try
            {
                if (!File.Exists(Original)) throw new FileNotFoundException("Preserved playable scene missing", Original);
                Directory.CreateDirectory(Evidence);
                SourceManifest.Clear();
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
                // ONE architectural section. The authentic original gate and its
                // integrated towers remain visible, rather than duplicating iron/oak
                // on top of the Hero V3 entrance. Rebuild the right wing as a
                // freestanding FULL-DEPTH masonry volume and attach a real kit tower.
                var wall=await Import("wall_panel_regular_4m");
                var tower=await Import("tower_square_short");
                var merlon=await Import("merlon_tall");
                RebuildGateWingWithTower(originalRoot,wall,tower,merlon,group.transform,
                                         "LowerGateRightWing",evidence);
                UnityEngine.Object.DestroyImmediate(wall);
                UnityEngine.Object.DestroyImmediate(tower);
                UnityEngine.Object.DestroyImmediate(merlon);
                if(evidence.Count<4)
                    throw new Exception("The complete modular wall+tower proof is incomplete");
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
                File.WriteAllLines(Path.Combine(Evidence, "source-glb-sha256.txt"), SourceManifest);
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
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(file))
                SourceManifest.Add(assetName + " " + BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant());
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

        // World-space socket reconstruction: one built masonry volume replaces
        // the old visual wall. No 0.16 depth scaling and no front-surface decals.
        // Old renderer state only is disabled; colliders/triggers/transforms live on.
        static void RebuildGateWingWithTower(GameObject original,GameObject wall,
                                            GameObject tower,GameObject merlon,Transform parent,
                                            string wing,List<string> notes)
        {
            var historical=FindNode(original,wing);
            var oldBounds=Bounds(historical.gameObject);
            var originals=historical.GetComponentsInChildren<Renderer>(true);
            if(originals.Length==0 || oldBounds.size.x<3.4f ||
               oldBounds.size.x>15f || oldBounds.size.y<1.5f)
                throw new Exception("Unsafe historical section bounds: "+wing+" "+oldBounds);
            int originalColliders=historical.GetComponentsInChildren<Collider>(true).Length;
            int segments=Mathf.CeilToInt(oldBounds.size.x/3.8f);
            if(segments<1||segments>4)throw new Exception("Unsupported wall span");
            float bayWidth=oldBounds.size.x/segments;
            var replacement=new GameObject("BUILT_SECTION_"+wing+"_full_volume");
            replacement.transform.SetParent(parent,false);
            Bounds aggregate=new Bounds();
            bool first=true;
            float rightEdge=0f,wallBase=oldBounds.min.y;
            for(int i=0;i<segments;i++)
            {
                var piece=CopyOn(wall,replacement.transform,"BUILT_MASONRY_BAY_"+i);
                MatchWidth(piece,bayWidth);
                RebindNeutralLimestone(piece);
                var size=Bounds(piece).size;
                if(size.z<.35f||size.z>4.5f)
                    throw new Exception("Masonry has no credible wall thickness: "+size.z);
                if(size.y<oldBounds.size.y*.55f||size.y>oldBounds.size.y*2.2f)
                    throw new Exception("New wall height mismatches old functional envelope");
                Centre(piece,new Vector3(oldBounds.min.x+(i+.5f)*bayWidth,
                    wallBase+size.y*.5f,oldBounds.center.z));
                var actual=Bounds(piece);
                if(Mathf.Abs(actual.min.y-wallBase)>.05f ||
                   Mathf.Abs(actual.center.z-oldBounds.center.z)>.05f)
                    throw new Exception("Misaligned full-volume wall: "+i);
                if(i>0 && Mathf.Abs(actual.min.x-rightEdge)>.075f)
                    throw new Exception("Wall bay joint leaves a visible gap at "+i);
                rightEdge=actual.max.x;
                if(first){aggregate=actual;first=false;}else aggregate.Encapsulate(actual);
                // Real load-bearing wall beneath individually socketed merlons.
                // Unlike an overlapping crenellated facade, these are disjoint
                // constructive blocks resting on the actual NEW wall top face.
                int count=Mathf.Max(3,Mathf.RoundToInt(bayWidth/.95f));
                float toothHeight=.92f;
                for(int tooth=0;tooth<count;tooth++)
                {
                    var cap=CopyOn(merlon,replacement.transform,
                        "BUILT_STONE_MERLON_BAY_"+i+"_"+tooth);
                    var sourceCap=Bounds(cap);
                    float uniform=toothHeight/sourceCap.size.y;
                    if(uniform<.4f||uniform>1.6f)
                        throw new Exception("Merlon kit scale outside construction envelope");
                    cap.transform.localScale*=uniform;
                    RebindNeutralLimestone(cap);
                    var mb=Bounds(cap);
                    float x=actual.min.x+(tooth+.5f)*bayWidth/count;
                    Centre(cap,new Vector3(x,actual.max.y+mb.size.y*.5f,
                        actual.center.z));
                    mb=Bounds(cap);
                    if(Mathf.Abs(mb.min.y-actual.max.y)>.035f ||
                       mb.min.x<actual.min.x-.03f ||
                       mb.max.x>actual.max.x+.03f ||
                       mb.size.z>actual.size.z+.08f ||
                       Mathf.Abs(mb.center.z-actual.center.z)>.035f)
                        throw new Exception("Merlon neither socketed nor truly supported: "+cap.name);
                    notes.Add("BUILT_REAL_MERLON_BAY_"+i+"_"+tooth);
                }
                notes.Add("BUILT_FULL_DEPTH_BAY_"+i);
            }
            if(Mathf.Abs(aggregate.min.x-oldBounds.min.x)>.075f ||
               Mathf.Abs(aggregate.max.x-oldBounds.max.x)>.075f)
                throw new Exception("New modular wall does not cover historic wall span");

            // Choose the OUTBOARD socket from the actual historical gate centre.
            // The old always-max-X rule put the kit tower on top of the existing
            // defended gate tower for negative-X wings, an observed ART FAIL.
            var gate=FindNode(original,"LowerGate");
            var gateBounds=Bounds(gate.gameObject);
            float wingDelta=oldBounds.center.x-gateBounds.center.x;
            if(Mathf.Abs(wingDelta)<.25f)
                throw new Exception("Unable to determine outboard tower socket from gate");
            bool outboardNegativeX=wingDelta<0f;
            var watch=CopyOn(tower,replacement.transform,"BUILT_SHORT_SQUARE_WATCHTOWER");
            RebindNeutralLimestone(watch);
            var tb=Bounds(watch);
            float towerOverlap=Mathf.Min(.48f,tb.size.x*.12f);
            float outboardCentreX=outboardNegativeX
                ? aggregate.min.x-tb.size.x*.5f+towerOverlap
                : aggregate.max.x+tb.size.x*.5f-towerOverlap;
            Centre(watch,new Vector3(outboardCentreX,
                wallBase+tb.size.y*.5f,aggregate.center.z));
            tb=Bounds(watch);
            float socketIntersection=outboardNegativeX
                ? tb.max.x-aggregate.min.x : aggregate.max.x-tb.min.x;
            float gateClearance=outboardNegativeX
                ? gateBounds.min.x-tb.max.x : tb.min.x-gateBounds.max.x;
            if(tb.size.x<3f||tb.size.z<3f||tb.size.y<4f||
               Mathf.Abs(tb.min.y-wallBase)>.05f ||
               socketIntersection<.20f || socketIntersection>.6f ||
               Mathf.Abs(tb.center.z-aggregate.center.z)>.05f ||
               gateClearance<.15f)
                throw new Exception("Kit tower is not connected outboard, or invades historic gate: socket="+
                    socketIntersection+" gateClearance="+gateClearance);
            notes.Add("OUTBOARD_TOWER_SOCKET_"+(outboardNegativeX?"MIN_X":"MAX_X"));
            float horizontalGap=Mathf.Max(0f,Mathf.Max(gateBounds.min.x-aggregate.max.x,
                aggregate.min.x-gateBounds.max.x));
            float depthGap=Mathf.Max(0f,Mathf.Max(gateBounds.min.z-aggregate.max.z,
                aggregate.min.z-gateBounds.max.z));
            if(horizontalGap>1f||depthGap>2.8f)
                throw new Exception("Rebuilt wall is not connected to defended entrance");
            notes.Add("BUILT_SOCKETED_TOWER_tower_square_short");
            notes.Add("CONNECTED_TO_EXISTING_LOWER_GATE_and_HERO_V3");

            // Renderer-level reversible replacement. Deliberately do not call
            // SetActive(false), destroy meshes, or change historical colliders.
            foreach(var renderer in originals)renderer.enabled=false;
            if(historical.GetComponentsInChildren<Collider>(true).Length!=originalColliders ||
               Array.Exists(originals,x=>x.enabled))
                throw new Exception("Old functional geometry changed or old wall still visible");
            notes.Add("HISTORICAL_WING_RENDERERS_DISABLED_"+originals.Length);
            notes.Add("FUNCTIONAL_COLLIDERS_PRESERVED_"+originalColliders);
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
