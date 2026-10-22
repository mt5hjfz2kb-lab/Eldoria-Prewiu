using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using Eldoria.World4XPrototype;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.World4XCanon
{
    /// <summary>
    /// Presentation-only Region 1 extension for the *integrated* Valoria world.
    /// Never makes WorldHotspots, commands, simulated cities, opponents or save data.
    /// Installed at scene load rather than editing the UI/Spawnling-owned integration scene.
    /// All three distances observe the same textured, deterministic mesh.
    /// </summary>
    public sealed class RegionOneCanonicalLandscape : MonoBehaviour
    {
        public const int LandscapeVersion = 1;
        // The strategic Region 1 fits in a landscape panorama. The much wider
        // textured continuation masks the outer bounds without forming an island.
        public const float CoreHalfWidth = 425f;
        public const float CoreHalfDepth = 260f;
        public const float LandscapeHalfWidth = 910f;
        public const float LandscapeHalfDepth = 780f;
        public const float BreachOffsetX = 8f;
        public const float BreachOffsetZ = 61f;
        public const float LocalThreshold = 41f;
        public const float RegionalThreshold = 152f;
        public const float LandscapePanoramaZoom = 244f;
        // Portrait requires a wider orthographic overview of the SAME XYZ
        // geography. 456 is bounded by the existing 1200x1070 camera travel
        // envelope and shared landscape mesh; no separate map or mobile HUD.
        public const float PortraitPanoramaZoom = 456f;

        static readonly Color Grass = new Color(.49f, .56f, .33f);
        static readonly Color DryGrass = new Color(.69f, .61f, .34f);
        static readonly Color DarkForest = new Color(.25f, .39f, .23f);
        static readonly Color Road = new Color(.67f, .54f, .32f);
        static readonly Color Rock = new Color(.52f, .49f, .40f);
        static readonly Color Water = new Color(.19f, .43f, .58f);
        static readonly Color Rift = new Color(.21f, .15f, .22f);
        static readonly Vector2[] ForestCentres = RegionOneAuthoredComposition.Centres();
        internal static readonly int[] ForestCorridorPairs = new int[0];

        ValoriaIntegratedWorld host;
        ZoneOneStrategicCamera navigation;
        Vector3 origin;
        GameObject detailTrees;
        GameObject peripheralFog;
        // Render-only LOD: the imported prairie is retained at local zoom,
        // but removed at regional/panorama where its hard square edge cuts
        // across the shared World4X territory. No collider is modified.
        bool legacyGroundHidden;
        readonly List<Renderer> originalGround = new List<Renderer>();
        readonly List<bool> originalGroundInitiallyEnabled = new List<bool>();
        readonly List<MeshRenderer> hiddenProvisionalCamps = new List<MeshRenderer>();
        readonly List<bool> hiddenProvisionalCampsOriginalStates = new List<bool>();
        readonly List<Renderer> existingGameRenderers = new List<Renderer>();
        readonly List<bool> existingGameRendererStates = new List<bool>();
        GameObject distantForestSilhouettes;
        GameObject distantGeology;
        Mesh terrainMesh;
        Texture2D terrainTexture, legacyCompareTexture;
        Texture2D hybridForestAtlas;
        Material surfaceMaterial, riftMaterial, fogMaterial, breachCoreMaterial, breachHaloMaterial;
        readonly List<Mesh> generatedMeshes = new List<Mesh>();
        bool built, farMode, fogMode;
        bool woodlandMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // AfterSceneLoad alone runs once per application, not on later
            // scene transitions. The real player can enter Valoria after a
            // bootstrap scene, and PlayMode tests also LoadScene dynamically.
            SceneManager.sceneLoaded -= WhenSceneLoaded;
            SceneManager.sceneLoaded += WhenSceneLoaded;
            AttachToCurrentIntegratedScene();
        }

        static void WhenSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToCurrentIntegratedScene();
        }

        static void AttachToCurrentIntegratedScene()
        {
            var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if (!host || UnityEngine.Object.FindFirstObjectByType<RegionOneCanonicalLandscape>()) return;
            var adapter = new GameObject("Region 1 canonical world · presentation adapter")
                .AddComponent<RegionOneCanonicalLandscape>();
            adapter.host = host;
        }

        IEnumerator Start()
        {
            if (!host) yield break;
            while (host && !host.Ready) yield return null;
            if (!host || !host.WorldRoot || !host.WorldNavigation || !host.WorldCamera) yield break;
            navigation = host.WorldNavigation;
            origin = host.WorldHome;
            if (host.WorldRoot.transform.Find("Region 1 canonical continuous geography"))
            {
                Debug.LogWarning("REGION1_CANON_LAYER_ALREADY_PRESENT");
                yield break;
            }
            InstallCameraLimits();
            Build(host.WorldRoot.transform);
            built = true;
            UpdateLevelOfDetail(true);
            Debug.Log("REGION1_CANON_WORLD_INSTALLED presentation_only=1 geography=1 save_changes=0");
        }

        void InstallCameraLimits()
        {
            // Bounds enclose a larger real terrain mesh, not a zoom onto empty sky.
            // The current integration sets regionalZoom=32 and localZoom=14 in Start.
            // The previous 17-unit close zoom filled the view with Valoria's
            // castle and prevented players from reading the surrounding 4X
            // territory. Widen both scales, retaining one real camera and
            // strong near -> regional -> panorama separation.
            navigation.localZoom = 36f;
            navigation.regionalZoom = 113f;
            navigation.globalZoom = navigation.View.aspect < .80f
                ? PortraitPanoramaZoom : LandscapePanoramaZoom;
            navigation.tilt = 66f;
            navigation.cameraDistance = Mathf.Max(navigation.cameraDistance, 540f);
            navigation.minimum = new Vector2(origin.x - 600f, origin.z - 535f);
            navigation.maximum = new Vector2(origin.x + 600f, origin.z + 535f);
            navigation.edgeMargin = 12f;
            navigation.FitToAuthoredBounds();
        }

        void Build(Transform existingWorld)
        {
            var root = new GameObject("Region 1 canonical continuous geography").transform;
            root.SetParent(existingWorld, true);
            // Existing hotspot transforms remain the only gameplay coordinate authority.
            // Lit terrain A/B trial: the real single mesh already has broad
            // shallow relief, but an Unlit surface hides its topography at
            // world-camera scale. No light, geometry, gameplay or HUD changes.
            surfaceMaterial = MakeMaterial("Region1 ground texture", Color.white, false);
            // Controlled hybrid trial: legacy mode is opt-in for exact
            // same-camera A/B only; the real game uses the richer surface.
            bool legacyOnly = Environment.GetEnvironmentVariable("ELDORIA_REGION1_HYBRID_BASELINE")=="1";
            terrainTexture = legacyOnly ? MakeTerrainTexture(1024)
                : RegionOneHybridTerrainArt.Bake(1024,LandscapeHalfWidth,
                    LandscapeHalfDepth,BreachOffsetX,BreachOffsetZ);
            if (surfaceMaterial.HasProperty("_BaseMap")) surfaceMaterial.SetTexture("_BaseMap", terrainTexture);
            if (surfaceMaterial.HasProperty("_MainTex")) surfaceMaterial.SetTexture("_MainTex", terrainTexture);
            var land = new GameObject("Region1 · one coherent terrain mesh");
            land.transform.SetParent(root, true);
            var filter = land.AddComponent<MeshFilter>();
            terrainMesh = MakeLandscapeMesh(80);
            filter.sharedMesh = terrainMesh;
            var renderer = land.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = surfaceMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            // Imported close-up prairie remains available only for local zoom;
            // the continuous terrain carries regional and panoramic scales.
            // All original hotspot colliders and logic remain intact.
            CollectLegacyGround(existingWorld);
            HideProvisionalEnemyCampDonors(existingWorld);
            CacheGameplayArtRenderers(existingWorld, root);
            // No terrain collider: world node selection continues to hit original WorldHotspots.
            BuildBreach(root);
            BuildTreeDetail(root, existingWorld);
            BuildDistantForests(root);
            BuildDistantGeology(root);
            BuildEdgeFog(root);
        }

        void CollectLegacyGround(Transform existingWorld)
        {
            foreach (var renderer in existingWorld.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer || renderer.transform.IsChildOf(transform)) continue;
                string name = renderer.name.ToLowerInvariant();
                bool isPrairie = name.Contains("continuous prairie") ||
                    name.Contains("organic macrogeography") ||
                    (renderer.transform.parent != null &&
                     renderer.transform.parent.name.IndexOf("continuous prairie",
                         StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (name.Contains("ground") && name.Contains("zoneone"));
                // Do not infer that the city, quarry, real trees or enemy rocks
                // are background; only the positively identified prairie.
                if (!isPrairie || renderer.bounds.size.x < 100f ||
                                  renderer.bounds.size.z < 100f) continue;
                originalGround.Add(renderer);
                originalGroundInitiallyEnabled.Add(renderer.enabled);
            }
            Debug.Log("REGION1_CANON_LEGACY_PRAIRIE_RENDERERS=" + originalGround.Count);
        }

        void HideProvisionalEnemyCampDonors(Transform existingWorld)
        {
            // The source GLB contains canvas/shelter placeholders labelled
            // as enemy camps. They are NOT the existing encounters. Hide only
            // those old donor meshes, leaving their WorldHotspots, colliders,
            // targeting, animated Spawnling and saved gameplay unchanged.
            foreach (Transform node in existingWorld.GetComponentsInChildren<Transform>(true))
            {
                if (node.name != "Enemy camp provisional existing canvas" &&
                    node.name != "Enemy camp existing shelter") continue;
                foreach (var renderer in node.GetComponentsInChildren<MeshRenderer>(true))
                {
                    hiddenProvisionalCamps.Add(renderer);
                    hiddenProvisionalCampsOriginalStates.Add(renderer.enabled);
                    renderer.enabled = false;
                }
            }
            Debug.Log("REGION1_CANON_OLD_CAMP_MESHES_HIDDEN="+hiddenProvisionalCamps.Count);
        }

        void CacheGameplayArtRenderers(Transform existingWorld, Transform canonicalArt)
        {
            foreach (var renderer in existingWorld.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer || renderer.transform.IsChildOf(canonicalArt)) continue;
                // Only renderer.enabled changes at a distant scale; every
                // WorldHotspot, collider, save, march and scene object remains.
                existingGameRenderers.Add(renderer);
                existingGameRendererStates.Add(renderer.enabled);
            }
            Debug.Log("REGION1_CANON_PANORAMA_GAME_ART_LOD=" + existingGameRenderers.Count);
        }

        Mesh MakeLandscapeMesh(int resolution)
        {
            int row = resolution + 1;
            var vertices = new Vector3[row * row];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[resolution * resolution * 6];
            for (int z = 0; z <= resolution; z++)
                for (int x = 0; x <= resolution; x++)
                {
                    float u = x / (float)resolution, v = z / (float)resolution;
                    float localX = (u * 2 - 1) * LandscapeHalfWidth;
                    float localZ = (v * 2 - 1) * LandscapeHalfDepth;
                    int index = z * row + x;
                    vertices[index] = new Vector3(origin.x + localX, SampleHeight(localX, localZ), origin.z + localZ);
                    uv[index] = new Vector2(u, v);
                }
            int t = 0;
            for (int z = 0; z < resolution; z++)
                for (int x = 0; x < resolution; x++)
                {
                    int i = z * row + x;
                    triangles[t++] = i; triangles[t++] = i + row; triangles[t++] = i + 1;
                    triangles[t++] = i + 1; triangles[t++] = i + row; triangles[t++] = i + row + 1;
                }
            var mesh = new Mesh { name = "Region 1 single terrain + peripheral continuation",
                vertices = vertices, uv = uv, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            return mesh;
        }

        public static float SampleHeight(float x, float z)
        {
            // Preserve near-field gameplay and native surface at ground level.
            // Only distant, non-interactive countryside gets smooth, genuine
            // 3D broad undulation. One deterministic XYZ for all zooms; no
            // extra colliders, assets or invented mountains.
            float radius = Mathf.Sqrt(x*x+z*z);
            float near = Mathf.Clamp01((radius - 62f) / 140f);
            float contour = Mathf.Sin(x * .017f) * Mathf.Cos(z * .014f) * .95f +
                            Mathf.Sin(z * .009f + x * .006f) * .55f;
            float distantRidge = Mathf.Clamp01((Mathf.Abs(x) - 330f) / 290f) * 3.5f;
            float outlands=Mathf.SmoothStep(0f,1f,
                Mathf.Clamp01((radius-225f)/240f));
            float slowRoll=(Mathf.PerlinNoise(x*.009f+13f,z*.010f+31f)-.5f)*18f+
                (Mathf.PerlinNoise(x*.021f+53f,z*.018f+71f)-.5f)*5f;
            // Coastal interpolation uses the SAME painted water transition.
            // The old binary IsCoast toggle made an artificial cliff at the
            // edge even though the diffuse coast was progressively blended.
            float coastalWater=CoastWaterMask(x,z);
            float landFactor=1f-Mathf.SmoothStep(0f,1f,coastalWater);
            // Extend the atlas's four restrained stone ridges into actual
            // low 3D hills. This changes presentation only and preserves the
            // real central gameplay corridor and all original colliders.
            float ridgeLift=RegionOneHybridTerrainArt.RidgeElevation(x,z);
            return -4.0f + near * (contour + distantRidge) +
                landFactor*(outlands*slowRoll+ridgeLift);
        }

        static float Noise(int x, int z)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393 + z * 668265263 + 20261022);
                n = (n ^ (n >> 13)) * 1274126177u;
                return ((n ^ (n >> 16)) & 65535u) / 65535f;
            }
        }

        public static bool IsForest(float x,float z)
        {
            return RegionOneAuthoredComposition.Woodland(x,z)>.32f;
        }

        public static float CoastWaterMask(float x, float z)
        {
            // Fractional water depth for a soft, painted shoreline. The
            // previous boolean mask formed an unnaturally hard blue block.
            // Geography-only texture; there is no new sea collider or travel.
            // A narrow southeastern shore, never an island outline.
            float coast = 366f + 12f * Mathf.Sin(z * .032f) +
                          7f * Mathf.Sin(z * .083f);
            float southEdge = -68f - .22f * (x - 366f) +
                              10f * Mathf.Sin(x * .022f);
            // Keep the established eastern edge and connect a short,
            // irregular southeast shoreline to it. Panorama-only geography:
            // no invented POIs, mesh colliders, ports or new water gameplay.
            // In portrait this reveals the coast near the *actual* southern
            // landscape border instead of an all-green central strip.
            float southShore = -370f + 12f * Mathf.Sin(x*.020f) +
                               7f * Mathf.Sin(x*.071f);
            float southWestLimit = -90f + 18f * Mathf.Sin(z*.018f);
            float eastDepth = z < southEdge ? x-coast : -10000f;
            float southDepth = x > southWestLimit ? southShore-z : -10000f;
            // A restrained winding inlet connects the actual eastern coast
            // with Region 1's southeast foreground. It only changes the
            // deterministic atlas and existing terrain elevations; no new
            // sea objects or gameplay are introduced. Its soft westward taper
            // avoids a geometric stripe across the bottom of the map.
            float bayShore = -214f + 15f*Mathf.Sin(x*.018f) +
                             8f*Mathf.Sin(x*.052f);
            float bayBlend = Mathf.SmoothStep(0f,1f,
                Mathf.Clamp01((x-95f)/110f));
            float bayDepth = (bayShore-z)*bayBlend - 10f*(1f-bayBlend);
            // Ten geographic units (~9 portrait pixels) of shallow coast
            // transition; same deterministic map for all view distances.
            return Mathf.Clamp01(Mathf.Max(Mathf.Max(eastDepth,southDepth),
                                             bayDepth)/10f);
        }

        public static bool IsCoast(float x,float z)
        {
            return CoastWaterMask(x,z)>0f;
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / Mathf.Max(.001f, Vector2.Dot(ab, ab)));
            return Vector2.Distance(point, a + ab * t);
        }

        static readonly Vector2[][] LandTracks = {
            new [] {new Vector2(0,0),new Vector2(-28,29),
                new Vector2(-82,73),new Vector2(-142,93),
                new Vector2(-220,130),new Vector2(-284,199),new Vector2(-401,247)},
            new [] {new Vector2(0,0),new Vector2(38,36),
                new Vector2(99,71),new Vector2(142,127),
                new Vector2(218,154),new Vector2(279,215),new Vector2(391,251)},
            new [] {new Vector2(0,0),new Vector2(-37,-41),
                new Vector2(-100,-91),new Vector2(-126,-151),
                new Vector2(-201,-210),new Vector2(-249,-291),new Vector2(-333,-359)},
            new [] {new Vector2(0,0),new Vector2(51,-31),
                new Vector2(117,-74),new Vector2(163,-112),
                new Vector2(224,-178),new Vector2(307,-208),new Vector2(409,-269)},
            new [] {new Vector2(-253,214),new Vector2(-190,267),
                new Vector2(-99,296),new Vector2(-11,284),new Vector2(55,289),
                new Vector2(150,235),new Vector2(258,214)},
            new [] {new Vector2(-244,-286),new Vector2(-123,-255),
                new Vector2(-31,-283),new Vector2(65,-267),new Vector2(163,-243),
                new Vector2(307,-207)},
            new [] {new Vector2(-96,94),new Vector2(-166,36),
                new Vector2(-219,-12),new Vector2(-280,-48)},
            new [] {new Vector2(146,118),new Vector2(197,62),
                new Vector2(252,25),new Vector2(334,-1)}
        };

        public static float RoadDistance(float x,float z)
        {
            // These are render-only paths. The previous low-amplitude
            // distortion left long almost straight radial lines; use broad,
            // continuous bends on the same established route network.
            // Identical mask drives terrain paint and forest clearances;
            // native march routes, WorldHotspots and save positions untouched.
            float curvatureX=16f*Mathf.Sin(z*.018f)+6f*Mathf.Sin(z*.061f);
            float curvatureZ=12f*Mathf.Sin(x*.013f)+4f*Mathf.Sin(x*.055f);
            Vector2 p=new Vector2(x+curvatureX,z+curvatureZ);
            float nearest=float.PositiveInfinity;
            for(int route=0;route<LandTracks.Length;route++)
            {
                var points=LandTracks[route];
                for(int i=0;i<points.Length-1;i++)
                {
                    Vector2 a=points[i],d=points[i+1];
                    if(p.x<Mathf.Min(a.x,d.x)-15f||
                       p.x>Mathf.Max(a.x,d.x)+15f||
                       p.y<Mathf.Min(a.y,d.y)-15f||
                       p.y>Mathf.Max(a.y,d.y)+15f)continue;
                    float distance=DistanceToSegment(p,a,d);
                    if(distance<nearest)nearest=distance;
                }
            }
            return nearest;
        }

        public static bool IsRoad(float x, float z)
        {
            return RoadDistance(x,z)<2.6f;
        }

        // Shared winding watershed for real landscape, hybrid painter and
        // A/B baseline. One water geography for all zooms, no new hotspots.
        public static float RiverCentre(float z)
        {
            return -104f + .19f*z + 49f*Mathf.Sin(z*.013f) +
                17f*Mathf.Sin(z*.034f);
        }

        public static bool IsRiver(float x, float z)
        {
            return Mathf.Abs(x - RiverCentre(z)) < 4f && z > -440f && z < 450f;
        }

        Texture2D MakeTerrainTexture(int size)
        {
            var image = new Texture2D(size, size, TextureFormat.RGB24, true, false);
            image.name = "Region1 continuous textured geography one shared space";
            var pixels = new Color32[size * size];
            Vector2 rift = new Vector2(BreachOffsetX, BreachOffsetZ);
            for (int z = 0; z < size; z++)
                for (int x = 0; x < size; x++)
                {
                    float worldX = ((x + .5f) / size * 2 - 1) * LandscapeHalfWidth;
                    float worldZ = ((z + .5f) / size * 2 - 1) * LandscapeHalfDepth;
                    // Smooth multiscale ground removes square pixel blocks.
                    float broad = Mathf.PerlinNoise(22f + worldX * .0065f, 55f + worldZ * .007f);
                    float mottling = Mathf.PerlinNoise(106f + worldX * .045f, 78f + worldZ * .05f);
                    float grain = Mathf.PerlinNoise(216f + worldX * .18f, 312f + worldZ * .17f);
                    float n = .46f * broad + .38f * mottling + .16f * grain;
                    Color c = Color.Lerp(Grass, DryGrass, Mathf.Clamp01(.10f + n * .85f));
                    // Colour continuity from the actual player-city biome to
                    // the surrounding region, without adding a separate map.
                    float radius = Mathf.Sqrt(worldX * worldX + worldZ * worldZ);
                    float meadowTransition = 1f - Mathf.SmoothStep(85f, 230f, radius);
                    c = Color.Lerp(c, new Color(.42f,.46f,.28f),
                        meadowTransition * .33f);
                    if (IsForest(worldX, worldZ))
                    {
                        float canopy = .42f + .20f * mottling;
                        c = Color.Lerp(c, DarkForest, canopy);
                        if (grain > .68f) c = Color.Lerp(c, Grass, .27f);
                    }
                    if (Mathf.Abs(worldX) > 430f && broad > .70f)
                        c = Color.Lerp(c, Rock, .12f + .12f * grain);
                    if (IsRoad(worldX, worldZ))
                        c = Color.Lerp(c, Road, .62f + .13f * grain);
                    float waterbank = .0f;
                    float riverX = RiverCentre(worldZ);
                    if (worldZ > -445f && worldZ < 455f)
                        waterbank = Mathf.Clamp01((5.4f - Mathf.Abs(worldX - riverX)) / 4f);
                    if (waterbank > 0) c = Color.Lerp(c, Water, waterbank * .91f);
                    if (IsCoast(worldX, worldZ))
                        c = Color.Lerp(Water, new Color(.25f,.48f,.61f), grain * .18f);
                    float d = Vector2.Distance(new Vector2(worldX, worldZ), rift);
                    if (d < 54f)
                    {
                        float strength = Mathf.Clamp01((54f - d) / 47f);
                        float cracks = Mathf.Abs(Mathf.Sin(
                            Mathf.Atan2(worldZ - rift.y, worldX - rift.x) * 12f + d * .035f));
                        c = Color.Lerp(c, Rift, strength * (.58f + .22f * cracks));
                        if (d < 10f) c = Color.Lerp(c, new Color(.60f,.18f,.68f),
                            Mathf.Clamp01((10f - d) / 8f) * .68f);
                    }
                    pixels[z * size + x] = c;
                }
            image.SetPixels32(pixels);
            image.Apply(true, true); // do not retain the CPU pixel array on mobile
            image.wrapMode = TextureWrapMode.Clamp;
            image.filterMode = FilterMode.Bilinear;
            return image;
        }

        void BuildBreach(Transform parent)
        {
            var fracture = new GameObject("Region1 · grounded geologic Breach (not a portal)").transform;
            fracture.SetParent(parent, true);
            riftMaterial = MakeMaterial("Region1 dark geological fracture", new Color(.24f, .13f, .25f), false);
            var points = new List<Vector3>();
            var indices = new List<int>();
            var centre = new Vector2(BreachOffsetX, BreachOffsetZ);
            // Thin branching cracks follow the terrain. No upright towers or enemy proxies.
            foreach (var line in RegionOneAuthoredComposition.Faults)
            for(int step=1;step<line.Length;step++)
            {
                Vector2 start=centre+line[step-1], end=centre+line[step];
                int pieces=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(start,end)/1.5f));
                for(int part=0;part<pieces;part++)
                {
                    Vector2 previous=Vector2.Lerp(start,end,part/(float)pieces);
                    Vector2 next=Vector2.Lerp(start,end,(part+1f)/pieces);
                    if(!RegionOneAuthoredComposition.FaultAllowed(previous.x,previous.y) ||
                       !RegionOneAuthoredComposition.FaultAllowed(next.x,next.y))continue;
                    float halfWidth=.40f*Mathf.Lerp(1f,.35f,
                        Mathf.Clamp01(Vector2.Distance(next,centre)/85f));
                    halfWidth*=Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(48f,65f,next.magnitude));
                    if(halfWidth<.005f)continue;
                    Vector2 perpendicular=(next-previous).normalized;
                    perpendicular=new Vector2(-perpendicular.y,perpendicular.x)*halfWidth;
                    int index=points.Count;
                    foreach(Vector2 location in new[]{previous-perpendicular,previous+perpendicular,
                                                       next-perpendicular,next+perpendicular})
                    {
                        float h=Mathf.Max(SampleHeight(location.x,location.y)+.20f,.19f);
                        points.Add(new Vector3(origin.x+location.x,h,origin.z+location.y));
                    }
                    indices.Add(index);indices.Add(index+1);indices.Add(index+2);
                    indices.Add(index+1);indices.Add(index+3);indices.Add(index+2);
                }
            }
            var mesh = new Mesh { name = "Region1 surface fracture only", vertices = points.ToArray(), triangles = indices.ToArray() };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            generatedMeshes.Add(mesh);
            var go = new GameObject("Region1 · fracture mesh · no collider");
            go.transform.SetParent(fracture, true);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = riftMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            // A low, luminous scar visible from the high-altitude strategy
            // camera, not an upright portal or selectable enemy.
            var core = new Mesh { name = "Region1 · Breach surface core" };
            var coreVertices = new Vector3[25];
            var coreIndices = new int[24 * 3];
            float coreY = Mathf.Max(SampleHeight(BreachOffsetX,BreachOffsetZ)+.23f,.22f);
            coreVertices[0] = new Vector3(origin.x+BreachOffsetX, coreY, origin.z+BreachOffsetZ);
            for (int i=0; i<24; i++)
            {
                float a=i*Mathf.PI/12f;
                float radius=3.0f+.9f*Mathf.Sin(i*5.6f);
                coreVertices[i+1]=new Vector3(origin.x+BreachOffsetX+Mathf.Cos(a)*radius,
                    coreY-.025f, origin.z+BreachOffsetZ+Mathf.Sin(a)*radius);
                coreIndices[i*3]=0;
                coreIndices[i*3+1]=1+(i+1)%24;
                coreIndices[i*3+2]=i+1;
            }
            core.vertices=coreVertices;
            core.triangles=coreIndices;
            core.RecalculateBounds();
            core.RecalculateNormals();
            generatedMeshes.Add(core);
            var coreGo = new GameObject("Region1 · horizontal Breach luminous fissure heart");
            coreGo.transform.SetParent(fracture,true);
            coreGo.AddComponent<MeshFilter>().sharedMesh=core;
            breachCoreMaterial=MakeMaterial("Region1 grounded bright fissure",
                new Color(.72f,.23f,.70f),true);
            var coreRenderer=coreGo.AddComponent<MeshRenderer>();
            coreRenderer.sharedMaterial=breachCoreMaterial;
            coreRenderer.shadowCastingMode=ShadowCastingMode.Off;
            BuildBreachHalo(fracture);
        }

        void BuildBreachHalo(Transform fracture)
        {
            // Low-opacity geometry sits in the ground plane: one shared rift,
            // not an elevated portal and without terrain/target colliders.
            var shader=Shader.Find("Sprites/Default");
            if(!shader) return;
            breachHaloMaterial=new Material(shader){name="Region1 ground-level Rift feathered scar"};
            breachHaloMaterial.renderQueue=3000;
            const int sectors=36;
            float[] radii={4.5f,10f,18f,29f};
            float[] opacity={.38f,.25f,.08f,0f};
            var vertices=new List<Vector3>();
            var colors=new List<Color>();
            var triangles=new List<int>();
            var originPoint=new Vector3(origin.x+BreachOffsetX,
                Mathf.Max(SampleHeight(BreachOffsetX,BreachOffsetZ)+.22f,.18f),
                origin.z+BreachOffsetZ);
            for(int layer=0;layer<radii.Length;layer++)
            {
                for(int i=0;i<sectors;i++)
                {
                    float a=i*2f*Mathf.PI/sectors;
                    float rough=1f+.11f*Mathf.Sin(i*1.83f+layer*.41f)+
                                    .07f*Mathf.Sin(i*.69f-layer*1.31f);
                    vertices.Add(originPoint+new Vector3(Mathf.Cos(a)*radii[layer]*rough,
                        -.055f,Mathf.Sin(a)*radii[layer]*rough));
                    colors.Add(new Color(.24f,.19f,.22f,opacity[layer]));
                    if(layer==0)continue;
                    int a0=(layer-1)*sectors+i,a1=(layer-1)*sectors+(i+1)%sectors;
                    int b0=layer*sectors+i,b1=layer*sectors+(i+1)%sectors;
                    triangles.Add(a0);triangles.Add(a1);triangles.Add(b0);
                    triangles.Add(a1);triangles.Add(b1);triangles.Add(b0);
                }
            }
            var mesh=new Mesh{name="Region1 irregular in-soil Breach halo",
                vertices=vertices.ToArray(),colors=colors.ToArray(),
                triangles=triangles.ToArray()};
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            generatedMeshes.Add(mesh);
            var go=new GameObject("Region1 geological fractured earth alpha / no collider");
            go.transform.SetParent(fracture,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=breachHaloMaterial;
            renderer.receiveShadows=false;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
        }

        void BuildTreeDetail(Transform parent, Transform original)
        {
            detailTrees = new GameObject("Region1 · reusable midrange forest silhouettes");
            detailTrees.transform.SetParent(parent, true);
            // Clone ONLY shared tree meshes/materials from existing art. No scene prefabs,
            // colliders, selection or save system are duplicated.
            var donor = original.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f =>
                f.sharedMesh != null && f.sharedMesh.vertexCount < 3500 &&
                f.GetComponent<MeshRenderer>() != null &&
                (f.name.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 f.name.IndexOf("pine", StringComparison.OrdinalIgnoreCase) >= 0));
            if (!donor) return;
            var oldRenderer = donor.GetComponent<MeshRenderer>();
            for (int i = 0; i < ForestCentres.Length + ForestCorridorPairs.Length/2; i++)
                for (int j = 0; j < 5; j++)
                {
                    float theta = (j * 2.39f + i) * Mathf.PI;
                    Vector2 centre;
                    if(i<ForestCentres.Length)centre=ForestCentres[i];
                    else
                    {
                        int link=(i-ForestCentres.Length)*2;
                        centre=(ForestCentres[ForestCorridorPairs[link]]+
                                ForestCentres[ForestCorridorPairs[link+1]])*.5f;
                    }
                    float x = centre.x + Mathf.Cos(theta) * (11f + j * 6f);
                    float z = centre.y + Mathf.Sin(theta) * (9f + j * 4f);
                    var tree = new GameObject("Region1 · reused-tree " + i + "-" + j);
                    tree.transform.SetParent(detailTrees.transform, true);
                    tree.transform.rotation = Quaternion.Euler(0, (i * 67 + j * 53) % 360, 0);
                    // Real Unity renderer survey showed 70m-wide, 39m-tall
                    // borrowed tree meshes in regional view: the old .08 minimum
                    // scale was much too large for the donor's source units.
                    // Bound BOTH height and footprint; do not touch the shared
                    // donor mesh, colliders, POIs, or any gameplay transform.
                    var sourceBounds = donor.sharedMesh.bounds;
                    float targetHeight = 2.2f + .4f * ((i + j) % 4);
                    float originalHeight = Mathf.Max(.01f, sourceBounds.size.y);
                    float originalFootprint = Mathf.Max(.01f,
                        Mathf.Max(sourceBounds.size.x,sourceBounds.size.z));
                    float scale = Mathf.Min(targetHeight/originalHeight,
                        (targetHeight*1.10f)/originalFootprint);
                    scale = Mathf.Clamp(scale,.0001f,3.5f);
                    tree.transform.localScale = Vector3.one*scale;
                    // Put the actual donor mesh base at ground height even if
                    // its geometry has a large, off-centre legacy pivot.
                    Vector3 sourceBase = new Vector3(sourceBounds.center.x,
                        sourceBounds.min.y,sourceBounds.center.z);
                    var ground = new Vector3(origin.x+x,SampleHeight(x,z)+.2f,origin.z+z);
                    tree.transform.position = ground-tree.transform.rotation*(sourceBase*scale);
                    tree.AddComponent<MeshFilter>().sharedMesh = donor.sharedMesh;
                    var meshRenderer = tree.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterials = oldRenderer.sharedMaterials;
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    meshRenderer.receiveShadows = false;
                }
        }

        readonly List<Material> farForestMaterials = new List<Material>();
        readonly List<Material> farGeologyMaterials = new List<Material>();

        void BuildDistantForests(Transform parent)
        {
            // Controlled 2.5D alternative to primitive dots / tiny cones:
            // three forest draw batches, one reusable alpha texture, no new
            // physics, node IDs or mobile per-frame billboard work.
            var hybrid=RegionOneHybridForestImpostors.Build(
                parent,origin,ForestCentres,SampleHeight,IsForest,
                RegionOneHybridTerrainArt.IsOpenWater,IsRoad,IsCoast);
            if(hybrid!=null && hybrid.trees>0)
            {
                distantForestSilhouettes=hybrid.root;
                hybridForestAtlas=hybrid.atlas;
                farForestMaterials.Add(hybrid.material);
                generatedMeshes.AddRange(hybrid.meshes);
                return;
            }
            // Keep the earlier cheap 3D batches as a safe fallback if
            // the sprite shader is not supported on this URP platform.
            distantForestSilhouettes = new GameObject("Region1 · batched distant organic forest canopies");
            distantForestSilhouettes.transform.SetParent(parent, true);
            // These are distant-scale tree silhouettes, never gameplay entities.
            // A handful of low-poly canopy batches keeps draw calls predictable,
            // unlike thousands of individual forest prefab renderers on iPhone.
            for (int band = 0; band < 3; band++)
            {
                var positions = new List<Vector3>();
                var indices = new List<int>();
                for (int cluster = band; cluster < ForestCentres.Length; cluster += 3)
                {
                    Vector2 centre = ForestCentres[cluster];
                    const int count = 80;
                    for (int j = 0; j < count; j++)
                    {
                        // Low-discrepancy placement, then reject candidates
                        // beyond the same organic woodland outline used by
                        // the terrain shader. Geometry stays 3 renderer batches.
                        float radial = Mathf.Sqrt((j + .7f) / count);
                        float theta = j * 2.399963f + cluster * 1.37f;
                        float radiusX = 41f + (cluster % 4) * 8.4f;
                        float radiusZ = 32f + (cluster % 3) * 7.3f;
                        float x = centre.x + Mathf.Cos(theta) * radiusX * radial;
                        float z = centre.y + Mathf.Sin(theta) * radiusZ * radial;
                        if (!IsForest(x,z) || Mathf.Sqrt(x * x + z * z) < 95f ||
                            RegionOneHybridTerrainArt.IsOpenWater(x,z) ||
                            IsRoad(x,z) || IsCoast(x,z)) continue;
                        float y = SampleHeight(x, z) + .1f;
                        float width = 1.35f + Noise(cluster * 31, j * 19) * 2.05f;
                        float height = 3.8f + Noise(j * 11, cluster * 43) * 3.8f;
                        // Triangular tapering spruce crown, with two independent
                        // fronds and no upright surface monument.
                        int begin = positions.Count;
                        positions.Add(new Vector3(origin.x+x, y+height, origin.z+z));
                        for (int k = 0; k < 6; k++)
                        {
                            float angle = k * Mathf.PI / 3f + .2f * cluster;
                            float radius = width * (k % 2 == 0 ? 1f : .80f);
                            positions.Add(new Vector3(origin.x+x + Mathf.Cos(angle)*radius,
                                y+1.05f, origin.z+z + Mathf.Sin(angle)*radius));
                        }
                        for (int k = 0; k < 6; k++)
                        {
                            indices.Add(begin); indices.Add(begin + 1 + (k + 1) % 6);
                            indices.Add(begin + 1 + k);
                        }
                    }
                }
                if (positions.Count == 0) continue;
                var mesh = new Mesh { name = "Region1 · distant spruce grove batch " + band,
                    vertices = positions.ToArray(), triangles = indices.ToArray() };
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                generatedMeshes.Add(mesh);
                var forest = new GameObject("Region1 · strategic tree batch " + band);
                forest.transform.SetParent(distantForestSilhouettes.transform, true);
                forest.AddComponent<MeshFilter>().sharedMesh = mesh;
                var material = MakeMaterial("Region1 strategic spruce " + band,
                    band == 0 ? new Color(.15f,.31f,.16f) :
                    band == 1 ? new Color(.22f,.36f,.17f) : new Color(.29f,.41f,.21f), false);
                farForestMaterials.Add(material);
                var renderer = forest.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.receiveShadows = false;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            distantForestSilhouettes.SetActive(false);
        }

        void BuildDistantGeology(Transform parent)
        {
            distantGeology = new GameObject("Region1 · four low-cost mountain outcrops");
            distantGeology.transform.SetParent(parent, true);
            // Few 3D peaks, batched by light-facing side. Never add new enemy,
            // city, resource hotspot, colliders or expensive mountain LODs.
            var vertices = new List<Vector3>();
            var lightTris = new List<int>();
            var shadeTris = new List<int>();
            var sites = new [] {
                new Vector2(-330f, 190f), new Vector2(326f,215f),
                new Vector2(-320f,-220f), new Vector2(267f,-170f)
            };
            for (int i=0;i<sites.Length;i++)
                for (int peak=0;peak<3;peak++)
                {
                    float theta=peak*2.399963f+i*.74f;
                    float off=peak==0?0f:14f+peak*2f;
                    float x=sites[i].x+Mathf.Cos(theta)*off;
                    float z=sites[i].y+Mathf.Sin(theta)*off;
                    float y=SampleHeight(x,z)+.2f;
                    float width=8.5f+peak*3.8f+(i%2)*1.3f;
                    float height=9f+peak*3.3f;
                    int n=vertices.Count;
                    vertices.Add(new Vector3(origin.x+x,y+height,origin.z+z));
                    vertices.Add(new Vector3(origin.x+x-width,y,origin.z+z-width*.53f));
                    vertices.Add(new Vector3(origin.x+x+width,y,origin.z+z-width*.61f));
                    vertices.Add(new Vector3(origin.x+x+width*.85f,y,origin.z+z+width));
                    vertices.Add(new Vector3(origin.x+x-width*.58f,y,origin.z+z+width*.83f));
                    lightTris.Add(n);lightTris.Add(n+2);lightTris.Add(n+1);
                    lightTris.Add(n);lightTris.Add(n+3);lightTris.Add(n+2);
                    shadeTris.Add(n);shadeTris.Add(n+4);shadeTris.Add(n+3);
                    shadeTris.Add(n);shadeTris.Add(n+1);shadeTris.Add(n+4);
                }
            for (int batch=0;batch<2;batch++)
            {
                var mesh=new Mesh{name="Region1 tiny ridges " + batch,
                    vertices=vertices.ToArray(),
                    triangles=(batch==0?lightTris:shadeTris).ToArray()};
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                generatedMeshes.Add(mesh);
                var obj=new GameObject("Region1 · mountain facets " + batch);
                obj.transform.SetParent(distantGeology.transform,true);
                obj.AddComponent<MeshFilter>().sharedMesh=mesh;
                var material=MakeMaterial("Region1 restrained mountain " + batch,
                    batch==0?new Color(.59f,.54f,.40f):new Color(.41f,.41f,.34f),false);
                farGeologyMaterials.Add(material);
                var renderer=obj.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=material;
                renderer.receiveShadows=false;
                renderer.shadowCastingMode=ShadowCastingMode.Off;
            }
            distantGeology.SetActive(false);
        }

        void BuildEdgeFog(Transform parent)
        {
            peripheralFog = new GameObject("Region1 · panoramic border haze only");
            peripheralFog.transform.SetParent(parent, true);
            Shader shader = Shader.Find("Sprites/Default");
            if (!shader) { peripheralFog.SetActive(false); return; }
            fogMaterial = new Material(shader) { name = "Region1 peripheral alpha no-volume" };
            fogMaterial.renderQueue = 3000;
            AddFogStrip(peripheralFog.transform, -1, 0);
            AddFogStrip(peripheralFog.transform, 1, 0);
            AddFogStrip(peripheralFog.transform, 0, -1);
            AddFogStrip(peripheralFog.transform, 0, 1);
        }

        void AddFogStrip(Transform parent, int sx, int sz)
        {
            const float westEast = 373f, northSouth = 222f, thickness = 84f;
            var verts = new List<Vector3>();
            var colors = new List<Color>();
            var indices = new List<int>();
            Color baseColor = new Color(.81f, .86f, .88f, 1);
            for (int step = 0; step <= 8; step++)
            {
                float value = step / 8f;
                float distance = (sx != 0 ? westEast : northSouth) + value * thickness;
                float x = sx != 0 ? sx * distance : -LandscapeHalfWidth;
                float z = sz != 0 ? sz * distance : -LandscapeHalfDepth;
                float alpha = .22f * value * value;
                if (sx != 0)
                {
                    verts.Add(new Vector3(origin.x + x, -1f, origin.z - LandscapeHalfDepth));
                    verts.Add(new Vector3(origin.x + x, -1f, origin.z + LandscapeHalfDepth));
                }
                else
                {
                    verts.Add(new Vector3(origin.x - LandscapeHalfWidth, -1f, origin.z + z));
                    verts.Add(new Vector3(origin.x + LandscapeHalfWidth, -1f, origin.z + z));
                }
                colors.Add(new Color(baseColor.r,baseColor.g,baseColor.b,alpha));
                colors.Add(new Color(baseColor.r,baseColor.g,baseColor.b,alpha));
                if (step == 0) continue;
                int a = (step - 1) * 2, b = step * 2;
                indices.Add(a); indices.Add(b); indices.Add(a+1);
                indices.Add(a+1); indices.Add(b); indices.Add(b+1);
            }
            var mesh = new Mesh { name = "Region1 border haze strip", vertices = verts.ToArray(),
                                  colors = colors.ToArray(), triangles = indices.ToArray() };
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            var go = new GameObject("Region1 · haze edge " + sx + " " + sz);
            go.transform.SetParent(parent,true);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = fogMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        static Material MakeMaterial(string name, Color color, bool emissive)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Unlit/Texture") ??
                         Shader.Find("Standard");
            var material = new Material(shader) { name = name, enableInstancing = true };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color);
            }
            return material;
        }

        /// <summary>
        /// Switches only the painted diffuse surface in the SAME genuine
        /// integrated Unity game camera, for before/after art comparison.
        /// No gameplay state, camera poses, meshes, saves or UI are mutated.
        /// Legacy texture is lazy and only built for opt-in capture/testing.
        /// </summary>
        public void CompareAtlas(bool baseline)
        {
            if (!surfaceMaterial || !terrainTexture) return;
            if (baseline && !legacyCompareTexture)
                legacyCompareTexture=MakeTerrainTexture(1024);
            var selected=baseline ? legacyCompareTexture : terrainTexture;
            if (surfaceMaterial.HasProperty("_BaseMap"))
                surfaceMaterial.SetTexture("_BaseMap",selected);
            if (surfaceMaterial.HasProperty("_MainTex"))
                surfaceMaterial.SetTexture("_MainTex",selected);
        }

        // Explicitly synchronize representations before a manually rendered Unity capture.
        public void RefreshForCapture()
        {
            if (built) UpdateLevelOfDetail(true);
        }

