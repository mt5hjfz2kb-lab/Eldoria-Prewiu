#if UNITY_EDITOR
using System;
using System.IO;
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
  public static void Build(){GameObject model=null;try{
   Directory.CreateDirectory(Root);AssetDatabase.Refresh();saved.Clear();index=0;
   const string input="Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb";
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(input);if(!source)throw new Exception("Native granary importer has no prefab");model=Object.Instantiate(source);
   foreach(var f in model.GetComponentsInChildren<MeshFilter>(true))f.sharedMesh=Save(f.sharedMesh);
   foreach(var r in model.GetComponentsInChildren<Renderer>(true)){
    var materials=r.sharedMaterials;
    for(int i=0;i<materials.Length;i++){
     var original=materials[i];var material=Save(original);if(!material)continue;
     foreach(var property in original.GetTexturePropertyNames()){var texture=original.GetTexture(property);if(texture)material.SetTexture(property,Save(texture));}
     EditorUtility.SetDirty(material);materials[i]=material;
    }r.sharedMaterials=materials;
   }
   var prefab=PrefabUtility.SaveAsPrefabAsset(model,Root+"/Granary.prefab");if(!prefab)throw new Exception("Granary prefab was not saved");AssetDatabase.SaveAssets();
   Directory.CreateDirectory("../docs/bastion-iii/native");File.WriteAllText("../docs/bastion-iii/native/granary-bake.json","{\"source\":\""+input+"\",\"geometry_changed\":false,\"materials_changed\":false,\"legacy_source_exclusion_safe\":true}");
   Debug.Log("BASTION_III_GRANARY_BAKE_PASS");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}finally{if(model)Object.DestroyImmediate(model);}}
 }
}
#endif
