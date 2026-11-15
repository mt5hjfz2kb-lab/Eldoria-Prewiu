// Experimental geography: new authored world landmarks, existing LocalGateway
// remains the exclusive authority on departure, victory, costs and rewards.
using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;

namespace Eldoria.ExperimentalAshenCitadel
{
    public sealed class AshenCitadelRoutes : IWorldMarchRoutes
    {
        private static readonly Dictionary<string, Vector3> Destinations =
            new Dictionary<string, Vector3>(StringComparer.Ordinal)
        {
            {"forest-valoria",new Vector3(-15,0,66)},
            {"quarry-valoria",new Vector3(15,0,48)},
            {"corrupt-scout",new Vector3(6,0,60)},
            {"engendro-valoria",new Vector3(17,0,67)},
            {"fissure-valoria",new Vector3(13,0,73)},
            {"hunt-wolf",new Vector3(-8,0,72)},
            {"hunt-boar",new Vector3(-12,0,61)},
            {"meat-valoria",new Vector3(-7,0,63)},
            {"ash-stalker-valoria",new Vector3(20,0,73)}
        };
        public bool TryRoute(PlayerState snapshot,string targetId,out List<WorldRoutePoint> route)
        {
            route=null;
            if(snapshot==null||string.IsNullOrEmpty(targetId)||snapshot.WorldId!="world-local")return false;
            if(!Destinations.TryGetValue(targetId,out var p))
            {
                var res=snapshot.WorldResourceLocations?.Find(x=>x.TargetId==targetId);
                if(res!=null&&res.Generation>0)p=new Vector3(res.X,res.Y,res.Z);
                else
                {
                    var enemy=snapshot.WorldCreatureLocations?.Find(x=>x.TargetId==targetId);
                    if(enemy==null||!enemy.Alive)return false;
                    p=new Vector3(enemy.X,enemy.Y,enemy.Z);
                }
            }
            var from=new Vector3(0,0,0);
            var mid=Vector3.Lerp(from,p,.53f);
            // Exactly three reproducible waypoints; backend validates scheduling
            // and persists the resulting route with each march.
            route=new List<WorldRoutePoint>{
                new WorldRoutePoint{X=from.x,Y=from.y,Z=from.z},
                new WorldRoutePoint{X=mid.x,Y=mid.y,Z=mid.z},
                new WorldRoutePoint{X=p.x,Y=p.y,Z=p.z}};
            return true;
        }
    }
}
