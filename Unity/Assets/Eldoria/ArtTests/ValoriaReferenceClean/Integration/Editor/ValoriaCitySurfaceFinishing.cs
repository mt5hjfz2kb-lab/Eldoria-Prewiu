#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaCitySurfaceFinishing {
 const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 const string Tool="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaCitySurfaceFinishing.cs";
 const string Source="57a27c1dbb21d20646e6593695e4deb15b65c00aa277242c5de18d9eaf36ec00";
 static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/city-surface235");
 [Serializable] public class Acceptance {public bool accepted;public string source_scene_sha256,tool_sha256;public string[] images,hashes;}
 static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
 static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
 // Measure the existing paving silhouette in half-metre sections; no guessed new roads.
 class Road {
  public float z0,z1;public Vector3[] sections;
  public Road(MeshFilter f){
   var v=f.sharedMesh.vertices.Select(p=>f.transform.TransformPoint(p)).ToArray();z0=v.Min(p=>p.z);z1=v.Max(p=>p.z);
   int n=Mathf.CeilToInt((z1-z0)*2)+1;sections=new Vector3[n];
   for(int i=0;i<n;i++){var q=v.Where(p=>Mathf.Abs(p.z-(z0+i*.5f))<.6f).ToArray();Need(q.Length>0,"Measured paving section absent");sections[i]=new Vector3(q.Min(p=>p.x),q.Max(p=>p.x),q.Min(p=>p.y));}
  }
  public float Weight(float x,float z,out float edgeY){
   edgeY=0;if(z<z0||z>z1)return 0;
   var s=sections[Mathf.Clamp(Mathf.RoundToInt((z-z0)*2),0,sections.Length-1)];edgeY=s.z-.025f;
   float distance=Mathf.Max(s.x-x,x-s.y);if(distance<-.3f)return 0;
   float width=1.05f+.85f*Mathf.PerlinNoise(x*.43f+27,z*.33f+9);
   return (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.15f,width,distance)))*Mathf.Clamp01((z-z0)*2)*Mathf.Clamp01((z1-z)*2);
  }
 }
 static void Views(ValoriaReferenceCleanGameplay city,string prefix){
  var c=city.PlayerCamera;var pos=c.transform.position;float zoom=c.orthographicSize;
  var roots=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,city.Parcels.RightBuilt};var active=roots.Select(r=>r.activeSelf).ToArray();
  try{for(int state=0;state<2;state++){
   roots[0].SetActive(state==0);roots[1].SetActive(state==1);roots[2].SetActive(state==1);roots[3].SetActive(state==1);
   string n=state==0?"I":"II";c.transform.position=pos;c.orthographicSize=zoom;
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+n+"-landscape.png",1448,1086);
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+n+"-portrait.png",900,1600);
   var r=roots[state].GetComponentsInChildren<MeshRenderer>(true).Single(x=>x.name=="Bastion_"+n);c.transform.position=r.bounds.center-c.transform.forward*100;c.orthographicSize=11;
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+n+"-detail.png",1448,1086);
  }}finally{c.transform.position=pos;c.orthographicSize=zoom;for(int i=0;i<4;i++)roots[i].SetActive(active[i]);}
 }
 public static void Trial(){Run(false);}
 public static void Integrate(){Run(true);}
 static void Run(bool integrate){try{
  Need(Hash(Scene)==Source,"Current reviewed integrated source required");Directory.CreateDirectory(Ev);
  var approved=new Dictionary<string,byte[]>();if(integrate){var a=JsonUtility.FromJson<Acceptance>(File.ReadAllText(Ev+"/accepted-candidate.json"));Need(a.accepted&&a.source_scene_sha256==Source&&a.tool_sha256==Hash(Tool),"Reviewed exact surface tool required");Need(a.images.Length==6&&a.hashes.Length==6,"Six views required");for(int i=0;i<6;i++){Need(Hash(Ev+"/"+a.images[i])==a.hashes[i],"Reviewed capture changed");approved.Add(a.images[i],File.ReadAllBytes(Ev+"/"+a.images[i]));}}
  EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var t=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();var old=t.terrainData;var collider=t.GetComponent<TerrainCollider>();var physics=collider.terrainData;
  var lights=Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(l=>l.type==LightType.Point&&l.name=="Torch glow"&&l.transform.position.z>27&&l.transform.position.z<32&&Mathf.Abs(l.transform.position.x)<4).ToArray();Need(lights.Length==4,"Four retained Bastion torch lights required");
  var bake=lights.Select(l=>l.lightmapBakeType).ToArray();var modes=lights.Select(l=>l.renderMode).ToArray();TerrainData next=null;bool saved=false;
  try{
   Views(city,"before");next=Object.Instantiate(old);next.name="Existing city paving / measured earthen shoulder contact";
   var filters=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);var roads=new[]{"Axial_Paving","Outer_Road"}.Select(n=>new Road(filters.Single(f=>f.name==n))).ToArray();
   int hn=next.heightmapResolution,an=next.alphamapResolution,changed=0;float maxDelta=0;var heights=next.GetHeights(0,0,hn,hn);
   for(int z=0;z<hn;z++)for(int x=0;x<hn;x++){
    float wx=t.transform.position.x+x/(float)(hn-1)*next.size.x,wz=t.transform.position.z+z/(float)(hn-1)*next.size.z;
    foreach(var road in roads){float weight=road.Weight(wx,wz,out float edgeY);if(weight<=0)continue;float ground=t.transform.position.y+heights[z,x]*next.size.y;
     float delta=Mathf.Clamp(edgeY-ground,0,.38f)*weight;if(delta>.0001f){heights[z,x]+=delta/next.size.y;maxDelta=Mathf.Max(maxDelta,delta);changed++;}}
   }
   Need(changed>100,"No measured road contact correction");next.SetHeights(0,0,heights);
   var alpha=next.GetAlphamaps(0,0,an,an);int layers=alpha.GetLength(2);
   for(int z=0;z<an;z++)for(int x=0;x<an;x++){
    float wx=t.transform.position.x+x/(float)(an-1)*next.size.x,wz=t.transform.position.z+z/(float)(an-1)*next.size.z;
    float w=0;foreach(var road in roads)w=Mathf.Max(w,road.Weight(wx,wz,out float unused));w*=.72f;
    for(int k=0;k<layers;k++)alpha[z,x,k]*=1-w;alpha[z,x,2]+=w;
   }
   next.SetAlphamaps(0,0,alpha);t.terrainData=next;t.Flush();Need(collider.terrainData==physics,"Collision terrain changed");
   foreach(var l in lights){l.lightmapBakeType=LightmapBakeType.Realtime;l.renderMode=LightRenderMode.ForcePixel;}
   Views(city,"after");Need(Hash(Scene)==Source,"Trial changed source scene");
   File.WriteAllText(Ev+"/unity-review.json","{\"source_scene_sha256\":\""+Source+"\",\"tool_sha256\":\""+Hash(Tool)+"\",\"changed_height_samples\":"+changed+",\"maximum_visual_raise_m\":"+maxDelta.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+",\"existing_torch_lights_reused\":4,\"new_lights\":0,\"new_shadow_maps\":0,\"collision_unchanged\":true,\"plots_created\":false,\"art_approved\":false}");
   if(integrate){
    var diffs=new List<string>();foreach(var im in approved){diffs.Add(ValoriaTerrainBankAuthoring.CompareRender(im.Value,Ev+"/"+im.Key));File.WriteAllBytes(Ev+"/approved-"+im.Key,im.Value);}File.WriteAllText(Ev+"/render-identity.json","["+string.Join(",",diffs)+"]");
    const string folder="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/CitySurface235";Need(!Directory.Exists(folder),"Refuse overwriting existing finish");Directory.CreateDirectory(folder);AssetDatabase.Refresh();AssetDatabase.CreateAsset(next,folder+"/Terrain.asset");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(city.gameObject.scene);Need(EditorSceneManager.SaveScene(city.gameObject.scene,Scene),"Save failed");saved=true;File.WriteAllText(Ev+"/integrated.json","{\"scene_sha256\":\""+Hash(Scene)+"\",\"published\":false}");
   }
  }finally{if(!saved){t.terrainData=old;t.Flush();for(int i=0;i<lights.Length;i++){lights[i].lightmapBakeType=bake[i];lights[i].renderMode=modes[i];}if(next)Object.DestroyImmediate(next);}}
  if(integrate)ValoriaIntegrationBuild.ValidateCurrent();else{Debug.Log("VALORIA_REFERENCE_CLEAN_CITY_SURFACE_TRIAL_COMPLETE");EditorApplication.Exit(0);}
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}}
#endif
