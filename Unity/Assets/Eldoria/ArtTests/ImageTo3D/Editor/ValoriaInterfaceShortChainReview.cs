using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Physical no-cut control. A genuinely safe IF1 needs to beat this route gate.
    public static class ValoriaInterfaceShortChainReview
    {
        const string Base = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Source = Base + "Source/";
        const string ScenePath = Base + "ValoriaInterfaceShortChainReview.unity";
        const string Output = "ValoriaInterfaceShortChainCaptures";

        [Serializable] class Sample
        {
            public string name;
            public Vector3 position;
            public float expectedFloor, nearestSurface, floorError;
            public int verticalHits;
            public bool floorWithinTolerance, clearance2p4;
        }
        [Serializable] class Metrics
        {
            public string scene = ScenePath;
            public string[] sources;
            public int[] sourceTriangles, sourceVertices;
            public bool[] uv0, normals, sourceMaterial, meshHit;
            public Vector3[] sourceBounds;
            public int colliders;
            public float gateStreetSocketError, streetTerraceSocketError;
            public Vector3 bounds;
            public Sample[] gateExitSamples;
            public bool[] capturesNonEmpty = new bool[4], backgroundMiss = new bool[4];
            public string interfaceVerdict = "FAIL: no physical IF1 applied; gate exit requires unsafe building cut or new support floor";
        }

        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            string[] sources = {
                Source + "Eldoria_Module_GateStreetRiseRock_50K.glb",
                Source + "Eldoria_Module_StreetLandingTransition_50K.glb",
                Source + "Eldoria_Module_TerraceStairRock_50K.glb"
            };
            GameObject[] prefabs = new GameObject[3];
            var r = new Metrics {
                sources = sources, sourceTriangles = new int[3], sourceVertices = new int[3],
                uv0 = new bool[3], normals = new bool[3], sourceMaterial = new bool[3],
                sourceBounds = new Vector3[3], meshHit = new bool[3]
            };
            for (int i = 0; i < 3; i++) {
                if (!File.Exists(sources[i])) throw new Exception("Missing exact certified source " + sources[i]);
                AssetDatabase.ImportAsset(sources[i], ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(sources[i]);
                if (!prefabs[i]) throw new Exception("GLB import failed " + sources[i]);
                var meshes = prefabs[i].GetComponentsInChildren<MeshFilter>();
                if (meshes.Length != 1 || !meshes[0].sharedMesh) throw new Exception("Unexpected mesh count " + sources[i]);
                var m = meshes[0].sharedMesh;
                r.sourceTriangles[i] = m.triangles.Length / 3;
                r.sourceVertices[i] = m.vertexCount;
                r.uv0[i] = m.uv.Length == m.vertexCount;
                r.normals[i] = m.normals.Length == m.vertexCount;
                r.sourceBounds[i] = Bounds(prefabs[i]).size;
                r.sourceMaterial[i] = prefabs[i].GetComponentsInChildren<Renderer>()
                    .Any(renderer => renderer.sharedMaterials.Any(material => material != null));
                if (r.sourceTriangles[i] < 49500 || r.sourceTriangles[i] > 50000 ||
                    !r.uv0[i] || !r.normals[i] || !r.sourceMaterial[i])
                    throw new Exception("Source attribute gate failed " + sources[i]);
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Gate → Street → Terrace | uncut physical control");
            var gate = Place(prefabs[0], root.transform, "01 Gate", new Vector3(0, 0, -11), 15f);
            // These positions use the independently measured source floors, not guessed rock bottoms.
            var gateOut = Point(gate, 0, .285f, -.25f);
            var street = Place(prefabs[1], root.transform, "02 Street", Vector3.zero, 8.2f);
            Align(street, Point(street, 0, .091f, .35f), gateOut + Vector3.forward * .25f);
            var streetOut = Point(street, 0, .178f, -.35f);
            var terrace = Place(prefabs[2], root.transform, "03 Terrace", Vector3.zero, 11.3f);
            Align(terrace, Point(terrace, -.055f, .028f, .35f), streetOut + Vector3.forward * .20f);
            r.gateStreetSocketError = Vector3.Distance(Point(street, 0, .091f, .35f), gateOut + Vector3.forward * .25f);
            r.streetTerraceSocketError = Vector3.Distance(Point(terrace, -.055f, .028f, .35f), streetOut + Vector3.forward * .20f);
            Physics.SyncTransforms();
            r.colliders = root.GetComponentsInChildren<MeshCollider>().Length;
            for (int i = 0; i < 3; i++) {
                var go = root.transform.GetChild(i);
                var collider = go.GetComponentInChildren<MeshCollider>();
                var bounds = collider.bounds;
                for (int gx = -2; gx <= 2 && !r.meshHit[i]; gx++)
                    for (int gz = -2; gz <= 2 && !r.meshHit[i]; gz++)
                        r.meshHit[i] = collider.Raycast(new Ray(
                            bounds.center + new Vector3(gx * bounds.extents.x * .3f,
                                bounds.extents.y * 2, gz * bounds.extents.z * .3f),
                            Vector3.down), out var hit, bounds.size.y * 3);
            }
            if (r.colliders != 3 || r.meshHit.Any(x => !x)) throw new Exception("Mesh selection failed");
            var scale = gate.transform.lossyScale.x;
            // At the upper plaza the center floor is Y=.290 at Z=-.25. At Z=-.30 it disappears.
            float[] sourceZ = { -.22f, -.24f, -.25f, -.26f, -.28f, -.30f, -.32f };
            r.gateExitSamples = new Sample[sourceZ.Length];
            var expectedFloor = Point(gate, 0, .290f, -.25f).y;
            for (int i = 0; i < sourceZ.Length; i++) {
                var world = Point(gate, 0, .29f, sourceZ[i]);
                var hitSet = Physics.RaycastAll(new Ray(world + Vector3.up * 8, Vector3.down), 16)
                    .Where(x => x.collider.transform.IsChildOf(gate.transform))
                    .OrderBy(x => Math.Abs(x.point.y - expectedFloor)).ToArray();
                var sample = new Sample { name = "gate source Z=" + sourceZ[i].ToString("F2"),
                    position = world, expectedFloor = expectedFloor, verticalHits = hitSet.Length,
                    nearestSurface = hitSet.Length > 0 ? hitSet[0].point.y : -999f };
                sample.floorError = hitSet.Length > 0 ? Math.Abs(sample.nearestSurface - expectedFloor) : 999f;
                sample.floorWithinTolerance = sample.floorError <= .25f;
                sample.clearance2p4 = sample.floorWithinTolerance &&
                    !Physics.Raycast(new Ray(world + Vector3.up * .08f, Vector3.up), 2.4f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                r.gateExitSamples[i] = sample;
            }
            if (scale < 10) throw new Exception("Unexpected scale convention");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Neutral review ground";
            ground.transform.position = new Vector3(0, -.15f, 0);
            ground.transform.localScale = new Vector3(5, 1, 5);
            var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(.36f, .35f, .33f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMat;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(.52f, .51f, .48f);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>()) {
                var list = renderer.sharedMaterials;
                for (int j = 0; j < list.Length; j++) list[j] = mat;
                renderer.sharedMaterials = list;
            }
            var light = new GameObject("Review sun").AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = .72f;
            light.transform.rotation = Quaternion.Euler(48, -42, 0);
            RenderSettings.ambientLight = new Color(.28f, .30f, .32f);
            var boundsAll = Bounds(root); r.bounds = boundsAll.size;
            var cam = new GameObject("Official zoom camera").AddComponent<Camera>();
            cam.orthographic = true; cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.64f, .71f, .77f);
            Directory.CreateDirectory(Output);
            string[] names = { "strategic", "city", "detail", "oblique" };
            float[] sizes = { 19, 12, 9, 16 };
            for (int i = 0; i < 4; i++) {
                var target = boundsAll.center + (i == 2 ? new Vector3(0, -.3f, 1.0f) : Vector3.up * .3f);
                cam.orthographicSize = sizes[i];
                cam.transform.position = target + (i == 3 ? new Vector3(-25, 20, -26) : new Vector3(15, 24, -26));
                cam.transform.LookAt(target);
                r.capturesNonEmpty[i] = Save(cam, Output + "/" + names[i] + ".png");
                r.backgroundMiss[i] = !Physics.Raycast(cam.ScreenPointToRay(new Vector3(3, 716)), out var hit, 150) ||
                    !hit.transform.IsChildOf(root.transform);
                if (!r.capturesNonEmpty[i] || !r.backgroundMiss[i]) throw new Exception("Capture gate failed " + names[i]);
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            File.WriteAllText(Output + "/metrics.json", JsonUtility.ToJson(r, true));
            Debug.Log("Uncut short-chain technical control captured. Interface verdict requires physical and visual review.");
        }

        static GameObject Place(GameObject prefab, Transform parent, string name, Vector3 at, float width)
        {
            var go = UnityEngine.Object.Instantiate(prefab, parent); go.name = name;
            var b = Bounds(go); go.transform.localScale *= width / Mathf.Max(b.size.x, b.size.z);
            go.transform.rotation = Quaternion.Euler(0, 180, 0);
            b = Bounds(go);
            go.transform.position += at - new Vector3(b.center.x, b.min.y, b.center.z);
            foreach (var f in go.GetComponentsInChildren<MeshFilter>()) {
                var collider = f.gameObject.AddComponent<MeshCollider>(); collider.sharedMesh = f.sharedMesh;
            }
            return go;
        }
        static Vector3 Point(GameObject go, float x, float y, float z) =>
            go.transform.TransformPoint(new Vector3(x, y, z));
        static void Align(GameObject go, Vector3 from, Vector3 to) { go.transform.position += to - from; }
        static Bounds Bounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }
        static bool Save(Camera camera, string path)
        {
            var rt = new RenderTexture(1280, 720, 24);
            var previous = RenderTexture.active;
            try {
                camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                var pixels = image.GetPixels32(); int min = 255, max = 0;
                for (int i = 0; i < pixels.Length; i += 257) {
                    var lum = (pixels[i].r + pixels[i].g + pixels[i].b) / 3;
                    min = Mathf.Min(min, lum); max = Mathf.Max(max, lum);
                }
                var bytes = image.EncodeToPNG(); File.WriteAllBytes(path, bytes);
                UnityEngine.Object.DestroyImmediate(image);
                return max - min >= 8 && bytes.Length >= 10000;
            } finally {
                camera.targetTexture = null; RenderTexture.active = previous;
                rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