#if UNITY_EDITOR
        [Serializable]
        sealed class AuthoringSnapshot
        {
            public Vector3 origin;
            public Vector3[] vertices;
            public Vector2[] uv;
            public int[] triangles;
            public bool[] protectedWater;
            public string context = "Actual integrated Unity terrain; no gameplay or save snapshot";
        }

        // Export exact existing geometry for the established Qwen/Blender
        // author. This is opt-in CI evidence, never player runtime I/O.
        public void ExportAuthoringSnapshot(string directory)
        {
            if (!built || !terrainMesh || !terrainTexture)
                throw new InvalidOperationException("Real terrain must exist before authoring export.");
            string file = System.IO.Path.Combine(directory, "terrain-source.json");
            if (System.IO.File.Exists(file))
                throw new InvalidOperationException("Do not overwrite frozen authoring evidence.");
            System.IO.Directory.CreateDirectory(directory);
            var positions=terrainMesh.vertices;
            var water=new bool[positions.Length];
            for(int i=0;i<positions.Length;i++)
                water[i]=RegionOneHybridTerrainArt.IsOpenWater(positions[i].x-origin.x,
                    positions[i].z-origin.z) || IsCoast(positions[i].x-origin.x,positions[i].z-origin.z);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"terrain-atlas.png"),
                terrainTexture.EncodeToPNG());
            System.IO.File.WriteAllText(file, JsonUtility.ToJson(new AuthoringSnapshot {
                origin=origin, vertices=terrainMesh.vertices,
                uv=terrainMesh.uv, triangles=terrainMesh.triangles, protectedWater=water
            }));
        }
