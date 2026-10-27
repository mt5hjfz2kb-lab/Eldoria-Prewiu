using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;
namespace Eldoria.World4XCanon {
 // Replaces ONLY the existing Valoria crossing's visible timber; original terrain collider is retained.
 public sealed class RegionOneStoneBridge:MonoBehaviour {
  readonly Dictionary<MeshFilter,Mesh> original=new Dictionary<MeshFilter,Mesh>();
  readonly List<Mesh> owned=new List<Mesh>();readonly Dictionary<Renderer,bool> rails=new Dictionary<Renderer,bool>();GameObject bridge;
  public static void Install(Transform parent,ValoriaIntegratedWorld host){
   var prefab=Resources.Load<GameObject>("ValoriaStrategic/StoneBridge");if(!prefab||parent.GetComponent<RegionOneStoneBridge>())return;
   var service=parent.gameObject.AddComponent<RegionOneStoneBridge>();service.Build(host,prefab);
  }
  void Build(ValoriaIntegratedWorld host,GameObject prefab){
   bool Centre(Vector3 p)=>Mathf.Abs(p.x-host.WorldHome.x)<30&&Mathf.Abs(p.z-host.WorldHome.z)<100;
   foreach(var filter in host.WorldRoot.GetComponentsInChildren<MeshFilter>(true)){
    var renderer=filter.GetComponent<Renderer>();if(!renderer||!filter.sharedMesh)continue;var mesh=filter.sharedMesh;var vertices=mesh.vertices;Mesh copy=null;
    for(int sub=0;sub<Mathf.Min(mesh.subMeshCount,renderer.sharedMaterials.Length);sub++){
     var name=renderer.sharedMaterials[sub].name;if(!name.StartsWith("Bridge weathered timber",StringComparison.Ordinal)&&!name.StartsWith("Bridge alternate weathered plank",StringComparison.Ordinal))continue;
     var triangles=mesh.GetTriangles(sub);var kept=new List<int>();for(int i=0;i<triangles.Length;i+=3){var point=filter.transform.TransformPoint((vertices[triangles[i]]+vertices[triangles[i+1]]+vertices[triangles[i+2]])/3);if(!Centre(point))kept.AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});}
     if(kept.Count!=triangles.Length){if(!copy)copy=Instantiate(mesh);copy.SetTriangles(kept,sub);}
    }
    if(copy){original[filter]=mesh;owned.Add(copy);filter.sharedMesh=copy;}
   }
   foreach(var renderer in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("BLD_CrossingRails_",StringComparison.Ordinal)&&Centre(r.bounds.center))){rails[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}
   bridge=Instantiate(prefab,transform);bridge.name="Valoria approved city-derived stone bridge";bridge.transform.position=host.WorldHome;bridge.transform.rotation=Quaternion.identity;bridge.transform.localScale=Vector3.one;
   foreach(var renderer in bridge.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
  }
  void OnDestroy(){foreach(var pair in original)if(pair.Key)pair.Key.sharedMesh=pair.Value;foreach(var pair in rails)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;foreach(var mesh in owned)if(mesh)Destroy(mesh);if(bridge)Destroy(bridge);}
 }
}
