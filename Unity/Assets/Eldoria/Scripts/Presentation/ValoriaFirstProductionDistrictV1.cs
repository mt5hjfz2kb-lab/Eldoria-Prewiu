using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaFirstProductionDistrictV1
 {
  public const string RootName="Valoria · First Production District v1";
  public static bool Enabled=true;
  public static bool UseGraneroCuartelSourceUpgrade=false;
  public static int Pieces;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   Pieces=0;
   if(!Enabled)return;
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);Pieces=0;

   SuppressByChain("valoria · production art · main gate","valoria · production art · west entry tower",
    "valoria · production art · east entry tower","valoria · production art · west entry curtain",
    "valoria · production art · east entry curtain","flat citadel production · cuartel",
    "flat citadel production · granero","cuartel · dedicated barracks","cuartel · fallback guardhouse","granero","nation1 fortification");

   var gate=Load("Valoria_MainGate_FPDv1");
   var tower=Load("Valoria_DefenseTower_FPDv1");
   var wall=Load("Valoria_DefenseWall_FPDv1");
   var granero=UseGraneroCuartelSourceUpgrade?LoadUpgrade("Valoria_Granero_GCSUv1"):Load("Valoria_Granero_FPDv1");
   var cuartel=UseGraneroCuartelSourceUpgrade?LoadUpgrade("Valoria_Cuartel_GCSUv1"):Load("Valoria_Cuartel_FPDv1");

   Piece(root,gate,"main gate",new Vector3(0f,.14f,-8.55f),5.35f,4.05f,0f);
   Piece(root,tower,"west gate tower",new Vector3(-4.15f,.13f,-8.15f),2.72f,4.35f,4f);
   Piece(root,tower,"east gate tower",new Vector3(4.15f,.13f,-8.15f),2.72f,4.35f,-4f);
   Piece(root,wall,"west visible curtain",new Vector3(-7.15f,.13f,-7.90f),3.65f,2.12f,5f);
   Piece(root,wall,"east visible curtain",new Vector3(7.15f,.13f,-7.90f),3.65f,2.12f,-5f);
   Piece(root,granero,"Granero production",new Vector3(-3.10f,.16f,-4.45f),5.05f,4.15f,7f);
   Piece(root,cuartel,"Cuartel production",new Vector3(6.90f,.16f,-3.65f),5.15f,4.35f,-8f);
   StripGameplay(root.gameObject);
  }

  static GameObject Load(string n)
  {
   var x=Resources.Load<GameObject>("Valoria/ProductionArt/FirstProductionDistrictV1/"+n);
   if(x==null)throw new InvalidOperationException("Missing First Production District resource "+n);
   return x;
  }

  static GameObject LoadUpgrade(string n)
  {
   var x=Resources.Load<GameObject>("Valoria/ProductionArt/GraneroCuartelSourceUpgradeV1/"+n);
   if(x==null)throw new InvalidOperationException("Missing Granero/Cuartel source-upgrade resource "+n);
   return x;
  }

  static void Piece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
  {
   var go=ValoriaKit.BenchmarkPiece("Valoria · FPD v1 · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
   if(go==null)throw new InvalidOperationException("Failed First Production District piece "+role);
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
