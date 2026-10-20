#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaTerrainPolish {
 const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/terrain-polish218");
 [Serializable] public class Bank {public string survey_sha256,source_scene_sha256;public Vector3[] vertices,normals;public Vector2[] uv;public int[] triangles;}
 [Serializable] public class Acceptance {public bool accepted;public string source_scene_sha256,candidate_sha256;public string[] images,hashes;}
 static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
 static void Need(bool x,string m){if(!x)throw new InvalidOperationException(m);}
 static void Views(ValoriaReferenceCleanGameplay city,string prefix){
  var c=city.PlayerCamera;var objects=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,city.Parcels.RightBuilt};var active=objects.Select(o=>o.activeSelf).ToArray();
  try{Capture("initial");city.BastionOne.SetActive(false);city.BastionTwo.SetActive(true);city.Parcels.LeftBuilt.SetActive(true);city.Parcels.RightBuilt.SetActive(true);Capture("built");}
  finally{for(int i=0;i<objects.Length;i++)objects[i].SetActive(active[i]);}
  void Capture(string state){ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+state+"-landscape.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+state+"-portrait.png",900,1600);}
 }
 public static void Trial(){Run(false);}
 public static void Integrate(){Run(true);}
 static void Run(bool integrate){try{
  var data=JsonUtility.FromJson<Bank>(File.ReadAllText(Ev+"/bank-weathered.json"));Need(data.source_scene_sha256==Hash(Scene)&&data.survey_sha256==Hash(Ev+"/survey.json"),"Exact current geometry source required");
  var approved=new Dictionary<string,byte[]>();if(integrate){var a=JsonUtility.FromJson<Acceptance>(File.ReadAllText(Ev+"/accepted-candidate.json"));Need(a.accepted&&a.source_scene_sha256==Hash(Scene)&&a.candidate_sha256==Hash(Ev+"/bank-weathered.json"),"Reviewed candidate required");Need(a.images.Length==4&&a.hashes.Length==4,"Four reviewed city states required");for(int i=0;i<4;i++){Need(Hash(Ev+"/"+a.images[i])==a.hashes[i],"Image source changed");approved.Add(a.images[i],File.ReadAllBytes(Ev+"/"+a.images[i]));}}
  EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var terrain=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();var bank=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="River_bank_rocks");var oldMesh=bank.sharedMesh;var original=terrain.terrainData;var collider=terrain.GetComponent<TerrainCollider>();var originalPhysics=collider.terrainData;var rr=bank.GetComponent<MeshRenderer>();var oldMaterials=rr.sharedMaterials;var owned=new List<Object>();bool saved=false;
  try{
   Views(city,"before");
   var mesh=new Mesh{name="Retained riverbank / rounded weathering and metric UV",indexFormat=IndexFormat.UInt32};mesh.vertices=data.vertices.Select(v=>bank.transform.InverseTransformPoint(v)).ToArray();mesh.normals=data.normals.Select(v=>bank.transform.InverseTransformDirection(v).normalized).ToArray();mesh.uv=data.uv;mesh.triangles=data.triangles;mesh.RecalculateBounds();mesh.RecalculateTangents();Need(mesh.triangles.Length/3<160000,"Bank triangle budget exceeded");owned.Add(mesh);bank.sharedMesh=mesh;
   // The old colliders remain exactly as validated: relief and access unchanged.
   var rockTexture=new Texture2D(2,2,TextureFormat.RGBA32,true){name="Retained CC0 rock3 river stone",wrapMode=TextureWrapMode.Repeat,anisoLevel=4};Need(rockTexture.LoadImage(File.ReadAllBytes("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/rock_3/rock_3_diff_1k.png")),"Retained rock map missing");owned.Add(rockTexture);
   var stone=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Weathered river stone / restrained fine scale"};stone.SetTexture("_BaseMap",rockTexture);stone.SetColor("_BaseColor",new Color(.78f,.77f,.73f));stone.SetFloat("_Smoothness",.07f);owned.Add(stone);rr.sharedMaterials=new[]{stone};
   var next=Object.Instantiate(original);next.name="Measured meadow / woodland and shoreline transitions";owned.Add(next);var layers=next.terrainLayers;var fresh=Object.Instantiate(layers[0]);fresh.name="Meadow sheltered green / existing grass texture";var source=layers[0].diffuseTexture;var pixels=source.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=new Color(pixels[i].r*.76f,pixels[i].g*.96f,pixels[i].b*1.12f,1);
   var grass=new Texture2D(source.width,source.height,TextureFormat.RGB24,true){name="Sheltered meadow source variation",wrapMode=TextureWrapMode.Repeat,anisoLevel=4};grass.SetPixels(pixels);grass.Apply();owned.Add(grass);fresh.diffuseTexture=grass;fresh.normalScale=.3f;owned.Add(fresh);var expanded=layers.Concat(new[]{fresh}).ToArray();next.terrainLayers=expanded;
   int n=next.alphamapResolution;var old=original.GetAlphamaps(0,0,n,n);var alpha=new float[n,n,expanded.Length];
   var trees=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Single(f=>f.name=="Fir_tree_group");var anchors=new Dictionary<Vector2Int,Vector3>();foreach(var v in trees.sharedMesh.vertices){var p=trees.transform.TransformPoint(v);float ground=terrain.SampleHeight(p)+terrain.transform.position.y;if(p.y-ground<-.1f||p.y-ground>.3f)continue;var key=new Vector2Int(Mathf.RoundToInt(p.x/3),Mathf.RoundToInt(p.z/3));if(!anchors.ContainsKey(key))anchors.Add(key,p);}
   for(int z=0;z<n;z++)for(int x=0;x<n;x++){
    float wx=terrain.transform.position.x+x/(float)(n-1)*next.size.x,wz=terrain.transform.position.z+z/(float)(n-1)*next.size.z;
    float broad=Mathf.PerlinNoise(wx*.047f+31,wz*.067f+18),detail=Mathf.PerlinNoise(wx*.19f+81,wz*.21f+8);
    float river=-23+1.5f*Mathf.Sin(wx*.065f)+.85f*Mathf.Sin(wx*.19f+.4f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,17,Mathf.Abs(wx)));
    float shore=Mathf.Abs(wz-river)-(7.1f+.75f*Mathf.Sin(wx*.095f+.7f)+.48f*Mathf.Sin(wx*.23f));
    float nearest=1000;foreach(var p in anchors.Values)nearest=Mathf.Min(nearest,(p.x-wx)*(p.x-wx)+(p.z-wz)*(p.z-wz));nearest=Mathf.Sqrt(nearest);
    float woodland=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.3f,6+detail*2,nearest)))*.32f;
    float bankGreen=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2,10,shore)))*.3f;
    float green=Mathf.Min(1-woodland,Mathf.Clamp01(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.28f,.67f,broad))*.65f+bankGreen));
    for(int k=0;k<layers.Length;k++)alpha[z,x,k]=old[z,x,k];
    float meadow=alpha[z,x,0];alpha[z,x,0]=meadow*(1-green-woodland);alpha[z,x,layers.Length]=meadow*green;alpha[z,x,2]+=meadow*woodland;
   }
   next.SetAlphamaps(0,0,alpha);terrain.terrainData=next;terrain.Flush();Need(collider.terrainData==originalPhysics,"Physics terrain unexpectedly changed");Views(city,"after");Need(Hash(Scene)==data.source_scene_sha256,"Trial saved source scene");
   File.WriteAllText(Ev+"/unity-review.json","{\"same_camera_light\":true,\"bank_triangles\":"+(mesh.triangles.Length/3)+",\"terrain_heights_unchanged\":true,\"colliders_unchanged\":true,\"plot_footprints_created\":false,\"scene_saved\":false,\"art_approved\":false}");
   if(integrate){var diffs=new List<string>();foreach(var a in approved){diffs.Add(ValoriaTerrainBankAuthoring.CompareRender(a.Value,Ev+"/"+a.Key));File.WriteAllBytes(Ev+"/approved-"+a.Key,a.Value);}File.WriteAllText(Ev+"/render-identity.json","["+string.Join(",",diffs)+"]");const string folder="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/TerrainPolish218";Need(!Directory.Exists(folder),"Existing art folder must not be overwritten");Directory.CreateDirectory(folder);AssetDatabase.Refresh();int i=0;foreach(var o in owned.OfType<Texture2D>())AssetDatabase.CreateAsset(o,folder+"/texture-"+(i++)+".asset");AssetDatabase.CreateAsset(fresh,folder+"/ShelteredGrass.terrainlayer");AssetDatabase.CreateAsset(next,folder+"/Terrain.asset");AssetDatabase.CreateAsset(mesh,folder+"/Riverbank.asset");AssetDatabase.CreateAsset(stone,folder+"/Riverstone.mat");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(city.gameObject.scene);Need(EditorSceneManager.SaveScene(city.gameObject.scene,Scene),"Save failed");saved=true;File.WriteAllText(Ev+"/integrated.json","{\"scene_sha256\":\""+Hash(Scene)+"\",\"gameplay_gate\":\"PENDING_CURRENT_PROBE\",\"published\":false}");}
  }finally{if(!saved){bank.sharedMesh=oldMesh;rr.sharedMaterials=oldMaterials;terrain.terrainData=original;terrain.Flush();foreach(var o in owned)Object.DestroyImmediate(o);}}
  if(integrate)ValoriaIntegrationBuild.ValidateCurrent();else{Debug.Log("VALORIA_REFERENCE_CLEAN_TERRAIN_POLISH_TRIAL_COMPLETE");EditorApplication.Exit(0);}
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}}
#endif
