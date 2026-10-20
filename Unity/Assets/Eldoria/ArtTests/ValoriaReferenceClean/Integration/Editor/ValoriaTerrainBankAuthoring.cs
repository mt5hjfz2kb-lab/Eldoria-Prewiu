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
 [Serializable] public class Geometry {public string name;public Vector3[] vertices; public int[] triangles;public Vector2[] uv;}
 [Serializable] public class SurveyData {public string scene_sha256,reference_sha256,source_commit;public Surface terrain; public Geometry[] meshes;public string camera,light;}
 [Serializable] public class Result {public string survey_sha256;public float[] heights;public Geometry[] meshes;public Geometry shore;public int changed_terrain_vertices,changed_bank_vertices;public bool art_approved;}
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
  var meshes=new List<Mesh>();var temporary=new List<Object>();var replaced=new List<Tuple<MeshFilter,Mesh>>();
  try{
   // Re-capture within this same process: exactly identical lighting/camera/URP.
   Views(c,"before");trial.SetHeights(0,0,hh);trial.SyncHeightmap();t.terrainData=trial;collider.terrainData=trial;t.Flush();
   var filters=Natural();foreach(var g in result.meshes){var f=filters.Single(m=>m.name==g.name);var old=f.sharedMesh;Need(old.vertexCount==g.vertices.Length,"Mesh topology changed");var copy=Object.Instantiate(old);copy.name=old.name+"_TerrainBank192_candidate";copy.vertices=g.vertices.Select(v=>f.transform.InverseTransformPoint(v)).ToArray();if(g.name=="River_bank_rocks")copy.RecalculateNormals();copy.RecalculateBounds();meshes.Add(copy);replaced.Add(Tuple.Create(f,old));f.sharedMesh=copy;}
   Need(result.shore!=null&&result.shore.vertices.Length>100&&result.shore.triangles.Length<18000,"Missing measured low bank");
   var bank=filters.Single(f=>f.name=="River_bank_rocks").GetComponent<Renderer>();
   var skin=new GameObject("Measured low river bank / candidate");temporary.Add(skin);var sm=new Mesh{name="Measured low river bank"};sm.vertices=result.shore.vertices;sm.triangles=result.shore.triangles;sm.uv=result.shore.uv;sm.RecalculateNormals();sm.RecalculateBounds();meshes.Add(sm);skin.AddComponent<MeshFilter>().sharedMesh=sm;var sr=skin.AddComponent<MeshRenderer>();sr.sharedMaterial=bank.sharedMaterials[0];
   // Coherent whole-meadow material treatment follows the geometry correction.
   // Preserve source textures/UVs; avoid the old dark localized blurred patch.
   var layers=trial.terrainLayers;
   foreach(int index in new[]{0,2,3}){
    var src=layers[index].diffuseTexture;var colors=src.GetPixels();var luminance=colors.Select(v=>v.grayscale).OrderBy(v=>v).ToArray();float low=luminance[luminance.Length/10],high=luminance[luminance.Length*9/10];
    for(int i=0;i<colors.Length;i++){float q=Mathf.Clamp01((colors[i].grayscale-low)/Mathf.Max(.01f,high-low));q=.15f+.70f*q;colors[i]=index==0?Color.Lerp(new Color(.32f,.38f,.10f),new Color(.66f,.62f,.25f),q):index==2?Color.Lerp(new Color(.47f,.38f,.25f),new Color(.67f,.55f,.36f),q):Color.Lerp(new Color(.40f,.40f,.14f),new Color(.72f,.63f,.33f),q);}
    var tex=new Texture2D(src.width,src.height,TextureFormat.RGB24,true){name="Measured meadow subdued fine contrast",wrapMode=TextureWrapMode.Repeat,anisoLevel=4};tex.SetPixels(colors);tex.Apply();temporary.Add(tex);
    var layer=Object.Instantiate(layers[index]);layer.diffuseTexture=tex;layer.smoothness=0;layer.metallic=0;layer.normalScale=Mathf.Min(layer.normalScale,.35f);if(index==0)layer.tileSize=new Vector2(7.5f,7.5f);temporary.Add(layer);layers[index]=layer;
   }
   trial.terrainLayers=layers;
   // Worn shoulders and damp soil connect the measured relief to existing access.
   int an=trial.alphamapResolution;var alpha=trial.GetAlphamaps(0,0,an,an);
   for(int z=0;z<an;z++)for(int x=0;x<an;x++){
    float wx=t.transform.position.x+x/(float)(an-1)*trial.size.x,wz=t.transform.position.z+z/(float)(an-1)*trial.size.z;
    float road=wz < -10?-1.5f+Mathf.Tan(14*Mathf.Deg2Rad)*(wz+10):-1.5f*Mathf.Clamp01((25.6f-wz)/35.6f);
    float noise=Mathf.PerlinNoise(wx*.31f+21,wz*.31f+14);
    float wear=wz>-78&&wz<27?(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3.1f,5.7f+noise,Mathf.Abs(wx-road))))*.65f:0;
    float river=-23+1.5f*Mathf.Sin(wx*.065f)+.85f*Mathf.Sin(wx*.19f+.4f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,17,Mathf.Abs(wx)));
    float width=7.1f+.75f*Mathf.Sin(wx*.095f+.7f)+.48f*Mathf.Sin(wx*.23f);
    float bankSoil=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.6f,6.4f+noise,Mathf.Abs(wz-river)-width)))*.68f;
    float amount=Mathf.Max(wear,bankSoil);for(int k=0;k<layers.Length;k++)alpha[z,x,k]*=1-amount;alpha[z,x,2]+=amount;
   }
   trial.SetAlphamaps(0,0,alpha);t.Flush();
   Physics.SyncTransforms();Views(c,"after");Need(stamp==Stamp(c)&&light==LightStamp(),"Camera/light changed");Need(Hash(Scene)==survey.scene_sha256,"Source scene mutated");
   File.WriteAllText(Ev+"/unity-review.json","{\"status\":\"REAL_UNITY_AB_REVIEW_REQUIRED\",\"same_camera\":true,\"same_light\":true,\"scene_saved\":false,\"art_approved\":false,\"gameplay_modified\":false,\"before_main_sha256\":\""+Hash(Ev+"/before-main.png")+"\",\"after_main_sha256\":\""+Hash(Ev+"/after-main.png")+"\",\"before_portrait_sha256\":\""+Hash(Ev+"/before-portrait.png")+"\",\"after_portrait_sha256\":\""+Hash(Ev+"/after-portrait.png")+"\"}");
  }finally{foreach(var pair in replaced)pair.Item1.sharedMesh=pair.Item2;t.terrainData=original;collider.terrainData=originalCollider;t.Flush();foreach(var m in meshes)Object.DestroyImmediate(m);foreach(var o in temporary)Object.DestroyImmediate(o);Object.DestroyImmediate(trial);}
 }
}}
#endif
