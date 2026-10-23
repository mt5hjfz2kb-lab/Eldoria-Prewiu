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
    /// Disposable test stores exercise real gateway commands; player saves are not modified.
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

        [Test]
        public void AdditionalResourcesUseRealGatherCommandAndPersistIndependentQuantities()
        {
            var clock=new PopulationClock();var store=new PopulationStore();var placement=new PopulationPlacement();
            var gateway=new Eldoria.Application.LocalGateway(clock,store);
            gateway.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            gateway.EnsureLocalRenewablePopulation(24,24);
            var before=gateway.Snapshot();Assert.That(before.WorldResourceLocations.Count,Is.EqualTo(48));
            Assert.That(before.WorldResourceLocations.Select(x=>x.TargetId).Distinct().Count(),Is.EqualTo(48));
            var id="region1:wood:001";
            var command=new Eldoria.Application.GameCommand("dynamic-resource-gather",before.PlayerId,"Gather",id,before.Revision);
            Assert.That(gateway.Execute(command).Ok,Is.True);
            Assert.That(gateway.Execute(command).Ok,Is.True,"Same request remains idempotent");
            clock.UtcTicks+=TimeSpan.FromSeconds(Eldoria.Domain.SliceRules.TravelSeconds*2+Eldoria.Domain.SliceRules.GatherSeconds+1).Ticks;
            gateway.Advance();var after=gateway.Snapshot();
            Assert.That(after.March.Phase,Is.EqualTo("idle"));
            Assert.That(after.ForestRemaining,Is.EqualTo(before.ForestRemaining),"The original node is not silently charged for another instance");
            Assert.That(Eldoria.Application.LocalGateway.ResourceRemaining(after,id),Is.EqualTo(1250-Eldoria.Domain.SliceRules.ForestLoad));
            Assert.That(after.Resources.Wood,Is.GreaterThan(before.Resources.Wood));
            var restarted=new Eldoria.Application.LocalGateway(clock,store);restarted.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));restarted.EnsureLocalRenewablePopulation(24,24);
            Assert.That(restarted.Snapshot().WorldResourceLocations.Count,Is.EqualTo(48));
            Assert.That(Eldoria.Application.LocalGateway.ResourceRemaining(restarted.Snapshot(),id),Is.EqualTo(Eldoria.Application.LocalGateway.ResourceRemaining(after,id)));
        }

        [Test]
        public void OfflineDepletionUsesSavedDeadlineInsteadOfRestartTime()
        {
            var clock=new PopulationClock();var store=new PopulationStore();
            var state=new Eldoria.Domain.PlayerState{ForestRemaining=0};
            state.WorldResourceLocations.Add(new Eldoria.Domain.WorldResourceLocation{TargetId="forest-valoria",SiteId="old-site",Generation=1,DepletedUtcTicks=clock.UtcTicks-TimeSpan.FromHours(1).Ticks});
            store.Save(state);var gateway=new Eldoria.Application.LocalGateway(clock,store);
            gateway.ConfigureLocalResourceRespawn(new PopulationPlacement(),TimeSpan.FromMinutes(30));
            var after=gateway.Snapshot();Assert.That(after.ForestRemaining,Is.EqualTo(1250));
            Assert.That(after.WorldResourceLocations.Single(x=>x.TargetId=="forest-valoria").Generation,Is.EqualTo(2));
            var restarted=new Eldoria.Application.LocalGateway(clock,store);restarted.ConfigureLocalResourceRespawn(new PopulationPlacement(),TimeSpan.FromMinutes(30));
            Assert.That(restarted.Snapshot().WorldResourceLocations.Single(x=>x.TargetId=="forest-valoria").Generation,Is.EqualTo(2));
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
        public IEnumerator ProductionResourcesHavePersistentDistinctTerrainSites()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var city=Member(host,"City");var gateway=(Eldoria.Application.LocalGateway)Member(city,"Gateway");
            var snapshot=gateway.Snapshot();var resources=snapshot.WorldResourceLocations.Where(x=>x.TargetId.StartsWith("region1:")).ToArray();
            Assert.That(resources.Length,Is.EqualTo(48),"24 wood + 24 stone identities must persist in the existing state");
            Assert.That(resources.Count(x=>x.Generation>0),Is.EqualTo(48),"Do not claim capacity from unplaced records");
            Assert.That(resources.Select(x=>x.SiteId).Distinct().Count(),Is.EqualTo(48));
            for(int i=0;i<resources.Length;i++)for(int j=i+1;j<resources.Length;j++)
                Assert.That(Vector2.Distance(new Vector2(resources[i].X,resources[i].Z),new Vector2(resources[j].X,resources[j].Z)),Is.GreaterThanOrEqualTo(20));
            foreach(var item in resources)
                Assert.That(Call(host,"FindTarget",item.TargetId),Is.Not.Null,"Saved resource must have a real selectable target");
        }

        [UnityTest]
        public IEnumerator RealTerrainRoutesReachPopulationAndReserveCities()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var placement=world.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneResourcePlacement");
            Assert.That((int)Member(placement,"ValidatedCityCount"),Is.GreaterThanOrEqualTo(12),"City footprints require measured real mesh clearance");
            Assert.That((int)Member(placement,"AccessibleCityCount"),Is.GreaterThanOrEqualTo(12),"Urban reserves require actual access from Valoria");
            var routes=(Eldoria.Application.IWorldMarchRoutes)Member(placement,"MarchRoutes");
            var gateway=(Eldoria.Application.LocalGateway)Member(Member(host,"City"),"Gateway");var snapshot=gateway.Snapshot();
            var ids=new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}.Concat(snapshot.WorldResourceLocations.Where(x=>x.TargetId.StartsWith("region1:")).Select(x=>x.TargetId));
            var unreachable=new System.Collections.Generic.List<string>();
            foreach(var id in ids){
                if(!routes.TryRoute(snapshot,id,out var route)){unreachable.Add(id);continue;}
                Assert.That(route.Count,Is.GreaterThan(1));
                for(int i=1;i<route.Count;i++)Assert.That(Vector3.Distance(new Vector3(route[i].X,route[i].Y,route[i].Z),new Vector3(route[i-1].X,route[i-1].Y,route[i-1].Z)),Is.LessThan(9));
            }
            Assert.That(unreachable,Is.Empty,"No terrain route to: "+string.Join(",",unreachable));
        }

        [UnityTest]
        public IEnumerator RealTerrainRespawnSurvivesGatewayRestartWithoutDuplicatingSites()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var component=world.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneResourcePlacement");
            var placement=(Eldoria.Application.IWorldResourcePlacement)component;
            var original=(Eldoria.Application.LocalGateway)Member(Member(host,"City"),"Gateway");
            var state=original.Snapshot();var item=state.WorldResourceLocations.First(x=>x.TargetId.StartsWith("region1:wood:"));
            string id=item.TargetId,oldSite=item.SiteId;int generation=item.Generation;item.Remaining=1;
            var clock=new PopulationClock();var store=new PopulationStore();store.Save(state);
            var gateway=new Eldoria.Application.LocalGateway(clock,store);
            gateway.ConfigureLocalMarchRoutes((Eldoria.Application.IWorldMarchRoutes)Member(component,"MarchRoutes"));
            gateway.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            var before=gateway.Snapshot();
            Assert.That(gateway.Execute(new Eldoria.Application.GameCommand("real-terrain-deplete",before.PlayerId,"Gather",id,before.Revision)).Ok,Is.True);
            clock.UtcTicks+=TimeSpan.FromSeconds(Eldoria.Domain.SliceRules.TravelSeconds*2+Eldoria.Domain.SliceRules.GatherSeconds+1).Ticks;
            gateway.Advance();Assert.That(Eldoria.Application.LocalGateway.ResourceRemaining(gateway.Snapshot(),id),Is.Zero);
            clock.UtcTicks+=TimeSpan.FromMinutes(31).Ticks;
            var resumed=new Eldoria.Application.LocalGateway(clock,store);resumed.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            var relocated=resumed.Snapshot().WorldResourceLocations.Single(x=>x.TargetId==id);
            Assert.That(relocated.Generation,Is.EqualTo(generation+1));Assert.That(relocated.SiteId,Is.Not.EqualTo(oldSite));
            Assert.That(relocated.Remaining,Is.GreaterThan(0));
            var restarted=new Eldoria.Application.LocalGateway(clock,store);restarted.ConfigureLocalResourceRespawn(placement,TimeSpan.FromMinutes(30));
            var final=restarted.Snapshot();Assert.That(final.WorldResourceLocations.Single(x=>x.TargetId==id).SiteId,Is.EqualTo(relocated.SiteId));
            Assert.That(final.WorldResourceLocations.Select(x=>x.TargetId).Distinct().Count(),Is.EqualTo(final.WorldResourceLocations.Count));
            foreach(var other in final.WorldResourceLocations.Where(x=>x.TargetId!=id&&x.Generation>0))
                Assert.That(Vector2.Distance(new Vector2(relocated.X,relocated.Z),new Vector2(other.X,other.Z)),Is.GreaterThanOrEqualTo(20));
            Debug.Log("REGION1_REAL_TERRAIN_RESPAWN_RESTART old="+oldSite+" new="+relocated.SiteId+" generation="+relocated.Generation);
        }

        [UnityTest]
        public IEnumerator ContactQueriesMatchActualImportedTriangles()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var floor=world.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_Terrain");
            var geology=world.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneBlenderLandscape");
            var height=geology.GetType().GetMethod("Height",BindingFlags.Public|BindingFlags.Static);var origin=(Vector3)Member(host,"WorldHome");
            var query=new GameObject("Test only exact surface raycast");query.layer=2;query.transform.SetPositionAndRotation(floor.transform.position,floor.transform.rotation);query.transform.localScale=floor.transform.lossyScale;
            var collider=query.AddComponent<MeshCollider>();collider.sharedMesh=floor.sharedMesh;Physics.SyncTransforms();
            float maximum=0;int samples=0;
            try{
                for(float z=-720;z<=820;z+=77)for(float x=-750;x<=650;x+=73){
                    Assert.That(collider.Raycast(new Ray(origin+new Vector3(x,250,z),Vector3.down),out var hit,500),Is.True);
                    float expected=origin.y+(float)height.Invoke(null,new object[]{x,z});maximum=Mathf.Max(maximum,Mathf.Abs(expected-hit.point.y));samples++;
                }
                Assert.That(maximum,Is.LessThan(.003f),"Contact height must match visible triangles, without collider displacement");
                Debug.Log("REGION1_EXACT_CONTACT samples="+samples+" max_error_m="+maximum);
            }finally{Object.DestroyImmediate(query);}
        }

        [UnityTest]
        public IEnumerator ResourceModelsHaveMeasuredTerrainSupportWithoutMovingColliders()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var modelService=world.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneResourceModels");
            var originalColliders=world.GetComponentsInChildren<Collider>(true).ToDictionary(x=>x,x=>x.transform.position);
            Call(modelService,"RefreshNow");
            var origin=(Vector3)Member(host,"WorldHome");var geology=world.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneBlenderLandscape");var height=geology.GetType().GetMethod("Height",BindingFlags.Public|BindingFlags.Static);
            float minimum=0,maximum=0;int parts=0;
            foreach(var root in world.GetComponentsInChildren<Transform>(true).Where(x=>x.name.StartsWith("Production resource visual ")))
            foreach(var part in root.GetComponentsInChildren<MeshFilter>(true)){
                var points=part.sharedMesh.vertices.Select(part.transform.TransformPoint).ToArray();float bottom=points.Min(p=>p.y);
                foreach(var p in points.Where(p=>p.y<bottom+.03f)){float ground=origin.y+(float)height.Invoke(null,new object[]{p.x-origin.x,p.z-origin.z});minimum=Mathf.Min(minimum,p.y-ground);maximum=Mathf.Max(maximum,p.y-ground);}parts++;
            }
            Assert.That(parts,Is.GreaterThan(200));Assert.That(maximum,Is.LessThan(.005f),"Floating resource supports");Assert.That(minimum,Is.GreaterThan(-.65f),"Excessive burial must be repaired, not hidden with collider offsets");
            foreach(var pair in originalColliders)Assert.That(pair.Key.transform.position,Is.EqualTo(pair.Value));
            Debug.Log("REGION1_RESOURCE_CONTACT parts="+parts+" min_gap_m="+minimum+" max_gap_m="+maximum+" collider_movement=0");
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
            foreach(var oldResource in geo.GetComponentsInChildren<Renderer>(true).Where(r=>r.name=="Reference existing quarry"||r.name=="Reference existing wood grove")) Assert.That(oldResource.enabled,Is.False,"Replaced resource graphic must stay hidden during capture too");
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
            Assert.That((float)directedShore.Invoke(null,new object[]{850f,0f}),Is.LessThan(-15f),
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
            Assert.That((float)directedHeight.Invoke(null,new object[]{-260f,150f}),
                Is.LessThanOrEqualTo((float)controlHeight.Invoke(null,new object[]{-260f,150f})+.03f),
                "The authored land cannot bury the grounded Breach pigment at its real anchor.");
            var coastMask=coastlineType.GetMethod("CoastWaterMask",
                BindingFlags.Public | BindingFlags.Static);
            Assert.That(coastMask,Is.Not.Null);
            float dry=(float)coastMask.Invoke(null,new object[]{0f,-360f});
            float sea=(float)coastMask.Invoke(null,new object[]{850f,-700f});
            Assert.That(dry,Is.EqualTo(0f).Within(.001f),
                "The inland edge of the southeastern coast remains dry.");
            Assert.That(sea,Is.GreaterThan(.95f),
                "The extended southern continental shelf must still end in genuine sea.");
            float inlet=(float)coastMask.Invoke(null,new object[]{850f,-255f});
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
            Assert.That(scar.bounds.center.x-home.x,Is.EqualTo(-260f).Within(6f));
            Assert.That(scar.bounds.center.z-home.z,Is.EqualTo(150f).Within(6f));
            Assert.That(scar.bounds.size.y,Is.LessThan(14f),"The geological floor is not a vertical portal.");
            Assert.That(scar.bounds.min.y-home.y,Is.LessThan(1.5f),"The Breach must cut below the certified AI meadow, not stand above it.");
            Assert.That(filter.GetComponentsInChildren<Collider>(true),Is.Empty);
            var geology=geo.GetComponentsInChildren<MonoBehaviour>(true).Single(x=>x&&x.GetType().Name=="RegionOneBlenderLandscape");
            var h=geology.GetType().GetMethod("Height",BindingFlags.Public|BindingFlags.Static);
            Assert.That((float)h.Invoke(null,new object[]{-260f,150f}),Is.LessThan(-4f),"Depth must exist in the imported terrain, not only a painted floor.");
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

        static void SaveRetainedSectorFrame(Camera camera,string path,int width,int height)
        {
            var previous=camera.targetTexture;var active=RenderTexture.active;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.aspect=width/(float)height;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();var png=pixels.EncodeToPNG();
                Assert.That(png.Length,Is.GreaterThan(2500));File.WriteAllBytes(path+".png",png);
                string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(png)).Replace("-","").ToLowerInvariant();
                File.WriteAllText(path+".json",JsonUtility.ToJson(new RenderIdentity{
                    sourceScene=SceneManager.GetActiveScene().path,version=UnityEngine.Application.unityVersion,
                    context="AF/AG camera and sector coordinates; integrated production lighting and materials",label=Path.GetFileName(path),
                    sha256=hash,actualUnityCamera=true,playMode=UnityEngine.Application.isPlaying,physicalIphone=false,
                    width=width,height=height,zoom=camera.orthographicSize,aspect=camera.aspect,
                    cameraPosition=camera.transform.position,cameraRotation=camera.transform.rotation},true));
            }
            finally{camera.targetTexture=previous;RenderTexture.active=active;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(pixels);}
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
            Vector3 stoneFocus = ((Component)Call(host,"FindTarget","quarry-valoria")).transform.position;
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
                        (2,"cercana-recurso-piedra",stoneFocus),
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
                // Same AF/AG sector coordinates and cameras, now inside the production continent.
                // These retain production lighting/materials: camera-matched evidence, not a claim of pixel parity.
                var sectorDirectory=Path.Combine(outdir,"retained-sector");Directory.CreateDirectory(sectorDirectory);
                Call(adapter,"SetBlockout",false);
                foreach(bool portrait in new[]{false,true})
                {
                    string orientation=portrait?"portrait":"landscape";
                    for(int level=0;level<3;level++)
                    {
                        float size=portrait?new[]{32f,52f,74f}[level]:new[]{24f,38f,52f}[level];
                        var focus=home+new Vector3(Mathf.Lerp(20,0,level/2f),2,Mathf.Lerp(95,50,level/2f));
                        camera.transform.position=focus+new Vector3(0,130,-145);camera.transform.LookAt(focus);
                        camera.orthographicSize=size;
                        SaveRetainedSectorFrame(camera,Path.Combine(sectorDirectory,orientation+"-"+new[]{"close","regional","panorama"}[level]),portrait?720:1280,portrait?1280:720);
                    }
                    string sequence=Path.Combine(sectorDirectory,orientation+"-zoom");Directory.CreateDirectory(sequence);
                    for(int frame=0;frame<=24;frame++)
                    {
                        float t=Mathf.SmoothStep(0,1,frame/24f);
                        var focus=home+new Vector3(Mathf.Lerp(20,0,t),2,Mathf.Lerp(95,50,t));
                        camera.transform.position=focus+new Vector3(0,130,-145);camera.transform.LookAt(focus);
                        camera.orthographicSize=portrait?Mathf.Lerp(32,74,t):Mathf.Lerp(24,52,t);
                        SaveRetainedSectorFrame(camera,Path.Combine(sequence,"frame-"+frame.ToString("D3")),portrait?540:960,portrait?960:540);
                    }
                }
                Assert.That(Directory.GetFiles(sectorDirectory,"*.png").Length,Is.EqualTo(6));
                Assert.That(Directory.GetFiles(outdir,"*-unity.png").Length, Is.EqualTo(16));
                if(Environment.GetEnvironmentVariable("ELDORIA_RIBERA_PILOT")=="1")
                {
                    var pilot=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Eldoria.World4XCanon.RegionOneRiberaPilot")).First(t=>t!=null);
                    pilot.GetMethod("Capture").Invoke(null,new object[]{camera,world.transform,home,Path.Combine(outdir,"ribera-pilot")});
                }
                Debug.Log("REGION1_CANON_NATIVE_PLAYER_CAMERA_CAPTURE_PASS views=16 (10 actual views + 2 historical-camera panorama controls + 4 blockout views) real_scene=1 disposable_gateway=1 physical_iphone=0");
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
