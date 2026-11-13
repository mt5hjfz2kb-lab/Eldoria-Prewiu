using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.World4XCanon
{
    // Geometry service for the existing LocalGateway. Owns no timer or save file.
    public sealed class RegionOneResourcePlacement : MonoBehaviour, IWorldResourcePlacement, IWorldCreaturePlacement
    {
        [Serializable] public sealed class CityReserve {public float x,z,radius;[NonSerialized] public bool Validated;}
        [Serializable] public sealed class FaultSegment {public float ax,az,bx,bz,radius;}
        [Serializable] sealed class RegionLayout {public CityReserve[] cities; public FaultSegment[] faults;}
        FaultSegment[] faults=Array.Empty<FaultSegment>();
        public static bool FaultExcluded(FaultSegment[] segments,float x,float z,float clearance=0)
        {
            foreach(var f in segments){var a=new Vector2(f.ax,f.az);var d=new Vector2(f.bx-f.ax,f.bz-f.az);
                float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(new Vector2(x,z)-a,d)/d.sqrMagnitude):0;
                if(Vector2.Distance(new Vector2(x,z),a+d*t)<=f.radius+clearance)return true;}
            return false;
        }
        CityReserve[] cities=Array.Empty<CityReserve>();
        public int ValidatedCityCount {get;private set;}
        public int AccessibleCityCount {get;private set;}
        sealed class Site { public string Id; public Vector3 Point; public bool Wood; }
        readonly List<Site> sites = new List<Site>();
        readonly Dictionary<string,Vector3> initial = new Dictionary<string,Vector3>();
        ValoriaIntegratedWorld host;
        ICommandGateway bound;
        RegionOneTerrainRoutes routes;
        float nextRefresh;
        readonly Dictionary<Collider,bool> panoramaEncounterState=new Dictionary<Collider,bool>();
        public static bool IsPanoramic(float zoom,float globalZoom)=>globalZoom>0f&&zoom>=globalZoom*.85f;
        public void RefreshForZoom(){if(host&&host.Ready)Synchronize();}
        public int VerifiedSiteCount => sites.Count;
        public IWorldMarchRoutes MarchRoutes=>routes;
        public static void Install(Transform root, ValoriaIntegratedWorld host, RegionOneBlenderLandscape landscape)
        {
            var service=root.gameObject.AddComponent<RegionOneResourcePlacement>();
            service.host=host;
            service.Scan(landscape);
            service.Synchronize();
        }
        // Eager and lazy must share the IDENTICAL coordinate/query/order algorithm.
        // The eager wrapper consumes the iterator synchronously for native parity;
        // only the owner-only deferred path lets WebGL reach the end of each frame.
        public static IEnumerator InstallStaged(Transform root,ValoriaIntegratedWorld host,RegionOneBlenderLandscape landscape)
        {
            var service=root.gameObject.AddComponent<RegionOneResourcePlacement>();
            service.host=host;
            yield return service.ScanStaged(landscape);
            service.Synchronize();
        }
        void Scan(RegionOneBlenderLandscape landscape)
        {
            var stages=ScanStaged(landscape);
            try { while(stages.MoveNext()) {} }
            finally { (stages as IDisposable)?.Dispose(); }
        }
        IEnumerator ScanStaged(RegionOneBlenderLandscape landscape)
        {
            foreach(var id in new[]{"forest-valoria","quarry-valoria"})
            {
                var target=host.FindTarget(id);
                if(target)initial[id]=target.transform.position;
            }
            var layout=Resources.Load<TextAsset>("World4X/Blender/production-region");
            if(layout){var data=JsonUtility.FromJson<RegionLayout>(layout.text);cities=data.cities??Array.Empty<CityReserve>();faults=data.faults??Array.Empty<FaultSegment>();}
            using(var tape=new TerrainQueryTape(host,landscape))
            {
            RegionOneBlenderLandscape.SurfaceRaycast surface=tape.Raycast;
            {
                foreach(var city in cities)
                {
                    float low=float.MaxValue,high=float.MinValue;bool valid=!FaultExcluded(faults,city.x,city.z,city.radius);
                    for(int i=0;i<=16;i++){
                        float a=i*Mathf.PI/8;float x=city.x+(i==16?0:Mathf.Cos(a)*city.radius),z=city.z+(i==16?0:Mathf.Sin(a)*city.radius);
                        if(!surface(new Ray(host.WorldHome+new Vector3(x,250,z),Vector3.down),out var hit,500)){valid=false;break;}
                        low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);
                    }
                    if(valid&&high-low<.5f){city.Validated=true;ValidatedCityCount++;}
                }
                var slice=new RegionOneFrameSlice(8);
                for(int z=-1500;z<=1600;z+=32)
                {
                 for(int x=-750;x<=600;x+=32)
                 {
                    if(FaultExcluded(faults,x,z,8))continue;
                    bool safe=true;float low=float.MaxValue,high=float.MinValue;Vector3 centre=default;
                    for(int i=0;i<=8;i++)
                    {
                        float a=i*Mathf.PI/4;float xx=x+(i==8?0:Mathf.Cos(a)*8),zz=z+(i==8?0:Mathf.Sin(a)*8);
                        if(RegionOneBlenderLandscape.Shore(xx,zz)<12||RegionOneBlenderLandscape.Water(xx,zz)>.01f||RegionOneBlenderLandscape.Road(xx,zz)>.05f){safe=false;break;}
                        var ray=new Ray(host.WorldHome+new Vector3(xx,250,zz),Vector3.down);
                        if(!surface(ray,out var hit,500)||Vector3.Dot(hit.normal,Vector3.up)<.94f){safe=false;break;}
                        low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);if(i==8)centre=hit.point;
                    }
                    if(!safe||high-low>1.6f)continue;
                    bool wood=false;
                    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;wood|=RegionOneBlenderLandscape.Forest(x+Mathf.Cos(a)*24,z+Mathf.Sin(a)*24)>.15f;}
                    sites.Add(new Site{Id="region1:"+x+":"+z,Point=centre,Wood=wood});
                 }
                 // Preserve complete row/query order and the SAME tape;
                 // yield on elapsed work or at most eight rows, never all rows.
                 if(slice.ShouldYield()){yield return null;slice.Resume();}
                }
            }
            if(routes==null)
            {
                // Route-grid scans used to run ~100k cells in one WebGL frame,
                // even with the 32 m site scan staged. Keep tape open across yields.
                if(ValoriaIntegratedWorld.P0LazyWorldProbe)
                    yield return RegionOneTerrainRoutes.CreateStaged(host,surface,faults,ready=>routes=ready);
                else
                    routes=new RegionOneTerrainRoutes(host,surface,faults);
            }
            tape.Complete();
            }
            var snapshot=host.City.Gateway.Snapshot();
            foreach(var city in cities)if(city.Validated&&routes.TryRouteTo(snapshot,host.WorldHome+new Vector3(city.x,0,city.z),out var route))AccessibleCityCount++;
        }
        public static byte[] CapturedTerrainQueries {get;private set;}
        sealed class TerrainQueryTape : IDisposable
        {
            const uint Magic=0x45545131;
            readonly MemoryStream stream;readonly BinaryReader reader;readonly BinaryWriter writer;
            readonly int expectedCount;readonly bool legacy;int count,compared;
#if !UNITY_WEBGL || UNITY_EDITOR
            readonly GameObject query;readonly MeshCollider collider;
#endif
            public TerrainQueryTape(ValoriaIntegratedWorld host,RegionOneBlenderLandscape landscape)
            {
                bool bake=false,validate=false;
                var asset=Resources.Load<TextAsset>("World4X/Blender/RuntimeTerrainQueries");
#if UNITY_EDITOR
                bake=Environment.GetEnvironmentVariable("ELDORIA_BAKE_TERRAIN_QUERIES")=="1";
                validate=Environment.GetEnvironmentVariable("ELDORIA_VALIDATE_TERRAIN_RAYCAST")=="1";
#endif
#if !UNITY_WEBGL || UNITY_EDITOR
                legacy=!asset&&!bake&&!validate;
                if(bake||validate||legacy){
                    var floor=landscape.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_Terrain");
                    query=new GameObject("Editor only exact resource surface certification");query.layer=2;
                    query.transform.SetPositionAndRotation(floor.transform.position,floor.transform.rotation);query.transform.localScale=floor.transform.lossyScale;
                    collider=query.AddComponent<MeshCollider>();collider.sharedMesh=floor.sharedMesh;Physics.SyncTransforms();
                }
#endif
                if(legacy)return;
                var mesh=landscape.DirectedTerrain;ulong indices=0;for(int i=0;i<mesh.subMeshCount;i++)indices+=mesh.GetIndexCount(i);
                if(bake){
                    CapturedTerrainQueries=null;stream=new MemoryStream();writer=new BinaryWriter(stream);
                    writer.Write(Magic);writer.Write(mesh.vertexCount);writer.Write(indices);Write(host.WorldHome);writer.Write(0);
                }else{
                    if(!asset)throw new InvalidOperationException("Certified terrain queries are missing; rebuild this frozen scene");
                    stream=new MemoryStream(asset.bytes,false);reader=new BinaryReader(stream);
                    if(reader.ReadUInt32()!=Magic||reader.ReadInt32()!=mesh.vertexCount||reader.ReadUInt64()!=indices||Read()!=host.WorldHome)
                        throw new InvalidOperationException("Certified terrain input signature differs");
                    expectedCount=reader.ReadInt32();if(expectedCount<10000||expectedCount>200000)throw new InvalidOperationException("Certified terrain query count invalid");
                }
            }
            void Write(Vector3 v){writer.Write(v.x);writer.Write(v.y);writer.Write(v.z);}
            Vector3 Read()=>new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
            public bool Raycast(Ray ray,out RaycastHit hit,float distance)
            {
                if(ray.direction!=Vector3.down)throw new InvalidOperationException("Certified query must be vertical");
                bool found;hit=default;
#if !UNITY_WEBGL || UNITY_EDITOR
                if(legacy)return collider.Raycast(ray,out hit,distance);
                if(writer!=null){
                    found=collider.Raycast(ray,out hit,distance);Write(ray.origin);writer.Write(distance);writer.Write(found);
                    if(found){Write(hit.point);Write(hit.normal);}count++;return found;
                }
#endif
                if(count>=expectedCount)throw new InvalidOperationException("Certified terrain query overflow");
                var origin=Read();float limit=reader.ReadSingle();found=reader.ReadBoolean();
                // Transcendental float rounding can differ across native/Web engines.
                // Less than one millimetre is accepted; the certified result itself is exact.
                if((origin-ray.origin).sqrMagnitude>.000001f||limit!=distance)
                    throw new InvalidOperationException("Certified terrain query sequence differs at "+count+" expected="+origin+" actual="+ray.origin);
                if(found){hit.point=Read();hit.normal=Read();hit.distance=ray.origin.y-hit.point.y;}
                count++;
#if UNITY_EDITOR
                if(collider){
                    bool reference=collider.Raycast(ray,out var original,distance);compared++;
                    if(reference!=found||(found&&((original.point-hit.point).sqrMagnitude>.000001f||(original.normal-hit.normal).sqrMagnitude>.000001f||
                       (original.normal.y>=.94f)!=(hit.normal.y>=.94f)||(original.normal.y>=.9f)!=(hit.normal.y>=.9f))))
                        throw new InvalidOperationException("Certified terrain response differs from original collider at "+ray.origin);
                }
#endif
                return found;
            }
            public void Complete()
            {
                if(legacy)return;
                if(writer!=null){
                    long end=stream.Position;stream.Position=28;writer.Write(count);stream.Position=end;writer.Flush();CapturedTerrainQueries=stream.ToArray();
                    Debug.Log("ELDORIA_TERRAIN_QUERY_BAKED count="+count+" bytes="+CapturedTerrainQueries.Length);
                }else{
                    if(count!=expectedCount||stream.Position!=stream.Length)throw new InvalidOperationException("Certified terrain queries not fully consumed");
                    Debug.Log("ELDORIA_TERRAIN_QUERY_TAPE count="+count+" runtime_cooking=0");
                    if(compared>0)Debug.Log("ELDORIA_TERRAIN_QUERY_PARITY count="+compared+" exact_source_responses=1");
                }
            }
            public void Dispose(){reader?.Dispose();writer?.Dispose();stream?.Dispose();
#if !UNITY_WEBGL || UNITY_EDITOR
                if(query)DestroyImmediate(query);
#endif
            }
        }
        // Enemy and resource sites share this single physical land/road/rivers
        // verifier. No secondary map or client-side save authority is created.
        public bool TryChoose(PlayerState snapshot,string targetId,string previousSiteId,int generation,out WorldCreatureLocation creature)
        {
            creature=null;
            if(snapshot.WorldId!="world-local"||sites.Count==0||routes==null)return false;
            var original=LocalGateway.Creature(snapshot,targetId);
            if(original==null||(original.CreatureKind!="boar"&&original.CreatureKind!="spawnling"))return false;
            uint hash=2166136261;
            foreach(char c in targetId)hash=unchecked((hash^c)*16777619);
            int first=(int)(hash%(uint)sites.Count);
            // Balance desired tiers without inventing extra 3D models or
            // imposing visible circular biome borders. Tier is ALWAYS
            // computed from the actual candidate coordinate and Brecha.
            int serial=1;
            int.TryParse(targetId.Substring(targetId.LastIndexOf(':')+1),out serial);
            int[] desired=original.CreatureKind=="boar"
                ?new[]{1,1,1,1,1,1,2,2,2,2,3,3,3,3,4,4,5,5}
                :new[]{1,1,2,2,3,3,3,3,4,4,4,4,4,5,5,5,5,5};
            int desiredLevel=desired[Mathf.Clamp(serial-1,0,desired.Length-1)];
            for(int pass=0;pass<3;pass++)
            for(int k=0;k<sites.Count;k++)
            {
                var site=sites[Eldoria.PlayerClarity.DistributedSiteOrder.Candidate(sites.Count,targetId,generation,k)];
                if(site.Id==previousSiteId)continue;
                float x=site.Point.x-host.WorldHome.x,z=site.Point.z-host.WorldHome.z;
                float breach=4000;
                foreach(var segment in faults)
                {
                    var a=new Vector2(segment.ax,segment.az);
                    var d=new Vector2(segment.bx-segment.ax,segment.bz-segment.az);
                    float t=d.sqrMagnitude>.0001f?
                        Mathf.Clamp01(Vector2.Dot(new Vector2(x,z)-a,d)/d.sqrMagnitude):0;
                    breach=Mathf.Min(breach,Vector2.Distance(new Vector2(x,z),a+d*t));
                }
                // Naturally overlapping distributions, never rigid artificial rings:
                // boars prefer safer meadows, spawnlings the influence of Brecha.
                bool preferred=original.CreatureKind=="boar"?breach>330:breach<540;
                // Require the intended danger first; only relax if the real
                // navigable ground has no compatible unoccupied placement.
                float jitter=((hash>>9)%101)/100f*.56f-.28f;
                int level=Mathf.Clamp(Mathf.RoundToInt(1f+4f*Mathf.Clamp01(1f-breach/620f)+jitter),1,5);
                if(pass==0&&level!=desiredLevel)continue;
                if(pass==1&&(Mathf.Abs(level-desiredLevel)>1||!preferred))continue;
                bool occupied=Vector2.Distance(new Vector2(site.Point.x,site.Point.z),
                    new Vector2(host.WorldHome.x,host.WorldHome.z))<40;
                foreach(var city in cities)
                    if(Vector2.Distance(new Vector2(x,z),new Vector2(city.x,city.z))<city.radius+24)occupied=true;
                foreach(var item in snapshot.WorldResourceLocations)
                    if(item.Generation>0&&LocalGateway.ResourceRemaining(snapshot,item.TargetId)>0&&
                       Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(item.X,item.Z))<28)occupied=true;
                foreach(var other in snapshot.WorldCreatureLocations)
                    if(other.TargetId!=targetId&&other.Alive&&
                       Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(other.X,other.Z))<56)occupied=true;
                foreach(var historic in new[]{"valoria-map-city","forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"})
                {
                    var target=host.FindTarget(historic);
                    if(target&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),
                       new Vector2(target.transform.position.x,target.transform.position.z))<35)occupied=true;
                }
                if(occupied||!routes.FootprintClear(site.Point,10))continue;
                if(!routes.TryRouteTo(snapshot,site.Point,out var route))continue;
                creature=new WorldCreatureLocation{
                    TargetId=targetId,CreatureKind=original.CreatureKind,
                    SiteId=site.Id,Generation=generation,Level=level,
                    X=site.Point.x,Y=site.Point.y,Z=site.Point.z};
                return true;
            }
            return false;
        }
        public bool TryChoose(PlayerState snapshot,string targetId,string previousSiteId,int generation,out WorldResourceLocation location)
        {
            location=null;
            if(snapshot.WorldId!="world-local"||snapshot.BastionLevel<1)return false;
            bool wood=LocalGateway.ResourceKind(snapshot,targetId)=="wood";
            if(!wood&&LocalGateway.ResourceKind(snapshot,targetId)!="stone")return false;
            // Deterministic pseudorandom full-cycle visitation (no repeats).
            // Resource/generation identity distributes sites across verified
            // terrain instead of walking each neighboring grid cell in order.
            for(int offset=0;offset<sites.Count;offset++)
            {
                var site=sites[Eldoria.PlayerClarity.DistributedSiteOrder.Candidate(sites.Count,targetId,generation,offset)];
                if(site.Wood!=wood||site.Id==previousSiteId||(initial.TryGetValue(targetId,out var original)&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(original.x,original.z))<24))continue;
                bool occupied=Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(host.WorldHome.x,host.WorldHome.z))<32;
                foreach(var city in cities)if(Vector2.Distance(new Vector2(site.Point.x-host.WorldHome.x,site.Point.z-host.WorldHome.z),new Vector2(city.x,city.z))<city.radius+9)occupied=true;
                foreach(var item in snapshot.WorldResourceLocations)
                    if(item.TargetId!=targetId&&item.Generation>0&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(item.X,item.Z))<44)occupied=true;
                // Do not place an exhausted node on the active creature population.
                foreach(var creature in snapshot.WorldCreatureLocations)
                    if(creature.Alive&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),
                      new Vector2(creature.X,creature.Z))<48)occupied=true;
                foreach(var hotspot in host.WorldRoot.GetComponentsInChildren<Eldoria.Presentation.WorldHotspot>(true))
                    if(hotspot.Id!=targetId&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(hotspot.transform.position.x,hotspot.transform.position.z))<38)occupied=true;
                // Same 9 m canopy footprint clearance, now including original AF/AG
                // trees as well as continental crowns, through the exact spatial index.
                if(occupied||routes==null||!routes.FootprintClear(site.Point,9))continue;
                // A flat, clear footprint is insufficient if the actual terrain disconnects its approach.
                if(routes==null||!routes.TryRouteTo(snapshot,site.Point,out var approach))continue;
                var p=site.Point;
                location=new WorldResourceLocation{TargetId=targetId,SiteId=site.Id,X=p.x,Y=p.y,Z=p.z,Generation=generation};
                return true;
            }
            return false;
        }
        void LateUpdate()
        {
            if(!host||!host.Ready||!host.City||Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.25f;
            Synchronize();
        }
        void Synchronize()
        {
            var gateway=host.City.Gateway;
            if(gateway==null)return;
            if(!ReferenceEquals(bound,gateway))
            {
                bound=gateway;
                if(bound is LocalGateway local&&bound.Snapshot().WorldId=="world-local")
                {
                    local.ConfigureLocalMarchRoutes(routes);
                    local.ConfigureLocalResourceRespawn(this,RenewablePopulationPolicy.Delay);
                    if(Resources.Load<GameObject>("World4X/Blender/RegionOneResources"))local.EnsureLocalRenewablePopulation(24,24);
                }
            }
            var state=bound.Snapshot();
            bool panorama=host.WorldCamera&&host.WorldNavigation&&IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
            foreach(var existing in host.WorldRoot.GetComponentsInChildren<Eldoria.Presentation.WorldHotspot>(true))
                if(existing.Id!=null&&(existing.Id.StartsWith("region1:wood:",StringComparison.Ordinal)||existing.Id.StartsWith("region1:stone:",StringComparison.Ordinal))){
                    bool alive=state.WorldResourceLocations.Any(x=>x.TargetId==existing.Id&&x.Generation>0&&LocalGateway.ResourceRemaining(state,x.TargetId)>0);
                    if(existing.gameObject.activeSelf!=alive)existing.gameObject.SetActive(alive);
                }
            foreach(var item in state.WorldResourceLocations)
            {
                var target=host.FindTarget(item.TargetId);
                if(!target&&item.Generation>0&&item.TargetId.StartsWith("region1:",StringComparison.Ordinal))
                {
                    var go=new GameObject("Region1 logical resource "+item.TargetId);go.transform.SetParent(host.WorldRoot.transform,true);go.layer=8;
                    target=go.AddComponent<Eldoria.Presentation.WorldHotspot>();target.Id=item.TargetId;
                    var box=go.AddComponent<BoxCollider>();box.isTrigger=true;box.center=new Vector3(0,3,0);box.size=new Vector3(10,6,10);
                }
                if(!target)continue;
                if(item.TargetId.StartsWith("region1:",StringComparison.Ordinal))target.gameObject.SetActive(item.Generation>0&&LocalGateway.ResourceRemaining(state,item.TargetId)>0);
                if(target.GetComponent<Collider>())target.GetComponent<Collider>().enabled=!panorama&&LocalGateway.ResourceRemaining(state,item.TargetId)>0;
                if(item.Generation>0)target.transform.position=new Vector3(item.X,item.Y,item.Z);
            }
            // The historical wood and stone targets can exist before their first
            // WorldResourceLocation is initialized. Never make either pickable in panorama.
            foreach(var id in new[]{"forest-valoria","quarry-valoria"})
            {
                var target=host.FindTarget(id);
                if(!target)continue;
                var hit=target.GetComponent<Collider>();
                if(hit)hit.enabled=!panorama&&LocalGateway.ResourceRemaining(state,id)>0;
            }
            // Campaign enemies are not renewable population. Preserve their exact
            // original collider state when leaving panorama, never force-resurrect.
            foreach(var id in new[]{"corrupt-scout","engendro-valoria"})
            {
                var target=host.FindTarget(id);
                if(!target)continue;
                var hit=target.GetComponent<Collider>();
                if(!hit)continue;
                if(panorama)
                {
                    if(!panoramaEncounterState.ContainsKey(hit))panoramaEncounterState.Add(hit,hit.enabled);
                    hit.enabled=false;
                }
                else if(panoramaEncounterState.TryGetValue(hit,out var prior))
                {
                    hit.enabled=prior;
                    panoramaEncounterState.Remove(hit);
                }
            }
        }
    }
#if UNITY_EDITOR
    public sealed class RegionOneTerrainCacheBuildGuard : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder=>0;
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            if(report.summary.platform==UnityEditor.BuildTarget.WebGL&&!Resources.Load<TextAsset>("World4X/Blender/RuntimeTerrainQueries"))
                throw new UnityEditor.Build.BuildFailedException("Bake and validate the frozen terrain query tape before building WebGL; see tools/eldoria-private-beta/native/run_structural.ps1");
        }
    }
#endif

}
