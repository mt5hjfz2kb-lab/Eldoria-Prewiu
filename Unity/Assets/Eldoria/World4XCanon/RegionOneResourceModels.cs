using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using Eldoria.Application;
using UnityEngine;

namespace Eldoria.World4XCanon
{
    [DefaultExecutionOrder(1400)]
    public sealed class RegionOneResourceModels : MonoBehaviour
    {
        sealed class Node { public string Id; public Transform Logical; public GameObject Visual; public Renderer[] Renderers; }
        readonly List<Node> nodes=new List<Node>();
        ValoriaIntegratedWorld host;
        GameObject library;
        public static void Install(Transform parent,ValoriaIntegratedWorld host)
        {
            var library=Resources.Load<GameObject>("World4X/Blender/RegionOneResources");
            if(!library)return;
            var service=parent.gameObject.AddComponent<RegionOneResourceModels>();service.host=host;service.library=library;
            foreach(var pair in new[]{("forest-valoria","Resource_Wood_0"),("quarry-valoria","Resource_Stone_0")})
            {
                var logical=host.FindTarget(pair.Item1);
                var source=library.GetComponentsInChildren<Transform>(true).Single(x=>x.name==pair.Item2);
                if(!logical)throw new InvalidOperationException("Missing canonical resource target "+pair.Item1);
                var visual=Instantiate(source.gameObject,parent);visual.name="Production resource visual "+pair.Item1;
                visual.transform.localScale=Vector3.one;visual.transform.rotation=Quaternion.Euler(0,180,0);
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
                service.nodes.Add(new Node{Id=pair.Item1,Logical=logical.transform,Visual=visual,Renderers=visual.GetComponentsInChildren<Renderer>(true)});
            }
        }
        void LateUpdate()
        {
            if(!host||!host.Ready)return;
            var state=host.City.Gateway.Snapshot();bool near=host.WorldCamera.orthographicSize<152;
            foreach(var item in state.WorldResourceLocations)
            {
                if(!item.TargetId.StartsWith("region1:",StringComparison.Ordinal)||item.Generation<=0||nodes.Any(x=>x.Id==item.TargetId))continue;
                var logical=host.FindTarget(item.TargetId);if(!logical)continue;
                string family=item.ResourceKind=="wood"?"Wood":"Stone";
                var source=library.GetComponentsInChildren<Transform>(true).Single(x=>x.name=="Resource_"+family+"_"+(nodes.Count%3));
                var visual=Instantiate(source.gameObject,transform);visual.name="Production resource visual "+item.TargetId;visual.transform.rotation=Quaternion.Euler(0,180,0);
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
                nodes.Add(new Node{Id=item.TargetId,Logical=logical.transform,Visual=visual,Renderers=visual.GetComponentsInChildren<Renderer>(true)});
            }
            foreach(var node in nodes)
            {
                var p=node.Logical.position;
                p.y=host.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(p.x-host.WorldHome.x,p.z-host.WorldHome.z);
                node.Visual.transform.position=p;
                bool alive=LocalGateway.ResourceRemaining(state,node.Id)>0;
                foreach(var renderer in node.Renderers)renderer.enabled=alive&&near;
            }
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                if(renderer.name=="Reference existing quarry"||renderer.name=="Reference existing wood grove")renderer.enabled=false;
        }
    }
}
