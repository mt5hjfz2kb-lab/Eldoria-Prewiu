using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaFunctionalScreenReadabilityV1
 {
  public const string RootName="Valoria · Functional Screen Readability v1";
  public static bool Enabled=false;
  public static int Pieces;

  // Single bounded hypothesis per building:
  // - keep the approved GCSU source geometry unchanged;
  // - move each source only enough to expose its authored functional face in played-camera screen space;
  // - increase apparent scale modestly;
  // - create separation by placement, not by small props/material tricks.
  public static readonly Vector3 GraneroPosition=new Vector3(-2.35f,.16f,-5.00f);
  public const float GraneroFootprint=5.68f;
  public const float GraneroHeight=4.68f;
  public const float GraneroYaw=0f;

  public static readonly Vector3 CuartelPosition=new Vector3(5.55f,.16f,-4.70f);
  public const float CuartelFootprint=5.62f;
  public const float CuartelHeight=4.72f;
  public const float CuartelYaw=0f;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   Pieces=0;
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   if(!Enabled)return;

   // FPD and GCSU remain intact as historical/certified inputs. This layer only replaces
   // their two visible building renderers. Gameplay targets, parcels, routes and colliders stay untouched.
   SuppressByChain(
    "valoria · fpd v1 · granero production",
    "valoria · fpd v1 · cuartel production",
    "valoria · gcsu v1 · granero upgraded source",
    "valoria · gcsu v1 · cuartel upgraded source");

   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   var granero=Load("Valoria_Granero_GCSUv1");
   var cuartel=Load("Valoria_Cuartel_GCSUv1");

   Piece(root,granero,"Granero readable layout",GraneroPosition,GraneroFootprint,GraneroHeight,GraneroYaw);
   Piece(root,cuartel,"Cuartel readable layout",CuartelPosition,CuartelFootprint,CuartelHeight,CuartelYaw);
   StripGameplay(root.gameObject);
  }

  static GameObject Load(string n)
  {
   var x=Resources.Load<GameObject>("Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/"+n);
   if(x==null)throw new InvalidOperationException("Missing approved Granero/Cuartel source "+n);
   return x;
  }

  static void Piece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · FSR v1 · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
   if(go==null)throw new InvalidOperationException("Failed functional-readability piece "+role);
   go.transform.SetParent(root,true);
   foreach(var r in go.GetComponentsInChildren<Renderer>(true))
   {
    r.receiveShadows=true;
    r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
   }
   StripGameplay(go);Pieces++;
  }

  static void SuppressByChain(params string[] needles)
  {
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    string chain="";
    for(var t=r.transform;t!=null;t=t.parent)chain+="|"+t.name.ToLowerInvariant();
    foreach(var n in needles)
     if(chain.Contains(n)){r.enabled=false;break;}
   }
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
  }
 }
}
