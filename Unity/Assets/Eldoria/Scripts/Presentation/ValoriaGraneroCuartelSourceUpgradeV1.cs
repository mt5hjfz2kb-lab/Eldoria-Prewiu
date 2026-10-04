using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaGraneroCuartelSourceUpgradeV1
 {
  public const string RootName="Valoria · Granero Cuartel Source Upgrade v1";
  public static bool Enabled=false;
  public static int Pieces;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   Pieces=0;
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   if(!Enabled)return;

   // Replace only the two weak FPD civilian/military building proxies. Defense, terrain,
   // camera, Final Look and gameplay geometry remain owned by their existing systems.
   SuppressByChain("valoria · fpd v1 · granero production","valoria · fpd v1 · cuartel production");

   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   var granero=Load("Valoria_Granero_GCSUv1");
   var cuartel=Load("Valoria_Cuartel_GCSUv1");

   Piece(root,granero,"Granero upgraded source",new Vector3(-3.10f,.16f,-4.45f),5.05f,4.15f,7f);
   Piece(root,cuartel,"Cuartel upgraded source",new Vector3(6.90f,.16f,-3.65f),5.15f,4.35f,-8f);
   StripGameplay(root.gameObject);
  }

  static GameObject Load(string n)
  {
   var x=Resources.Load<GameObject>("Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/"+n);
   if(x==null)throw new InvalidOperationException("Missing Granero/Cuartel source-upgrade resource "+n);
   return x;
  }

  static void Piece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · GCSU v1 · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
   if(go==null)throw new InvalidOperationException("Failed Granero/Cuartel upgraded piece "+role);
   go.transform.SetParent(root,true);
   foreach(var r in go.GetComponentsInChildren<Renderer>(true)){r.receiveShadows=true;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;}
   StripGameplay(go);Pieces++;
  }

  static void SuppressByChain(params string[] needles)
  {
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    string chain="";for(var t=r.transform;t!=null;t=t.parent)chain+="|"+t.name.ToLowerInvariant();
    foreach(var n in needles)if(chain.Contains(n)){r.enabled=false;break;}
   }
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
  }
 }
}
