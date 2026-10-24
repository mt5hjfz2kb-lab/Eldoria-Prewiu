using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.Rendering;
namespace Eldoria.World4XCanon
{
 // Reference-derived strategic representations attached to existing destinations.
 // Never creates or moves a gameplay target, collider, command or save field.
 [DefaultExecutionOrder(1200)]
 public sealed class RegionOneReferenceNodes : MonoBehaviour
 {
  ValoriaIntegratedWorld host;Material nodes,trees;bool before;
  readonly Dictionary<string,Vector3> resourceOrigins=new Dictionary<string,Vector3>();
  readonly List<Renderer> old=new List<Renderer>();readonly List<bool> states=new List<bool>();readonly List<bool> mountains=new List<bool>();
  readonly List<LODGroup> lods=new List<LODGroup>();readonly List<bool> lodStates=new List<bool>();
  readonly List<Renderer> current=new List<Renderer>();readonly List<GameObject> gates=new List<GameObject>();readonly List<Mesh> meshes=new List<Mesh>();
  public static RegionOneReferenceNodes Install(Transform parent,ValoriaIntegratedWorld host)
  {
   var texture=Resources.Load<Texture2D>("World4X/RegionOneReferenceNodes");var shader=Shader.Find("Eldoria/World4X/LocalCanopy");
   if(!texture||!shader)return null;
   var c=parent.gameObject.AddComponent<RegionOneReferenceNodes>();c.host=host;
   c.nodes=new Material(shader){name="Region1 reference city quarry ruin"};c.nodes.mainTexture=texture;
   c.trees=new Material(shader){name="Region1 real wood node grove"};c.trees.mainTexture=Resources.Load<Texture2D>("World4X/RegionOnePaintedForest");
   c.Build();return c;
  }
  Transform Find(string name)=>host.WorldRoot.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
  Bounds BoundsOf(GameObject go)
  {var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  void Hide(GameObject go,bool mountain=false)
  {if(!go)return;foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))if(!lods.Contains(lod)){lods.Add(lod);lodStates.Add(lod.enabled);lod.enabled=false;}foreach(var r in go.GetComponentsInChildren<Renderer>(true))if(!old.Contains(r)){old.Add(r);states.Add(r.enabled);mountains.Add(mountain);}}
  Vector3 GroundFoot(Vector3 p){p.y=host.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(p.x-host.WorldHome.x,p.z-host.WorldHome.z)+.08f;return p;}
  Vector3 Foot(Bounds b)=>GroundFoot(new Vector3(b.center.x,b.min.y,b.min.z));
  void Build()
  {
   if(host.WorldBastionOne){var b=BoundsOf(host.WorldBastionOne);Sprite("Reference city Bastion I",Foot(b),24,0,host.WorldBastionOne);Hide(host.WorldBastionOne);}
   if(host.WorldBastionTwo){var b=BoundsOf(host.WorldBastionTwo);Sprite("Reference city Bastion II",Foot(b),28,0,host.WorldBastionTwo);Hide(host.WorldBastionTwo);}
   Hide(host.WorldSawmill);
   var quarry=host.FindTarget("quarry-valoria");if(quarry){var f=quarry.transform.position;f.y=host.WorldHome.y+RegionOneCanonicalLandscape.SampleHeight(f.x-host.WorldHome.x,f.z-host.WorldHome.z)+.1f;Sprite("Reference existing quarry",f,13,1,null);}
   foreach(var t in host.WorldRoot.GetComponentsInChildren<Transform>(true))
   {
    if(t.name.StartsWith("Existing Holotna rock",StringComparison.Ordinal))
    {var b=BoundsOf(t.gameObject);if(t.gameObject.activeInHierarchy&&t.name!="Existing Holotna rock.001")Sprite("Reference pale field rock "+t.name,Foot(b),Mathf.Clamp(b.size.x,4,9),1,null);Hide(t.gameObject);}
    if(t.name.StartsWith("Valoria map / retained ruins",StringComparison.Ordinal)||t.name.StartsWith("Zone1 quarry /",StringComparison.Ordinal)||t.name.StartsWith("Retained functional quarry equipment /",StringComparison.Ordinal)||t.name=="pile_of_planks"||t.name.StartsWith("Quarry ",StringComparison.Ordinal)||t.name=="Connected graded extraction apron"||t.name=="Forest cut timber and sawbuck"||t.name=="Forest fallen timber / retained clearing")Hide(t.gameObject);
   }
   var ruin=host.FindTarget("old-watch-ruin");
   if(ruin)
   {var f=ruin.transform.position;f=GroundFoot(f);Sprite("Reference existing neutral watch ruin",f,11,2,null);
    foreach(var name in new[]{"Recovered neutral ruin Arch_Gothic.fbx","Recovered neutral ruin Wall_Broken.fbx","Recovered neutral ruin Column_Round.fbx"}){var t=Find(name);if(t)Hide(t.gameObject);}}
   var timber=Find("Wood resource existing timber stock");var wood=host.FindTarget("forest-valoria");
   if(timber&&wood){var b=BoundsOf(timber.gameObject);BuildWoodGrove(GroundFoot(wood.transform.position));Hide(timber.gameObject);}
   foreach(var t in host.WorldRoot.GetComponentsInChildren<Transform>(true))
    if(t.name.StartsWith("Existing Holotna mountain",StringComparison.Ordinal))Hide(t.gameObject,true);
   foreach(var id in new[]{"forest-valoria","quarry-valoria"}){var t=host.FindTarget(id);if(t)resourceOrigins[id]=t.transform.position;}
   Debug.Log("REGION1_REFERENCE_NODES visuals="+current.Count+" old_renderers="+old.Count+" new_hotspots=0 new_colliders=0");Refresh();
  }
  void Sprite(string name,Vector3 foot,float width,int cell,GameObject gate)
  {
   // Measured alpha bounds in the original2172x724asset, no destructive cropping.
   var boxes=new[]{new Vector4(56,16,675,681),new Vector4(773,111,1420,643),new Vector4(1515,129,2130,650)};
   var b=boxes[cell];float height=width*(b.w-b.y)/(b.z-b.x);
   var uv=new[]{new Vector2(b.x/2172f,1-b.w/724f),new Vector2(b.z/2172f,1-b.w/724f),new Vector2(b.z/2172f,1-b.y/724f),new Vector2(b.x/2172f,1-b.y/724f)};
   Create(name,new[]{foot,foot,foot,foot},uv,new[]{new Vector2(-width/2,0),new Vector2(width/2,0),new Vector2(width/2,height),new Vector2(-width/2,height)},new[]{0,1,2,0,2,3},nodes,gate);
  }
  void BuildWoodGrove(Vector3 centre)
  {
   var v=new List<Vector3>();var uv=new List<Vector2>();var offsets=new List<Vector2>();var tris=new List<int>();
   for(int i=0;i<7;i++)
   {float a=i*2.4f,r=i==0?0:2.5f+i*.19f;var foot=centre+new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);float h=5.8f+(i%3)*.7f,w=h*.65f;int n=v.Count;
    for(int k=0;k<4;k++){bool right=k==1||k==2,top=k>=2;v.Add(foot);uv.Add(new Vector2((i%3+(right?.995f:.005f))/3f,top?1:0));offsets.Add(new Vector2((right?.5f:-.5f)*w,top?h:0));}tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
   Create("Reference existing wood grove",v.ToArray(),uv.ToArray(),offsets.ToArray(),tris.ToArray(),trees,null);
  }
  void Create(string name,Vector3[] v,Vector2[] uv,Vector2[] offsets,int[] tris,Material material,GameObject gate)
  {
   var mesh=new Mesh{name=name,vertices=v,uv=uv,uv2=offsets,triangles=tris};mesh.RecalculateBounds();var bounds=mesh.bounds;bounds.Expand(65);mesh.bounds=bounds;meshes.Add(mesh);
   var go=new GameObject(name);go.transform.SetParent(transform,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;current.Add(r);gates.Add(gate);
  }
  public void SetBefore(bool value){before=value;Refresh();}
  public void Refresh()
  {
   if(!host||!host.WorldCamera)return;float zoom=host.WorldCamera.orthographicSize;bool far=zoom>=152;float near=host.WorldCamera.aspect<.8f?72:41;
   for(int i=0;i<lods.Count;i++)if(lods[i])lods[i].enabled=before&&lodStates[i];
   for(int i=0;i<old.Count;i++)if(old[i])old[i].enabled=before&&!far&&states[i]&&(!mountains[i]||zoom<=near);
   var state=host.City.Gateway.Snapshot();
   for(int i=0;i<current.Count;i++)if(current[i]){
    string id=current[i].name=="Reference existing quarry"?"quarry-valoria":current[i].name=="Reference existing wood grove"?"forest-valoria":null;
    bool available=id==null||(id=="forest-valoria"?state.ForestRemaining>0:state.QuarryRemaining>0);
    if(id!=null&&resourceOrigins.TryGetValue(id,out var original)){var target=host.FindTarget(id);if(target){var delta=target.transform.position-original;var saved=state.WorldResourceLocations.Find(x=>x.TargetId==id);if(saved!=null&&saved.Generation>0)delta.y=target.transform.position.y-GroundFoot(original).y;current[i].transform.position=delta;}}
    current[i].enabled=!(id!=null&&Resources.Load<GameObject>("World4X/Blender/RegionOneResources"))&&!before&&!far&&available&&(!gates[i]||gates[i].activeInHierarchy);
   }
  }
  void LateUpdate(){Refresh();}
  void OnDestroy(){for(int i=0;i<lods.Count;i++)if(lods[i])lods[i].enabled=lodStates[i];for(int i=0;i<old.Count;i++)if(old[i])old[i].enabled=states[i];foreach(var r in current)if(r)Destroy(r.gameObject);foreach(var m in meshes)if(m)Destroy(m);if(nodes)Destroy(nodes);if(trees)Destroy(trees);}
 }
}
