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
        sealed class PopulationClock : Eldoria.Application.IClock { public long UtcTicks {get;set;} = TimeSpan.FromDays(2).Ticks; }
        sealed class PopulationStore : Eldoria.Application.IStateStore
        {
            public string Json; public int Writes;
            public Eldoria.Domain.PlayerState Load()=>Json==null?null:JsonUtility.FromJson<Eldoria.Domain.PlayerState>(Json);
            public void Save(Eldoria.Domain.PlayerState state){Json=JsonUtility.ToJson(state);Writes++;}
        }
        sealed class PopulationPlacement : Eldoria.Application.IWorldResourcePlacement
        {
            public bool Occupied;
            public bool TryChoose(Eldoria.Domain.PlayerState state,string id,string previous,int generation,out Eldoria.Domain.WorldResourceLocation site)
            {
                site=new Eldoria.Domain.WorldResourceLocation{TargetId=id,SiteId=id+"/site-"+generation,X=generation*20,Y=4,Z=80};
                return !Occupied && state.BastionLevel>=1;
            }
        }
        [Test]
        public void RealGatewayResourceRespawnPersistsWithoutDuplicatingOrRevivingCampaign()
        {
            var clock=new PopulationClock();var store=new PopulationStore();
            store.Save(new Eldoria.Domain.PlayerState{ForestRemaining=0,QuarryRemaining=0,ScoutDefeated=true,EngendroDefeated=true});
            var placement=new PopulationPlacement();
            var gateway=new Eldoria.Application.LocalGateway(clock,store);
            gateway.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            var waiting=gateway.Snapshot();Assert.That(waiting.WorldResourceLocations.Count,Is.EqualTo(2));
            long due=waiting.WorldResourceLocations[0].RespawnUtcTicks;
            clock.UtcTicks=due-1;gateway.Advance();Assert.That(gateway.Snapshot().ForestRemaining,Is.Zero);
            // A process restart reconstructs from real serialized PlayerState, not fixture positions.
            gateway=new Eldoria.Application.LocalGateway(clock,store);
            gateway.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            placement.Occupied=true;clock.UtcTicks=due;gateway.Advance();
            Assert.That(gateway.Snapshot().ForestRemaining,Is.Zero,"No valid unoccupied placement means defer");
            placement.Occupied=false;gateway.Advance();var respawn=gateway.Snapshot();
            Assert.That(respawn.ForestRemaining,Is.EqualTo(1250));Assert.That(respawn.QuarryRemaining,Is.EqualTo(5200));
            Assert.That(respawn.WorldResourceLocations.All(x=>x.Generation==1&&!string.IsNullOrEmpty(x.SiteId)),Is.True);
            int writes=store.Writes;gateway.Advance();gateway.Advance();Assert.That(store.Writes,Is.EqualTo(writes));
            respawn.WorldResourceLocations[0].SiteId="mutated snapshot";
            Assert.That(gateway.Snapshot().WorldResourceLocations[0].SiteId,Is.Not.EqualTo("mutated snapshot"));
            gateway=new Eldoria.Application.LocalGateway(clock,store);gateway.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            var loaded=gateway.Snapshot();Assert.That(loaded.WorldResourceLocations.Count,Is.EqualTo(2));
            Assert.That(loaded.WorldResourceLocations.All(x=>x.Generation==1),Is.True);
            Assert.That(loaded.ScoutDefeated&&loaded.EngendroDefeated,Is.True,"Campaign victories remain permanent");
            loaded.WorldId="shared-world";store.Save(loaded);
            var shared=new Eldoria.Application.LocalGateway(clock,store);
            Assert.Throws<InvalidOperationException>(()=>shared.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30)));
        }

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
                .Count(f => f && f.name == "BLD_Terrain"), Is.EqualTo(1));
            var authoredCanopies=geo.GetComponentsInChildren<MeshFilter>(true)
                .Where(f=>f.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)).ToArray();
            Assert.That(authoredCanopies.Length,Is.GreaterThanOrEqualTo(20),"Blender forest mass export must survive import.");
            var retainedTreeSources=world.GetComponentsInChildren<MeshRenderer>(true)
                .Where(r=>r.name.StartsWith("Forest retained Valoria pine",StringComparison.Ordinal) ||
                    r.name.StartsWith("Zone1 retained pine ",StringComparison.Ordinal) ||
                    r.name.StartsWith("Zone1 woodland edge / retained pine ",StringComparison.Ordinal) ||
                    r.name.StartsWith("Existing conifer canopy / forest ecological lobe ",StringComparison.Ordinal) ||
                    r.name=="Forest understory / young conifer" ||
                    r.name.StartsWith("Region1 · reused-tree ",StringComparison.Ordinal)).ToArray();
            Assert.That(retainedTreeSources.Count(r=>r.name.StartsWith("Forest retained Valoria pine",StringComparison.Ordinal)),Is.EqualTo(98));
            Assert.That(retainedTreeSources.SelectMany(r=>r.GetComponentsInChildren<LODGroup>(true))
                .Any(lod=>lod.enabled),Is.False,"Replaced source LODs must not revive legacy foliage during culling.");
            foreach(int level in new[]{2,1,0,2})
            {
                Call(navigation,"SetZoomLevel",level,true);
                var activeAdapter=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape");
                Call(activeAdapter,"RefreshForCapture");
                Assert.That(retainedTreeSources.SelectMany(r=>r.GetComponentsInChildren<Renderer>(true)).All(r=>!r.enabled),Is.True,
                    "Neither donor nor child LOD renderer may reappear beside clean woodland at any zoom.");
            }
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
            foreach(var name in new[]{"Reference city Bastion I","Reference existing quarry","Reference existing neutral watch ruin","Reference existing wood grove"})
                Assert.That(geo.GetComponentsInChildren<MeshFilter>(true).Any(f=>f.name==name),Is.True,"Reference-derived real node art missing: "+name);
            foreach(var donor in world.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Quarry Geological Scarp /",StringComparison.Ordinal)||t.name=="Connected graded extraction apron"||t.name.StartsWith("Valoria map / retained ruins",StringComparison.Ordinal)||t.name=="Forest cut timber and sawbuck"))
                Assert.That(donor.GetComponentsInChildren<Renderer>(true).All(r=>!r.enabled),Is.True,"A replaced legacy node visual reappeared: "+donor.name);
            var directed=geo.GetComponentsInChildren<MonoBehaviour>(true).FirstOrDefault(x=>x&&x.GetType().Name=="RegionOneBlenderLandscape");
            Assert.That(directed,Is.Not.Null,"The Blender GLB authority must be physically installed; no legacy geography builder.");
            var directedType=directed.GetType();
            var directedShore=directedType.GetMethod("Shore",BindingFlags.Public|BindingFlags.Static);
            var directedWater=directedType.GetMethod("Water",BindingFlags.Public|BindingFlags.Static);
            foreach(var p in new[]{new Vector2(-400,0),new Vector2(-400,240),new Vector2(-350,-250),new Vector2(0,500)})
                Assert.That((float)directedShore.Invoke(null,new object[]{p.x,p.y}),Is.GreaterThan(15f),
                    "Continental land must continue west/north; do not regress to an isolated narrow island.");
            Assert.That((float)directedShore.Invoke(null,new object[]{410f,0f}),Is.LessThan(-15f),
                "A limited eastern sea must remain, not a terrain rectangle everywhere.");
            var actualHome=(Vector3)Member(host,"WorldHome");
            foreach(var node in colliders)
            {
                var p=node.transform.position-actualHome;
                Assert.That((float)directedShore.Invoke(null,new object[]{p.x,p.z}),Is.GreaterThan(5f),"Existing node must remain on the authored land: "+Member(node,"Id"));
                Assert.That((float)directedWater.Invoke(null,new object[]{p.x,p.z}),Is.LessThan(.1f),"Authored rivers/lakes cannot flood an existing node: "+Member(node,"Id"));
            }
            Assert.That(geo.GetComponentsInChildren<MeshFilter>(true).Count(x=>x.name=="BLD_Terrain"),Is.EqualTo(1));
            // Art runtime belongs to a separate Unity assembly. Match the
            // existing test's reflection-only contract rather than adding an
            // unsupported direct assembly reference to this PlayMode suite.
            var coastlineType=AppDomain.CurrentDomain.GetAssemblies()
                .Select(a=>a.GetType("Eldoria.World4XCanon.RegionOneCanonicalLandscape",false))
                .FirstOrDefault(t=>t!=null);
            Assert.That(coastlineType,Is.Not.Null);
            var directedHeight=directedType.GetMethod("Height",BindingFlags.Public|BindingFlags.Static);
            var controlHeight=coastlineType.GetMethod("SampleHeight",BindingFlags.Public|BindingFlags.Static);
            Assert.That((float)directedHeight.Invoke(null,new object[]{8f,61f}),
                Is.LessThanOrEqualTo((float)controlHeight.Invoke(null,new object[]{8f,61f})+.03f),
                "The authored land cannot bury the grounded Breach pigment at its real anchor.");
            var coastMask=coastlineType.GetMethod("CoastWaterMask",
                BindingFlags.Public | BindingFlags.Static);
            Assert.That(coastMask,Is.Not.Null);
            float dry=(float)coastMask.Invoke(null,new object[]{0f,-360f});
            float sea=(float)coastMask.Invoke(null,new object[]{0f,-700f});
            Assert.That(dry,Is.EqualTo(0f).Within(.001f),
                "The inland edge of the southeastern coast remains dry.");
            Assert.That(sea,Is.GreaterThan(.95f),
                "The extended southern continental shelf must still end in genuine sea.");
            float inlet=(float)coastMask.Invoke(null,new object[]{290f,-255f});
            float mainland=(float)coastMask.Invoke(null,new object[]{70f,-200f});
            Assert.That(inlet,Is.GreaterThan(.95f),
                "The broad strategic view must include its limited eastern coastal inlet.");
            Assert.That(mainland,Is.EqualTo(0f).Within(.001f),
                "The bay must not flood the existing central landmass.");
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
            var filter=geo.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="BLD_BreachFloor");
            var scar=filter.GetComponent<MeshRenderer>();
            var home=(Vector3)Member(host,"WorldHome");
            Assert.That(scar.bounds.center.x-home.x,Is.EqualTo(8f).Within(2f));
            Assert.That(scar.bounds.center.z-home.z,Is.EqualTo(61f).Within(2f));
            Assert.That(scar.bounds.size.y,Is.LessThan(8f),"The geological floor is not a vertical portal.");
            Assert.That(scar.bounds.center.y-home.y,Is.LessThan(-6f),"The Breach must be recessed beneath the -4m meadow datum.");
            Assert.That(filter.GetComponentsInChildren<Collider>(true),Is.Empty);
            var geology=geo.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneBlenderLandscape");
            var h=geology.GetType().GetMethod("Height",BindingFlags.Public|BindingFlags.Static);
            Assert.That((float)h.Invoke(null,new object[]{8f,61f}),Is.LessThan(-8f),"Depth must exist in the imported terrain, not only a painted floor.");
            var adapter=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape");
            var fixedVertices=filter.sharedMesh.vertices;var fixedPosition=filter.transform.position;
            foreach(int level in new[]{2,0,1})
            {
                Call(navigation,"SetZoomLevel",level,true);Call(adapter,"RefreshForCapture");
                Assert.That(scar.enabled,Is.True);
                Assert.That(filter.sharedMesh.vertices,Is.EqualTo(fixedVertices));
                Assert.That(filter.transform.position,Is.EqualTo(fixedPosition),"Zoom must not relocate geology.");
            }
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
                Assert.That(terrain.All(x => !x.enabled), Is.True,
                    "Candidate near ground must not overlap the old rectangular prairie.");
                var continuous=world.GetComponentsInChildren<MeshRenderer>(true).Single(r=>r.name=="BLD_Terrain");
                Assert.That(continuous.enabled&&continuous.gameObject.activeSelf&&continuous.transform.parent.gameObject.activeSelf&&continuous.sharedMaterial&&
                    continuous.sharedMaterials.All(m=>m&&m.shader&&m.shader.isSupported),Is.True,
                    "A supported continuous terrain must actually replace the hidden near ground.");
                Call(adapter,"SetBlockout",true);
                Assert.That(terrain.All(x=>!x.enabled),Is.True,"Blockout must not revive older terrain.");
                Call(adapter,"SetBlockout",false);
                Assert.That(world.GetComponentsInChildren<MeshFilter>(true).Any(f=>f.name=="Region1 · one coherent terrain mesh"),Is.False,
                    "The retired terrain must not be constructed beneath the clean surface.");

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
                var simplified = world.GetComponentsInChildren<Transform>(true).First(t=>t.name=="BLD_ForestMass_00");
                Assert.That(simplified, Is.Not.Null);
                Assert.That(simplified.gameObject.activeSelf, Is.True,
                    "Panorama must retain forest silhouettes, not blank green circles.");
                Call(navigation, "SetZoomLevel", 1, true);
                Call(adapter,"RefreshForCapture");
                Assert.That(terrain.All(x => !x.enabled), Is.True,
                    "Regional view must not retain the legacy rectangular terrain edge.");
                Assert.That(simplified.gameObject.activeSelf, Is.True,
                    "Regional view must retain shared woodland canopies, not only flat forest masks.");
                foreach (float intermediateZoom in new [] { 55f, 90f, 145f, 153f, 113f })
                {
                    Call(navigation,"RestoreView",target,intermediateZoom);
                    Call(adapter,"RefreshForCapture");
                    Assert.That(simplified.gameObject.activeSelf, Is.True,
                        "Woodland must not disappear between regional and panoramic marker LODs.");
                }
                Call(navigation,"SetZoomLevel",2,true);
                Call(adapter,"RefreshForCapture");
                Assert.That(simplified.gameObject.activeSelf, Is.True,
                    "Near view must keep the same woodland geography, without replacing it at a zoom threshold.");
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
            public Quaternion cameraRotation;
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
            Call(adapter, "ExportAuthoringSnapshot", outdir);
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
                        (0,"panoramica-control",home),
                        (0,"panoramica-blockout",home),
                        (1,"regional-blockout",home)})
                    {
                        Call(navigation, "SetZoomLevel", stage.Item1, true);
                        Call(navigation, "PanTo", stage.Item3);
                        Call(adapter, "RefreshForCapture");
                        yield return null;
                        Call(adapter, "RefreshForCapture");
                        if(stage.Item2=="panoramica-control")
                        {Call(navigation,"RestoreView",home,format.Item3=="vertical"?456f:244f);Call(adapter,"RefreshForCapture");}
                        if(stage.Item2=="cercana" && format.Item3=="vertical")
                            Assert.That(camera.orthographicSize*2f*camera.aspect,Is.GreaterThan(55f),
                                "Near portrait must include context around actual gameplay nodes.");
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
                            Assert.That(portraitFog,Is.Null,"Clean world must not construct old opaque fog strips.");
                        }
                        if(stage.Item2=="panoramica")
                        {
                            var terrain=world.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="BLD_Terrain");
                            foreach(var vertex in terrain.sharedMesh.vertices)
                            {
                                var framed=camera.WorldToViewportPoint(terrain.transform.TransformPoint(vertex));
                                Assert.That(framed.z,Is.InRange(camera.nearClipPlane,camera.farClipPlane),"Region clipped by camera depth");
                                Assert.That(framed.x,Is.InRange(.005f,.995f),"Panorama crops actual regional mesh horizontally");
                                Assert.That(framed.y,Is.InRange(.005f,.995f),"Panorama crops actual regional mesh vertically");
                            }
                        }
                        // Clean blockout/full review: SAME actual geometry and camera.
                        // Before/after uses archived runs, never a hidden old terrain.
                        Call(adapter, "CompareAtlas", false);
                        Call(adapter, "SetBlockout", stage.Item2.EndsWith("-blockout"));
                        if(!stage.Item2.EndsWith("-blockout"))
                        {
                            var actualTerrain=world.GetComponentsInChildren<MeshRenderer>(true).Single(r=>r.name=="BLD_Terrain");
                            Assert.That(actualTerrain.enabled&&actualTerrain.gameObject.activeInHierarchy,Is.True,"Continuous candidate terrain must be active after genuine world entry, before capture.");
                        }
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
                                cameraRotation=camera.transform.rotation,
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
                Assert.That(Directory.GetFiles(outdir,"*-unity.png").Length, Is.EqualTo(14));
                if(Environment.GetEnvironmentVariable("ELDORIA_RIBERA_PILOT")=="1")
                {
                    var pilot=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Eldoria.World4XCanon.RegionOneRiberaPilot")).First(t=>t!=null);
                    pilot.GetMethod("Capture").Invoke(null,new object[]{camera,world.transform,home,Path.Combine(outdir,"ribera-pilot")});
                }
                Debug.Log("REGION1_CANON_NATIVE_PLAYER_CAMERA_CAPTURE_PASS views=14 (8 actual views + 2 historical-camera panorama controls + 4 blockout views) real_scene=1 disposable_gateway=1 physical_iphone=0");
            }
            finally
            {
                Call(adapter, "CompareAtlas", false);
                Call(adapter, "SetBlockout", false);
                camera.aspect=originalAspect;
                camera.targetTexture=originalRT;
                RenderTexture.active=originalActive;
                Call(navigation,"RestoreView",originalTarget,originalZoom);
            }
        }

    }
}
