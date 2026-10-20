#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaTerrainBankAuthoring {
 const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 const string RefHash="86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d";
 static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/terrain-bank192");
 [Serializable] public class Surface { public int resolution;public Vector3 origin,size;public float[] heights; }
 [Serializable] public class Geometry {public string name;public Vector3[] vertices; public int[] triangles;}
 [Serializable] public class SurveyData {public string scene_sha256,reference_sha256,source_commit;public Surface terrain; public Geometry[] meshes;public string camera,light;}
 [Serializable] public class Result {public string survey_sha256;public float[] heights;public Geometry[] meshes;public int changed_terrain_vertices,changed_bank_vertices;public bool art_approved;}
 static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
 static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
 static string Stamp(Camera c)=>c.transform.position.ToString("R")+c.transform.rotation.ToString("R")+c.orthographicSize.ToString("R");
 static string LightStamp()=>RenderSettings.sun?RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.color.ToString("R")+RenderSettings.sun.intensity.ToString("R"):"none";
 static Terrain Ground()=>Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy).OrderBy(t=>Vector3.Distance(t.transform.position,Vector3.zero)).First();
 static MeshFilter[] Natural()=>Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.gameObject.activeInHierarchy&&f.sharedMesh&&(f.name=="River_bank_rocks"||f.name.StartsWith("River_shingle")||f.name.StartsWith("River_sedge")||f.name.StartsWith("Meadow_tufts")||f.name.StartsWith("Fir_tree_group")||f.name.StartsWith("Shrub_group")||f.name.StartsWith("Woodland_ground_rocks"))).ToArray();
 static Camera Open(){Need(Hash("../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg")==RefHash,"Canonical reference changed");Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var c=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Need(c&&c.PlayerCamera,"Integrated city absent");return c.PlayerCamera;}
 static void Views(Camera c,string prefix){ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-portrait.png",900,1600);}
 public static void Survey(){try {
  var c=Open();var t=Ground();var d=t.terrainData;int n=d.heightmapResolution;var h=d.GetHeights(0,0,n,n);var flat=new float[n*n];for(int z=0;z<n;z++)for(int x=0;x<n;x++)flat[z*n+x]=h[z,x];
  var data=new SurveyData{scene_sha256=Hash(Scene),reference_sha256=RefHash,source_commit=Environment.GetEnvironmentVariable("GITHUB_SHA"),terrain=new Surface{resolution=n,origin=t.transform.position,size=d.size,heights=flat},meshes=Natural().Select(f=>new Geometry{name=f.name,vertices=f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v)).ToArray(),triangles=f.sharedMesh.triangles}).ToArray(),camera=Stamp(c),light=LightStamp()};
  Need(data.meshes.Count(m=>m.name=="River_bank_rocks")==1,"Ambiguous current bank geometry");Views(c,"before");File.WriteAllText(Ev+"/survey.json",JsonUtility.ToJson(data));Debug.Log("VALORIA_REFERENCE_CLEAN_TERRAIN_SURVEY_COMPLETE");EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 public static void Trial(){try{Apply(false);Debug.Log("VALORIA_REFERENCE_CLEAN_TERRAIN_BANK_TRIAL_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 static void Apply(bool promote){
  Need(!promote,"Candidate must pass independent Unity review before promotion");var c=Open();var stamp=Stamp(c);var light=LightStamp();var survey=JsonUtility.FromJson<SurveyData>(File.ReadAllText(Ev+"/survey.json"));var result=JsonUtility.FromJson<Result>(File.ReadAllText(Ev+"/candidate.json"));
  Need(Hash(Scene)==survey.scene_sha256&&result.survey_sha256==Hash(Ev+"/survey.json"),"Exact live source/survey mismatch");Need(!result.art_approved&&result.changed_terrain_vertices>100&&result.changed_bank_vertices>100,"No geometric change or false approval");
  var t=Ground();var original=t.terrainData;var collider=t.GetComponent<TerrainCollider>();var originalCollider=collider.terrainData;var trial=Object.Instantiate(original);int n=trial.heightmapResolution;Need(result.heights.Length==n*n,"Height topology changed");var hh=new float[n,n];for(int z=0;z<n;z++)for(int x=0;x<n;x++){float v=result.heights[z*n+x];Need(!float.IsNaN(v)&&v>=0&&v<=1,"Invalid height");hh[z,x]=v;}
  var meshes=new List<Mesh>();var replaced=new List<Tuple<MeshFilter,Mesh>>();
  try{
   // Re-capture within this same process: exactly identical lighting/camera/URP.
   Views(c,"before");trial.SetHeights(0,0,hh);trial.SyncHeightmap();t.terrainData=trial;collider.terrainData=trial;t.Flush();
   var filters=Natural();foreach(var g in result.meshes){var f=filters.Single(m=>m.name==g.name);var old=f.sharedMesh;Need(old.vertexCount==g.vertices.Length,"Mesh topology changed");var copy=Object.Instantiate(old);copy.name=old.name+"_TerrainBank192_candidate";copy.vertices=g.vertices.Select(v=>f.transform.InverseTransformPoint(v)).ToArray();if(g.name=="River_bank_rocks")copy.RecalculateNormals();copy.RecalculateBounds();meshes.Add(copy);replaced.Add(Tuple.Create(f,old));f.sharedMesh=copy;}
   Physics.SyncTransforms();Views(c,"after");Need(stamp==Stamp(c)&&light==LightStamp(),"Camera/light changed");Need(Hash(Scene)==survey.scene_sha256,"Source scene mutated");
   File.WriteAllText(Ev+"/unity-review.json","{\"status\":\"REAL_UNITY_AB_REVIEW_REQUIRED\",\"same_camera\":true,\"same_light\":true,\"scene_saved\":false,\"art_approved\":false,\"gameplay_modified\":false,\"before_main_sha256\":\""+Hash(Ev+"/before-main.png")+"\",\"after_main_sha256\":\""+Hash(Ev+"/after-main.png")+"\",\"before_portrait_sha256\":\""+Hash(Ev+"/before-portrait.png")+"\",\"after_portrait_sha256\":\""+Hash(Ev+"/after-portrait.png")+"\"}");
  }finally{foreach(var pair in replaced)pair.Item1.sharedMesh=pair.Item2;t.terrainData=original;collider.terrainData=originalCollider;t.Flush();foreach(var m in meshes)Object.DestroyImmediate(m);Object.DestroyImmediate(trial);}
 }
}}
#endif
