using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaSemanticSourceReauthoringV1
 {
  public const string RootName="Valoria · Semantic Source Reauthoring Proof v1";
  public static int HiddenSourceRenderers,PlacedModules;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   HiddenSourceRenderers=PlacedModules=0;

   HideChain("certified hero bastion");
   HideChain("aserradero · dedicated sawmill");
   HideChain("breakthrough · front west 1");

   PlaceHero(root);
   PlaceBenchmark(root,"Valoria/SemanticSourceReauthoring_v1/SSRA_Aserradero_v1","Aserradero semantic source",
       new Vector3(-7.20f,.42f,-2.70f),3.50f,3.80f,Quaternion.Euler(0,180f,0));
   PlaceBenchmark(root,"Valoria/SemanticSourceReauthoring_v1/SSRA_WallSegment_v1","Front west semantic wall",
       new Vector3(-6.647f,.18f,-6.28f),2.05f,1.31f,Quaternion.Euler(0,1.25f,0));
   StripGameplay(root.gameObject);
  }

  static void HideChain(string fragment)
  {
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   {
    if(r==null||!r.enabled)continue;
    for(var t=r.transform;t!=null;t=t.parent)
    {
     if(t.name.IndexOf(fragment,StringComparison.OrdinalIgnoreCase)<0)continue;
     r.enabled=false;HiddenSourceRenderers++;break;
    }
   }
  }

  static void PlaceHero(Transform root)
  {
   var src=Resources.Load<GameObject>("Valoria/SemanticSourceReauthoring_v1/SSRA_HeroBastion_v1");
   if(src==null)throw new InvalidOperationException("Missing SSRA Hero source.");
   var go=Object.Instantiate(src);go.name="SSRA · Hero Bastion semantic source";
   go.transform.rotation=Quaternion.Euler(0,180f,0);
   var b=Bounds(go);float span=Mathf.Max(b.size.x,b.size.z);
   float scale=Mathf.Min(12.6f/Mathf.Max(.001f,span),10.2f/Mathf.Max(.001f,b.size.y));
   go.transform.localScale*=scale;b=Bounds(go);
   var targetCenter=new Vector3(0f,0f,8.75f);
   go.transform.position+=new Vector3(targetCenter.x-b.center.x,2.52f-b.min.y,targetCenter.z-b.center.z);
   go.transform.SetParent(root,true);StripGameplay(go);PlacedModules++;
  }

  static void PlaceBenchmark(Transform root,string resource,string name,Vector3 p,float footprint,float h,Quaternion q)
  {
   var src=Resources.Load<GameObject>(resource);
   if(src==null)throw new InvalidOperationException("Missing SSRA resource: "+resource);
   var go=ValoriaKit.BenchmarkPiece("SSRA · "+name,src,p,footprint,h,q);
   if(go==null)throw new InvalidOperationException("Failed to place "+resource);
   go.transform.SetParent(root,true);StripGameplay(go);PlacedModules++;
  }

  static Bounds Bounds(GameObject go)
  {
   var rs=go.GetComponentsInChildren<Renderer>(true);
   if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
   var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
  }
 }
}
