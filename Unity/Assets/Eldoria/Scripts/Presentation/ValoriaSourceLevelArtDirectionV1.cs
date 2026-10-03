using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaSourceLevelArtDirectionV1
 {
  public const string RootName="Valoria · Source-Level Art Direction Proof v1";
  public static int Modules;
  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);Modules=0;
   // Hero interface: shallow authored terraces in front of fused rock, preserving source silhouette.
   Add(root,"Valoria/SourceLevelArtDirection_v1/SLAD_HeroRockInterface_v1","Hero rock interface",
       new Vector3(0f,.13f,5.15f),5.9f,1.75f,Quaternion.identity);
   // Aserradero: source building stays intact; authored plinth/eaves/braces share Hero/wall vocabulary.
   Add(root,"Valoria/SourceLevelArtDirection_v1/SLAD_AserraderoKit_v1","Aserradero authored kit",
       new Vector3(-5.95f,.03f,-1.55f),4.8f,2.15f,Quaternion.identity);
   // One representative front-west curtain segment only; bounded proof, not city rollout.
   Add(root,"Valoria/SourceLevelArtDirection_v1/SLAD_WallSegment_v1","Front west authored wall",
       new Vector3(-5.55f,.02f,-6.25f),4.8f,2.35f,Quaternion.identity);
   StripGameplay(root.gameObject);
  }
  static void Add(Transform root,string resource,string name,Vector3 p,float footprint,float h,Quaternion q)
  {
   var src=Resources.Load<GameObject>(resource);
   if(src==null)throw new InvalidOperationException("Missing source-level authored resource: "+resource);
   var go=ValoriaKit.BenchmarkPiece("SLAD · "+name,src,p,footprint,h,q);
   if(go==null)throw new InvalidOperationException("Failed to place "+resource);
   go.transform.SetParent(root,true);StripGameplay(go);Modules++;
  }
  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
  }
 }
}
