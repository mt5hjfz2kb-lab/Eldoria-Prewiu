#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Domain;
using Eldoria.World4XCanon;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 [InitializeOnLoad] public static class ValoriaConsolidationIntegration {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity",Flag="Valoria.ConsolidationIntegration";
  const string Assets="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Consolidation20261027",ResourcesPath="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/ValoriaStrategic";
  static string Input=>Environment.GetEnvironmentVariable("ELDORIA_CONSOLIDATION_INPUT");
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/consolidation-20261027/"+Environment.GetEnvironmentVariable("GITHUB_RUN_ID"));
  static ValoriaConsolidationIntegration(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
  static string Hash(string p){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
  static void Fail(Exception e){Debug.LogException(e);Directory.CreateDirectory(Ev);File.WriteAllText(Ev+"/failure.txt",e.ToString());EditorApplication.Exit(1);}
  static Dictionary<string,Mesh> Meshes(){
   var result=new Dictionary<string,Mesh>();foreach(var p in JsonUtility.FromJson<ValoriaConsolidationTrial.Parts>(File.ReadAllText(Input+"/mesh-output.json")).parts){var m=new Mesh{name=p.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=p.vertices,normals=p.normals,uv=p.uv,colors=p.colors,subMeshCount=p.slots.Length};for(int i=0;i<p.slots.Length;i++)m.SetTriangles(p.slots[i].triangles,i);m.RecalculateBounds();result.Add(p.asset,m);}return result;
  }
  static Mesh Save(Mesh m,string path){var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(m,existing);Object.DestroyImmediate(m);EditorUtility.SetDirty(existing);return existing;}AssetDatabase.CreateAsset(m,path);return m;}
  public static void Integrate(){try{
   Directory.CreateDirectory(Ev);Need(Directory.Exists(Input),"Exact reviewed artifact missing");
   var expected=new Dictionary<string,string>{{"mesh-output.json","582ded9a1c1b38fd148da669399dbc92cf73c099b51b99f6d799c14c45bde4ce"},{"BastionI.png","e6518c47943956d3de271a9800014d375129c8ca1a790e364967e60ee82e789e"},{"BastionII.png","38aafb771b326ff54b6e73b872aefc105bae110f59226031fd3a3fb91bdb2a08"},{"BastionI-anchor.json","09ad319bf3df53ad12f6afe0e589cd7000ed37e25b5dabcab1589f6191eb8230"},{"BastionII-anchor.json","d7524ac113b106f7fccae69f48efb10a2f623eea47da1801a1585348adb14cfa"}};
   foreach(var pair in expected)Need(Hash(Input+"/"+pair.Key)==pair.Value,"Reviewed byte mismatch: "+pair.Key);
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);Directory.CreateDirectory(Assets);Directory.CreateDirectory(ResourcesPath);
   foreach(var key in new[]{"BastionI","BastionII"}){foreach(var extension in new[]{".png","-anchor.json"})File.Copy(Input+"/"+key+extension,ResourcesPath+"/"+key+extension,true);}
   AssetDatabase.Refresh();foreach(var key in new[]{"BastionI","BastionII"}){var importer=(TextureImporter)AssetImporter.GetAtPath(ResourcesPath+"/"+key+".png");importer.textureType=TextureImporterType.Default;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=1024;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
   var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();var bank=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(f=>f.name=="River_bank_rocks"&&!f.transform.IsChildOf(host.WorldRoot.transform));var foam=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(f=>f.name=="River_shore_foam"&&!f.transform.IsChildOf(host.WorldRoot.transform));
   var meshes=Meshes();var candidate=meshes.First(p=>p.Key.EndsWith("/TerrainPolish218/Riverbank.asset",StringComparison.Ordinal)).Value;var newFoam=ValoriaConsolidationTrial.Foam(bank,candidate,foam);bank.sharedMesh=Save(candidate,Assets+"/Riverbank.asset");foam.sharedMesh=Save(newFoam,Assets+"/RiverFoam.asset");EditorUtility.SetDirty(bank);EditorUtility.SetDirty(foam);AssetDatabase.SaveAssets();Need(EditorSceneManager.SaveScene(scene,Scene),"Localized scene bindings not saved");
   File.WriteAllText(Assets+"/source-receipt.json","{\"reviewed_run\":40425753661,\"reviewed_source\":\"b7ef0667dc407b4e9b1a6d93ba46d90d8d4b2076\",\"reviewed_mesh_json_sha256\":\"582ded9a1c1b38fd148da669399dbc92cf73c099b51b99f6d799c14c45bde4ce\",\"breach_changed\":false,\"paid_calls\":0,\"gameplay_authority_changed\":false}");AssetDatabase.Refresh();
   SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }catch(Exception e){Fail(e);}}
  static void Entered(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var h=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(!h){Fail(new Exception("Actual host missing"));return;}h.StartCoroutine(Guard(h));};}
  static IEnumerator Guard(ValoriaIntegratedWorld h){var p=Build(h);while(true){object value=null;bool more=false;try{more=p.MoveNext();if(more)value=p.Current;}catch(Exception e){Fail(e);yield break;}if(!more)yield break;yield return value;}}
  static IEnumerator Build(ValoriaIntegratedWorld h){yield return null;yield return new WaitForSecondsRealtime(1);Need(h.Ready,"Actual host not ready");if(!h.TrySetWorld(true)){var offer=h.Read("sawmill",false).Building.Action;Need(offer!=null&&offer.Kind=="Build","Real progression offer missing");h.Submit(offer);((ValoriaReferenceCleanGameplay.TestClock)h.City.Clock).Step(SliceRules.SawmillBuildSeconds+1);h.City.Gateway.Advance();yield return null;Need(h.TrySetWorld(true),"Actual World locked");}yield return null;
   if(!AssetDatabase.LoadAssetAtPath<GameObject>(ResourcesPath+"/StoneBridge.prefab")){
    var toggle=ValoriaConsolidationTrial.Bridge(h,Meshes());toggle(true);var root=h.WorldRoot.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Trial approved city stone bridge").gameObject;
    foreach(var f in root.GetComponentsInChildren<MeshFilter>(true)){var m=f.sharedMesh;var vertices=m.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]-=h.WorldHome;m.vertices=vertices;m.RecalculateBounds();f.sharedMesh=Save(m,Assets+"/"+f.name+".asset");foreach(var mat in f.GetComponent<Renderer>().sharedMaterials)Need(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mat)),"City source material must remain an existing asset");}
    root.transform.position=h.WorldHome;root.SetActive(true);Need(PrefabUtility.SaveAsPrefabAsset(root,ResourcesPath+"/StoneBridge.prefab"),"Reviewed bridge prefab save failed");AssetDatabase.SaveAssets();
   }
   File.WriteAllText(Ev+"/integration-receipt.json",File.ReadAllText(Assets+"/source-receipt.json"));Debug.Log("VALORIA_REFERENCE_CLEAN_CONSOLIDATION_INTEGRATION_COMPLETE exact_reviewed_bytes=1 no_publish=1");EditorApplication.Exit(0);
  }
 }
}
#endif
