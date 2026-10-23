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
        sealed class Node { public string Id; public Transform Logical; public GameObject Visual; public Renderer[] Renderers; public Renderer Impostor; }
        readonly List<Node> nodes=new List<Node>();
        ValoriaIntegratedWorld host;
        GameObject library;
        Material impostorMaterial;readonly Mesh[] impostorMeshes=new Mesh[2];
        Renderer MakeImpostor(string id,Transform parent)
        {
            if(!impostorMaterial)return null;
            int tile=id=="forest-valoria"||id.StartsWith("region1:wood:",StringComparison.Ordinal)?0:1;
            if(!impostorMeshes[tile]){
                float u=tile*.5f;var mesh=new Mesh{name="Resource actual-model impostor "+tile};
                mesh.vertices=new[]{Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero};mesh.uv=new[]{new Vector2(u,0),new Vector2(u+.5f,0),new Vector2(u+.5f,1),new Vector2(u,1)};
                mesh.uv2=new[]{new Vector2(-6,-1),new Vector2(6,-1),new Vector2(6,11),new Vector2(-6,11)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.bounds=new Bounds(Vector3.up*5,Vector3.one*24);impostorMeshes[tile]=mesh;
            }
            var go=new GameObject("Production resource impostor "+id);go.transform.SetParent(parent,false);go.layer=29;go.AddComponent<MeshFilter>().sharedMesh=impostorMeshes[tile];var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=impostorMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.enabled=false;return renderer;
        }
        void OnDestroy(){if(impostorMaterial)Destroy(impostorMaterial);foreach(var mesh in impostorMeshes)if(mesh)Destroy(mesh);}

        public static void Install(Transform parent,ValoriaIntegratedWorld host)
        {
            var library=Resources.Load<GameObject>("World4X/Blender/RegionOneResources");
            if(!library)return;
            var service=parent.gameObject.AddComponent<RegionOneResourceModels>();service.host=host;service.library=library;
            var atlas=Resources.Load<Texture2D>("World4X/Blender/RegionOneResourceImpostors");
            if(atlas){service.impostorMaterial=new Material(Shader.Find("Eldoria/World4X/LocalCanopy"));service.impostorMaterial.mainTexture=atlas;}
            foreach(var pair in new[]{("forest-valoria","Resource_Wood_0"),("quarry-valoria","Resource_Stone_0")})
            {
                var logical=host.FindTarget(pair.Item1);
                var source=library.GetComponentsInChildren<Transform>(true).Single(x=>x.name==pair.Item2);
                if(!logical)throw new InvalidOperationException("Missing canonical resource target "+pair.Item1);
                var visual=Instantiate(source.gameObject,parent);visual.name="Production resource visual "+pair.Item1;
                visual.transform.localScale=Vector3.one;visual.transform.rotation=Quaternion.Euler(0,180,0);
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
                service.nodes.Add(new Node{Id=pair.Item1,Logical=logical.transform,Visual=visual,Renderers=visual.GetComponentsInChildren<Renderer>(true),Impostor=service.MakeImpostor(pair.Item1,parent)});
            }
        }
        void LateUpdate()
        {
            if(!host||!host.Ready)return;
            var state=host.City.Gateway.Snapshot();float zoom=host.WorldCamera.orthographicSize;bool near=zoom<110;bool regional=zoom>=110&&zoom<host.WorldNavigation.globalZoom*.85f;
            foreach(var item in state.WorldResourceLocations)
            {
                if(!item.TargetId.StartsWith("region1:",StringComparison.Ordinal)||item.Generation<=0||nodes.Any(x=>x.Id==item.TargetId))continue;
                var logical=host.FindTarget(item.TargetId);if(!logical)continue;
                string family=item.ResourceKind=="wood"?"Wood":"Stone";
                var source=library.GetComponentsInChildren<Transform>(true).Single(x=>x.name=="Resource_"+family+"_"+(nodes.Count%3));
                var visual=Instantiate(source.gameObject,transform);visual.name="Production resource visual "+item.TargetId;visual.transform.rotation=Quaternion.Euler(0,180,0);
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
                nodes.Add(new Node{Id=item.TargetId,Logical=logical.transform,Visual=visual,Renderers=visual.GetComponentsInChildren<Renderer>(true),Impostor=MakeImpostor(item.TargetId,transform)});
            }
            foreach(var node in nodes)
            {
                var p=node.Logical.position;
                p.y=host.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(p.x-host.WorldHome.x,p.z-host.WorldHome.z);
                node.Visual.transform.position=p;
                bool alive=LocalGateway.ResourceRemaining(state,node.Id)>0;
                foreach(var renderer in node.Renderers)renderer.enabled=alive&&near;
                if(node.Impostor){node.Impostor.transform.position=p;node.Impostor.enabled=alive&&regional;}
            }
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                if(renderer.name=="Reference existing quarry"||renderer.name=="Reference existing wood grove")renderer.enabled=false;
        }
    }
}
