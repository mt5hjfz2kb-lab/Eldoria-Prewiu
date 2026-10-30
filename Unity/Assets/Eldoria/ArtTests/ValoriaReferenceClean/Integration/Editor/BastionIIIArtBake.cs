#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 // Extracts the existing authored barn into build-safe assets. No geometry/material redesign.
 // Android's established packaging intentionally removes the legacy Resources/Valoria GLBs.
 public static class BastionIIIArtBake {
  const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII/Granary";
  static readonly Dictionary<Object,Object> saved=new Dictionary<Object,Object>();static int index;
  static T Save<T>(T original) where T:Object {
   if(!original)return null;if(saved.TryGetValue(original,out var prior))return (T)prior;
   var clone=Object.Instantiate(original);clone.name=original.name;
   string file=Root+"/part-"+(index++)+".asset";var existing=AssetDatabase.LoadAssetAtPath<T>(file);
   if(existing){EditorUtility.CopySerialized(clone,existing);Object.DestroyImmediate(clone);clone=existing;EditorUtility.SetDirty(clone);}else AssetDatabase.CreateAsset(clone,file);
   saved.Add(original,clone);return clone;
  }
  // Rehome original geometry, PBR materials and textures, so source GLBs can
  // safely be excluded from the signed Android APK without losing visuals.
  static void Rehome(GameObject model){
   foreach(var f in model.GetComponentsInChildren<MeshFilter>(true))f.sharedMesh=Save(f.sharedMesh);
   foreach(var r in model.GetComponentsInChildren<Renderer>(true)){
    var materials=r.sharedMaterials;
    for(int i=0;i<materials.Length;i++){
     var original=materials[i];var material=Save(original);if(!material)continue;
     foreach(var property in original.GetTexturePropertyNames()){
      var texture=original.GetTexture(property);if(texture)material.SetTexture(property,Save(texture));
     }
     EditorUtility.SetDirty(material);materials[i]=material;
    }
    r.sharedMaterials=materials;
   }
  }
  // EA03 production stock prefabs point into legacy Valoria GLB meshes.
  // Source-independent clones are mandatory because Android intentionally
  // excludes those GLBs. Preserve EXACT approved art/LOD0, no procedural proxy.
  static void RehomeProductionStock(string name){
   string input="Assets/Eldoria/Resources/Valoria/UrbanProps/"+name+".prefab";
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(input);
   if(!source)throw new Exception("Missing original authored production stock: "+input);
   var stock=Object.Instantiate(source);
   try{
    if(PrefabUtility.IsPartOfPrefabInstance(stock))
     PrefabUtility.UnpackPrefabInstance(stock,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
    foreach(var collider in stock.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
    Rehome(stock);
    var path=Root+"/Stock"+name+".prefab";
    var prefab=PrefabUtility.SaveAsPrefabAsset(stock,path);
    if(!prefab||prefab.GetComponentsInChildren<Renderer>(true).Length==0)
     throw new Exception("Missing build-safe stock artwork: "+path);
   }finally{if(stock)Object.DestroyImmediate(stock);}
  }
  static void RehomeStoneWorkshop(){
   const string input="Assets/EmaceArt/Slavic World Free/Prefabs/Village/Building/Shed/EA03_Village_OutBuilding_Shed_03b_PRE.prefab";
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(input);
   if(!source)throw new Exception("Missing original textured stone workshop: "+input);
   var wrapper=new GameObject("Stone workshop I-IV");
   try{
    var model=Object.Instantiate(source,wrapper.transform,false);
    if(PrefabUtility.IsPartOfPrefabInstance(model))PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
    foreach(var collider in model.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
    Rehome(wrapper);
    if(!PrefabUtility.SaveAsPrefabAsset(wrapper,Root+"/StoneWorkshop.prefab"))throw new Exception("Stone workshop prefab was not saved");
   }finally{Object.DestroyImmediate(wrapper);}
  }
  public static void Build(){GameObject model=null;try{
   Directory.CreateDirectory(Root);AssetDatabase.Refresh();saved.Clear();index=0;
   foreach(var resource in new[]{"AshWolf","PrivateFissure"}){
    string file="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII/"+resource+".glb";
    var imported=AssetDatabase.LoadAssetAtPath<GameObject>(file);
    if(!imported||imported.GetComponentsInChildren<Renderer>(true).Length==0)throw new Exception("Required III art failed native import: "+resource);
    if(resource=="AshWolf"&&(!imported.GetComponentInChildren<Animation>(true)||!AssetDatabase.LoadAllAssetsAtPath(file).OfType<AnimationClip>().Any(x=>x.name=="Idle")))throw new Exception("Authored wolf Idle/legacy animation missing");
   }
   const string input="Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb";
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(input);if(!source)throw new Exception("Native granary importer has no prefab");model=Object.Instantiate(source);
   Rehome(model);
   var prefab=PrefabUtility.SaveAsPrefabAsset(model,Root+"/Granary.prefab");
   if(!prefab)throw new Exception("Granary prefab was not saved");
   foreach(var stock in new[]{"Crate","Sack","Barrel"})RehomeProductionStock(stock);
   RehomeStoneWorkshop();
   AssetDatabase.SaveAssets();
   // Prove Android package independence, not merely prefab existence.
   // The signing job removes ALL legacy Valoria GLB sources before compiling.
   foreach(var item in new[]{"Granary","StockCrate","StockSack","StockBarrel","StoneWorkshop"}){
    var path=Root+"/"+item+".prefab";
    var dependencies=AssetDatabase.GetDependencies(path,true);
    var leaked=dependencies.Where(d=>d.StartsWith("Assets/Eldoria/Resources/Valoria/",StringComparison.OrdinalIgnoreCase)&&
       d.EndsWith(".glb",StringComparison.OrdinalIgnoreCase)).ToArray();
    if(leaked.Length>0)throw new Exception("Android prefab depends on excluded legacy GLB: "+item+" -> "+string.Join(",",leaked));
   }
   Directory.CreateDirectory("../docs/bastion-iii/native");File.WriteAllText("../docs/bastion-iii/native/granary-bake.json","{\"source\":\""+input+"\",\"geometry_changed\":false,\"materials_changed\":false,\"legacy_source_exclusion_safe\":true}");
   Debug.Log("BASTION_III_GRANARY_AND_STOCK_BUILD_SAFE_PASS");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}finally{if(model)Object.DestroyImmediate(model);}}
 }
}
#endif
