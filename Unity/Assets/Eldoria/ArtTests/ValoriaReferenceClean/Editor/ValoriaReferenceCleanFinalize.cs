using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
// Package the observed scene and current materials; no layout or gameplay changes.
public static class ValoriaReferenceCleanFinalize {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Scene=Root+"/Valoria_Reference_Clean.unity";
 static readonly string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable]class Report{public string source_sha,scene=Scene,prefab_lighting="Portable prefabs use light probes; placed scene keeps its baked GI";public string[] prefabs;public int scene_triangles,scene_renderers;public bool geometry_unchanged,art_approved=false,physical_mobile_profiled=false;}
 static void Portable(GameObject root){foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true)){r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);}}
 public static void Build(){try{
  var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var cam=Camera.main;var meshes=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);int before=meshes.Sum(f=>f.sharedMesh.triangles.Length/3);var results=new List<string>();
  void Section(string name,Func<MeshFilter,bool> match,Vector3 pivot){var group=new GameObject(name);group.transform.position=pivot;try{foreach(var mf in meshes.Where(match)){var copy=Object.Instantiate(mf.gameObject,mf.transform.parent);copy.transform.SetParent(group.transform,true);}if(group.transform.childCount==0)throw new Exception("Prefab source missing: "+name);Portable(group);group.transform.position=Vector3.zero;string path=Root+"/Prefabs/"+name+".prefab";PrefabUtility.SaveAsPrefabAsset(group,path);results.Add(path);}finally{Object.DestroyImmediate(group);}}
  Section("Bastion_I",f=>f.name.StartsWith("Bastion_I"),new Vector3(0,.35f,36.5f));Section("FortifiedGate",f=>f.name.StartsWith("Gate_"),new Vector3(-1.5f,.35f,-10));
  float st=Mathf.Sin(-14*Mathf.Deg2Rad),ct=Mathf.Cos(-14*Mathf.Deg2Rad);Section("MedievalBridge",f=>f.name.StartsWith("Bridge_"),new Vector3(-1.5f+13*st,.5f,-13*ct-10));
  Section("Paths",f=>f.name.StartsWith("Axial_Paving")||f.name.StartsWith("Outer_Road"),Vector3.zero);Section("BuildablePlots",f=>f.name.StartsWith("Plot_"),Vector3.zero);Section("Ruins",f=>f.name.StartsWith("Ruin_"),Vector3.zero);Section("RiverSurface",f=>f.name=="River water",Vector3.zero);
  var authored=GameObject.Find("Valoria authored architecture and environment");if(!authored)throw new Exception("Authored root missing");var assembly=Object.Instantiate(authored);try{Portable(assembly);string path=Root+"/Prefabs/ValoriaArchitecture.prefab";PrefabUtility.SaveAsPrefabAsset(assembly,path);results.Add(path);}finally{Object.DestroyImmediate(assembly);}
  int after=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Sum(f=>f.sharedMesh.triangles.Length/3);var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),prefabs=results.ToArray(),scene_triangles=after,scene_renderers=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length,geometry_unchanged=before==after};if(!report.geometry_unchanged)throw new Exception("Prefab packaging left geometry in scene");
  File.WriteAllText(Ev+"/editable-delivery.json",JsonUtility.ToJson(report,true));ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/D-final.png",1448,1086);EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();AssetDatabase.ExportPackage(new[]{Scene,Root+"/Prefabs"},Ev+"/Valoria_Reference_Clean.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_FINALIZE_COMPLETE");EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
}
