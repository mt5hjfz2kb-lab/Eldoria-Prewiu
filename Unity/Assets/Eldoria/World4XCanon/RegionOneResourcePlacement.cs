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
        [Serializable] public sealed class CityReserve {public float x,z,radius;}
        [Serializable] sealed class RegionLayout {public CityReserve[] cities;}
        CityReserve[] cities=Array.Empty<CityReserve>();
        public int ValidatedCityCount {get;private set;}
        sealed class Site { public string Id; public Vector3 Point; public bool Wood; }
        readonly List<Site> sites = new List<Site>();
        readonly Dictionary<string,Vector3> initial = new Dictionary<string,Vector3>();
        ValoriaIntegratedWorld host;
        LocalGateway bound;
        RegionOneTerrainRoutes routes;
        float nextRefresh;
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
            if(layout)cities=JsonUtility.FromJson<RegionLayout>(layout.text).cities??Array.Empty<CityReserve>();
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
                    float low=float.MaxValue,high=float.MinValue;bool valid=true;
                    for(int i=0;i<=16;i++){
                        float a=i*Mathf.PI/8;float x=city.x+(i==16?0:Mathf.Cos(a)*city.radius),z=city.z+(i==16?0:Mathf.Sin(a)*city.radius);
                        if(!collider.Raycast(new Ray(host.WorldHome+new Vector3(x,250,z),Vector3.down),out var hit,500)){valid=false;break;}
                        low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);
                    }
                    if(valid&&high-low<.5f)ValidatedCityCount++;
                }
                for(int z=-700;z<=800;z+=32)for(int x=-750;x<=600;x+=32)
                {
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
            finally {if(routes==null)routes=new RegionOneTerrainRoutes(host,collider);DestroyImmediate(query);}
        }
        public bool TryChoose(PlayerState snapshot,string targetId,string previousSiteId,int generation,out WorldResourceLocation location)
        {
            location=null;
            if(snapshot.WorldId!="world-local"||snapshot.BastionLevel<1)return false;
            bool wood=LocalGateway.ResourceKind(snapshot,targetId)=="wood";
            if(!wood&&LocalGateway.ResourceKind(snapshot,targetId)!="stone")return false;
            // Stable ordering and a persisted chosen site: no client random seed.
            for(int offset=0;offset<sites.Count;offset++)
            {
                var site=sites[(offset+generation*17)%sites.Count];
                if(site.Wood!=wood||site.Id==previousSiteId||(initial.TryGetValue(targetId,out var original)&&Vector3.Distance(site.Point,original)<24))continue;
                bool occupied=Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(host.WorldHome.x,host.WorldHome.z))<32;
                foreach(var city in cities)if(Vector2.Distance(new Vector2(site.Point.x-host.WorldHome.x,site.Point.z-host.WorldHome.z),new Vector2(city.x,city.z))<city.radius+9)occupied=true;
                foreach(var item in snapshot.WorldResourceLocations)
                    if(item.TargetId!=targetId&&item.Generation>0&&Vector3.Distance(site.Point,new Vector3(item.X,item.Y,item.Z))<20)occupied=true;
                foreach(var hotspot in host.WorldRoot.GetComponentsInChildren<Eldoria.Presentation.WorldHotspot>(true))
                    if(hotspot.Id!=targetId&&Vector3.Distance(site.Point,hotspot.transform.position)<24)occupied=true;
                foreach(var renderer in host.WorldRoot.GetComponentsInChildren<Renderer>(true))
                    if(renderer.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal))
                    {var b=renderer.bounds;b.Expand(18);if(site.Point.x>=b.min.x&&site.Point.x<=b.max.x&&site.Point.z>=b.min.z&&site.Point.z<=b.max.z)occupied=true;}
                if(occupied)continue;
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
            var gateway=host.City.Gateway as LocalGateway;
            if(gateway==null)return;
            if(!ReferenceEquals(bound,gateway))
            {
                bound=gateway;
                if(bound.Snapshot().WorldId=="world-local")
                {
                    bound.ConfigureLocalMarchRoutes(routes);
                    bound.ConfigureLocalResourceRespawn(this,TimeSpan.FromMinutes(30));
                    if(Resources.Load<GameObject>("World4X/Blender/RegionOneResources"))bound.EnsureLocalRenewablePopulation(24,24);
                }
            }
            var state=bound.Snapshot();
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
                if(target.GetComponent<Collider>())target.GetComponent<Collider>().enabled=LocalGateway.ResourceRemaining(state,item.TargetId)>0;
                if(item.Generation>0)target.transform.position=new Vector3(item.X,item.Y,item.Z);
            }
        }
    }
}
