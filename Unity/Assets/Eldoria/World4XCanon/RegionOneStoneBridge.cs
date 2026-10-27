using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEngine;
namespace Eldoria.World4XCanon {
 // Replaces ONLY the existing Valoria crossing's visible timber; original terrain collider is retained.
 public sealed class RegionOneStoneBridge:MonoBehaviour {
  readonly Dictionary<Renderer,Material[]> original=new Dictionary<Renderer,Material[]>();
  readonly List<Material> owned=new List<Material>();readonly Dictionary<Renderer,bool> rails=new Dictionary<Renderer,bool>();GameObject bridge;
  public static void Install(Transform parent,ValoriaIntegratedWorld host){
   var prefab=Resources.Load<GameObject>("ValoriaStrategic/StoneBridge");if(!prefab||parent.GetComponent<RegionOneStoneBridge>())return;
   var service=parent.gameObject.AddComponent<RegionOneStoneBridge>();service.Build(host,prefab);
  }
  void Build(ValoriaIntegratedWorld host,GameObject prefab){
   bool Centre(Vector3 p)=>Mathf.Abs(p.x-host.WorldHome.x)<30&&Mathf.Abs(p.z-host.WorldHome.z)<100;
   // Retain the large terrain mesh and its collider byte-for-byte. Only two
   // tiny material instances clip the replaced deck at the measured crossing.
   foreach(var renderer in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)){
    var materials=renderer.sharedMaterials;Material[] replacement=null;
    for(int sub=0;sub<materials.Length;sub++){
     var source=materials[sub];if(!source)continue;
     if(!source.name.StartsWith("Bridge weathered timber",StringComparison.Ordinal)&&!source.name.StartsWith("Bridge alternate weathered plank",StringComparison.Ordinal))continue;
     if(!source.HasProperty("_HiddenCrossing"))throw new InvalidOperationException("Canonical crossing surface shader missing");
     if(replacement==null)replacement=(Material[])materials.Clone();
     var material=new Material(source){name=source.name+" retained outside Valoria"};
     material.SetVector("_HiddenCrossing",new Vector4(host.WorldHome.x,host.WorldHome.y,host.WorldHome.z,1));
     owned.Add(material);replacement[sub]=material;
    }
    if(replacement!=null){original[renderer]=materials;renderer.sharedMaterials=replacement;}
   }
   Debug.Log("VALORIA_STONE_BRIDGE terrain_mesh_copies=0 retained_collision=1 clipped_materials="+owned.Count);
   foreach(var renderer in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.name.StartsWith("BLD_CrossingRails_",StringComparison.Ordinal)&&Centre(r.bounds.center))){rails[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}
   bridge=Instantiate(prefab,transform);bridge.name="Valoria approved city-derived stone bridge";bridge.transform.position=host.WorldHome;bridge.transform.rotation=Quaternion.identity;bridge.transform.localScale=Vector3.one;
   foreach(var renderer in bridge.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
  }
  void OnDestroy(){foreach(var pair in original)if(pair.Key)pair.Key.sharedMaterials=pair.Value;foreach(var pair in rails)if(pair.Key)pair.Key.forceRenderingOff=pair.Value;foreach(var material in owned)if(material)Destroy(material);if(bridge)Destroy(bridge);}
 }
}
