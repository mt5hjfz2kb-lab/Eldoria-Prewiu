using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.Presentation {
public static class ValoriaProductionArtResetV1 {
 public const string RootName="Valoria · Production Art Reset v1"; public static int Pieces;
 public static void Apply(Transform canonicalRoot,PlayerState state){
  if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
  var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);Pieces=0;
  var wall=Load("Valoria_WallSegment_v1");var gate=Load("Valoria_MainGate_v1");var tower=Load("Valoria_Tower_v1");var house=Load("Valoria_CivicHouse_v1");var workshop=Load("Valoria_Workshop_v1");
  HideOldSupportArchitecture();
  Piece(root,gate,"main gate",new Vector3(0,.10f,-6.52f),5.35f,4.65f,0);
  Piece(root,tower,"gate west tower",new Vector3(-3.55f,.10f,-6.15f),2.55f,4.70f,2);Piece(root,tower,"gate east tower",new Vector3(3.55f,.10f,-6.15f),2.55f,4.70f,-2);
  foreach(var x in new[]{-7.85f,-5.45f,5.45f,7.85f})Piece(root,wall,"front curtain",new Vector3(x,.09f,-6.28f),3.15f,2.35f,0);
  foreach(var z in new[]{-3.2f,.15f,3.5f,6.85f}){Piece(root,wall,"west curtain",new Vector3(-9.25f,.09f,z),3.35f,2.35f,90);Piece(root,wall,"east curtain",new Vector3(9.25f,.09f,z),3.35f,2.35f,90);}
  foreach(var x in new[]{-7.1f,-3.55f,0f,3.55f,7.1f})Piece(root,wall,"rear curtain",new Vector3(x,.09f,9f),3.45f,2.25f,0);
  Piece(root,tower,"rear west tower",new Vector3(-9.05f,.10f,8.82f),2.45f,4.35f,172);Piece(root,tower,"rear east tower",new Vector3(9.05f,.10f,8.82f),2.45f,4.35f,188);
  Building(root,house,"upper west civic",new Vector3(-5.75f,.13f,3.15f),2.75f,3.65f,14);Building(root,house,"upper east civic",new Vector3(5.75f,.13f,3.12f),2.75f,3.65f,-14);
  Building(root,house,"plaza west civic",new Vector3(-3.15f,.13f,.95f),2.30f,3.15f,7);Building(root,house,"plaza east civic",new Vector3(3.15f,.13f,.95f),2.30f,3.15f,-7);
  Building(root,workshop,"lower west workshop",new Vector3(-5.25f,.13f,-3.45f),2.65f,3.10f,12);Building(root,workshop,"lower east workshop",new Vector3(5.10f,.13f,-3.65f),2.65f,3.10f,-12);
  ApplyFinalLook(root);StripGameplay(root.gameObject);
 }
 static GameObject Load(string n){var x=Resources.Load<GameObject>("Valoria/ProductionArt/StarterFamily/"+n);if(x==null)throw new InvalidOperationException("Missing production art starter resource "+n);return x;}
 static void HideOldSupportArchitecture(){foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled)continue;string n=Chain(r.transform);bool w=(n.Contains("breakthrough")||n.Contains("flat citadel production")||n.Contains("art consolidation"))&&(n.Contains("wall")||n.Contains("tower")||n.Contains("gate")||n.Contains("curtain")||n.Contains("retaining")||n.Contains("watchtower"));if(w)r.enabled=false;}}
 static void Piece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw){var go=ValoriaKit.BenchmarkPiece("Valoria · Production Art · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));if(go==null)throw new InvalidOperationException("Failed production piece "+role);go.transform.SetParent(root,true);StripGameplay(go);Pieces++;}
 static void Building(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)=>Piece(root,src,role,p,footprint,height,yaw);
 static string Chain(Transform t){string s="";for(;t!=null;t=t.parent)s+="|"+t.name.ToLowerInvariant();return s;}
 static void ApplyFinalLook(Transform root){RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.70f,.78f,.84f,1);RenderSettings.ambientEquatorColor=new Color(.56f,.50f,.42f,1);RenderSettings.ambientGroundColor=new Color(.24f,.21f,.18f,1);RenderSettings.ambientIntensity=1.02f;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.46f,.52f,.54f,1);RenderSettings.fogStartDistance=40;RenderSettings.fogEndDistance=72;foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){if(l.type!=LightType.Directional)continue;l.color=new Color(1,.91f,.78f,1);l.intensity=Mathf.Max(l.intensity,1.15f);l.shadows=LightShadows.Soft;l.shadowStrength=.72f;l.transform.rotation=Quaternion.Euler(48,-34,0);}Warm(root,"gate",new Vector3(0,1.45f,-5.75f),.58f,4);}
 static void Warm(Transform root,string n,Vector3 p,float i,float range){var go=new GameObject("Valoria · Production Art · warm "+n);go.transform.SetParent(root,true);go.transform.position=p;var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.57f,.28f);l.intensity=i;l.range=range;l.shadows=LightShadows.None;}
 static void StripGameplay(GameObject go){foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);}
}}