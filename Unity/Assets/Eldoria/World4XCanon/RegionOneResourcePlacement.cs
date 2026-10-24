using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.World4XCanon
{
    // Geometry service for the existing LocalGateway. Owns no timer or save file.
    public sealed class RegionOneResourcePlacement : MonoBehaviour, IWorldResourcePlacement
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
        void Scan(RegionOneBlenderLandscape landscape)
        {
            foreach(var id in new[]{"forest-valoria","quarry-valoria"})
            {
                var target=host.FindTarget(id);
                if(target)initial[id]=target.transform.position;
            }
            var layout=Resources.Load<TextAsset>("World4X/Blender/production-region");
            if(layout){var data=JsonUtility.FromJson<RegionLayout>(layout.text);cities=data.cities??Array.Empty<CityReserve>();faults=data.faults??Array.Empty<FaultSegment>();}
            var floor=landscape.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_Terrain");
            var query=new GameObject("Temporary exact resource surface validation");
            query.layer=2; query.transform.SetPositionAndRotation(floor.transform.position,floor.transform.rotation);
            query.transform.localScale=floor.transform.lossyScale;
            var collider=query.AddComponent<MeshCollider>();collider.sharedMesh=floor.sharedMesh;
            Physics.SyncTransforms();
            try
            {
                foreach(var city in cities)
                {
                    float low=float.MaxValue,high=float.MinValue;bool valid=!FaultExcluded(faults,city.x,city.z,city.radius);
                    for(int i=0;i<=16;i++){
                        float a=i*Mathf.PI/8;float x=city.x+(i==16?0:Mathf.Cos(a)*city.radius),z=city.z+(i==16?0:Mathf.Sin(a)*city.radius);
                        if(!collider.Raycast(new Ray(host.WorldHome+new Vector3(x,250,z),Vector3.down),out var hit,500)){valid=false;break;}
                        low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);
                    }
                    if(valid&&high-low<.5f){city.Validated=true;ValidatedCityCount++;}
                }
                for(int z=-1500;z<=1600;z+=32)for(int x=-750;x<=600;x+=32)
                {
                    if(FaultExcluded(faults,x,z,8))continue;
                    bool safe=true;float low=float.MaxValue,high=float.MinValue;Vector3 centre=default;
                    for(int i=0;i<=8;i++)
                    {
                        float a=i*Mathf.PI/4;float xx=x+(i==8?0:Mathf.Cos(a)*8),zz=z+(i==8?0:Mathf.Sin(a)*8);
                        if(RegionOneBlenderLandscape.Shore(xx,zz)<12||RegionOneBlenderLandscape.Water(xx,zz)>.01f||RegionOneBlenderLandscape.Road(xx,zz)>.05f){safe=false;break;}
                        var ray=new Ray(host.WorldHome+new Vector3(xx,250,zz),Vector3.down);
                        if(!collider.Raycast(ray,out var hit,500)||Vector3.Dot(hit.normal,Vector3.up)<.94f){safe=false;break;}
                        low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);if(i==8)centre=hit.point;
                    }
                    if(!safe||high-low>1.6f)continue;
                    bool wood=false;
                    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;wood|=RegionOneBlenderLandscape.Forest(x+Mathf.Cos(a)*24,z+Mathf.Sin(a)*24)>.15f;}
                    sites.Add(new Site{Id="region1:"+x+":"+z,Point=centre,Wood=wood});
                }
            }
            finally {if(routes==null)routes=new RegionOneTerrainRoutes(host,collider,faults);DestroyImmediate(query);}
            var snapshot=host.City.Gateway.Snapshot();
            foreach(var city in cities)if(city.Validated&&routes.TryRouteTo(snapshot,host.WorldHome+new Vector3(city.x,0,city.z),out var route))AccessibleCityCount++;
        }
        public bool TryChoose(PlayerState snapshot,string targetId,string previousSiteId,int generation,out WorldResourceLocation location)
        {
            location=null;
            if(snapshot.WorldId!="world-local"||snapshot.BastionLevel<1)return false;
            bool wood=LocalGateway.ResourceKind(snapshot,targetId)=="wood";
            if(!wood&&LocalGateway.ResourceKind(snapshot,targetId)!="stone")return false;
            // Stable identity-specific ordering spreads resources over valid biomes.
            // The chosen site remains authoritative persisted state, never client randomness.
            uint identity=2166136261;foreach(char c in targetId)identity=unchecked((identity^c)*16777619);
            int first=(int)(identity%(uint)Math.Max(1,sites.Count));
            for(int offset=0;offset<sites.Count;offset++)
            {
                var site=sites[(first+offset+generation*17)%sites.Count];
                if(site.Wood!=wood||site.Id==previousSiteId||(initial.TryGetValue(targetId,out var original)&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(original.x,original.z))<24))continue;
                bool occupied=Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(host.WorldHome.x,host.WorldHome.z))<32;
                foreach(var city in cities)if(Vector2.Distance(new Vector2(site.Point.x-host.WorldHome.x,site.Point.z-host.WorldHome.z),new Vector2(city.x,city.z))<city.radius+9)occupied=true;
                foreach(var item in snapshot.WorldResourceLocations)
                    if(item.TargetId!=targetId&&item.Generation>0&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(item.X,item.Z))<20)occupied=true;
                foreach(var hotspot in host.WorldRoot.GetComponentsInChildren<Eldoria.Presentation.WorldHotspot>(true))
                    if(hotspot.Id!=targetId&&Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(hotspot.transform.position.x,hotspot.transform.position.z))<24)occupied=true;
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
                    local.ConfigureLocalResourceRespawn(this,TimeSpan.FromMinutes(30));
                    if(Resources.Load<GameObject>("World4X/Blender/RegionOneResources"))local.EnsureLocalRenewablePopulation(24,24);
                }
            }
            var state=bound.Snapshot();
            bool panorama=host.WorldCamera&&host.WorldNavigation&&IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
            foreach(var existing in host.WorldRoot.GetComponentsInChildren<Eldoria.Presentation.WorldHotspot>(true))
                if(existing.Id!=null&&existing.Id.StartsWith("region1:",StringComparison.Ordinal)){
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
}
