using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;
namespace Eldoria.World4XCanon {
 // Replaces ONLY the existing Valoria crossing's visible timber; original terrain collider is retained.
 public sealed class RegionOneStoneBridge:MonoBehaviour {
  readonly Dictionary<Material,Vector4> original=new Dictionary<Material,Vector4>();
  readonly Dictionary<Renderer,bool> rails=new Dictionary<Renderer,bool>();GameObject bridge;
  public static void Install(Transform parent,ValoriaIntegratedWorld host){
   var prefab=Resources.Load<GameObject>("ValoriaStrategic/StoneBridge");if(!prefab||parent.GetComponent<RegionOneStoneBridge>())return;
   var service=parent.gameObject.AddComponent<RegionOneStoneBridge>();service.Build(host,prefab);
  }
  void Build(ValoriaIntegratedWorld host,GameObject prefab){
   bool Centre(Vector3 p)=>Mathf.Abs(p.x-host.WorldHome.x)<30&&Mathf.Abs(p.z-host.WorldHome.z)<100;
   // LookDevelopment already owns per-world material instances. Keep those
   // instances so zoom-dependent haze continues to update; never clone terrain.
   foreach(var renderer in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)){
    foreach(var material in renderer.sharedMaterials){
     if(!material||original.ContainsKey(material))continue;
     if(!material.name.StartsWith("Bridge weathered timber",StringComparison.Ordinal)&&!material.name.StartsWith("Bridge alternate weathered plank",StringComparison.Ordinal))continue;
     if(!material.HasProperty("_HiddenCrossing")||!material.name.EndsWith(" AG look",StringComparison.Ordinal))throw new InvalidOperationException("Scoped canonical crossing surface material missing");
     original[material]=material.GetVector("_HiddenCrossing");
     material.SetVector("_HiddenCrossing",new Vector4(host.WorldHome.x,host.WorldHome.y,host.WorldHome.z,1));
    }
   }
   Debug.Log("VALORIA_STONE_BRIDGE terrain_mesh_copies=0 retained_collision=1 clipped_materials="+original.Count);
   foreach(var renderer in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("BLD_CrossingRails_",StringComparison.Ordinal)&&Centre(r.bounds.center))){rails[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}
   bridge=Instantiate(prefab,transform);bridge.name="Valoria approved city-derived stone bridge";bridge.transform.position=host.WorldHome;bridge.transform.rotation=Quaternion.identity;bridge.transform.localScale=Vector3.one;
   foreach(var renderer in bridge.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
  }
  void OnDestroy(){foreach(var pair in original)if(pair.Key)pair.Key.SetVector("_HiddenCrossing",pair.Value);foreach(var pair in rails)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;if(bridge)Destroy(bridge);}
 }
}
