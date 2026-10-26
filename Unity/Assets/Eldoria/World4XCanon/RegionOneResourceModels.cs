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
        sealed class Node { public string Id; public Transform Logical; public GameObject Visual; public Renderer[] Renderers; public Renderer Impostor; public MeshFilter[] Parts; public Vector3[] BasePositions; public Mesh[] SourceMeshes; public Vector3 LastPosition; public bool Grounded; public bool AnchorCached; public Vector3 VisualAnchorOffset; public Collider Hit; public bool PickCached; public Vector3 HitCenter,HitSize; public float HitRadius; public bool LastPickRegional; public float LastPickScale=-1; public Vector3 LastPickPosition; public Quaternion LastPickRotation; }
        readonly List<Node> nodes=new List<Node>();
        float lastImpostorScale=-1;
        ValoriaIntegratedWorld host;
        GameObject library;RegionOneLookDevelopment look;
        Eldoria.Domain.PlayerState cachedState;ICommandGateway cachedGateway;float nextState;
        Material impostorMaterial;readonly Mesh[] impostorMeshes=new Mesh[2];
        public void RefreshNow(){nextState=0;LateUpdate();}
        readonly List<Mesh> authoredContactCopies=new List<Mesh>();
        public const float NearCityWidthRatio=.85f;
        // Measure real imported vertices at the playable heading, not asset-local
        // scale multipliers or renderer bounds expanded for shader billboards.
        public static float FitNearWidth(GameObject visual,float desiredWidth)
        {
            float lo=float.PositiveInfinity,hi=float.NegativeInfinity;
            foreach(var part in visual.GetComponentsInChildren<MeshFilter>(true))
                if(part.sharedMesh)foreach(var vertex in part.sharedMesh.vertices){
                    float x=part.transform.TransformPoint(vertex).x;
                    lo=Mathf.Min(lo,x);hi=Mathf.Max(hi,x);
                }
            foreach(var part in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if(part.sharedMesh)foreach(var vertex in part.sharedMesh.vertices){
                    float x=part.transform.TransformPoint(vertex).x;
                    lo=Mathf.Min(lo,x);hi=Mathf.Max(hi,x);
                }
            if(float.IsInfinity(lo)||hi-lo<.001f)throw new InvalidOperationException("Missing measurable model "+visual.name);
            visual.transform.localScale*=desiredWidth/(hi-lo);
            return visual.transform.localScale.x;
        }
        float SoilAt(Vector3 point) => host.WorldHome.y+
            RegionOneBlenderLandscape.Height(point.x-host.WorldHome.x,point.z-host.WorldHome.z);

        void GroundParts(Node node,Vector3 position)
        {
            if(node.Grounded&&(node.LastPosition-position).sqrMagnitude<.000001f)return;
            if(node.Parts==null)
            {
                node.Parts=node.Visual.GetComponentsInChildren<MeshFilter>(true);
                node.BasePositions=node.Parts.Select(x=>x.transform.localPosition).ToArray();
                // Immutable imported Blender topology, reused on resource relocation.
                node.SourceMeshes=node.Parts.Select(x=>x.sharedMesh).ToArray();
            }
            for(int i=0;i<node.Parts.Length;i++)
            {
                var part=node.Parts[i];
                if(!part||!node.SourceMeshes[i])continue;
                // Do not cumulatively deform previously grounded resources.
                if(part.sharedMesh!=node.SourceMeshes[i])
                {
                    var prior=part.sharedMesh;
                    part.sharedMesh=node.SourceMeshes[i];
                    if(prior&&authoredContactCopies.Remove(prior))Destroy(prior);
                }
                part.transform.localPosition=node.BasePositions[i];
                var source=node.SourceMeshes[i];
                var vertices=source.vertices;
                if(part.name.StartsWith("Quarry cleared extraction floor",StringComparison.Ordinal))
                {
                    // A cleared extraction surface follows this real site's
                    // existing ground instead of being buried beneath its grass.
                    // Only the per-instance copy changes; the donor, site and
                    // selection collider retain their original coordinates.
                    var floor=Instantiate(source);
                    floor.name=source.name+" · actual terrain extraction surface";
                    var fitted=floor.vertices;
                    var floorUV=new Vector2[fitted.Length];
                    var footprint=source.bounds;
                    for(int v=0;v<fitted.Length;v++)floorUV[v]=new Vector2(
                        (fitted[v].x-footprint.min.x)/Mathf.Max(.001f,footprint.size.x),
                        (fitted[v].z-footprint.min.z)/Mathf.Max(.001f,footprint.size.z));
                    floor.uv=floorUV;
                    for(int v=0;v<fitted.Length;v++)
                    {
                        var world=part.transform.TransformPoint(fitted[v]);
                        world.y=SoilAt(world)+.003f;
                        fitted[v]=part.transform.InverseTransformPoint(world);
                    }
                    floor.vertices=fitted;floor.RecalculateBounds();floor.RecalculateNormals();
                    part.sharedMesh=floor;authoredContactCopies.Add(floor);
                    continue;
                }
                float bottom=vertices.Min(v=>part.transform.TransformPoint(v).y);
                float highestGap=float.NegativeInfinity;
                foreach(var vertex in vertices)
                {
                    var world=part.transform.TransformPoint(vertex);
                    if(world.y>bottom+.03f)continue;
                    highestGap=Mathf.Max(highestGap,world.y-SoilAt(world));
                }
                if(float.IsNegativeInfinity(highestGap))continue;
                // Preserve exact roof, trunks and ore silhouettes: only move the
                // model PART, never its saved site, hotspot or selection collider.
                part.transform.position-=Vector3.up*highestGap;
                float settledBottom=bottom-highestGap;
                float deepest=0f;
                foreach(var vertex in vertices)
                {
                    var world=part.transform.TransformPoint(vertex);
                    if(world.y>settledBottom+.03f)continue;
                    deepest=Mathf.Min(deepest,world.y-SoilAt(world));
                }
                // A single rigid base can cross uneven genuine terrain. The old
                // max-gap fit buried some near-view quarry/timber corners 0.823 m.
                // Only for those actual cases, gently conform the *cloned* lower
                // geometry to soil; never mutate the protected Blender donor mesh.
                if(deepest<-.55f)
                {
                    var copy=Instantiate(source);
                    copy.name=source.name+" · local grounded contact";
                    var moved=copy.vertices;
                    for(int v=0;v<moved.Length;v++)
                    {
                        var world=part.transform.TransformPoint(vertices[v]);
                        float above=world.y-settledBottom;
                        if(above>1.25f)continue;
                        float depth=SoilAt(world)-world.y;
                        if(depth<=.08f)continue;
                        float weight=1f-Mathf.SmoothStep(0f,1f,
                            Mathf.Clamp01((above-.12f)/1.13f));
                        if(weight<=0f)continue;
                        var corrected=world+Vector3.up*((depth-.08f)*weight);
                        moved[v]=part.transform.InverseTransformPoint(corrected);
                    }
                    copy.vertices=moved;
                    copy.RecalculateBounds();
                    copy.RecalculateNormals();
                    part.sharedMesh=copy;
                    authoredContactCopies.Add(copy);
                }
            }
            node.LastPosition=position;node.Grounded=true;
        }
        void FitRegionalPicking(Node node,Vector3 position,float scale,bool regional)
        {
            if(!node.PickCached){
                node.Hit=node.Logical.GetComponent<Collider>();node.PickCached=true;
                if(node.Hit is BoxCollider box){node.HitCenter=box.center;node.HitSize=box.size;}
                else if(node.Hit is SphereCollider sphere){node.HitCenter=sphere.center;node.HitRadius=sphere.radius;}
            }
            if(!node.Hit)return;
            var rotation=host.WorldCamera.transform.rotation;
            if(node.LastPickRegional==regional&&Mathf.Abs(node.LastPickScale-scale)<.0001f&&
                (node.LastPickPosition-position).sqrMagnitude<.000001f&&Quaternion.Angle(node.LastPickRotation,rotation)<.001f)return;
            node.LastPickRegional=regional;node.LastPickScale=scale;node.LastPickPosition=position;node.LastPickRotation=rotation;
            if(!regional){
                var nearBounds=new Bounds(node.Logical.InverseTransformPoint(node.Renderers[0].bounds.center),Vector3.zero);
                foreach(var renderer in node.Renderers){
                    var b=renderer.bounds;
                    for(int k=0;k<8;k++)nearBounds.Encapsulate(node.Logical.InverseTransformPoint(b.center+
                        Vector3.Scale(b.extents,new Vector3((k&1)==0?-1:1,(k&2)==0?-1:1,(k&4)==0?-1:1))));
                }
                if(node.Hit is BoxCollider box){box.center=nearBounds.center;box.size=Vector3.Max(nearBounds.size+Vector3.one*.3f,node.HitSize);}
                else if(node.Hit is SphereCollider sphere){sphere.center=nearBounds.center;sphere.radius=Mathf.Max(node.HitRadius,nearBounds.extents.magnitude);}
                return;
            }
            // Fit the existing selection volume to the same camera-facing model
            // image. Logical sites, target IDs, route geometry and saves stay put.
            var right=host.WorldCamera.transform.right;var up=host.WorldCamera.transform.up;
            var bounds=new Bounds(node.Logical.InverseTransformPoint(position),Vector3.zero);
            for(int corner=0;corner<4;corner++){
                float x=(corner&1)==0?-16f:16f,y=(corner&2)==0?-1f:25f;
                bounds.Encapsulate(node.Logical.InverseTransformPoint(position+(right*x+up*y)*scale));
            }
            if(node.Hit is BoxCollider pickBox){pickBox.center=bounds.center;pickBox.size=bounds.size+Vector3.one*.5f;}
            else if(node.Hit is SphereCollider pickSphere){pickSphere.center=bounds.center;pickSphere.radius=bounds.extents.magnitude;}
        }
        Renderer MakeImpostor(string id,Transform parent)
        {
            if(!impostorMaterial)return null;
            int tile=id=="forest-valoria"||id.StartsWith("region1:wood:",StringComparison.Ordinal)?0:1;
            if(!impostorMeshes[tile]){
                float u=tile*.5f;var mesh=new Mesh{name="Resource actual-model impostor "+tile};
                mesh.vertices=new[]{Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero};mesh.uv=new[]{new Vector2(u,0),new Vector2(u+.5f,0),new Vector2(u+.5f,1),new Vector2(u,1)};
                mesh.uv2=new[]{new Vector2(-16,-1),new Vector2(16,-1),new Vector2(16,25),new Vector2(-16,25)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.bounds=new Bounds(Vector3.up*12,Vector3.one*40);impostorMeshes[tile]=mesh;
            }
            var go=new GameObject("Production resource impostor "+id);go.transform.SetParent(parent,false);go.layer=29;go.AddComponent<MeshFilter>().sharedMesh=impostorMeshes[tile];var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=impostorMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.enabled=false;return renderer;
        }
        void OnDestroy()
        {
            if(impostorMaterial)Destroy(impostorMaterial);
            foreach(var mesh in impostorMeshes)if(mesh)Destroy(mesh);
            foreach(var copy in authoredContactCopies)if(copy)Destroy(copy);
        }

        public static void Install(Transform parent,ValoriaIntegratedWorld host)
        {
            var library=Resources.Load<GameObject>("World4X/Blender/RegionOneResources");
            if(!library)return;
            var service=parent.gameObject.AddComponent<RegionOneResourceModels>();service.host=host;service.library=library;service.look=parent.GetComponentInChildren<RegionOneLookDevelopment>();
            var atlas=Resources.Load<Texture2D>("World4X/Blender/RegionOneResourceImpostors");
            if(atlas){service.impostorMaterial=new Material(Shader.Find("Eldoria/World4X/LocalCanopy"));service.impostorMaterial.mainTexture=atlas;}
            foreach(var pair in new[]{("forest-valoria","Resource_Wood_0"),("quarry-valoria","Resource_Stone_0")})
            {
                var logical=host.FindTarget(pair.Item1);
                var source=library.GetComponentsInChildren<Transform>(true).Single(x=>x.name==pair.Item2);
                if(!logical)throw new InvalidOperationException("Missing canonical resource target "+pair.Item1);
                var visual=Instantiate(source.gameObject,parent);visual.name="Production resource visual "+pair.Item1;
                if(service.look)service.look.ApplyMaterials(visual);
                visual.transform.localScale=Vector3.one*(pair.Item1=="forest-valoria"?1.55f:1.35f);visual.transform.rotation=Quaternion.Euler(0,180,0);
                FitNearWidth(visual,RegionOneReferenceNodes.BastionOneVisualWidth*NearCityWidthRatio);
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
                service.nodes.Add(new Node{Id=pair.Item1,Logical=logical.transform,Visual=visual,Renderers=visual.GetComponentsInChildren<Renderer>(true),Impostor=service.MakeImpostor(pair.Item1,parent)});
            }
        }
        void LateUpdate()
        {
            if(!host||!host.Ready)return;
            if(cachedState==null||!ReferenceEquals(cachedGateway,host.City.Gateway)||Time.unscaledTime>=nextState){cachedGateway=host.City.Gateway;cachedState=cachedGateway.Snapshot();nextState=Time.unscaledTime+.25f;}
            var state=cachedState;float zoom=host.WorldCamera.orthographicSize;bool near=zoom<110;bool regional=zoom>=110&&!RegionOneResourcePlacement.IsPanoramic(zoom,host.WorldNavigation.globalZoom);
            foreach(var item in state.WorldResourceLocations)
            {
                if(!item.TargetId.StartsWith("region1:",StringComparison.Ordinal)||item.Generation<=0||nodes.Any(x=>x.Id==item.TargetId))continue;
                var logical=host.FindTarget(item.TargetId);if(!logical)continue;
                string family=item.ResourceKind=="wood"?"Wood":"Stone";
                var source=library.GetComponentsInChildren<Transform>(true).Single(x=>x.name=="Resource_"+family+"_"+(nodes.Count%3));
                var visual=Instantiate(source.gameObject,transform);visual.name="Production resource visual "+item.TargetId;
                visual.transform.localScale=Vector3.one*(family=="Wood"?1.55f:1.35f);visual.transform.rotation=Quaternion.Euler(0,180,0);
                FitNearWidth(visual,RegionOneReferenceNodes.BastionOneVisualWidth*NearCityWidthRatio);
                if(look)look.ApplyMaterials(visual);
                foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
                nodes.Add(new Node{Id=item.TargetId,Logical=logical.transform,Visual=visual,Renderers=visual.GetComponentsInChildren<Renderer>(true),Impostor=MakeImpostor(item.TargetId,transform)});
            }
            float screenScale=Mathf.Lerp(1f,1.4f,Mathf.InverseLerp(110f,280f,zoom))*Mathf.Max(1f,zoom/280f);
            if(impostorMaterial&&Mathf.Abs(lastImpostorScale-screenScale)>.0001f){
                impostorMaterial.SetFloat("_BillboardScale",screenScale);lastImpostorScale=screenScale;
            }
            foreach(var node in nodes)
            {
                var p=node.Logical.position;
                p.y=host.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(p.x-host.WorldHome.x,p.z-host.WorldHome.z);
                // The imported wood/stone meshes need not share the logical
                // resource's modelling pivot. Align their visible footprint
                // once with the saved hotspot; the mid/far billboard already
                // uses that same hotspot as its anchor.
                if(!node.AnchorCached)
                {
                    var bounds=node.Renderers[0].bounds;
                    for(int i=1;i<node.Renderers.Length;i++)bounds.Encapsulate(node.Renderers[i].bounds);
                    node.VisualAnchorOffset=new Vector3(
                        node.Visual.transform.position.x-bounds.center.x,
                        0f,
                        node.Visual.transform.position.z-bounds.center.z);
                    node.AnchorCached=true;
                }
                node.Visual.transform.position=p+node.VisualAnchorOffset;
                GroundParts(node,p);
                bool alive=LocalGateway.ResourceRemaining(state,node.Id)>0;
                foreach(var renderer in node.Renderers)if(renderer.enabled!=(alive&&near))renderer.enabled=alive&&near;
                if(node.Impostor){
                    // The near mesh's imported pivot is recentered once above
                    // using VisualAnchorOffset. Its rendered footprint now
                    // coincides with the saved logical site. The regional image
                    // shares that SAME anchor at every zoom and camera aspect.
                    // No per-frame bounds calculation or logical site relocation.
                    node.Impostor.transform.position=p;
                    node.Impostor.enabled=alive&&regional;
                    node.Impostor.localBounds=new Bounds(Vector3.up*12f*screenScale,Vector3.one*40f*screenScale);
                    FitRegionalPicking(node,p,screenScale,regional);
                }
            }
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                if(renderer.name=="Reference existing quarry"||renderer.name=="Reference existing wood grove")renderer.enabled=false;
        }
    }
}