#endif

        void LateUpdate()
        {
            if (built) UpdateLevelOfDetail(false);
        }

        void UpdateLevelOfDetail(bool force)
        {
            if (!navigation || !navigation.View) return;
            // Retain complete horizontal panorama and show almost twice as
            // much of Region 1 in portrait. Orthographic width is
            // 2*zoom*aspect; the old 244 yielded only ~225 of 850 world units.
            // Keep all real world coordinates, touch hit tests and POIs intact.
            float wantedPanorama = navigation.View.aspect < .80f
                ? PortraitPanoramaZoom : LandscapePanoramaZoom;
            if (Mathf.Abs(navigation.globalZoom - wantedPanorama) > .5f)
            {
                bool wasPanoramic = navigation.View.orthographicSize >= 190f;
                navigation.globalZoom = wantedPanorama;
                // On live orientation changes only global view re-frames;
                // regional/near zoom and player pan/selection stay untouched.
                if (wasPanoramic) navigation.SetZoomLevel(0, true);
            }
            float size = navigation.View.orthographicSize;
            bool distant = size >= RegionalThreshold;
            bool modeChanged = distant != farMode;
            // Woodland has a different LOD boundary from gameplay markers.
            // The regional camera is 113, below the 152 marker threshold:
            // coupling both hid every batched canopy in the regional view.
            // Blend the SAME forest geography into view as local ground ends.
            float woodlandAlpha = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(navigation.localZoom, LocalThreshold + 14f, size));
            bool showWoodland = woodlandAlpha > .001f;
            if (force || showWoodland != woodlandMode)
            {
                woodlandMode = showWoodland;
                if (distantForestSilhouettes) distantForestSilhouettes.SetActive(showWoodland);
            }
            foreach (var material in farForestMaterials)
            {
                if (!material || !material.HasProperty("_Color")) continue;
                Color tint = material.GetColor("_Color");
                if (Mathf.Abs(tint.a - woodlandAlpha) < .001f) continue;
                tint.a = woodlandAlpha;
                material.SetColor("_Color", tint);
            }
            bool hideOldSquare = size >= LocalThreshold;
            // Border strips are authored near the complete region's edge.
            // They would cross the middle of the new portrait overview as
            // two straight pale seams. Only show them when the horizontal
            // panorama actually reaches the outer geographic margins.
            bool showFog = size >= 190f && host.InWorld &&
                navigation.View.aspect >= .80f;
            // Progressive pitch: local remains readable, panoramas become
            // more top-down. The same stable geographical XYZ is retained.
            float desiredTilt = Mathf.Lerp(56f, 74f,
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(17f, 244f, size)));
            float nextTilt = force ? desiredTilt : Mathf.Lerp(navigation.tilt,
                desiredTilt, 1f - Mathf.Exp(-7f * Time.unscaledDeltaTime));
            if (Mathf.Abs(nextTilt - navigation.tilt) > .04f)
            {
                navigation.tilt = nextTilt;
                navigation.FitToAuthoredBounds();
            }
            if (modeChanged)
            {
                // At panoramic scale every tiny existing 3D resource/city/
                // enemy representation is omitted *visually*, as the owner
                // approved. Preserve the state of renderers for the return.
                for (int i=0; i<existingGameRenderers.Count; i++)
                {
                    if (!existingGameRenderers[i]) continue;
                    if (distant)
                    {
                        existingGameRendererStates[i] = existingGameRenderers[i].enabled;
                        existingGameRenderers[i].enabled = false;
                    }
                    else existingGameRenderers[i].enabled = existingGameRendererStates[i];
                }
            }
            if (force || modeChanged)
            {
                farMode = distant;
                if (detailTrees) detailTrees.SetActive(!distant);
                if (distantGeology) distantGeology.SetActive(distant);
            }
            // Reassert AFTER gameplay renderers are restored on panorama exit.
            // Only visuals are affected; never disable colliders or hotspots.
            if (force || modeChanged || hideOldSquare != legacyGroundHidden)
            {
                legacyGroundHidden = hideOldSquare;
                for (int i = 0; i < originalGround.Count; i++)
                    if (originalGround[i])
                        originalGround[i].enabled = !hideOldSquare && originalGroundInitiallyEnabled[i];
            }
            if (force || showFog != fogMode)
            {
                fogMode = showFog;
                if (peripheralFog) peripheralFog.SetActive(showFog);
            }
        }

        void OnDestroy()
        {
            for (int i=0;i<hiddenProvisionalCamps.Count;i++)
                if (hiddenProvisionalCamps[i])
                    hiddenProvisionalCamps[i].enabled=hiddenProvisionalCampsOriginalStates[i];
            if (farMode)
                for (int i=0; i<existingGameRenderers.Count; i++)
                    if (existingGameRenderers[i])
                        existingGameRenderers[i].enabled = existingGameRendererStates[i];
            for (int i = 0; i < originalGround.Count; i++)
                if (originalGround[i]) originalGround[i].enabled = originalGroundInitiallyEnabled[i];
            foreach (var mesh in generatedMeshes) if (mesh) Destroy(mesh);
            if (terrainTexture) Destroy(terrainTexture);
            if (legacyCompareTexture) Destroy(legacyCompareTexture);
            if (hybridForestAtlas) Destroy(hybridForestAtlas);
            if (surfaceMaterial) Destroy(surfaceMaterial);
            if (riftMaterial) Destroy(riftMaterial);
            if (breachCoreMaterial) Destroy(breachCoreMaterial);
            if (breachHaloMaterial) Destroy(breachHaloMaterial);
            if (fogMaterial) Destroy(fogMaterial);
            foreach (var mat in farForestMaterials) if (mat) Destroy(mat);
            foreach (var mat in farGeologyMaterials) if (mat) Destroy(mat);
        }
    }
}
