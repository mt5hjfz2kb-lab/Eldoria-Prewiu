using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEngine;
namespace Eldoria.World4XCanon
{
    // Grid is sampled from real imported walkable triangles, including bridge decks.
    // Each dispatched route is persisted by LocalGateway; this component owns no clock/save.
    public sealed class RegionOneTerrainRoutes : IWorldMarchRoutes
    {
        readonly Dictionary<Vector2Int,Vector3> cells=new Dictionary<Vector2Int,Vector3>();
        readonly ValoriaIntegratedWorld host;
        static readonly Vector2Int[] directions={Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down};
        public int WalkableCells=>cells.Count;
        public RegionOneTerrainRoutes(ValoriaIntegratedWorld host,MeshCollider terrain)
        {
            this.host=host;
            var obstacles=host.WorldRoot.GetComponentsInChildren<Renderer>(true)
                .Where(x=>x.name.StartsWith("Pilot_Pine",StringComparison.Ordinal)||x.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)).Select(x=>x.bounds).ToArray();
            for(int z=-124;z<=124;z++)for(int x=-112;x<=112;x++)
            {
                float xx=x*8,zz=z*8;
                if(RegionOneBlenderLandscape.Shore(xx,zz)<4||RegionOneBlenderLandscape.Water(xx,zz)>.08f)continue;
                var ray=new Ray(host.WorldHome+new Vector3(xx,250,zz),Vector3.down);
                if(!terrain.Raycast(ray,out var hit,500)||Vector3.Dot(hit.normal,Vector3.up)<.9f)continue;
                bool blocked=false;
                foreach(var bounds in obstacles)
                    if(hit.point.x>bounds.min.x-1&&hit.point.x<bounds.max.x+1&&hit.point.z>bounds.min.z-1&&hit.point.z<bounds.max.z+1){blocked=true;break;}
                if(!blocked)cells[new Vector2Int(x,z)]=hit.point;
            }
        }
        HashSet<Vector2Int> OccupiedCells(PlayerState state)
        {
            var result=new HashSet<Vector2Int>();var points=new List<Vector3>();
            foreach(var id in new[]{"forest-valoria","quarry-valoria"}){var target=host.FindTarget(id);if(target&&LocalGateway.ResourceRemaining(state,id)>0)points.Add(target.transform.position);}
            foreach(var item in state.WorldResourceLocations)if(item.Generation>0&&LocalGateway.ResourceRemaining(state,item.TargetId)>0)points.Add(new Vector3(item.X,item.Y,item.Z));
            foreach(var point in points){var local=point-host.WorldHome;var key=new Vector2Int(Mathf.RoundToInt(local.x/8),Mathf.RoundToInt(local.z/8));
                for(int z=-1;z<=1;z++)for(int x=-1;x<=1;x++){var k=key+new Vector2Int(x,z);if(cells.TryGetValue(k,out var p)&&(new Vector2(p.x-point.x,p.z-point.z)).sqrMagnitude<49)result.Add(k);}}
            return result;
        }
        bool Nearest(Vector3 point,HashSet<Vector2Int> occupied,out Vector2Int result)
        {
            result=default;float best=24*24;bool found=false;
            var local=point-host.WorldHome;var centre=new Vector2Int(Mathf.RoundToInt(local.x/8),Mathf.RoundToInt(local.z/8));
            for(int z=-3;z<=3;z++)for(int x=-3;x<=3;x++)
            {
                var key=centre+new Vector2Int(x,z);
                if(!cells.TryGetValue(key,out var p)||occupied.Contains(key))continue;
                float d=(new Vector2(p.x,p.z)-new Vector2(point.x,point.z)).sqrMagnitude;
                if(d<best){best=d;result=key;found=true;}
            }
            return found;
        }
        public bool TryRoute(PlayerState snapshot,string targetId,out List<WorldRoutePoint> route)
        {
            route=null;var target=host.FindTarget(targetId);
            return target&&TryRouteTo(snapshot,target.transform.position,out route);
        }
        public bool TryRouteTo(PlayerState snapshot,Vector3 destination,out List<WorldRoutePoint> route)
        {
            route=null;var occupied=OccupiedCells(snapshot);var source=host.FindTarget("valoria-map-city");
            if(!source||!Nearest(source.transform.position,occupied,out var start)||!Nearest(destination,occupied,out var goal))return false;
            // Uniform-cost BFS over orthogonal edges. No diagonal corner cutting.
            var queue=new Queue<Vector2Int>();var previous=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(start);previous[start]=start;
            while(queue.Count>0)
            {
                var a=queue.Dequeue();if(a==goal)break;
                foreach(var d in directions)
                {
                    var b=a+d;if(previous.ContainsKey(b)||!cells.TryGetValue(b,out var p)||occupied.Contains(b)||Mathf.Abs(p.y-cells[a].y)>3.2f)continue;
                    // Mid-edge semantic checks prevent stepping across a narrow river missed by endpoint samples.
                    var mid=(p+cells[a])*.5f-host.WorldHome;
                    if(RegionOneBlenderLandscape.Water(mid.x,mid.z)>.08f)continue;
                    previous[b]=a;queue.Enqueue(b);
                }
            }
            if(!previous.ContainsKey(goal)){
                // A destination has an approach area, not one arbitrarily chosen grid cell.
                // Search only already reached, collision-free terrain cells in the same 24m limit.
                float best=24*24;bool found=false;
                foreach(var pair in previous){var p=cells[pair.Key];float d=(new Vector2(p.x-destination.x,p.z-destination.z)).sqrMagnitude;
                    if(d<best){best=d;goal=pair.Key;found=true;}}
                if(!found){Debug.LogWarning("REGION1_ROUTE_DISCONNECTED start="+start+" goal="+goal+" reachable="+previous.Count+" walkable="+cells.Count);return false;}
                Debug.Log("REGION1_ROUTE_APPROACH alternate_reachable_cell="+goal+" distance_m="+Mathf.Sqrt(best));
            }
            var points=new List<Vector2Int>();for(var p=goal;;p=previous[p]){points.Add(p);if(p==start)break;}points.Reverse();
            route=points.Select(k=>{var p=cells[k];return new WorldRoutePoint{X=p.x,Y=p.y,Z=p.z};}).ToList();
            return route.Count>1;
        }
    }
}
