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
        sealed class Site { public string Id; public Vector3 Point; public bool Wood; }
        readonly List<Site> sites = new List<Site>();
        readonly Dictionary<string,Vector3> initial = new Dictionary<string,Vector3>();
        ValoriaIntegratedWorld host;
        LocalGateway bound;
        float nextRefresh;
        public int VerifiedSiteCount => sites.Count;
        public static void Install(Transform root, ValoriaIntegratedWorld host, RegionOneBlenderLandscape landscape)
        {
            var service=root.gameObject.AddComponent<RegionOneResourcePlacement>();
            service.host=host;
            service.Scan(landscape);
        }
        void Scan(RegionOneBlenderLandscape landscape)
        {
            foreach(var id in new[]{"forest-valoria","quarry-valoria"})
            {
                var target=host.FindTarget(id);
                if(target)initial[id]=target.transform.position;
            }
            var floor=landscape.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_Terrain");
            var query=new GameObject("Temporary exact resource surface validation");
            query.layer=2; query.transform.SetPositionAndRotation(floor.transform.position,floor.transform.rotation);
            query.transform.localScale=floor.transform.lossyScale;
            var collider=query.AddComponent<MeshCollider>();collider.sharedMesh=floor.sharedMesh;
            Physics.SyncTransforms();
            try
            {
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
            finally {DestroyImmediate(query);}
        }
        public bool TryChoose(PlayerState snapshot,string targetId,string previousSiteId,int generation,out WorldResourceLocation location)
        {
            location=null;
            if(snapshot.WorldId!="world-local"||snapshot.BastionLevel<1||!initial.ContainsKey(targetId))return false;
            bool wood=targetId=="forest-valoria";
            if(!wood&&targetId!="quarry-valoria")return false;
            // Stable ordering and a persisted chosen site: no client random seed.
            for(int offset=0;offset<sites.Count;offset++)
            {
                var site=sites[(offset+generation*17)%sites.Count];
                if(site.Wood!=wood||site.Id==previousSiteId||Vector3.Distance(site.Point,initial[targetId])<24)continue;
                bool occupied=Vector2.Distance(new Vector2(site.Point.x,site.Point.z),new Vector2(host.WorldHome.x,host.WorldHome.z))<32;
                foreach(var item in snapshot.WorldResourceLocations)
                    if(item.TargetId!=targetId&&item.Generation>0&&Vector3.Distance(site.Point,new Vector3(item.X,item.Y,item.Z))<20)occupied=true;
                foreach(var hotspot in host.WorldRoot.GetComponentsInChildren<Eldoria.Presentation.WorldHotspot>(true))
                    if(hotspot.Id!=targetId&&Vector3.Distance(site.Point,hotspot.transform.position)<24)occupied=true;
                foreach(var renderer in host.WorldRoot.GetComponentsInChildren<Renderer>())
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
            var gateway=host.City.Gateway as LocalGateway;
            if(gateway==null)return;
            if(!ReferenceEquals(bound,gateway))
            {
                bound=gateway;
                if(bound.Snapshot().WorldId=="world-local")bound.ConfigureLocalResourceRespawn(this,TimeSpan.FromMinutes(30));
            }
            var state=bound.Snapshot();
            foreach(var item in state.WorldResourceLocations)
            {
                var target=host.FindTarget(item.TargetId);if(!target)continue;
                if(item.Generation>0)target.transform.position=new Vector3(item.X,item.Y,item.Z);
            }
        }
    }
}
