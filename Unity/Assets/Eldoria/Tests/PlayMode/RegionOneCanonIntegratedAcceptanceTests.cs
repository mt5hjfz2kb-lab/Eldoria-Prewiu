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
        sealed class CreaturePlacement : Eldoria.Application.IWorldCreaturePlacement
        {
            public bool Blocked;
            public bool TryChoose(Eldoria.Domain.PlayerState state,string id,string previous,int generation,
                out Eldoria.Domain.WorldCreatureLocation result)
            {
                result=new Eldoria.Domain.WorldCreatureLocation{
                    TargetId=id,SiteId=id+"site"+generation,CreatureKind=id.Contains(":boar:")?"boar":"spawnling",
                    Level=Math.Min(5,generation),Generation=generation,X=25*generation,Y=2,Z=55*generation};
                return !Blocked;
            }
        }
        sealed class CreatureRoutes : Eldoria.Application.IWorldMarchRoutes
        {
            public bool TryRoute(Eldoria.Domain.PlayerState state,string id,
                out System.Collections.Generic.List<Eldoria.Domain.WorldRoutePoint> route)
            {
                route=new System.Collections.Generic.List<Eldoria.Domain.WorldRoutePoint>{
                    new Eldoria.Domain.WorldRoutePoint{X=15,Y=2,Z=55}};
                return true;
            }
        }
        [Test]
        public void TripoBoarAndEngendroLevelsKeepTheOriginalGatewayCombatAndOfflineSave()
        {
            var clock=new PopulationClock();var store=new PopulationStore();
            store.Save(new Eldoria.Domain.PlayerState{ScoutDefeated=true,EngendroDefeated=true});
            store.Json=store.Json.Replace(",\"WorldCreatureLocations\":[]","");
            var gateway=new Eldoria.Application.LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().WorldCreatureLocations,Is.Empty,"Load a genuine legacy schema-one save");
            gateway.ConfigureLocalMarchRoutes(new CreatureRoutes());
            var placement=new CreaturePlacement();
            gateway.ConfigureLocalCreatureRespawn(placement,TimeSpan.FromMinutes(15));
            gateway.EnsureLocalCreaturePopulation(3,3);
            var original=gateway.Snapshot();
            Assert.That(original.WorldCreatureLocations.Count,Is.EqualTo(6));
            Assert.That(original.WorldCreatureLocations.All(x=>x.Alive&&x.Generation==1),Is.True);
            Assert.That(original.WorldCreatureLocations.Select(x=>x.TargetId).Distinct().Count(),Is.EqualTo(6));
            foreach(string family in new[]{"boar","spawnling"})
            {
                var last=Eldoria.Domain.WorldCreatureCatalog.For(family,1);
                for(int level=2;level<=5;level++)
                {
                    var next=Eldoria.Domain.WorldCreatureCatalog.For(family,level);
                    Assert.That(next.Health,Is.GreaterThan(last.Health));
                    Assert.That(next.Attack,Is.GreaterThan(last.Attack));
                    Assert.That(next.Defense,Is.GreaterThan(last.Defense));
                    Assert.That(next.RewardWood,Is.GreaterThan(last.RewardWood));
                    last=next;
                }
            }
            var standard=Eldoria.Domain.WorldCreatureCatalog.For("spawnling",3);
            Assert.That(standard.Health,Is.EqualTo(760));
            Assert.That(standard.Attack,Is.EqualTo(84));
            Assert.That(standard.Defense,Is.EqualTo(72));
            string id="region1:boar:001";
            var command=new Eldoria.Application.GameCommand("combat-1",original.PlayerId,"Fight",id,original.Revision);
            Assert.That(gateway.Execute(command).Ok,Is.True);
            Assert.That(gateway.Execute(command).Ok,Is.True,"Original idempotency must remain");
            clock.UtcTicks+=TimeSpan.FromSeconds(Eldoria.Domain.SliceRules.TravelSeconds*2+1).Ticks;
            gateway.Advance();
            var battled=gateway.Snapshot();
            Assert.That(battled.March.Phase,Is.EqualTo("idle"));
            Assert.That(battled.LastBattleReport.TargetId,Is.EqualTo(id));
            Assert.That(battled.LastBattleReport.Won,Is.True);
            Assert.That(battled.Resources.Wood,Is.GreaterThan(original.Resources.Wood));
            var dead=Eldoria.Application.LocalGateway.Creature(battled,id);
            Assert.That(dead.Alive,Is.False);
            Assert.That(dead.RespawnUtcTicks,Is.GreaterThan(clock.UtcTicks));
            var restart=new Eldoria.Application.LocalGateway(clock,store);
            restart.ConfigureLocalMarchRoutes(new CreatureRoutes());
            restart.ConfigureLocalCreatureRespawn(placement,TimeSpan.FromMinutes(15));
            Assert.That(Eldoria.Application.LocalGateway.Creature(restart.Snapshot(),id).Alive,Is.False);
            clock.UtcTicks=dead.RespawnUtcTicks;
            placement.Blocked=true;restart.Advance();
            Assert.That(Eldoria.Application.LocalGateway.Creature(restart.Snapshot(),id).Alive,Is.False,"No route, no fake respawn");
            placement.Blocked=false;restart.Advance();
            var after=restart.Snapshot();
            var returned=Eldoria.Application.LocalGateway.Creature(after,id);
            Assert.That(returned.Alive,Is.True);
            Assert.That(returned.Generation,Is.EqualTo(2));
            Assert.That(returned.Level,Is.EqualTo(2));
            Assert.That(returned.SiteId,Is.Not.EqualTo(dead.SiteId));
            Assert.That(after.WorldCreatureLocations.Count,Is.EqualTo(6));
            Assert.That(after.ScoutDefeated&&after.EngendroDefeated,Is.True);
            var shared=restart.Snapshot();shared.WorldId="external-server";store.Save(shared);
            Assert.Throws<InvalidOperationException>(()=>
                new Eldoria.Application.LocalGateway(clock,store)
                    .ConfigureLocalCreatureRespawn(placement,TimeSpan.FromMinutes(15)));
        }

        [Serializable] sealed class EncounterFaultLayout { public EncounterFaultSegment[] faults; }
        [Serializable] sealed class EncounterFaultSegment { public float ax,az,bx,bz,radius; }
        [UnityTest]
        public IEnumerator TargetedEcotoneHasRealTwoDonorCanopiesAndDoesNotCreateGameplayNodes()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var forest=world.GetComponentsInChildren<MeshFilter>(true)
                .Where(x=>x.name.StartsWith("BLD_ForestMass_Ecotone_",StringComparison.Ordinal)).ToArray();
            Assert.That(forest.Length,Is.GreaterThanOrEqualTo(30),
                "Physical transition must break the former rectangular AF/AG woodland cutoff");
            Assert.That(forest.Select(x=>x.sharedMesh).Distinct().Count(),Is.LessThanOrEqualTo(2),
                "Use only the two previously imported immutable conifer source meshes");
            Assert.That(forest.All(x=>x.GetComponent<Collider>()==null),Is.True,
                "Ecotone trees are not additional fake game targets");
            var before=((Eldoria.Application.ICommandGateway)Member(Member(host,"City"),"Gateway")).Snapshot();
            Assert.That(before.WorldResourceLocations.Count(x=>x.TargetId.StartsWith("region1:")),Is.EqualTo(48));
            Assert.That(before.WorldCreatureLocations.Count,Is.EqualTo(36));
            Call(navigation,"SetZoomLevel",0,true);
            Call(Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .Single(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape"),
                "RefreshForCapture");
            Assert.That(JsonUtility.ToJson(((Eldoria.Application.ICommandGateway)Member(Member(host,"City"),"Gateway")).Snapshot()),
                Is.EqualTo(JsonUtility.ToJson(before)),"Panorama must retain all authentic resource and creature state");
        }

        [UnityTest]
        public IEnumerator RealTerrainHasBothTripoFamiliesAcrossFiveDangerLevels()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var state=((Eldoria.Application.ICommandGateway)Member(Member(host,"City"),"Gateway")).Snapshot();
            var placement=world.GetComponentsInChildren<MonoBehaviour>(true)
                .Single(x=>x&&x.GetType().Name=="RegionOneResourcePlacement");
            var routes=(Eldoria.Application.IWorldMarchRoutes)Member(placement,"MarchRoutes");
            Assert.That(state.WorldCreatureLocations.Count,Is.EqualTo(36));
            Assert.That(state.WorldCreatureLocations.Select(x=>x.TargetId).Distinct().Count(),Is.EqualTo(36));
            // Real imported fracture coordinates; not guessed camera-colour rings.
            var authored=Resources.Load<TextAsset>("World4X/Blender/production-region");
            Assert.That(authored,Is.Not.Null);
            var faults=JsonUtility.FromJson<EncounterFaultLayout>(authored.text).faults;
            Assert.That(faults,Is.Not.Null);
            float ToBreach(float x,float z)
            {
                float distance=float.MaxValue;
                foreach(var f in faults)
                {
                    float ax=f.ax,az=f.az,bx=f.bx,bz=f.bz;
                    var a=new Vector2(ax,az);var delta=new Vector2(bx-ax,bz-az);
                    float t=delta.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(new Vector2(x,z)-a,delta)/delta.sqrMagnitude):0;
                    distance=Mathf.Min(distance,Vector2.Distance(new Vector2(x,z),a+delta*t));
                }
                return distance;
            }
            foreach(string family in new[]{"boar","spawnling"})
            {
                var alive=state.WorldCreatureLocations.Where(x=>x.Alive&&x.CreatureKind==family).ToArray();
                Assert.That(alive.Length,Is.EqualTo(18),"Original Tripo family missing real encounter sites: "+family);
                Assert.That(alive.Select(x=>x.Level).Distinct(),Is.EquivalentTo(new[]{1,2,3,4,5}),
                    "The whole Region1 must exhibit all five authorized power tiers: "+family);
                var home=(Vector3)Member(host,"WorldHome");
                float low=alive.Where(x=>x.Level==1).Average(x=>ToBreach(x.X-home.x,x.Z-home.z));
                float high=alive.Where(x=>x.Level==5).Average(x=>ToBreach(x.X-home.x,x.Z-home.z));
                Assert.That(high,Is.LessThan(low),"The real fault must drive creature danger, not artificial rings");
                foreach(var item in alive)
                {
                    var point=new Vector3(item.X,item.Y,item.Z);
                    Assert.That((bool)Call(routes,"FootprintClear",point,10f),Is.True,
                        "Encounter intersects an original or authored forest silhouette: "+item.TargetId);
                    var target=Call(host,"FindTarget",item.TargetId) as Component;
                    Assert.That(target,Is.Not.Null,item.TargetId+" is not an actual interactive world object");
                    Assert.That(target.transform.position,Is.EqualTo(point),
                        "Zoom/entity presentation must share one persistent coordinate");
                }
            }
        }


        [UnityTest]
        public IEnumerator BothTripoFamiliesRemainReadableInBothRegionalOrientations()
        {
            Assert.That(Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE"),Is.EqualTo("1"),
                "Legibility verification requires the existing disposable gameplay probe.");
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            bool opened=(bool)Call(host,"TrySetWorld",true);
            if(!opened)
            {
                var offer=Member(Member(Call(host,"Read","sawmill",false),"Building"),"Action");
                Assert.That(offer,Is.Not.Null);
                Assert.That(Member(offer,"Kind"),Is.EqualTo("Build"));
                Call(host,"Submit",offer);
                var city=Member(host,"City");
                Call(Member(city,"Clock"),"Step",Eldoria.Domain.SliceRules.SawmillBuildSeconds+1);
                Call(Member(city,"Gateway"),"Advance");
                yield return null;
                opened=(bool)Call(host,"TrySetWorld",true);
            }
            Assert.That(opened&&world.activeInHierarchy&&camera.enabled,Is.True,
                "Measure targets after entering Mundo through the real gameplay gate.");
            var adapter=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Single(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape");
            var gateway=(Eldoria.Application.ICommandGateway)Member(Member(host,"City"),"Gateway");
            var state=gateway.Snapshot();string saved=JsonUtility.ToJson(state);
            float oldAspect=camera.aspect;
            try
            {
                foreach(float aspect in new[]{16f/9f,390f/844f})
                {
                    camera.aspect=aspect;Call(adapter,"RefreshForCapture");
                    Call(navigation,"SetZoomLevel",1,true);
                    yield return null;
                    Call(adapter,"RefreshForCapture");
                    foreach(string family in new[]{"boar","spawnling"})
                    {
                        var site=state.WorldCreatureLocations.First(x=>x.Alive&&x.CreatureKind==family);
                        var target=Call(host,"FindTarget",site.TargetId) as Component;
                        var far=target.GetComponentsInChildren<Transform>(true)
                            .Single(x=>x.name=="Original Tripo "+family+" far "+site.TargetId);
                        Assert.That(far.gameObject.activeInHierarchy,Is.True);
                        var renderers=far.GetComponentsInChildren<Renderer>(true);
                        Assert.That(renderers.Length,Is.GreaterThan(0));
                        var bounds=renderers[0].bounds;
                        foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                        float minimum=float.PositiveInfinity,maximum=float.NegativeInfinity;
                        for(int corner=0;corner<8;corner++)
                        {
                            var p=bounds.center+Vector3.Scale(bounds.extents,new Vector3(
                                (corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
                            float y=camera.WorldToViewportPoint(p).y;
                            minimum=Mathf.Min(minimum,y);maximum=Mathf.Max(maximum,y);
                        }
                        float height=maximum-minimum;
                        Assert.That(height,Is.InRange(.035f,.18f),
                            family+" needs a readable regional silhouette without covering the map; aspect="+aspect);
                        Assert.That(target.GetComponent<Collider>().enabled,Is.True);
                        Assert.That(target.GetComponent<Collider>().bounds.Contains(bounds.center),Is.True,
                            "The visible encounter must remain selectable at its own model centre");
                        Assert.That(target.transform.position,Is.EqualTo(new Vector3(site.X,site.Y,site.Z)));
                        TestContext.WriteLine("ACTUAL_REGIONAL_CREATURE_VIEWPORT family="+family+
                            " aspect="+aspect+" heightFraction="+height+" zoom="+camera.orthographicSize);
                    }
                    foreach(string kind in new[]{"wood","stone"})
                    {
                        var site=state.WorldResourceLocations.First(x=>x.Generation>0&&x.ResourceKind==kind&&
                            Eldoria.Application.LocalGateway.ResourceRemaining(state,x.TargetId)>0);
                        var target=Call(host,"FindTarget",site.TargetId) as Component;
                        var image=world.GetComponentsInChildren<MeshRenderer>(true)
                            .Single(x=>x.name=="Production resource impostor "+site.TargetId);
                        Assert.That(image.enabled&&image.gameObject.activeInHierarchy,Is.True);
                        var offsets=image.GetComponent<MeshFilter>().sharedMesh.uv2;
                        float scale=image.sharedMaterial.GetFloat("_BillboardScale");
                        float lo=offsets.Min(x=>x.y),hi=offsets.Max(x=>x.y);
                        var bottom=image.transform.position+camera.transform.up*lo*scale;
                        var top=image.transform.position+camera.transform.up*hi*scale;
                        float footprint=camera.WorldToViewportPoint(top).y-camera.WorldToViewportPoint(bottom).y;
                        Assert.That(footprint,Is.InRange(.06f,.11f),
                            kind+" must retain a readable actual-model image in both regional orientations");
                        var centre=(bottom+top)*.5f;
                        Physics.SyncTransforms();
                        var ray=camera.ViewportPointToRay(camera.WorldToViewportPoint(centre));
                        Assert.That(target.GetComponent<Collider>().Raycast(ray,out _,100000f),Is.True,
                            "The visible regional resource image must select its same saved gameplay target");
                        Assert.That(target.transform.position,Is.EqualTo(new Vector3(site.X,site.Y,site.Z)));
                        TestContext.WriteLine("ACTUAL_REGIONAL_RESOURCE_VIEWPORT kind="+kind+
                            " aspect="+aspect+" heightFraction="+footprint+" zoom="+camera.orthographicSize);
                    }
                    Call(navigation,"SetZoomLevel",0,true);Call(adapter,"RefreshForCapture");
                    foreach(var renderer in world.GetComponentsInChildren<Renderer>(true)
                        .Where(x=>x.transform.GetComponentsInParent<Transform>(true)
                            .Any(t=>t.name.StartsWith("Original Tripo ",StringComparison.Ordinal))))
                        Assert.That(renderer.enabled&&renderer.gameObject.activeInHierarchy,Is.False,
                            "Panorama must hide creature models and their shadows in both orientations");
                }
                Assert.That(JsonUtility.ToJson(gateway.Snapshot()),Is.EqualTo(saved));
            }
            finally
            {
                camera.aspect=oldAspect;
                Call(navigation,"SetZoomLevel",1,true);Call(adapter,"RefreshForCapture");
            }
        }

        [Test]
        public void LegacySchemaOneJsonWithoutPopulationFieldPreservesTheWholeSnapshot()
        {
            var clock=new PopulationClock();var store=new PopulationStore();
            store.Save(new Eldoria.Domain.PlayerState{BastionLevel=2,ScoutDefeated=true,EngendroDefeated=true});
            var prepared=new Eldoria.Application.LocalGateway(clock,store).Snapshot();
            store.Save(prepared);string baseline=store.Json;
            Assert.That(baseline,Does.Contain(",\"WorldResourceLocations\":[]"));
            // Exact legacy field absence, through the real JSON store and gateway constructor.
            store.Json=baseline.Replace(",\"WorldResourceLocations\":[]","");
            Assert.That(store.Json,Does.Not.Contain("WorldResourceLocations"));
            var loaded=new Eldoria.Application.LocalGateway(clock,store);
            Assert.That(JsonUtility.ToJson(loaded.Snapshot()),Is.EqualTo(baseline),"Loading legacy schema 1 must preserve every existing serialized field");
            loaded.ConfigureLocalResourceRespawn(new PopulationPlacement(),TimeSpan.FromMinutes(30));loaded.EnsureLocalRenewablePopulation(24,24);
            var after=loaded.Snapshot();Assert.That(after.WorldResourceLocations.Count,Is.EqualTo(48));
            Assert.That(after.BastionLevel,Is.EqualTo(prepared.BastionLevel));
            Assert.That(after.ScoutDefeated&&after.EngendroDefeated,Is.True);
            Assert.That(JsonUtility.ToJson(after.Resources),Is.EqualTo(JsonUtility.ToJson(prepared.Resources)));
            var restarted=new Eldoria.Application.LocalGateway(clock,store).Snapshot();
            Assert.That(restarted.WorldResourceLocations.Select(x=>x.TargetId).Distinct().Count(),Is.EqualTo(48));
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
        public IEnumerator PanoramaHidesRealResourcesAndEncountersWithoutChangingSavedState()
        {
            MonoBehaviour host=null;GameObject world=null;Component navigation=null;Camera camera=null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var adapter=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Single(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape");
            var gateway=(Eldoria.Application.ICommandGateway)Member(Member(host,"City"),"Gateway");
            var snapshot=gateway.Snapshot();
            string initial=JsonUtility.ToJson(snapshot);
            var ids=new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}
                .Concat(snapshot.WorldResourceLocations.Where(x=>x.TargetId.StartsWith("region1:"))
                    .Select(x=>x.TargetId)).Distinct().ToArray();
            var sites=ids.ToDictionary(id=>id,id=>{
                var spot=Call(host,"FindTarget",id) as Component;
                Assert.That(spot,Is.Not.Null,"Missing existing gameplay ID: "+id);
                return spot.transform.position;
            });
            Call(navigation,"SetZoomLevel",1,true);
            Call(adapter,"RefreshForCapture");
            Assert.That(camera.orthographicSize,Is.LessThan((float)Member(navigation,"globalZoom")*.85f));
            var visuals=world.GetComponentsInChildren<Transform>(true)
                .Where(x=>x.name.StartsWith("Production resource visual ",StringComparison.Ordinal)||
                    x.name.StartsWith("Production resource impostor ",StringComparison.Ordinal)).ToArray();
            Assert.That(visuals.Length,Is.GreaterThanOrEqualTo(48));
            Assert.That(visuals.SelectMany(x=>x.GetComponentsInChildren<Renderer>(true))
                .Any(x=>x.enabled&&x.name.StartsWith("Production resource impostor ",StringComparison.Ordinal)),
                Is.True,"Regional view lost the real resources");
            Call(navigation,"SetZoomLevel",0,true);
            Call(adapter,"RefreshForCapture");
            foreach(var id in ids)
            {
                var spot=Call(host,"FindTarget",id) as Component;
                Assert.That(spot.GetComponent<Collider>().enabled,Is.False,"Panorama can pick "+id);
                Assert.That(spot.transform.position,Is.EqualTo(sites[id]),"Panorama displaced "+id);
            }
            foreach(var renderer in visuals.SelectMany(x=>x.GetComponentsInChildren<Renderer>(true)))
                Assert.That(renderer.enabled,Is.False,"Panorama rendered "+renderer.name);
            var actor=world.GetComponentsInChildren<MonoBehaviour>(true)
                .FirstOrDefault(x=>x&&x.GetType().Name=="SpawnlingEncounterVisual");
            Assert.That(actor,Is.Not.Null,"The original Tripo engendro is required");
            var model=Member(actor,"ModelRoot") as GameObject;
            Assert.That(model,Is.Not.Null);
            Assert.That(model.activeSelf,Is.False,
                "Even with the collider disabled, the original Pick() fallback must be gated");
            Assert.That(model.GetComponentsInChildren<Renderer>(true).All(x=>!x.enabled||!x.gameObject.activeInHierarchy),Is.True,
                "The engendro remained visible in panorama");
            Assert.That(JsonUtility.ToJson(gateway.Snapshot()),Is.EqualTo(initial),
                "Visual zoom changed authoritative save/gameplay");
            Call(navigation,"SetZoomLevel",1,true);
            Call(adapter,"RefreshForCapture");
            if(!snapshot.EngendroDefeated)
                Assert.That(model.activeSelf,Is.True,"The original actor did not return in regional view");
            Assert.That(visuals.SelectMany(x=>x.GetComponentsInChildren<Renderer>(true))
                .Any(x=>x.enabled&&x.name.StartsWith("Production resource impostor ",StringComparison.Ordinal)),Is.True);
            foreach(var id in ids)
            {
                var spot=Call(host,"FindTarget",id) as Component;
                Assert.That(spot.transform.position,Is.EqualTo(sites[id]),"Regional return displaced "+id);
                if(Eldoria.Application.LocalGateway.ResourceKind(snapshot,id)!=null&&
                    Eldoria.Application.LocalGateway.ResourceRemaining(snapshot,id)>0)
                    Assert.That(spot.GetComponent<Collider>().enabled,Is.True,"Picking not restored for "+id);
            }
            Assert.That(JsonUtility.ToJson(gateway.Snapshot()),Is.EqualTo(initial));
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
            Debug.Log("REGION1_ACTUAL_CITY_RESERVES valid="+Member(placement,"ValidatedCityCount")+" accessible="+Member(placement,"AccessibleCityCount"));
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
            foreach(var item in snapshot.WorldResourceLocations.Where(x=>x.Generation>0&&x.TargetId.StartsWith("region1:",StringComparison.Ordinal)))
            {
                var point=new Vector3(item.X,item.Y,item.Z);
                Assert.That((bool)Call(routes,"FootprintClear",point,9f),Is.True,"Resource intersects original or continental canopy: "+item.TargetId);
                // Independent renderer-bounds oracle also verifies the index has no false negatives.
                foreach(var renderer in world.GetComponentsInChildren<Renderer>(true).Where(x=>x.name.StartsWith("Pilot_Pine",StringComparison.Ordinal)||x.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal))){
                    var b=renderer.bounds;b.Expand(18);
                    Assert.That(point.x>=b.min.x&&point.x<=b.max.x&&point.z>=b.min.z&&point.z<=b.max.z,Is.False,item.TargetId+" overlaps "+renderer.name);
                }
            }
            var domainOrigin=(Vector3)Member(host,"WorldHome");
            Assert.That(snapshot.WorldResourceLocations.Any(x=>x.Generation>0&&x.Z-domainOrigin.z>1000),Is.True,"Northern continuation must contain reachable real population");
            Assert.That(snapshot.WorldResourceLocations.Any(x=>x.Generation>0&&x.Z-domainOrigin.z< -1000),Is.True,"Southern continuation must contain reachable real population");
            // Exercise occupancy invalidation on the actual imported terrain, without
            // changing a GameObject, gateway state, save or production resource identity.
            var farTarget=snapshot.WorldResourceLocations.Where(x=>x.Generation>0)
                .OrderByDescending(x=>new Vector2(x.X,x.Z).sqrMagnitude).First().TargetId;
            Assert.That(routes.TryRoute(snapshot,farTarget,out var before),Is.True);
            Assert.That(before.Count,Is.GreaterThan(6));
            var changed=JsonUtility.FromJson<Eldoria.Domain.PlayerState>(JsonUtility.ToJson(snapshot));
            var moved=changed.WorldResourceLocations.First(x=>x.TargetId!=farTarget&&x.Generation>0);
            var middle=before[before.Count/2];moved.X=middle.X;moved.Y=middle.Y;moved.Z=middle.Z;moved.Remaining=100;
            if(routes.TryRoute(changed,farTarget,out var detour))
                foreach(var point in detour)Assert.That(Vector2.Distance(new Vector2(point.X,point.Z),new Vector2(moved.X,moved.Z)),Is.GreaterThanOrEqualTo(7),"A changed occupied cell must invalidate cached connectivity");
            Assert.That(routes.TryRoute(snapshot,farTarget,out var restored),Is.True);
            Assert.That(restored.Select(p=>new Vector3(p.X,p.Y,p.Z)),Is.EqualTo(before.Select(p=>new Vector3(p.X,p.Y,p.Z))),"Restoring occupancy must restore the deterministic terrain route");

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
                foreach(float z in new[]{-1700f,-1450f,-1100f,1100f,1450f,1700f})for(float x=-750;x<=650;x+=73){
                    Assert.That(collider.Raycast(new Ray(origin+new Vector3(x,250,z),Vector3.down),out var hit,500),Is.True,"Extended continent requires real terrain");
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
            var gateway=(Eldoria.Application.LocalGateway)Member(Member(host,"City"),"Gateway");
            var expectedIds=gateway.Snapshot().WorldResourceLocations
                .Where(x=>x.Generation>0&&x.TargetId.StartsWith("region1:",StringComparison.Ordinal)).Select(x=>x.TargetId)
                .Concat(new[]{"forest-valoria","quarry-valoria"}).ToArray();
            var actualRoots=world.GetComponentsInChildren<Transform>(true)
                .Where(x=>x.name.StartsWith("Production resource visual ",StringComparison.Ordinal)).ToArray();
            Assert.That(actualRoots.Select(x=>x.name.Substring("Production resource visual ".Length)),Is.EquivalentTo(expectedIds),
                "Measure every actual saved resource and both campaign nodes; no missing or duplicate visual roots");
            foreach(var root in actualRoots)Assert.That(root.GetComponentsInChildren<MeshFilter>(true),Is.Not.Empty,"Every real node needs measured model geometry");
            float minimum=0,maximum=0;int parts=0;
            foreach(var root in actualRoots)
            foreach(var part in root.GetComponentsInChildren<MeshFilter>(true)){
                var points=part.sharedMesh.vertices.Select(part.transform.TransformPoint).ToArray();float bottom=points.Min(p=>p.y);
                foreach(var p in points.Where(p=>p.y<bottom+.03f)){float ground=origin.y+(float)height.Invoke(null,new object[]{p.x-origin.x,p.z-origin.z});minimum=Mathf.Min(minimum,p.y-ground);maximum=Mathf.Max(maximum,p.y-ground);}parts++;
            }
            // Approved coherent models join structural parts so the crane and canopy
            // remain assembled while grounding. Coverage follows the complete real
            // resource inventory, independent of the donor's old fragment count.
            Assert.That(parts,Is.GreaterThanOrEqualTo(actualRoots.Length));Assert.That(maximum,Is.LessThan(.005f),"Floating resource supports");Assert.That(minimum,Is.GreaterThan(-.65f),"Excessive burial must be repaired, not hidden with collider offsets");
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
            var adapter=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape");
            var woodPosition=wood.transform.position;
            float oldAspect=camera.aspect;
            float oldZoom=camera.orthographicSize;
            var oldTarget=(Vector3)Member(navigation,"target");
            try
            {
                foreach (float aspect in new[]{1280f/720f,390f/844f})
                {
                    camera.aspect=aspect;Call(adapter,"RefreshForCapture");
                    Call(navigation,"SetZoomLevel",0,true);Call(adapter,"RefreshForCapture");
                    Assert.That(Vector3.Dot(camera.transform.right,aspect>1?Vector3.back:Vector3.right),Is.GreaterThan(.999f),"Whole-region camera must use the intended orientation, not rescale geography");
                    var groundArgs=new object[]{camera.pixelRect.center,Vector3.zero};
                    Assert.That((bool)navigation.GetType().GetMethod("GroundPoint").Invoke(navigation,groundArgs),Is.True);
                    Assert.That(Vector3.Distance((Vector3)groundArgs[1],(Vector3)Member(navigation,"target")),Is.LessThan(.02f),"Rotated camera centre must still resolve to its real world target");
                    Call(navigation,"SetZoomLevel",2,true);
                    Call(navigation,"PanTo",wood.transform.position);Call(adapter,"RefreshForCapture");
                    Assert.That(Vector3.Dot(camera.transform.right,Vector3.right),Is.GreaterThan(.999f),"Return to close view must restore the original heading");
                    Assert.That(wood.transform.position,Is.EqualTo(woodPosition),"Camera orientation cannot relocate the resource");
                    var pos=camera.WorldToViewportPoint(wood.transform.position);
                    Assert.That(pos.z, Is.GreaterThan(camera.nearClipPlane));
                    Assert.That(pos.x, Is.InRange(.10f,.90f),
                        "Actual wood target cannot be centred within horizontally bounded near view.");
                    Assert.That(pos.y, Is.InRange(.10f,.90f),
                        "Actual wood target cannot be centred within vertically bounded near view.");
                    var minimum=(Vector2)Member(navigation,"minimum");var maximum=(Vector2)Member(navigation,"maximum");
                    var centre=new Vector3((minimum.x+maximum.x)*.5f,woodPosition.y,(minimum.y+maximum.y)*.5f);
                    Call(navigation,"EnterStrategicView",centre);
                    // Test the SAME frame, before the landscape's LateUpdate
                    // can repair an insufficient camera distance after zoom.
                    foreach(var offset in new[]{Vector2.right*80,Vector2.left*80,Vector2.up*80,Vector2.down*80}){
                        var rayArgs=new object[]{camera.pixelRect.center+offset,Vector3.zero};
                        Assert.That((bool)navigation.GetType().GetMethod("GroundPoint").Invoke(navigation,rayArgs),Is.True,"Every strategic-entry gesture ray must meet real ground immediately");
                    }
                    Call(adapter,"RefreshForCapture");
                    Assert.That(camera.orthographicSize,Is.GreaterThan((float)Member(navigation,"localZoom")*3),"Strategic entry must remain materially wider than close view");
                    foreach(var delta in new[]{Vector3.right*80,Vector3.left*80,Vector3.forward*80,Vector3.back*80}){
                        Call(navigation,"PanTo",centre);var start=(Vector3)Member(navigation,"target");
                        Call(navigation,"PanTo",start+delta);
                        Assert.That(Vector3.Dot((Vector3)Member(navigation,"target")-start,delta.normalized),Is.GreaterThan(.5f),"Strategic entry must retain real ground movement in every direction and orientation");
                    }
                }
            }
            finally
            {
                camera.aspect=oldAspect;
                Call(navigation,"RestoreView",oldTarget,oldZoom);Call(adapter,"RefreshForCapture");
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
            float maximumFloorGapError=0f;
            foreach(var vertex in filter.sharedMesh.vertices){
                var point=filter.transform.TransformPoint(vertex);
                float expected=home.y+(float)h.Invoke(null,new object[]{point.x-home.x,point.z-home.z});
                maximumFloorGapError=Mathf.Max(maximumFloorGapError,Mathf.Abs(point.y-expected-.015f));
            }
            Assert.That(maximumFloorGapError,Is.LessThan(.005f),"Every Breach floor vertex must follow the actual imported terrain, not bridge it with a floating plate");
            var adapter=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(x=>x&&x.GetType().Name=="RegionOneCanonicalLandscape");
            var fixedVertices=filter.sharedMesh.vertices;var fixedPosition=filter.transform.position;
            var continuation=geo.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="BLD_ContinentalContinuation");
            var regionalGround=geo.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="BLD_Terrain");
            var continuationVertices=continuation.sharedMesh.vertices;
            var continuationPosition=continuation.transform.position;
            var continuationWorld=continuationVertices.Select(continuation.transform.TransformPoint).ToArray();
            var edgeLookup=continuationWorld.GroupBy(p=>new Vector2Int(Mathf.RoundToInt(p.x*1000),Mathf.RoundToInt(p.z*1000))).ToDictionary(g=>g.Key,g=>g.First());
            int sharedEdge=0;
            foreach(var vertex in regionalGround.sharedMesh.vertices)
            {
                var point=regionalGround.transform.TransformPoint(vertex);var relative=point-home;
                if(Mathf.Abs(Mathf.Abs(relative.x)-910)>.002f && Mathf.Abs(Mathf.Abs(relative.z)-1800)>.002f)continue;
                var key=new Vector2Int(Mathf.RoundToInt(point.x*1000),Mathf.RoundToInt(point.z*1000));
                Assert.That(edgeLookup.ContainsKey(key),Is.True,"Every actual regional boundary vertex must continue into authored neighbouring land");
                Assert.That(Vector3.Distance(point,edgeLookup[key]),Is.LessThan(.003f),"Continental continuation must share the real ground edge without a gap or overlay");
                sharedEdge++;
            }
            Assert.That(sharedEdge,Is.GreaterThan(500));
            Debug.Log("REGION1_ACTUAL_CONTINENTAL_SHARED_EDGE_VERTICES "+sharedEdge);
            foreach(int level in new[]{2,0,1})
            {
                Call(navigation,"SetZoomLevel",level,true);Call(adapter,"RefreshForCapture");
                Assert.That(scar.enabled,Is.True);
                Assert.That(filter.sharedMesh.vertices,Is.EqualTo(fixedVertices));
                Assert.That(filter.transform.position,Is.EqualTo(fixedPosition),"Zoom must not relocate geology.");
                Assert.That(continuation.GetComponent<MeshRenderer>().enabled,Is.True);
                Assert.That(continuation.sharedMesh.vertices,Is.EqualTo(continuationVertices));
                Assert.That(continuation.transform.position,Is.EqualTo(continuationPosition),"Neighbouring land must be the same physical geometry at every zoom.");
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
                Assert.That(allActualArt.All(x => !x.enabled || !x.gameObject.activeInHierarchy), Is.True,
                    "Panorama must not render old scenery, enemy meshes or cast their shadows. Inactive Tripo model roots are deliberately preserved, not deleted.");
                Assert.That(terrain.All(x => !x.enabled), Is.True,
                    "Panorama must not reveal the old hard-edged rectangular prairie.");
                var canopies = world.GetComponentsInChildren<MeshFilter>(true).Where(t=>t.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)||t.name.StartsWith("BLD_ForestFar_",StringComparison.Ordinal)).ToArray();
                Assert.That(canopies.Length,Is.GreaterThanOrEqualTo(40),"Both real canopy LOD populations must remain imported");
                // Equal projected granularity across the protected/continental seam:
                // compare actual imported crown bounds, not implementation scales.
                var retainedFar=world.GetComponentsInChildren<MeshRenderer>(true)
                    .Where(r=>r.name.StartsWith("Far_Pilot_Pine",StringComparison.Ordinal)&&r.gameObject.activeSelf).ToArray();
                var continentFar=world.GetComponentsInChildren<MeshRenderer>(true)
                    .Where(r=>r.name.StartsWith("BLD_ForestFar_",StringComparison.Ordinal)&&r.gameObject.activeSelf).ToArray();
                Assert.That(retainedFar.Length,Is.GreaterThan(10),"The same retained woodland needs a legible strategic representation.");
                var retainedWidths=retainedFar.Select(r=>r.bounds.size.x).OrderBy(x=>x).ToArray();
                var continentWidths=continentFar.Select(r=>r.bounds.size.x).OrderBy(x=>x).ToArray();
                var retainedHeights=retainedFar.Select(r=>r.bounds.size.y).OrderBy(x=>x).ToArray();
                var continentHeights=continentFar.Select(r=>r.bounds.size.y).OrderBy(x=>x).ToArray();
                Assert.That(retainedWidths[retainedWidths.Length/2]/continentWidths[continentWidths.Length/2],Is.InRange(.65f,1.45f),
                    "Regional crown footprints must not expose the old rectangular sector through a finer tree scale.");
                Assert.That(retainedHeights[retainedHeights.Length/2]/continentHeights[continentHeights.Length/2],Is.InRange(.65f,1.45f),
                    "Regional crown height must remain coherent across the same continuous world.");
                Assert.That(canopies.All(t=>t.gameObject.activeSelf), Is.True,
                    "Panorama must retain forest silhouettes, not blank green circles.");
                Call(navigation, "SetZoomLevel", 1, true);
                Call(adapter,"RefreshForCapture");
                Assert.That(terrain.All(x => !x.enabled), Is.True,
                    "Regional view must not retain the legacy rectangular terrain edge.");
                Assert.That(canopies.All(t=>t.gameObject.activeSelf), Is.True,
                    "Regional view must retain shared woodland canopies, not only flat forest masks.");
                var closeDetail=world.GetComponentsInChildren<MeshRenderer>(true).Where(r=>
                    r.GetComponentsInParent<Transform>(true).Any(t=>t.name=="Blender authored geography")&&
                    r.sharedMaterials.Any(m=>m&&(m.name.StartsWith("Near underbrush",StringComparison.Ordinal)||
                        m.name.StartsWith("Near riverbank living shrubs",StringComparison.Ordinal)||
                        m.name.StartsWith("Near dry scrub",StringComparison.Ordinal)||
                        m.name.StartsWith("Near grass shade",StringComparison.Ordinal)||
                        m.name.StartsWith("Near meadow dry grass",StringComparison.Ordinal)||
                        m.name.StartsWith("Near bank soft olive",StringComparison.Ordinal)||
                        m.name.StartsWith("Understory ",StringComparison.Ordinal)||
                        m.name.StartsWith("Z woodland understory",StringComparison.Ordinal)))).ToArray();
                Assert.That(closeDetail.Length,Is.GreaterThanOrEqualTo(4),"Inspect the actual multi-material detail primitives");
                foreach(var renderer in closeDetail){
                    TestContext.WriteLine("ACTUAL_REGIONAL_GROUND_DETAIL name="+renderer.name+" parent="+renderer.transform.parent.name+" enabled="+renderer.enabled+" bounds="+renderer.bounds);
                    Assert.That(renderer.enabled,Is.False,"A close-only ground-detail primitive exposed the AF/AG rectangle: "+renderer.name);
                }
                foreach (float intermediateZoom in new [] { 55f, 90f, 145f, 153f, 113f })
                {
                    Call(navigation,"RestoreView",target,intermediateZoom);
                    Call(adapter,"RefreshForCapture");
                    Assert.That(canopies.All(t=>t.gameObject.activeSelf), Is.True,
                        "Woodland must not disappear between regional and panoramic marker LODs.");
                }
                Call(navigation,"SetZoomLevel",2,true);
                Call(adapter,"RefreshForCapture");
                Assert.That(canopies.All(t=>t.gameObject.activeSelf), Is.True,
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
            public string sourceScene, version, label, sha256, context, targetId;
            public bool actualUnityCamera, playMode, physicalIphone;
            public int width, height;
            public float zoom, aspect;
            public Vector3 cameraPosition, worldTarget;
            public Quaternion cameraRotation;
        }

        static void SaveRetainedSectorFrame(Camera camera,string path,int width,int height,string targetId=null,Vector3? worldTarget=null)
        {
            var previous=camera.targetTexture;var active=RenderTexture.active;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.aspect=width/(float)height;camera.targetTexture=rt;
                var sceneLights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
                var originalSun=RenderSettings.sun;
                var lightStates=sceneLights.Select(l=>(l,l.transform.rotation,l.shadows,l.shadowStrength,l.shadowBias,l.shadowNormalBias,l.enabled)).ToArray();
                var pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                var shadowProperty=pipeline?pipeline.GetType().GetProperty("shadowDistance"):null;
                var shadowDistance=shadowProperty?.GetValue(pipeline);var cascadeProperty=pipeline?pipeline.GetType().GetProperty("shadowCascadeCount"):null;var cascadeCount=cascadeProperty?.GetValue(pipeline);var background=camera.backgroundColor;
                camera.Render();
                Assert.That(camera.backgroundColor,Is.EqualTo(background),"Peripheral world atmosphere must restore camera clear colour");
                foreach(var state in lightStates)if(state.l){
                    Assert.That(Quaternion.Angle(state.l.transform.rotation,state.rotation),Is.LessThan(.001f),"World rendering must restore city light rotation");
                    Assert.That(state.l.shadows,Is.EqualTo(state.shadows));Assert.That(state.l.shadowStrength,Is.EqualTo(state.shadowStrength));Assert.That(state.l.shadowBias,Is.EqualTo(state.shadowBias));Assert.That(state.l.shadowNormalBias,Is.EqualTo(state.shadowNormalBias));Assert.That(state.l.enabled,Is.EqualTo(state.enabled));
                }
                Assert.That(RenderSettings.sun,Is.EqualTo(originalSun),"World sky fill must restore the city sun reference");
                if(shadowProperty!=null)Assert.That(shadowProperty.GetValue(pipeline),Is.EqualTo(shadowDistance),"World rendering must restore shared pipeline shadow distance");
                if(cascadeProperty!=null)Assert.That(cascadeProperty.GetValue(pipeline),Is.EqualTo(cascadeCount),"World rendering must restore shared shadow cascades");
                RenderTexture.active=rt;
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();var png=pixels.EncodeToPNG();
                Assert.That(png.Length,Is.GreaterThan(2500));File.WriteAllBytes(path+".png",png);
                string hash;using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(png)).Replace("-","").ToLowerInvariant();
                File.WriteAllText(path+".json",JsonUtility.ToJson(new RenderIdentity{
                    sourceScene=SceneManager.GetActiveScene().path,version=UnityEngine.Application.unityVersion,
                    context=path.Contains("breach")?"Actual playable camera at the same persistent Breach geology":path.Contains("continent")?"Actual production geography and resource camera across Region 1":"AF/AG camera and sector coordinates; restored scoped AG shader and lighting",label=Path.GetFileName(path),targetId=targetId,
                    sha256=hash,actualUnityCamera=true,playMode=UnityEngine.Application.isPlaying,physicalIphone=false,
                    width=width,height=height,zoom=camera.orthographicSize,aspect=camera.aspect,
                    cameraPosition=camera.transform.position,cameraRotation=camera.transform.rotation,worldTarget=worldTarget??Vector3.zero},true));
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
                            var framedMin=new Vector2(float.MaxValue,float.MaxValue);var framedMax=new Vector2(float.MinValue,float.MinValue);
                            foreach(var vertex in terrain.sharedMesh.vertices)
                            {
                                var framed=camera.WorldToViewportPoint(terrain.transform.TransformPoint(vertex));
                                framedMin=Vector2.Min(framedMin,new Vector2(framed.x,framed.y));framedMax=Vector2.Max(framedMax,new Vector2(framed.x,framed.y));
                                Assert.That(framed.z,Is.InRange(camera.nearClipPlane,camera.farClipPlane),"Region clipped by camera depth");
                                Assert.That(framed.x,Is.InRange(.005f,.995f),"Panorama crops actual regional mesh horizontally");
                                Assert.That(framed.y,Is.InRange(.005f,.995f),"Panorama crops actual regional mesh vertically");
                            }
                            float occupied=(framedMax.x-framedMin.x)*(framedMax.y-framedMin.y);
                            Assert.That(occupied,Is.GreaterThan(.65f),"Both orientations must frame the actual full terrain without wasting most of the viewport");
                            Debug.Log("REGION1_ACTUAL_PANORAMA_VIEWPORT_AREA "+format.Item3+" "+occupied.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
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
                foreach(bool portrait in new[]{false,true})
                {
                    camera.aspect=portrait?9f/16:16f/9;Call(adapter,"RefreshForCapture");
                    float far=(float)Member(navigation,"globalZoom");
                    string sequence=Path.Combine(sectorDirectory,(portrait?"portrait":"landscape")+"-continent-zoom");Directory.CreateDirectory(sequence);
                    for(int frame=0;frame<=24;frame++)
                    {
                        float t=Mathf.SmoothStep(0,1,frame/24f);
                        Call(navigation,"RestoreView",home,Mathf.Lerp(portrait?64:36,far,t));Call(adapter,"RefreshForCapture");
                        yield return null;
                        SaveRetainedSectorFrame(camera,Path.Combine(sequence,"frame-"+frame.ToString("D3")),portrait?540:960,portrait?960:540);
                    }
                    Assert.That(Directory.GetFiles(sequence,"*.png").Length,Is.EqualTo(25));
                }
                // Move the real playable camera to the existing geological feature.
                // The source mesh, coordinates and persistent world are identical at all zooms.
                string breachDirectory=Path.Combine(sectorDirectory,"breach-details");Directory.CreateDirectory(breachDirectory);
                var breachFilter=world.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_BreachFloor");
                var breachMesh=breachFilter.sharedMesh;var breachPosition=breachFilter.transform.position;
                var breachFocus=home+new Vector3(-260f,0,150f);
                foreach(bool portrait in new[]{false,true})
                {
                    camera.aspect=portrait?9f/16:16f/9;
                    foreach(bool close in new[]{true,false})
                    {
                        Call(navigation,"RestoreView",breachFocus,close?(portrait?64f:36f):(portrait?456f:244f));
                        Call(adapter,"RefreshForCapture");yield return null;
                        var actualFocus=(Vector3)Member(navigation,"target");
                        Assert.That(Vector2.Distance(new Vector2(actualFocus.x,actualFocus.z),new Vector2(breachFocus.x,breachFocus.z)),Is.LessThan(.02f),"Playable camera must actually reach the Breach");
                        Assert.That(breachFilter.GetComponent<MeshRenderer>().enabled,Is.True);
                        Assert.That(breachFilter.sharedMesh,Is.SameAs(breachMesh));
                        Assert.That(breachFilter.transform.position,Is.EqualTo(breachPosition));
                        SaveRetainedSectorFrame(camera,Path.Combine(breachDirectory,(close?"close":"regional")+"-"+(portrait?"portrait":"landscape")),portrait?720:1280,portrait?1280:720,null,actualFocus);
                    }
                }
                Assert.That(Directory.GetFiles(breachDirectory,"*.png").Length,Is.EqualTo(4));
                string shadowStudy=Path.Combine(sectorDirectory,"shadow-study");Directory.CreateDirectory(shadowStudy);
                foreach(bool portrait in new[]{false,true})foreach(bool breach in new[]{false,true})foreach(bool panoramic in new[]{false,true})
                {
                    if(breach&&panoramic)continue;
                    camera.aspect=portrait?9f/16:16f/9;
                    var focus=breach?breachFocus:home;
                    Call(navigation,"RestoreView",focus,panoramic?(float)Member(navigation,"globalZoom"):(portrait?456f:280f));Call(adapter,"RefreshForCapture");yield return null;
                    string label=(breach?"breach":"home")+"-"+(panoramic?"panorama":"regional")+"-"+(portrait?"portrait":"landscape");
                    var studyPipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                    var distanceProperty=studyPipeline.GetType().GetProperty("shadowDistance");
                    var countProperty=studyPipeline.GetType().GetProperty("shadowCascadeCount");int baselineCascades=(int)countProperty.GetValue(studyPipeline);
                    var studySun=RenderSettings.sun;float oldBias=studySun?studySun.shadowBias:0;float oldNormal=studySun?studySun.shadowNormalBias:0;
                    System.Action<UnityEngine.Rendering.ScriptableRenderContext,Camera> baseline=(context,view)=>{
                        if(view!=camera)return;
                        distanceProperty.SetValue(studyPipeline,300f);countProperty.SetValue(studyPipeline,baselineCascades);
                        if(studySun){studySun.shadowBias=oldBias;studySun.shadowNormalBias=oldNormal;}
                    };
                    UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering+=baseline;
                    try{SaveRetainedSectorFrame(camera,Path.Combine(shadowStudy,label+"-fixed300"),portrait?720:1280,portrait?1280:720,null,(Vector3)Member(navigation,"target"));}
                    finally{UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering-=baseline;}
                    SaveRetainedSectorFrame(camera,Path.Combine(shadowStudy,label+"-ground-coverage"),portrait?720:1280,portrait?1280:720,null,(Vector3)Member(navigation,"target"));
                }
                Assert.That(Directory.GetFiles(shadowStudy,"*.png").Length,Is.EqualTo(12),"Actual same-camera fixed/adaptive shadow comparisons required");
                // Actual same-camera controls for the coherent meadow finish:
                // source textures/geometry/lighting are identical between each pair.
                var meadowLook=world.GetComponentsInChildren<MonoBehaviour>(true)
                    .Single(x=>x&&x.GetType().Name=="RegionOneLookDevelopment");
                try{
                    foreach(bool portrait in new[]{false,true})foreach(int level in new[]{2,1,0}){
                        camera.aspect=portrait?9f/16:16f/9;Call(adapter,"RefreshForCapture");
                        Call(navigation,"PanTo",home);Call(navigation,"SetZoomLevel",level,true);Call(adapter,"RefreshForCapture");
                        yield return null;
                        string label="pigment-"+new[]{"panorama","regional","close"}[level]+"-"+(portrait?"portrait":"landscape");
                        var focus=(Vector3)Member(navigation,"target");
                        Call(meadowLook,"SetMeadowFinish",false);
                        SaveRetainedSectorFrame(camera,Path.Combine(sectorDirectory,label+"-control"),portrait?720:1280,portrait?1280:720,null,focus);
                        Call(meadowLook,"SetMeadowFinish",true);
                        SaveRetainedSectorFrame(camera,Path.Combine(sectorDirectory,label+"-finished"),portrait?720:1280,portrait?1280:720,null,focus);
                    }
                }finally{Call(meadowLook,"SetMeadowFinish",true);}

                string continentalDetails=Path.Combine(sectorDirectory,"continental-details");Directory.CreateDirectory(continentalDetails);
                var playableGateway=(Eldoria.Application.LocalGateway)Member(Member(host,"City"),"Gateway");
                var actualResources=playableGateway.Snapshot().WorldResourceLocations;
                foreach(string kind in new[]{"wood","stone"})
                {
                    var item=actualResources.Where(x=>x.Generation>0&&x.TargetId.StartsWith("region1:"+kind+":",StringComparison.Ordinal)&&Mathf.Abs(x.X-home.x)>140)
                        .OrderBy(x=>Mathf.Abs(x.X-home.x)).First();
                    var target=(Component)Call(host,"FindTarget",item.TargetId);Assert.That(target,Is.Not.Null);
                    foreach(bool portrait in new[]{false,true}){
                        camera.aspect=portrait?9f/16:16f/9;Call(navigation,"RestoreView",target.transform.position,portrait?64f:36f);Call(adapter,"RefreshForCapture");
                        yield return null;
                        SaveRetainedSectorFrame(camera,Path.Combine(continentalDetails,kind+"-"+(portrait?"portrait":"landscape")),portrait?720:1280,portrait?1280:720,item.TargetId,target.transform.position);
                    }
                }
                                // Actual two-family Tripo evidence on the same continuous regional and
                // near game map. No fake copies or manually drawn proof frames.
                string creatureDir=Path.Combine(sectorDirectory,"creature-details");
                Directory.CreateDirectory(creatureDir);
                var creatureSnapshot=playableGateway.Snapshot();
                foreach(var kind in new[]{"boar","spawnling"})
                {
                    var entry=creatureSnapshot.WorldCreatureLocations
                        .FirstOrDefault(x=>x.Alive&&x.CreatureKind==kind);
                    Assert.That(entry,Is.Not.Null,"Real Tripo family missing from persistent playable world: "+kind);
                    var hotspot=(Component)Call(host,"FindTarget",entry.TargetId);
                    Assert.That(hotspot,Is.Not.Null,"Creature must have a real gameplay target: "+entry.TargetId);
                    foreach(bool portrait in new[]{false,true})
                    {
                        camera.aspect=portrait?9f/16:16f/9;
                        Call(navigation,"RestoreView",hotspot.transform.position,portrait?64f:36f);
                        Call(adapter,"RefreshForCapture");
                        yield return null;
                        SaveRetainedSectorFrame(camera,Path.Combine(creatureDir,kind+"-"+(portrait?"portrait":"landscape")),
                            portrait?720:1280,portrait?1280:720,entry.TargetId,hotspot.transform.position);
                    }
                }
                Assert.That(Directory.GetFiles(creatureDir,"*.png").Length,Is.EqualTo(4),
                    "Genuine near Tripo boar and engendro camera captures are required");
                Assert.That(Directory.GetFiles(continentalDetails,"*.png").Length,Is.EqualTo(4));
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
