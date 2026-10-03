using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
 public static class ValoriaVisualQualityBreakthroughV1
 {
  public const string RootName="Valoria · Visual Quality Breakthrough v1";
  public static string HeroVariant="low14";
  public static int HiddenLegacyWallRenderers,AuthoredWallModules,SecondaryFoundationModules,BastionTransitionModules,ExteriorModules;

  public static void Apply(Transform canonicalRoot,PlayerState state)
  {
   if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
   var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
   var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
   HiddenLegacyWallRenderers=AuthoredWallModules=SecondaryFoundationModules=BastionTransitionModules=ExteriorModules=0;

   var hero=GameObject.Find("Valoria · Flat Citadel · Hero Bastion");
   if(hero==null)throw new InvalidOperationException("Canonical Hero Bastion not found.");

   string key="Valoria/VQBSegments/Valoria_HeroBastion_segmented_"+HeroVariant;
   var source=Resources.Load<GameObject>(key);
   if(source==null)throw new InvalidOperationException("Segmented Hero candidate missing: "+key);

   var originalRenderers=hero.GetComponentsInChildren<Renderer>(true);
   Material response=null;
   foreach(var r in originalRenderers)
   {
    if(response==null&&r.sharedMaterials.Length>0)response=r.sharedMaterials[0];
    if(r.enabled){r.enabled=false;HiddenLegacyWallRenderers++;}
   }

   var candidate=Object.Instantiate(source);
   candidate.name="Valoria · breakthrough · Hero Bastion "+HeroVariant;
   candidate.transform.SetParent(hero.transform.parent,true);
   candidate.transform.position=hero.transform.position;
   candidate.transform.rotation=hero.transform.rotation;
   candidate.transform.localScale=hero.transform.localScale;

   foreach(var r in candidate.GetComponentsInChildren<Renderer>(true))
   {
    if(response!=null)
    {
     var mats=r.sharedMaterials;
     for(int i=0;i<mats.Length;i++)mats[i]=response;
     r.sharedMaterials=mats;
    }
   }
   StripGameplay(candidate);
   BastionTransitionModules=1;
  }

  static void StripGameplay(GameObject go)
  {
   foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
   foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
   foreach(var lod in go.GetComponentsInChildren<LODGroup>(true))lod.ForceLOD(0);
  }
 }
}
