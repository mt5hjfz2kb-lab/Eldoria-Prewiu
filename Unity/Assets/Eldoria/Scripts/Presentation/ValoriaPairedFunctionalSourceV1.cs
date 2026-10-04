using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaPairedFunctionalSourceV1
 {
  public const string RootName="Valoria · Paired Functional Source v1";
  public static bool Enabled=false;
  public static int Pieces;

  static readonly Vector3 GraneroPosition=new Vector3(-3.10f,.16f,-4.45f);
  const float GraneroFootprint=5.55f;
  const float GraneroHeight=4.50f;
  const float GraneroYaw=0f;

  static readonly Vector3 CuartelPosition=new Vector3(6.90f,.16f,-3.65f);
  const float CuartelFootprint=6.10f;
  const float CuartelHeight=4.55f;
  const float CuartelYaw=0f;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   Pieces=0;
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   if(!Enabled)return;

   SuppressByChain(
    "valoria · fpd v1 · granero production",
    "valoria · fpd v1 · cuartel production",
    "valoria · gcsu v1 · granero upgraded source",
    "valoria · gcsu v1 · cuartel upgraded source",
    "valoria · fsr v1 · granero readable layout",
    "valoria · fsr v1 · cuartel readable layout",
    "valoria · cfs v1 · granero camera-first",
    "valoria · cfs v1 · cuartel camera-first");

   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   Piece(root,LoadGranero(),"Granero rich camera-first",GraneroPosition,GraneroFootprint,GraneroHeight,GraneroYaw);
   Piece(root,LoadCuartel(),"Cuartel rich camera-first",CuartelPosition,CuartelFootprint,CuartelHeight,CuartelYaw);
   StripGameplay(root.gameObject);
  }

  static GameObject LoadGranero()
  {
   var x=Resources.Load<GameObject>("Valoria/ProductionArt/GraneroRichCameraFirstV1/Valoria_Granero_RCFv1");
   if(x==null)throw new InvalidOperationException("Missing Granero RCFv1");
   return x;
  }

  static GameObject LoadCuartel()
  {
   var x=Resources.Load<GameObject>("Valoria/ProductionArt/CameraFirstFunctionalSourceV1/Valoria_Cuartel_CFSv1");
   if(x==null)throw new InvalidOperationException("Missing Cuartel CFSv1");
   return x;
  }

  static void Piece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · PFS v1 · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
   if(go==null)throw new InvalidOperationException("Failed paired functional source "+role);
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
