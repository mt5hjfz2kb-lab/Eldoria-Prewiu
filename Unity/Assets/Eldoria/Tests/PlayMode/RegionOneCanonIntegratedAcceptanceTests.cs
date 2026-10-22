using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Eldoria.Tests
{
    /// <summary>
    /// End-to-end structural assertions in the playable integrated scene.
    /// No save writes, constructed fake targets or forced gameplay unlocks.
    /// This is not visual-approval or mobile hardware evidence.
    /// </summary>
    public sealed class RegionOneCanonIntegratedAcceptanceTests
    {
        static object Member(object component, string name)
        {
            Assert.That(component, Is.Not.Null);
            var t = component.GetType();
            var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null) return field.GetValue(component);
            var property = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "Missing integrated member: " + name);
            return property.GetValue(component);
        }

        static object Call(object component, string method, params object[] args)
        {
            var m = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => x.Name == method && x.GetParameters().Length == args.Length);
            Assert.That(m, Is.Not.Null, "Missing strategy camera API: " + method);
            return m.Invoke(component, args);
        }

        static IEnumerator Boot(Action<MonoBehaviour, GameObject, Component, Camera> ready)
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            for (int i = 0; i < 240; i++)
            {
                yield return null;
                var host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x != null && x.GetType().Name == "ValoriaIntegratedWorld");
                if (!host || !(bool)Member(host, "Ready")) continue;
                var world = Member(host, "WorldRoot") as GameObject;
                var camera = Member(host, "WorldCamera") as Camera;
                var navigation = Member(host, "WorldNavigation") as Component;
                if (!world || !camera || !navigation) continue;
                if (!world.transform.Find("Region 1 canonical continuous geography")) continue;
                ready(host, world, navigation, camera);
                yield break;
            }
            Assert.Fail("Canonical adapter did not attach to the actual integrated scene");
        }

        [UnityTest]
        public IEnumerator OneGeographyAndNoInventedGameplayHotspots()
        {
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var geo = world.transform.Find("Region 1 canonical continuous geography");
            Assert.That(geo, Is.Not.Null);
            Assert.That(geo.GetComponentsInChildren<MeshFilter>(true)
                .Count(f => f && f.name == "Region1 · one coherent terrain mesh"), Is.EqualTo(1));
            Assert.That(geo.GetComponentsInChildren<Collider>(true), Is.Empty,
                "Art layer must never steal taps or replace gameplay colliders");
            Assert.That(geo.GetComponentsInChildren<MonoBehaviour>(true)
                .Any(x=>x && x.GetType().Name == "WorldHotspot"), Is.False,
                "No decorative resource/city/enemy node may impersonate gameplay");
            var colliders = world.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(x => x && x.GetType().Name == "WorldHotspot").ToArray();
            Assert.That(colliders.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(colliders.Any(x => (string)Member(x, "Id") == "forest-valoria"), Is.True);
            Assert.That(colliders.Any(x => (string)Member(x, "Id") == "quarry-valoria"), Is.True);
        }

        [UnityTest]
        public IEnumerator RealWoodNodeRemainsReachableWithBothMobileCameraAspects()
        {
            MonoBehaviour host = null; GameObject world = null;
            Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var wood=Call(host,"FindTarget","forest-valoria") as Component;
            Assert.That(wood, Is.Not.Null,
                "The real wood resource must remain available in the 4X world.");
            Assert.That(wood.GetComponent<Collider>(), Is.Not.Null,
                "Do not replace the clickable wood hotspot with painted terrain.");
            float oldAspect=camera.aspect;
            float oldZoom=camera.orthographicSize;
            var oldTarget=(Vector3)Member(navigation,"target");
            try
            {
                foreach (float aspect in new[]{1280f/720f,390f/844f})
                {
                    camera.aspect=aspect;
                    Call(navigation,"SetZoomLevel",2,true);
                    Call(navigation,"PanTo",wood.transform.position);
                    var pos=camera.WorldToViewportPoint(wood.transform.position);
                    Assert.That(pos.z, Is.GreaterThan(camera.nearClipPlane));
                    Assert.That(pos.x, Is.InRange(.10f,.90f),
                        "Actual wood target cannot be centred within horizontally bounded near view.");
                    Assert.That(pos.y, Is.InRange(.10f,.90f),
                        "Actual wood target cannot be centred within vertically bounded near view.");
                }
            }
            finally
            {
                camera.aspect=oldAspect;
                Call(navigation,"RestoreView",oldTarget,oldZoom);
            }
        }

        [UnityTest]
        public IEnumerator EnemyCampPlaceholdersAreVisuallyAbsentButRealTargetsRemain()
        {
            MonoBehaviour host = null; GameObject world = null;
            Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var originals = world.GetComponentsInChildren<Transform>(true);
            foreach (var donor in new[]{
                ("Enemy camp provisional existing canvas","corrupt-scout"),
                ("Enemy camp existing shelter","engendro-valoria")})
            {
                var root=originals.FirstOrDefault(t => t.name==donor.Item1);
                Assert.That(root, Is.Not.Null, "Existing named donor was not retained in real integrated World");
                var art=root.GetComponentsInChildren<MeshRenderer>(true);
                Assert.That(art.Length, Is.GreaterThan(0), "Original camp donor mesh missing");
                Assert.That(art.All(z=>!z.enabled), Is.True,
                    "Provisional enemy camp must not masquerade as an encounter");
                var logicalTarget=Call(host,"FindTarget",donor.Item2) as Component;
                Assert.That(logicalTarget, Is.Not.Null,
                    "Removing visual placeholders must not remove real enemy hotspots");
                Assert.That(logicalTarget.GetComponent<Collider>(), Is.Not.Null,
                    "Selection physics must remain unchanged");
            }
        }

        [UnityTest]
        public IEnumerator ThreeScalesUseSameWorldHomeWithoutCameraResetOrFakeBounds()
        {
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            Vector3 originalTarget = (Vector3)Member(navigation, "target");
            float originalZoom = camera.orthographicSize;
            var home = (Vector3)Member(host, "WorldHome");
            try
            {
                Call(navigation, "SetZoomLevel", 2, true);
                float near = camera.orthographicSize;
                Call(navigation, "SetZoomLevel", 1, true);
                float regional = camera.orthographicSize;
                Call(navigation, "SetZoomLevel", 0, true);
                float panorama = camera.orthographicSize;
                Assert.That(near, Is.GreaterThan(10));
                Assert.That(regional / near, Is.GreaterThan(3f));
                Assert.That(panorama / regional, Is.GreaterThan(2f));
                Assert.That((float)Member(navigation, "globalZoom"), Is.GreaterThan(200));
                Assert.That(camera.orthographic, Is.True);
                Call(navigation, "PanTo", home);
                Assert.That(Vector3.Distance((Vector3)Member(navigation, "target"),home),
                    Is.LessThan(2f), "Strategic home remains on the same geographical XYZ.");
                Assert.That(world.transform.Find("Region 1 canonical continuous geography"), Is.Not.Null);
            }
            finally
            {
                Call(navigation, "RestoreView", originalTarget, originalZoom);
            }
        }

        [UnityTest]
        public IEnumerator BreachIsGeologicalGroundGeometryNotTargetOrVerticalPortal()
        {
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var geo = world.transform.Find("Region 1 canonical continuous geography");
            var fracture = geo.Find("Region1 · grounded geologic Breach (not a portal)");
            Assert.That(fracture, Is.Not.Null);
            var filter = fracture.GetComponentInChildren<MeshFilter>(true);
            Assert.That(filter, Is.Not.Null);
            Assert.That(filter.sharedMesh.vertexCount, Is.GreaterThan(80));
            var b = filter.sharedMesh.bounds;
            Assert.That(b.size.y, Is.LessThan(8f), "Fracture is drawn along the ground, not a tower.");
            Assert.That(b.size.x, Is.GreaterThan(25f));
            Assert.That(b.size.z, Is.GreaterThan(25f));
            Assert.That(filter.sharedMesh.normals.Count(n => n.y > .5f), Is.GreaterThan(80),
                "Surface cracks must face the top-down strategy camera, never backface-culled.");
            Assert.That(fracture.GetComponentsInChildren<Collider>(true), Is.Empty);
        }
        [UnityTest]
        public IEnumerator PanoramaHidesOldSquareWithoutRemovingWorldGameObjects()
        {
            MonoBehaviour host = null; GameObject world = null;
            Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var adapter = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x && x.GetType().Name == "RegionOneCanonicalLandscape");
            Assert.That(adapter, Is.Not.Null);
            var terrain = world.GetComponentsInChildren<MeshRenderer>(true)
                .Where(x => (x.name.IndexOf("continuous prairie",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                    x.name.IndexOf("organic macrogeography",
                    StringComparison.OrdinalIgnoreCase) >= 0) &&
                    x.bounds.size.x > 100f && x.bounds.size.z > 100f).ToArray();
            Assert.That(terrain.Length, Is.GreaterThan(0),
                "Must identify the genuinely imported local ground; do not hide arbitrary art.");
            // Boot can already be in regional mode, where old prairie is
            // deliberately hidden. Its current visibility is not its authored
            // initial state; test visibility at each explicit camera scale.
            int existingColliderCount = world.GetComponentsInChildren<Collider>(true).Length;
            float zoom = camera.orthographicSize;
            var target = (Vector3)Member(navigation,"target");
            try
            {
                Call(navigation, "SetZoomLevel", 2, true);
                Call(adapter,"RefreshForCapture");
                Assert.That(terrain.Any(x => x.enabled), Is.True,
                    "The close-up camera must restore the imported prairie detail.");
                float nearPitch = (float)Member(navigation,"tilt");
                Call(navigation, "SetZoomLevel", 0, true);
                Call(adapter,"RefreshForCapture");
                float panoramicPitch = (float)Member(navigation,"tilt");
                Assert.That(panoramicPitch, Is.GreaterThan(nearPitch + 12f),
                    "Wide world must also become more top-down, not only zoom farther.");
                var allActualArt = world.GetComponentsInChildren<Renderer>(true)
                    .Where(x => !x.transform.IsChildOf(
                        world.transform.Find("Region 1 canonical continuous geography"))).ToArray();
                Assert.That(allActualArt.All(x => !x.enabled), Is.True,
                    "Panorama must hide tiny existing resource, enemy and city geometry without disabling colliders.");
                Assert.That(terrain.All(x => !x.enabled), Is.True,
                    "Panorama must not reveal the old hard-edged rectangular prairie.");
                var simplified = world.transform.Find("Region 1 canonical continuous geography")
                    .Find("Region1 · batched distant organic forest canopies");
                Assert.That(simplified, Is.Not.Null);
                Assert.That(simplified.gameObject.activeSelf, Is.True,
                    "Panorama must retain forest silhouettes, not blank green circles.");
                Call(navigation, "SetZoomLevel", 1, true);
                Call(adapter,"RefreshForCapture");
                Assert.That(terrain.All(x => !x.enabled), Is.True,
                    "Regional view must not retain the legacy rectangular terrain edge.");
                Assert.That(world.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(existingColliderCount),
                    "Regional visual LOD must preserve every original gameplay collider.");
            }
            finally
            {
                Call(navigation,"RestoreView",target,zoom);
                Call(adapter,"RefreshForCapture");
            }
        }

        [Serializable]
        sealed class RenderIdentity
        {
            public string sourceScene, version, label, sha256, context;
            public bool actualUnityCamera, playMode, physicalIphone;
            public int width, height;
            public float zoom, aspect;
            public Vector3 cameraPosition, worldTarget;
        }

        [UnityTest]
        public IEnumerator ActualPlayableWorldCapturesThreeZoomsOnDisposableProbe()
        {
            // Do not purchase anything on the owner's save. This is the same
            // opt-in disposable CI store already used by the game's own tests.
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Real-camera capture requires disposable Valoria native probe store.");
            var outdir = Environment.GetEnvironmentVariable("ELDORIA_REGION1_CAPTURE_OUT");
            if (string.IsNullOrEmpty(outdir))
                Assert.Ignore("Capture gate needs a new explicit artifacts directory.");
            Assert.That(Directory.Exists(outdir) && Directory.GetFileSystemEntries(outdir).Length != 0,
                Is.False, "Never replace previous evidence.");
            Directory.CreateDirectory(outdir);
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            // All interactions below are the ACTUAL gateway, never a presentation unlock.
            bool opened = (bool)Call(host, "TrySetWorld", true);
            if (!opened)
            {
                var snapshot = Call(host, "Read", "sawmill", false);
                var building = Member(snapshot, "Building");
                var realOffer = Member(building, "Action");
                Assert.That(realOffer, Is.Not.Null,
                    "Disposable test needs an authorized real sawmill offer; never force world access.");
                Assert.That(Member(realOffer, "Kind"), Is.EqualTo("Build"),
                    "Only the actual gameplay sawmill purchase may unlock World.");
                Call(host, "Submit", realOffer);
                // Sawmill is a genuine timed construction. Only a COMPLETE
                // building satisfies the real world's progression gate.
                // Advance the existing disposable gameplay clock, never the
                // player's save or the production rules.
                var city = Member(host, "City");
                var clock = Member(city, "Clock");
                Call(clock, "Step", Eldoria.Domain.SliceRules.SawmillBuildSeconds + 1);
                Call(Member(city, "Gateway"), "Advance");
                yield return null;
                opened = (bool)Call(host, "TrySetWorld", true);
            }
            Assert.That(opened, Is.True, "The real session still cannot enter Mundo after a real gateway action.");
            Assert.That((bool)Member(host, "InWorld"), Is.True);
            Assert.That(world.activeInHierarchy, Is.True);
            Assert.That(camera.enabled, Is.True);
            var geo = world.transform.Find("Region 1 canonical continuous geography");
            Assert.That(geo, Is.Not.Null);
            // Record exact existing renderer identity + extents, so visual
            // comparison can remove only terrain sheets, never resources.
            var survey = world.GetComponentsInChildren<Renderer>(true)
                .Where(r => r.bounds.size.x > 55f && r.bounds.size.z > 55f)
                .OrderByDescending(r => r.bounds.size.x * r.bounds.size.z)
                .Select(r => string.Join("\t", r.name,
                    r.bounds.size.x.ToString("F2"), r.bounds.size.y.ToString("F2"),
                    r.bounds.size.z.ToString("F2"), r.enabled.ToString(),
                    r.gameObject.activeInHierarchy.ToString(),
                    r.sharedMaterial ? r.sharedMaterial.name : "none",
                    r.transform.parent ? r.transform.parent.name : "root")).ToArray();
            File.WriteAllLines(Path.Combine(outdir, "world-wide-renderer-survey.tsv"), survey);
            var adapter = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .FirstOrDefault(x=>x && x.GetType().Name == "RegionOneCanonicalLandscape");
            Assert.That(adapter, Is.Not.Null);
            var home = (Vector3)Member(host, "WorldHome");
            var realForest = Call(host, "FindTarget", "forest-valoria") as Component;
            Assert.That(realForest, Is.Not.Null,
                "A genuine selectable wood resource must exist in the integrated region.");
            Assert.That(realForest.gameObject.activeInHierarchy, Is.True,
                "Never showcase an unavailable or invented resource target.");
            Vector3 forestFocus = realForest.transform.position;
            float originalZoom = camera.orthographicSize, originalAspect = camera.aspect;
            Vector3 originalTarget = (Vector3)Member(navigation,"target");
            RenderTexture originalRT = camera.targetTexture;
            RenderTexture originalActive = RenderTexture.active;
            try
            {
                foreach (var format in new[]{(1280,720,"horizontal"),(390,844,"vertical")})
                {
                    camera.aspect = format.Item1 / (float)format.Item2;
                    // The fourth image is another genuine part of the SAME
                    // territory: move the actual camera to an existing wood
                    // hotspot, never manufacture a drawable resource.
                    foreach(var stage in new[]{
                        (2,"cercana",home),
                        (1,"regional",home),
                        (0,"panoramica",home),
                        (2,"cercana-recurso-madera",forestFocus),
                        (0,"panoramica-antes-hibrido",home),
                        (1,"regional-antes-hibrido",home)})
                    {
                        Call(navigation, "SetZoomLevel", stage.Item1, true);
                        Call(navigation, "PanTo", stage.Item3);
                        Call(adapter, "RefreshForCapture");
                        yield return null;
                        Call(adapter, "RefreshForCapture");
                        if(stage.Item2=="panoramica" && format.Item3=="vertical")
                        {
                            // Core Region1 is 850 units across; the previous
                            // portrait overview displayed ~225 (26.5%). The
                            // orientation-adaptive real camera must now show
                            // >=400 units without a second map or fake image.
                            Assert.That(camera.orthographicSize*2f*camera.aspect,
                                Is.GreaterThanOrEqualTo(400f),
                                "Portrait panorama still crops the full world to a narrow strip.");
                            // Portrait framing has not reached the real map
                            // boundary; fixed straight-edged fog strips must
                            // not cut horizontally across its central land.
                            var portraitFog=world.transform.Find(
                                "Region 1 canonical continuous geography/Region1 · panoramic border haze only");
                            Assert.That(portraitFog,Is.Not.Null);
                            Assert.That(portraitFog.gameObject.activeSelf,Is.False,
                                "No hard fog seams may cross portrait map interior.");
                        }
                        if(stage.Item2=="panoramica" && format.Item3=="horizontal")
                            Assert.That(camera.orthographicSize,Is.InRange(239f,249f),
                                "Landscape overview must retain existing framing.");
                        // Genuine A/B: SAME WorldCamera, XYZ, tilt, terrain,
                        // original gameplay and LOD. Only the diffuse atlas
                        // changes. No owner-reference image is projected.
                        Call(adapter, "CompareAtlas", stage.Item2.EndsWith("-antes-hibrido"));
                        var texture = new RenderTexture(format.Item1,format.Item2,24,
                            RenderTextureFormat.ARGB32);
                        var pixels = new Texture2D(format.Item1,format.Item2,TextureFormat.RGB24,false);
                        try
                        {
                            camera.targetTexture=texture;
                            camera.Render();
                            RenderTexture.active=texture;
                            pixels.ReadPixels(new Rect(0,0,format.Item1,format.Item2),0,0);
                            pixels.Apply(false);
                            var png = pixels.EncodeToPNG();
                            Assert.That(png.Length, Is.GreaterThan(2500),
                                "Unity produced an implausibly empty game camera image.");
                            string name=stage.Item2+"-"+format.Item3;
                            File.WriteAllBytes(Path.Combine(outdir,name+"-unity.png"),png);
                            string hash;
                            using(var sha=SHA256.Create())
                                hash=BitConverter.ToString(sha.ComputeHash(png)).Replace("-","").ToLowerInvariant();
                            var metadata=new RenderIdentity {
                                sourceScene=SceneManager.GetActiveScene().path,
                                version=UnityEngine.Application.unityVersion,
                                context="Actual integrated Valoria World PlayMode / disposable genuine Gateway progression",
                                label=name, sha256=hash,
                                actualUnityCamera=true,playMode=UnityEngine.Application.isPlaying,
                                physicalIphone=false,width=format.Item1,height=format.Item2,
                                zoom=camera.orthographicSize,aspect=camera.aspect,
                                cameraPosition=camera.transform.position,
                                worldTarget=(Vector3)Member(navigation,"target")
                            };
                            File.WriteAllText(Path.Combine(outdir,name+"-source.json"),
                                JsonUtility.ToJson(metadata,true));
                        }
                        finally
                        {
                            camera.targetTexture=originalRT;
                            RenderTexture.active=originalActive;
                            texture.Release();
                            Object.Destroy(texture);
                            Object.Destroy(pixels);
                        }
                    }
                }
                Assert.That(Directory.GetFiles(outdir,"*-unity.png").Length, Is.EqualTo(12));
                Debug.Log("REGION1_CANON_NATIVE_PLAYER_CAMERA_CAPTURE_PASS views=12 (8 genuine views + 4 exact-camera old-atlas comparisons) real_scene=1 disposable_gateway=1 physical_iphone=0");
            }
            finally
            {
                Call(adapter, "CompareAtlas", false);
                camera.aspect=originalAspect;
                camera.targetTexture=originalRT;
                RenderTexture.active=originalActive;
                Call(navigation,"RestoreView",originalTarget,originalZoom);
            }
        }

    }
}
